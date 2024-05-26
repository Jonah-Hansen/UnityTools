using System;
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

    /// <summary>
    /// can be called whenever the control scheme changes, to change button icons accordingly
    /// </summary>
    public abstract void SetButtonIcons(InputService.UIButtonIconSet icons);

    public virtual void Initialize() {
        CanvasGroup.alpha = 0;
        CanvasGroup.interactable = false;
        gameObject.SetActive(true);
        // gives awake a chance to run Awake() before the page is hidden, and for layouts to arrange themselves.
        Utilities.WaitOneFrame(() => {
            gameObject.SetActive(false);
            CanvasGroup.alpha = 1;
        });
    }
    public virtual void OnCloseButton() {
        RequestClose();
        AudioManager audio = AudioManager.Instance;
        if(audio) {
            audio.Play("sfx-ui-buttonconf");
        }
    }

    public virtual void Show() {
        gameObject.SetActive(true);
        CanvasGroup.interactable = true;
        EventSystem.current.SetSelectedGameObject(SelectedObject);
    }

    public virtual void Hide(bool force = false) {
        CloseEvent?.Invoke();
        gameObject.SetActive(false);
        CanvasGroup.interactable = false;
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
    protected void RequestClose() {
        CloseRequestEvent?.Invoke();
    }

    private void Reset() {
        // defaults for required components
        RectTransform rect = GetComponent<RectTransform>();
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        canvasGroup.ignoreParentGroups = true;
    }
}
