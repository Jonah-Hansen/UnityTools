using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// Input service base class. <para/>
/// <c>T</c> is the class from your InputActionsAsset generated c# file<br/>
/// it is the type used for the <c>Controls</c> property.
/// </summary>
/// <typeparam name="T"> the class from your InputActionsAsset generated c# file</typeparam>
public abstract class InputService<T>: ScriptableObject
where T : IInputActionCollection2, IDisposable, new() 
{
    public T Controls { get; private set; }

    InputActionMap _currentActionMap;
    public InputActionMap CurrentActionMap {
        get => _currentActionMap;
        set {
            if(value == _currentActionMap) {
                return;
            }
            _currentActionMap?.Disable();
            _currentActionMap = value;
            _currentActionMap.Enable();
        }
    }

    private InputDevice _device;
    public InputDevice CurrentDevice { get => _device; private set {
        if(value == _device) {
            return;
        }
        _device = value;
        CurrentControlScheme = GetControlScheme(value);
        DeviceChangedEvent?.Invoke(value);
    }}
    
    private InputControlScheme _controlScheme;
    public InputControlScheme CurrentControlScheme { get => _controlScheme; 
        private set {
            if(value == _controlScheme) {
                return;
            }
            _controlScheme = value;
            ControlSchemeChangedEvent?.Invoke(value);
        }
    }

    #region subscribable events
    public event Action<InputDevice> DeviceChangedEvent;
    public event Action<InputControlScheme> ControlSchemeChangedEvent;
    /// <summary> same as InputSystem.onDeviceChange </summary>
    public event Action<InputDevice, InputDeviceChange> DeviceConfigChangedEvent;
    #endregion

    /// <summary>
    /// 1. create an instance of your Action Callbacks class which extends the InputActionAsset's I<actionmap>Actions interfaces<para/>
    /// <c> Callbacks actionCallbacks = new(this);</c> <para/>
    /// 2. for each action map in your InputActionsAsset do <para/>
    /// <example><c>
    /// Controls.<actionmap>.SetCallbacks(ActionCallbacks);<br/>
    /// Controls.<actionmap>.Disable();<para/>
    /// </c></example>
    /// 3. you can also set the default action map by assigning <para/>
    /// <c>CurrentActionMap = Controls.<defaultmap>;</c>
    /// </summary>
    protected abstract void Initialize();
    // {
    //     Callbacks actionCallbacks = new(this);
    //     Controls.PlayerCharacter.SetCallbacks(ActionCallbacks);
    //     Controls.PlayerCharacter.Disable();
    //     Controls.UI.SetCallbacks(ActionCallbacks);
    //     Controls.UI.Disable();  
    //     CurrentActionMap = Controls.UI;
    // }

    //? example Callbacks class 
    // class Callbacks : PlayerControls.IGameplayActions, PlayerControls.IUIActions {
    //     PlayerControlsInputService InputService { get; set; }
    //     public Callbacks(PlayerControlsInputService service) => InputService = service;
    //     public void OnCancel(InputAction.CallbackContext context) {
    //         if(context.performed) InputService.UICancelEvent?.Invoke();
    //     }
    // }

    /// <summary> 
    /// returns the control scheme associated with the given device <para/>
    /// <example><code>
    /// InputControlScheme defaultScheme = Controls.GamepadScheme; <br/>
    /// if(device is Gamepad) return Controls.GamepadScheme; <br/>
    /// if(device is Keyboard) return Controls.KeyboardScheme; <br/>
    /// else return defaultScheme; <br/>
    /// </code></example>
    /// </summary>
    protected abstract InputControlScheme GetControlScheme(InputDevice device); 
    // {
    //   InputControlScheme defaultScheme = Controls.GamepadScheme;
    //   if(device is Gamepad) return Controls.GamepadScheme;
    //   if(device is Keyboard) return Controls.KeyboardScheme;
    //   else return defaultScheme;
    // }

    private void OnEnable() {
        // initialize properties
        Controls = new();
        // control scheme and device configuration changes
        InputSystem.onDeviceChange += OnDeviceConfigurationChange;
        InputSystem.onEvent += OnDeviceChange;
        // initialize action maps
        Initialize();
    }

    private void OnDisable() {
        InputSystem.onDeviceChange -= OnDeviceConfigurationChange;
        InputSystem.onEvent -= OnDeviceChange;
    }

    private void OnDeviceConfigurationChange(InputDevice device, InputDeviceChange change) {
        DeviceConfigChangedEvent?.Invoke(device, change);
    }

    /// <summary> sets CurrentDevice and CurrentControlScheme when active device changes </summary>
    private void OnDeviceChange(InputEventPtr eventPtr, InputDevice device)  {
        if (device == CurrentDevice) {
            return;
        }
        // ignore irelevent events
        if (eventPtr.type != StateEvent.Type) {
            return;
        }
        // ignore noise
        if (!eventPtr.EnumerateChangedControls(device, 0.01F).Any()) {
            return;
        }
        // set new device
        CurrentDevice = device;
    }
}
