using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public abstract class UIElement : MonoBehaviour
{
    [field: SerializeField] public GameObject SelectedObject { get; set; }
    RectTransform _rect;
    public RectTransform RectTransform => _rect;
    CanvasGroup _canvasGroup;
    protected CanvasGroup CanvasGroup => _canvasGroup;
    public event Action CloseRequestEvent;
    public event Action CloseEvent;

    public virtual void Initialize() {
        WaitOneFrame(() => gameObject.SetActive(false));
    }
    
    protected virtual void Awake() {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    protected virtual void OnCloseButton() => CloseRequestEvent?.Invoke();

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
    protected void RaiseCloseEvent() => CloseEvent?.Invoke();

    protected void WaitOneFrame(Action callback) {
        StartCoroutine(Coroutine(callback));
        static IEnumerator Coroutine(Action callback) {
            yield return null;
            callback();
        }
    }

    public abstract void SetButtonIcons(UIButtonIconSet icons);

    [Serializable]
    public class UIButtonIconSet {
        [field:SerializeField] public string Confirm { get; set; }
        [field:SerializeField] public string Cancel { get; set; }
    }
}
