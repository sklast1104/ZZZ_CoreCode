using System;
using System.Collections.Generic;
using Cinemachine;
using JM.Indicator;
using JM.TimelineAction;
using JM.UI;
using Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JM
{
    public class SimplePlayerSystem : MonoBehaviour
    {
        public static SimplePlayerSystem Instance { get; private set; }

        [SerializeField] private CinemachineVirtualCamera vcam;
        [SerializeField] private List<SimpleRoleCtrl> roleCtrls;
        [SerializeField] private GameObject parrySuccessParticlePrefab;
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip parrySfxClip;

        public GameInput input { get; private set; }
        public SimpleRoleCtrl FrontRoleCtrl => roleCtrls[_frontRoleIdx];
        public List<SimpleRoleCtrl> RoleCtrls => roleCtrls;

        private int _frontRoleIdx;

        private int FrontRoleIdx
        {
            get => _frontRoleIdx;
            set
            {
                if (_frontRoleIdx == value) return;
                _frontRoleIdx = value;

                var roleCtrl = roleCtrls[_frontRoleIdx];
                roleCtrl.VCam = vcam;
                vcam.Follow = roleCtrl.vcamFollow;
                vcam.LookAt = roleCtrl.vcamLookAt;
            }
        }

        private bool _enableInput = true;
        public bool EnableInput
        {
            get => _enableInput;
            set
            {
                _enableInput = value;
                var provider = vcam.GetComponent<GameInputCameraProvider>();
                if (provider != null) provider.enabled = value;
                if (value) input.Combat.Enable();
                else input.Combat.Disable();
            }
        }

        public bool switchEnabled = true;

        // 음량 조절
        [Range(0f, 1f)] private float _masterVol = 1f;
        [Range(0f, 1f)] private float _musicVol = 0.5f;
        [Range(0f, 1f)] private float _sfxVol = 1f;
        private bool _showVolume;

        private void Awake()
        {
            Instance = this;
            input = new GameInput();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            input.Dispose();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Start()
        {
            if (roleCtrls == null) roleCtrls = new List<SimpleRoleCtrl>();

            // Remove null entries
            roleCtrls.RemoveAll(rc => rc == null);

            if (roleCtrls.Count == 0)
            {
                var found = FindObjectOfType<SimpleRoleCtrl>();
                if (found != null)
                    roleCtrls = new List<SimpleRoleCtrl> { found };
            }

            // Activate all roles so Awake() runs, then background ones get hidden by InitBackground
            foreach (var rc in roleCtrls)
            {
                if (!rc.gameObject.activeSelf)
                    rc.gameObject.SetActive(true);
            }

            // Inject GameInput to all roles
            foreach (var rc in roleCtrls)
                rc.GameInput = input;

            // Init first role as front, rest as background
            _frontRoleIdx = 0;
            var front = roleCtrls[0];
            front.VCam = vcam;
            vcam.Follow = front.vcamFollow;
            vcam.LookAt = front.vcamLookAt;

            var provider = vcam.GetComponent<GameInputCameraProvider>();
            if (provider != null) provider.SetGameInput(input);

            roleCtrls[0].InitFront();
            for (int i = 1; i < roleCtrls.Count; i++)
                roleCtrls[i].InitBackground();

            // Switch character inputs (separate from foreach to avoid command routing)
            input.Combat.SwitchCharactersNext.started += HandleSwitchNext;
            input.Combat.SwitchCharactersPrevious.started += HandleSwitchPrev;

            foreach (var action in input.Combat.Get().actions)
            {
                action.started += HandleStartedInput;
                action.canceled += HandleCanceledInput;
            }

            input.Combat.Enable();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            InitBattleHUD();

            // 음량 초기값 적용 + BGM 재생
            if (AudioMgr.Instance != null)
            {
                AudioMgr.Instance.MainVolume = _masterVol;
                AudioMgr.Instance.MusicVolume = _musicVol;
                AudioMgr.Instance.SFXVolume = _sfxVol;
                if (bgmClip != null)
                    AudioMgr.Instance.PlayMusic(bgmClip);
            }
        }

        private void InitBattleHUD()
        {
            if (SimpleUIMgr.Instance == null) return;

            // // Monster HP bars (runtime instantiated per monster)
            // foreach (var monster in SimpleMonsterCtrl.AllMonsters)
            // {
            //     var hpBar = SimpleUIMgr.Instance.AddHUD<SimpleMonsterHPBar>();
            //     if (hpBar != null) hpBar.Init(monster);
            // }

            // PartyHUD is pre-placed in the scene, just needs Init
            var partyHUD = FindObjectOfType<SimplePartyHUD>();
            if (partyHUD != null) partyHUD.Init(this, roleCtrls);

            var skillDisplay = FindObjectOfType<SimpleSkillDisplay>();
            if (skillDisplay != null) skillDisplay.Init(input);
        }

        private int GetNext(int idx) => (idx + 1) % roleCtrls.Count;
        private int GetPrev(int idx) => (idx - 1 + roleCtrls.Count) % roleCtrls.Count;

        private void HandleSwitchNext(InputAction.CallbackContext ctx)
        {
            if (!switchEnabled || roleCtrls.Count <= 1) return;
            SwitchRole(GetNext(_frontRoleIdx), true);
        }

        private void HandleSwitchPrev(InputAction.CallbackContext ctx)
        {
            if (!switchEnabled || roleCtrls.Count <= 1) return;
            SwitchRole(GetPrev(_frontRoleIdx), false);
        }

        private void SwitchRole(int idx, bool isNext)
        {
            var prev = FrontRoleCtrl;
            FrontRoleIdx = idx;

            var monster = SimpleMonsterCtrl.ClosestDodgeDetectMonster(prev.transform.position, out _);
            if (monster != null)
            {
                prev.ShouldSwitchOutAided();
                FrontRoleCtrl.SwitchInParryAid(monster);

                if (parrySuccessParticlePrefab != null)
                {
                    var pos = FrontRoleCtrl.transform.TransformPoint(new Vector3(0, 1, 1.5f));
                    ParticleMgr.Instance.PlayAt(parrySuccessParticlePrefab, pos);
                }

                if (parrySfxClip != null && AudioMgr.Instance != null)
                    AudioMgr.Instance.PlaySFX(parrySfxClip, FrontRoleCtrl.transform.position);

                if (PostVolumeMgr.Instance != null)
                    PostVolumeMgr.Instance.StartPerfectDodgeProfile();
            }
            else
            {
                prev.ShouldSwitchOutNormal();
                FrontRoleCtrl.SwitchInNormal(prev, isNext);
            }
        }

        private bool TryConvertInputActionToCommand(Guid id, out EActionCommand command)
        {
            if (id == input.Combat.BaseAttack.id) command = EActionCommand.BaseAttack;
            else if (id == input.Combat.Dodge.id) command = EActionCommand.Dodge;
            else if (id == input.Combat.Move.id) command = EActionCommand.Move;
            else if (id == input.Combat.Evade.id) command = EActionCommand.Evade;
            else if (id == input.Combat.SpecialAttack.id) command = EActionCommand.SpecialAttack;
            else if (id == input.Combat.Ultimate.id) command = EActionCommand.Ultimate;
            else if (id == input.Combat.ExSpecialAttack.id) command = EActionCommand.ExSpecialAttack;
            else { command = EActionCommand.None; return false; }
            return true;
        }

        private void HandleStartedInput(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Disabled) return;
            if (TryConvertInputActionToCommand(ctx.action.id, out var command))
                FrontRoleCtrl.ActionCtrl.InputCommand(command, EActionCommandPhase.Down);
        }

        private void HandleCanceledInput(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Disabled) return;
            if (TryConvertInputActionToCommand(ctx.action.id, out var command))
                FrontRoleCtrl.ActionCtrl.InputCommand(command, EActionCommandPhase.Up);
        }

        private void Update()
        {
            if (input.Combat.Esc.WasPressedThisFrame())
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    _showVolume = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    _showVolume = false;
                }
            }

            if (!_enableInput) return;

            if (input.Combat.BaseAttack.phase is InputActionPhase.Started or InputActionPhase.Performed)
                FrontRoleCtrl.ActionCtrl.InputCommand(EActionCommand.BaseAttack, EActionCommandPhase.Press);
            if (input.Combat.Dodge.phase is InputActionPhase.Started or InputActionPhase.Performed)
                FrontRoleCtrl.ActionCtrl.InputCommand(EActionCommand.Dodge, EActionCommandPhase.Press);
            if (input.Combat.Move.phase is InputActionPhase.Started or InputActionPhase.Performed)
                FrontRoleCtrl.ActionCtrl.InputCommand(EActionCommand.Move, EActionCommandPhase.Press);
            if (input.Combat.Evade.phase is InputActionPhase.Started or InputActionPhase.Performed)
                FrontRoleCtrl.ActionCtrl.InputCommand(EActionCommand.Evade, EActionCommandPhase.Press);
            if (input.Combat.SpecialAttack.phase is InputActionPhase.Started or InputActionPhase.Performed)
                FrontRoleCtrl.ActionCtrl.InputCommand(EActionCommand.SpecialAttack, EActionCommandPhase.Press);
            if (input.Combat.Ultimate.phase is InputActionPhase.Started or InputActionPhase.Performed)
                FrontRoleCtrl.ActionCtrl.InputCommand(EActionCommand.Ultimate, EActionCommandPhase.Press);
        }

        // private void OnGUI()
        // {
        //     if (roleCtrls == null) return;
        //
        //     float w = 380f;
        //     float margin = 10f;
        //
        //     for (int i = 0; i < roleCtrls.Count; i++)
        //     {
        //         var rc = roleCtrls[i];
        //         if (rc == null) continue;
        //         float x = i == 0 ? margin : Screen.width - w - margin;
        //         rc.DrawDebugUI(x, margin, i, w);
        //     }
        // }

        private void OnGUI()
        {
            if (!_showVolume) return;

            var audioMgr = AudioMgr.Instance;
            if (audioMgr == null) return;

            float w = 260f, h = 130f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), "Volume");

            float labelW = 60f, sliderW = 160f;
            float row = y + 25f;
            float rowH = 30f;

            GUI.Label(new Rect(x + 10, row, labelW, 20), "Master");
            float newMaster = GUI.HorizontalSlider(new Rect(x + 70, row + 5, sliderW, 20), _masterVol, 0f, 1f);
            if (newMaster != _masterVol) { _masterVol = newMaster; audioMgr.MainVolume = _masterVol; }

            row += rowH;
            GUI.Label(new Rect(x + 10, row, labelW, 20), "Music");
            float newMusic = GUI.HorizontalSlider(new Rect(x + 70, row + 5, sliderW, 20), _musicVol, 0f, 1f);
            if (newMusic != _musicVol) { _musicVol = newMusic; audioMgr.MusicVolume = _musicVol; }

            row += rowH;
            GUI.Label(new Rect(x + 10, row, labelW, 20), "SFX");
            float newSfx = GUI.HorizontalSlider(new Rect(x + 70, row + 5, sliderW, 20), _sfxVol, 0f, 1f);
            if (newSfx != _sfxVol) { _sfxVol = newSfx; audioMgr.SFXVolume = _sfxVol; }
        }
    }
}
