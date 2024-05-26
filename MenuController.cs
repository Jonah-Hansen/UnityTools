using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public abstract class MenuController : MonoBehaviour
{
    [SerializeField] private PlayerControlsInputService _inputService;
    private PlayerControlsInputService InputService => _inputService;

    [SerializeField] private UIElement[] _pages;
    private UIElement[] Pages => _pages;

    private Stack<UIElement> Stack { get; } = new();

    private Image Background { get; set; }

    public event Action MenuClosedEvent;
    public event Action MenuOpenedEvent;

    /// <summary> opens the first page in the menu </summary>
    public virtual void Open() {
        if(Stack.TryPeek(out UIElement top)) {
            return;
        }
        Stack.Push(Pages[0]);
        Pages[0].Show();
        OpenMenu();
    }

    protected abstract void SubscribeEvents();
    protected abstract void UnsubscribeEvents();

    protected virtual void Start() {
        Stack.Clear();
        Background.enabled = false;
        foreach(UIElement page in Pages) {
            page.Initialize();
        }
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
        Debug.LogError($"no page of type {typeof(T)} found");
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
        Background.enabled = false;
        InputService.CurrentActionMap = InputService.Controls.PlayerCharacter;
        InputService.UICancelEvent -= Back;
    }

    protected void Back() {
        Back(false);
    }

    private void OpenMenu() {
        Background.enabled = true;
        InputService.CurrentActionMap = InputService.Controls.UI;
        MenuOpenedEvent?.Invoke();
        OnControlSchemeChange(InputService.CurrentControlScheme);
        InputService.UICancelEvent += Back;
    }

    private void Awake() {
        Background = GetComponent<Image>();
    }

    private void Reset() {
        // default background is black at 50% opacity
        GetComponent<Image>().color = new Color(0,0,0,0.5f);
    }

    private void OnEnable() {
        SubscribeEvents();
        InputService.ControlSchemeChangedEvent += OnControlSchemeChange;
    }

    private void OnDisable() {
        UnsubscribeEvents();
        InputService.ControlSchemeChangedEvent -= OnControlSchemeChange;
        InputService.UICancelEvent -= Back;
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
        InputService.UIButtonIconSet icons = Array.Find(InputService.UIButtonIcons, (icons) => icons.Scheme == scheme.name);
        if(icons == null) {
            Debug.LogWarning($"no icons found for scheme {scheme.name}");
            return;
        }
        foreach (UIElement page in Pages) {
            page.SetButtonIcons(icons);
        }
    }
}
