using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class MenuControllerService : ScriptableObject
{
    [SerializeField] PlayerControlsInputService _inputService;
    PlayerControlsInputService InputService => _inputService;

    [Header("Element 0 should be the initial page for this menu")]
    [SerializeField] GameObject[] _pagePrefabs;
    [SerializeField] ControlSchemeButtonIcons[] _controlSchemeIcons;
    ControlSchemeButtonIcons[] ControlSchemeIcons => _controlSchemeIcons;
    UIElement[] Pages { get; set; } 
    Stack<UIElement> Stack { get; } = new();

    public event Action MenuClosedEvent;
    public event Action MenuOpenedEvent;

    public virtual void Initialize(Canvas rootCanvas) {
        Stack.Clear();
        List<UIElement> pagesList = new();
        foreach(GameObject pagePrefab in _pagePrefabs) {
            GameObject pageObject = Instantiate(pagePrefab, rootCanvas.transform);
            if(pageObject.TryGetComponent(out UIElement page)) {
                pagesList.Add(page);
                page.Initialize();
            } else {
                Destroy(pageObject);
                Debug.LogError($"Menu Controller {this} tried to initialize {pageObject} which is not a UIElement.");
            }
        }
        Pages = pagesList.ToArray();
    }

    protected abstract void SubscribeEvents();
    protected abstract void UnsubscribeEvents();

    void SubscribeInputEvents() {
        InputService.UICancelEvent += Back;
        InputService.ControlSchemeChangedEvent += OnControlSchemeChange;
    }
    void UnsubscribeInputEvents() {
        InputService.UICancelEvent -= Back;
        InputService.ControlSchemeChangedEvent -= OnControlSchemeChange;
    }

    protected T GetPage<T>() where T : UIElement {
        for(int i = 0; i < Pages.Length; i++) {
            if(Pages[i] is T page) return page;
        } 
        Debug.LogError($"no page of type {typeof(T)} found");
        return null;
    }

    /// <summary> opens the first page in the menu </summary>
    public void Open() {
        if(Stack.TryPeek(out UIElement top)) return;
        InputService.CurrentActionMap = InputService.Controls.UI;
        Stack.Push(Pages[0]);
        Pages[0].Show();
        MenuOpenedEvent?.Invoke();
        SubscribeInputEvents();
        OnControlSchemeChange(InputService.CurrentControlScheme);
        SubscribeEvents();
    }

    public void Open(UIElement page, bool hidePrev = false) {
        if(page.gameObject.activeInHierarchy) return;
        if(hidePrev) {
            Stack.Peek().Hide();
        }
        Stack.Push(page);
        page.Show();
    }
    
    /// <summary> Closes the entire Menu </summary>
    protected void Close() {
        while(Stack.Count > 0) Back(true);
        MenuClosedEvent?.Invoke();
        UnsubscribeInputEvents();
        UnsubscribeEvents();
    }

    protected void Back() {
        Back(false);
    }
    void Back(bool force) {
        Stack.Pop().Hide(force);
        if(!Stack.TryPeek(out UIElement prev)) {
            Close();
            return;
        }
        EventSystem.current.SetSelectedGameObject(prev.SelectedObject);
        if(!prev.gameObject.activeInHierarchy) prev.Show();
    }

    void OnControlSchemeChange(InputControlScheme scheme) {
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
    class ControlSchemeButtonIcons {
        [Header("must match a Scheme name in the Input Actions Asset")]
        [SerializeField] string _scheme;
        public string Scheme => _scheme;
        [field:SerializeField] public UIElement.UIButtonIconSet Icons { get; set; }
    }
}
