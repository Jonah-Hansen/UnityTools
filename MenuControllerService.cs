using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuControllerService : ScriptableObject
{
    [SerializeField] PlayerInputService _inputService;
    PlayerInputService InputService => _inputService;

    [Header("Element 0 should be the initial page for this menu")]
    [SerializeField] GameObject[] _pagePrefabs;
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

    public abstract void SubscribeEvents();
    public abstract void UnsubscribeEvents();

    public T GetPage<T>() where T : UIElement {
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
        InputService.UICancelEvent += Back;
        Stack.Push(Pages[0]);
        Pages[0].Show();
        MenuOpenedEvent?.Invoke();
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
    public void Close() {
        while(Stack.Count > 0) Back(true);
        InputService.UICancelEvent -= Back;
        MenuClosedEvent?.Invoke();
        UnsubscribeEvents();
    }

    public void Back() {
        Back(false);
    }
    public void Back(bool force) {
        Stack.Pop().Hide(force);
        if(!Stack.TryPeek(out UIElement prev)) {
            Close();
            return;
        }
        EventSystem.current.SetSelectedGameObject(prev.SelectedObject);
        if(!prev.gameObject.activeInHierarchy) prev.Show();
    }
}
