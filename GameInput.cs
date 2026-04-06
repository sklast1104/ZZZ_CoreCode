// Auto-generated C# class for GameInput.inputactions
// This mirrors the structure Unity's Input System code generator would produce.

using System;
using UnityEngine.InputSystem;

public partial class @GameInput : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }

    public @GameInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""GameInput"",
    ""maps"": [
        {
            ""name"": ""Combat"",
            ""id"": ""a1b2c3d4-0001-0001-0001-000000000001"",
            ""actions"": [
                { ""name"": ""Move"", ""type"": ""Value"", ""id"": ""a1b2c3d4-0002-0001-0001-000000000001"", ""expectedControlType"": ""Vector2"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": true },
                { ""name"": ""BaseAttack"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-0002-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Dodge"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-0003-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Evade"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-0004-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""SpecialAttack"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-000e-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Ultimate"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-0005-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""SwitchCharactersNext"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-0006-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""ExSpecialAttack"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-000f-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""SwitchCharactersPrevious"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-0007-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Interact"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-0008-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Esc"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-0009-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""CharacterPanel"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-000a-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""OpenQuestPanel"", ""type"": ""Button"", ""id"": ""a1b2c3d4-0002-000b-0001-000000000001"", ""expectedControlType"": ""Button"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": false },
                { ""name"": ""Look"", ""type"": ""Value"", ""id"": ""a1b2c3d4-0002-000c-0001-000000000001"", ""expectedControlType"": ""Vector2"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": true },
                { ""name"": ""Scroll"", ""type"": ""Value"", ""id"": ""a1b2c3d4-0002-000d-0001-000000000001"", ""expectedControlType"": ""Axis"", ""processors"": """", ""interactions"": """", ""initialStateCheck"": true }
            ],
            ""bindings"": [
                { ""name"": ""WASD"", ""id"": ""b1000001-0001-0001-0001-000000000001"", ""path"": ""2DVector"", ""interactions"": """", ""processors"": """", ""groups"": """", ""action"": ""Move"", ""isComposite"": true, ""isPartOfComposite"": false },
                { ""name"": ""up"", ""id"": ""b1000001-0001-0001-0002-000000000001"", ""path"": ""<Keyboard>/w"", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""down"", ""id"": ""b1000001-0001-0001-0003-000000000001"", ""path"": ""<Keyboard>/s"", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""left"", ""id"": ""b1000001-0001-0001-0004-000000000001"", ""path"": ""<Keyboard>/a"", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""right"", ""id"": ""b1000001-0001-0001-0005-000000000001"", ""path"": ""<Keyboard>/d"", ""action"": ""Move"", ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": """", ""id"": ""b1000002-0001-0001-0001-000000000001"", ""path"": ""<Mouse>/leftButton"", ""action"": ""BaseAttack"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b1000003-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/leftShift"", ""action"": ""Dodge"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b1000004-0001-0001-0001-000000000001"", ""path"": ""<Mouse>/rightButton"", ""action"": ""Evade"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b1000005-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/q"", ""action"": ""Ultimate"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b1000006-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/e"", ""action"": ""SpecialAttack"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b100000f-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/r"", ""action"": ""ExSpecialAttack"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b1000007-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/t"", ""action"": ""SwitchCharactersPrevious"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b1000008-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/f"", ""action"": ""Interact"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b1000009-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/escape"", ""action"": ""Esc"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b100000a-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/c"", ""action"": ""CharacterPanel"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b100000b-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/j"", ""action"": ""OpenQuestPanel"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b100000c-0001-0001-0001-000000000001"", ""path"": ""<Mouse>/delta"", ""action"": ""Look"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b100000d-0001-0001-0001-000000000001"", ""path"": ""<Mouse>/scroll/y"", ""action"": ""Scroll"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """", ""id"": ""b100000e-0001-0001-0001-000000000001"", ""path"": ""<Keyboard>/space"", ""action"": ""SwitchCharactersNext"", ""isComposite"": false, ""isPartOfComposite"": false }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        m_Combat = asset.FindActionMap("Combat", throwIfNotFound: true);
        m_Combat_Move = m_Combat.FindAction("Move", throwIfNotFound: true);
        m_Combat_BaseAttack = m_Combat.FindAction("BaseAttack", throwIfNotFound: true);
        m_Combat_Dodge = m_Combat.FindAction("Dodge", throwIfNotFound: true);
        m_Combat_Evade = m_Combat.FindAction("Evade", throwIfNotFound: true);
        m_Combat_SpecialAttack = m_Combat.FindAction("SpecialAttack", throwIfNotFound: true);
        m_Combat_Ultimate = m_Combat.FindAction("Ultimate", throwIfNotFound: true);
        m_Combat_SwitchCharactersNext = m_Combat.FindAction("SwitchCharactersNext", throwIfNotFound: true);
        m_Combat_ExSpecialAttack = m_Combat.FindAction("ExSpecialAttack", throwIfNotFound: true);
        m_Combat_SwitchCharactersPrevious = m_Combat.FindAction("SwitchCharactersPrevious", throwIfNotFound: true);
        m_Combat_Interact = m_Combat.FindAction("Interact", throwIfNotFound: true);
        m_Combat_Esc = m_Combat.FindAction("Esc", throwIfNotFound: true);
        m_Combat_CharacterPanel = m_Combat.FindAction("CharacterPanel", throwIfNotFound: true);
        m_Combat_OpenQuestPanel = m_Combat.FindAction("OpenQuestPanel", throwIfNotFound: true);
        m_Combat_Look = m_Combat.FindAction("Look", throwIfNotFound: true);
        m_Combat_Scroll = m_Combat.FindAction("Scroll", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public System.Collections.Generic.IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable() { asset.Enable(); }
    public void Disable() { asset.Disable(); }

    public bool Contains(InputAction action) { return asset.Contains(action); }

    public System.Collections.Generic.IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Combat action map
    private InputActionMap m_Combat;
    private InputAction m_Combat_Move;
    private InputAction m_Combat_BaseAttack;
    private InputAction m_Combat_Dodge;
    private InputAction m_Combat_Evade;
    private InputAction m_Combat_SpecialAttack;
    private InputAction m_Combat_Ultimate;
    private InputAction m_Combat_SwitchCharactersNext;
    private InputAction m_Combat_ExSpecialAttack;
    private InputAction m_Combat_SwitchCharactersPrevious;
    private InputAction m_Combat_Interact;
    private InputAction m_Combat_Esc;
    private InputAction m_Combat_CharacterPanel;
    private InputAction m_Combat_OpenQuestPanel;
    private InputAction m_Combat_Look;
    private InputAction m_Combat_Scroll;

    public struct CombatActions
    {
        private @GameInput m_Wrapper;
        public CombatActions(@GameInput wrapper) { m_Wrapper = wrapper; }

        public InputAction @Move => m_Wrapper.m_Combat_Move;
        public InputAction @BaseAttack => m_Wrapper.m_Combat_BaseAttack;
        public InputAction @Dodge => m_Wrapper.m_Combat_Dodge;
        public InputAction @Evade => m_Wrapper.m_Combat_Evade;
        public InputAction @SpecialAttack => m_Wrapper.m_Combat_SpecialAttack;
        public InputAction @Ultimate => m_Wrapper.m_Combat_Ultimate;
        public InputAction @SwitchCharactersNext => m_Wrapper.m_Combat_SwitchCharactersNext;
        public InputAction @ExSpecialAttack => m_Wrapper.m_Combat_ExSpecialAttack;
        public InputAction @SwitchCharactersPrevious => m_Wrapper.m_Combat_SwitchCharactersPrevious;
        public InputAction @Interact => m_Wrapper.m_Combat_Interact;
        public InputAction @Esc => m_Wrapper.m_Combat_Esc;
        public InputAction @CharacterPanel => m_Wrapper.m_Combat_CharacterPanel;
        public InputAction @OpenQuestPanel => m_Wrapper.m_Combat_OpenQuestPanel;
        public InputAction @Look => m_Wrapper.m_Combat_Look;
        public InputAction @Scroll => m_Wrapper.m_Combat_Scroll;

        public InputActionMap Get() { return m_Wrapper.m_Combat; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;

        public static implicit operator InputActionMap(CombatActions set) { return set.Get(); }
    }

    private CombatActions m_CombatActionsCallbackInterface;
    public CombatActions @Combat => new CombatActions(this);
}
