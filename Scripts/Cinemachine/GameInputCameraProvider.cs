using Cinemachine;
using UnityEngine;

namespace JM
{
    public class GameInputCameraProvider : MonoBehaviour, AxisState.IInputAxisProvider
    {
        private enum State { Active, Suppressed, WaitBlend, Restoring }

        private GameInput _gameInput;
        private CinemachineBrain _brain;

        private State _state = State.Active;
        private float _inputMultiplier = 1f;
        private float _restoreTimer;
        private float _restoreDuration;
        private int _skipFrames;

        public void SetGameInput(GameInput input) => _gameInput = input;

        private void Awake()
        {
            var mainCam = Camera.main;
            if (mainCam != null)
                _brain = mainCam.GetComponent<CinemachineBrain>();
        }

        public void SuppressInput()
        {
            _state = State.Suppressed;
            _inputMultiplier = 0f;
        }

        public void BeginRestore(float duration = 0.3f)
        {
            _restoreDuration = duration;
            _state = State.WaitBlend;
            // skip 2 frames to let brain start blending
            _skipFrames = 2;
        }

        private void Update()
        {
            switch (_state)
            {
                case State.WaitBlend:
                    if (_skipFrames > 0) { _skipFrames--; break; }
                    if (_brain == null || !_brain.IsBlending)
                    {
                        _state = State.Restoring;
                        _restoreTimer = 0f;
                    }
                    break;

                case State.Restoring:
                    _restoreTimer += Time.deltaTime;
                    _inputMultiplier = Mathf.Clamp01(_restoreTimer / _restoreDuration);
                    if (_inputMultiplier >= 1f)
                    {
                        _inputMultiplier = 1f;
                        _state = State.Active;
                    }
                    break;
            }
        }

        public float GetAxisValue(int axis)
        {
            if (_gameInput == null) return 0f;
            float raw = axis switch
            {
                0 => _gameInput.Combat.Look.ReadValue<Vector2>().x,
                1 => _gameInput.Combat.Look.ReadValue<Vector2>().y,
                2 => _gameInput.Combat.Scroll.ReadValue<float>(),
                _ => 0f
            };
            return raw * _inputMultiplier;
        }
    }
}
