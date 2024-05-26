using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// the concept for this system of using the unity input system with events and without needing a PlayerInput component is inspired by this video https://www.youtube.com/watch?v=ZHOWqF-b51k&t=1098s

/// <summary>
/// Input service base class. <para/>
/// <c>T</c> is the class from your InputActionsAsset generated c# file<br/>
/// it is the type used for the <c>Controls</c> property.
/// </summary>
/// <typeparam name="T"> the class from your InputActionsAsset generated c# file</typeparam>
public abstract class InputService<T>: ScriptableObject
where T : IInputActionCollection2, IDisposable, new()
{
    [field:SerializeField] public UIButtonIconSet[] UIButtonIcons { get; set; }
    public T Controls { get; private set; }

    #nullable enable
    private InputActionMap? _currentActionMap;
    public InputActionMap? CurrentActionMap {
        get => _currentActionMap;
        set {
            if(value == _currentActionMap) {
                return;
            }
            _currentActionMap?.Disable();
            _currentActionMap = value;
            _currentActionMap?.Enable();
            Debug.Log($"changed action map to {value?.name}");
            ActionMapChangedEvent?.Invoke(value);
        }
    }
    #nullable disable

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
    public event Action<InputActionMap> ActionMapChangedEvent;
    /// <summary> same as InputSystem.onDeviceChange </summary>
    public event Action<InputDevice, InputDeviceChange> DeviceConfigChangedEvent;
    #endregion

    public void DisableControls()
    {
        if(CurrentActionMap != null && CurrentActionMap.enabled)
        {
            CurrentActionMap.Disable();
        }
    }

    public void EnableControls()
    {
        try
        {
            if(!CurrentActionMap.enabled)
            {
                CurrentActionMap.Enable();
            }
        }
        catch(NullReferenceException error)
        {
            Debug.LogError("Current Action Map is not set \n" + error);
        }
    }

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
    // public event Action UICancelEvent;
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
    //   return defaultScheme;
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

    [Serializable]
    public class UIButtonIconSet {
        /// <summary>
        /// must match a scheme name in Controls.controlSchemes
        /// </summary>
        // [SerializeField] private string _scheme;
        // public string Scheme => _scheme;
        // [SerializeField] private IconSet _confirm;
        // public IconSet Confirm => _confirm;
        // [SerializeField] private IconSet _cancel;
        // public IconSet Cancel => _cancel;

        // [Serializable] public struct IconSet
        // {
        //     [SerializeField] public Sprite primary;
        //     [SerializeField] public Sprite secondary;
        //     [SerializeField] public Sprite alternate;
        // }
    }
}
