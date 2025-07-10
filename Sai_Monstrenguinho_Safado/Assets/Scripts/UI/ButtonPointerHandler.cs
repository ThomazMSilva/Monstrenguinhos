using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonPointerHandler : MonoBehaviour
    , IPointerEnterHandler
    , IPointerExitHandler
    , IPointerUpHandler
    , ISelectHandler
    , IDeselectHandler
    , ISubmitHandler
    //, ICancelHandler
{
    #region ATTRIBUTES
    [Header("References")]
    [SerializeField] private RectTransform foregroundButton;
    [SerializeField] private RectTransform backgroundImage;
    [SerializeField] private RectTransform wingIMG1;
    [SerializeField] private RectTransform wingIMG2;

    [Header("Settings")]
    [SerializeField] private float hoverYOffset = 10f;
    [SerializeField] private float buttonTweenDuration = 0.1f;
    [SerializeField] private float wingsTweenDuration = 0.1f;
    [SerializeField] private float wingsRotation = 30f;
    [SerializeField] private Ease buttonEaseType = Ease.InOutBounce;
    [SerializeField] private Ease wingsEaseType = Ease.InOutBounce;
    [SerializeField] private LoopType wingsLoopType = LoopType.Yoyo;

    private Vector2 originalPosition;
    private Vector2 backgroundPosition;
    private Vector3 wing1OriginalRotation;
    private Vector3 wing2OriginalRotation;
    private bool isHovering = false;
    private Tween wing1Tween;
    private Tween wing2Tween;
    private Tween moveTween;
    #endregion

    private void Awake()
    {
        originalPosition = foregroundButton.anchoredPosition;
        backgroundPosition = backgroundImage.anchoredPosition;

        wing2OriginalRotation = wingIMG2.eulerAngles;
        wing1OriginalRotation = wingIMG1.eulerAngles;
    }

    private void OnDestroy()
    {
        wing1Tween?.Kill();
        wing2Tween?.Kill();
        moveTween?.Kill();
    }

    #region INTERFACE_IMPLEMENTATION
    public void OnPointerEnter(PointerEventData eventData) => Select();

    public void OnSelect(BaseEventData eventData) => Select();

    public void OnPointerExit(PointerEventData eventData) => Deselect();

    public void OnDeselect(BaseEventData eventData) => Deselect();

    public void OnPointerDown(PointerEventData eventData) => Press();

    public void OnSubmit(BaseEventData eventData) => Press();

    public void OnPointerUp(PointerEventData eventData) => Release();

    public void OnCancel(BaseEventData eventData) => Release();
    #endregion

    #region METHODS
    private void Select()
    {
        if (isHovering) return;
        isHovering = true;
        MoveToPosition(new Vector2(
            originalPosition.x,
            originalPosition.y + hoverYOffset
        ));

        wing1Tween = wingIMG1
            .DORotate
            (
                new
                (
                    wing1OriginalRotation.x,
                    wing1OriginalRotation.y,
                    wing1OriginalRotation.z + wingsRotation
                ),
                wingsTweenDuration
            )
            .SetEase(wingsEaseType)
            .SetLoops(-1, wingsLoopType);

        wing2Tween = wingIMG2
            .DORotate
            (
                new
                (
                    wing2OriginalRotation.x,
                    wing2OriginalRotation.y,
                    wing2OriginalRotation.z + wingsRotation
                ),
                wingsTweenDuration
            )
            .SetEase(wingsEaseType)
            .SetLoops(-1, wingsLoopType);
    }

    private void Deselect()
    {
        if (!isHovering) return;
        isHovering = false;
        MoveToPosition(originalPosition);
        wing1Tween?.Kill();
        wing2Tween?.Kill();
        wing1Tween = wingIMG1.DORotate(wing1OriginalRotation, wingsTweenDuration);
        wing2Tween = wingIMG2.DORotate(wing2OriginalRotation, wingsTweenDuration);
    }

    private void Press()
    {
        MoveToPosition(backgroundPosition);
    }

    private void Release()
    {
        MoveToPosition(isHovering
            ? new Vector2(originalPosition.x, originalPosition.y + hoverYOffset)
            : originalPosition);
    }

    private void MoveToPosition(Vector2 targetPosition)
    {
        moveTween = foregroundButton.DOAnchorPos(targetPosition, buttonTweenDuration, true).SetEase(buttonEaseType);
    }
    #endregion
}
