using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public abstract class UIElement : MonoBehaviour
{
    RectTransform _rect;
    public RectTransform RectTransform => _rect;
    CanvasGroup _canvasGroup;
    public CanvasGroup CanvasGroup => _canvasGroup;
    [field: SerializeField] public GameObject SelectedObject { get; set; }

    public event Action CloseRequestEvent;
    public event Action CloseEvent;

    public virtual void Initialize() {
        WaitOneFrame(() => gameObject.SetActive(false));
    }
    
    public virtual void Awake() {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void OnCloseButton() => CloseRequestEvent?.Invoke();

    public virtual void Show() {
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(SelectedObject);
    } 

    public virtual void Hide(bool force = false) {
        CloseEvent?.Invoke();
        gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary> in case if overriding Hide, need to manually Raise CloseEvent </summary>
    public void RaiseCloseEvent() => CloseEvent?.Invoke();

    public void WaitOneFrame(Action callback) {
        StartCoroutine(Coroutine(callback));
        static IEnumerator Coroutine(Action callback) {
            yield return null;
            callback();
        }
    }
}
