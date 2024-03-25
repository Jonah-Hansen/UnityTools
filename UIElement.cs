using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public abstract class UIElement : MonoBehaviour
{
    [field: SerializeField] public GameObject SelectedObject { get; set; }
    
    private RectTransform _rect;
    public RectTransform RectTransform => _rect;

    private CanvasGroup _canvasGroup;
    protected CanvasGroup CanvasGroup => _canvasGroup;

    public event Action CloseRequestEvent;
    public event Action CloseEvent;

    public abstract void SetButtonIcons(UIButtonIconSet icons);

    public virtual void Initialize() {
        gameObject.SetActive(true);
        // gives awake a chance to run Awake() before the page is hidden, and for layouts to arrange themselves.
        WaitOneFrame(() => gameObject.SetActive(false));
    }
    public virtual void OnCloseButton() {
        CloseRequestEvent?.Invoke();
        AudioManager audio = AudioManager.Instance;
        if(audio) {
            audio.Play("sfx-ui-buttonconf");
        }
    }

    public virtual void Show() {
        gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(SelectedObject);
    } 

    public virtual void Hide(bool force = false) {
        CloseEvent?.Invoke();
        gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

      protected virtual void Awake() {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary> in case of overriding Hide, need to manually Raise CloseEvent </summary>
    protected void RaiseCloseEvent() {
        CloseEvent?.Invoke();
    }

    protected void WaitOneFrame(Action callback) {
        StartCoroutine(Coroutine(callback));
        static IEnumerator Coroutine(Action callback) {
            yield return null;
            callback();
        }
    }

    [Serializable]
    public class UIButtonIconSet {
        [field:SerializeField] public string Confirm { get; set; }
        [field:SerializeField] public string Cancel { get; set; }
    }
}
