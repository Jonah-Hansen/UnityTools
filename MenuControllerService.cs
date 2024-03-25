using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
    
public abstract class MenuController : MonoBehaviour
{
    [SerializeField] private PlayerControlsInputService _inputService;
    private PlayerControlsInputService InputService => _inputService;

    [SerializeField] private ControlSchemeButtonIcons[] _controlSchemeIcons;
    private ControlSchemeButtonIcons[] ControlSchemeIcons => _controlSchemeIcons;

    [SerializeField] private UIElement[] _pages;
    private UIElement[] Pages => _pages;
    
    private Stack<UIElement> Stack { get; } = new();

    public event Action MenuClosedEvent;
    public event Action MenuOpenedEvent;

    protected virtual void Start() {
        Stack.Clear();
        foreach(UIElement page in Pages) {
            page.Initialize();
        }
    }

    protected abstract void SubscribeEvents();
    protected abstract void UnsubscribeEvents();

    /// <summary> opens the first page in the menu </summary>
    public void Open() {
        if(Stack.TryPeek(out UIElement top)) {
            return;
        }
        Stack.Push(Pages[0]);
        Pages[0].Show();
        OpenMenu();
    }

    protected T GetPage<T>() where T : UIElement {
        for(int i = 0; i < Pages.Length; i++) {
            if(Pages[i] is T page) {
                return page;
            }
        } 
        Debug.LogError($"no page of type {typeof(T)} found");
        return null;
    }
    
    protected bool TryGetPage<T>(out T page) where T : UIElement {
        page = null;
        for(int i = 0; i < Pages.Length; i++) {
            if(Pages[i] is T found) {
                page = found;
                return true;
            }
        } 
        return false;
    }

    protected void Open(UIElement page, bool hidePrev = false) {
        if(page.gameObject.activeInHierarchy) {
            return;
        }
        if(Stack.TryPeek(out UIElement top)) {
            if(hidePrev) {
                top.Hide();
            }
        } else {
            OpenMenu();
        }
        Stack.Push(page);
        page.Show();
    }
    
    /// <summary> Closes the entire Menu </summary>
    protected void Close() {
        while(Stack.Count > 0) Back(true);
        MenuClosedEvent?.Invoke();
    }

    protected void Back() {
        Back(false);
    }

    private void OpenMenu() {
        InputService.CurrentActionMap = InputService.Controls.UI;
        MenuOpenedEvent?.Invoke();
        OnControlSchemeChange(InputService.CurrentControlScheme);
    }

    private void OnEnable() {
        SubscribeEvents();
        InputService.UICancelEvent += Back;
        InputService.ControlSchemeChangedEvent += OnControlSchemeChange;
    }

    private void OnDisable() {
    UnsubscribeEvents();
        InputService.UICancelEvent -= Back;
        InputService.ControlSchemeChangedEvent -= OnControlSchemeChange;
    }

    private void Back(bool force) {
        Stack.Pop().Hide(force);
        if(!Stack.TryPeek(out UIElement prev)) {
            Close();
            return;
        }
        EventSystem.current.SetSelectedGameObject(prev.SelectedObject);
        if(!prev.gameObject.activeInHierarchy) {
            prev.Show();
        }
    }

    private void OnControlSchemeChange(InputControlScheme scheme) {
        ControlSchemeButtonIcons icons = Array.Find(ControlSchemeIcons, (icons) => icons.Scheme == scheme.name);
        if(icons == null) {
            Debug.LogWarning($"no icons found for scheme {scheme.name}");
            return;
        }
        foreach (UIElement page in Pages) {
            page.SetButtonIcons(icons.Icons);
        }
    }

    [Serializable]
    private class ControlSchemeButtonIcons {
        [Header("must match a Scheme name in the Input Actions Asset")]
        [SerializeField] string _scheme;
        public string Scheme => _scheme;
        [field:SerializeField] public UIElement.UIButtonIconSet Icons { get; set; }
    }
}
