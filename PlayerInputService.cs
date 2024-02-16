using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[CreateAssetMenu(fileName = "PlayerInputService", menuName = "ScriptableObjects/Services/PlayerInput")]
public class PlayerInputService : ScriptableObject
{
    InputActionMap _currentActionMap;
    public InputActionMap CurrentActionMap {
        get => _currentActionMap;
        set {
            if(value == _currentActionMap) return;
            _currentActionMap?.Disable();
            _currentActionMap = value;
            _currentActionMap.Enable();
        }
    }
    InputDevice _device;
    public InputDevice CurrentDevice { get => _device; private set {
        if(value == _device) return;
        _device = value;
        CurrentControlScheme = GetControlScheme(value);
        DeviceChangedEvent?.Invoke(value);
    }}
    InputControlScheme _controlScheme;
    public InputControlScheme CurrentControlScheme { get => _controlScheme; 
        private set {
            if(value == _controlScheme) return;
            _controlScheme = value;
            ControlSchemeChangedEvent?.Invoke(value);
        }
    }
    public PlayerControls Controls { get; private set; }
    Callbacks ActionCallbacks { get; set; }

    #region subscribable events
    public event Action<InputDevice> DeviceChangedEvent;
    public event Action<InputControlScheme> ControlSchemeChangedEvent;
    /// <summary> same as InputSystem.onDeviceChange </summary>
    public event Action<InputDevice, InputDeviceChange> DeviceConfigChangedEvent;
    public event Action UICancelEvent;

    #endregion

    void OnEnable() {
        // initialize properties
        Controls = new();
        ActionCallbacks = new(this);

        // control scheme and device configuration changes
        InputSystem.onDeviceChange += OnDeviceConfigurationChange;
        InputSystem.onEvent += OnDeviceChange;
        
        // initialize action maps
        Controls.PlayerCharacter.SetCallbacks(ActionCallbacks);
        Controls.PlayerCharacter.Disable();
        Controls.UI.SetCallbacks(ActionCallbacks);
        Controls.UI.Disable();

        // default action map
        CurrentActionMap = Controls.UI;
    }

    void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceConfigurationChange;
        InputSystem.onEvent -= OnDeviceChange;
    }

    /// <summary> returns the control scheme associated with the given device </summary>
    InputControlScheme GetControlScheme(InputDevice device) {
        InputControlScheme defaultScheme = Controls.GamepadScheme;
        if(device is Gamepad) return Controls.GamepadScheme;
        // if(device is Keyboard) return Controls.KeyboardScheme;
        else return defaultScheme;
    }
    void OnDeviceConfigurationChange(InputDevice device, InputDeviceChange change) => DeviceConfigChangedEvent?.Invoke(device, change);

    /// <summary> sets CurrentDevice and CurrentControlScheme when active device changes </summary>
    void OnDeviceChange(InputEventPtr eventPtr, InputDevice device)  {
        if (device == CurrentDevice) return;
        // ignore irelevent events
        if (eventPtr.type != StateEvent.Type) return;
        // ignore noise
        if (!eventPtr.EnumerateChangedControls(device, 0.01F).Any()) return;
        // set new device
        CurrentDevice = device;
    }

    #region input system action callbacks
    /// <summary>
    /// Contains the callbacks for all the input actions.<para />
    /// 
    /// when adding a new action map, make sure callbacks extends its actions interface, then add a new subscribable events class for it and implement the events as needed. <br />
    /// 
    /// be sure to also initialise the new action map in OnEnable()
    /// </summary>
    class Callbacks : PlayerControls.IPlayerCharacterActions, PlayerControls.IUIActions {
        public Callbacks(PlayerInputService service) {
            InputService = service;
        }
        PlayerInputService InputService { get; }
        
        // PlayerCharacter
        public void OnContextAction(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }

        public void OnMove(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }

        public void OnSwitchSkill(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }

        public void OnUseSkill(InputAction.CallbackContext context) {
            throw new NotImplementedException();
        }

        // UI
        public void OnCancel(InputAction.CallbackContext context) {
            if(context.performed) InputService.UICancelEvent?.Invoke();
        }

        public void OnClick(InputAction.CallbackContext context) {}

        public void OnMiddleClick(InputAction.CallbackContext context) {}

        public void OnNavigate(InputAction.CallbackContext context) {}

        public void OnPoint(InputAction.CallbackContext context) {}

        public void OnRightClick(InputAction.CallbackContext context) {}

        public void OnScrollWheel(InputAction.CallbackContext context) {}

        public void OnSubmit(InputAction.CallbackContext context) {}

    }
    #endregion
}
