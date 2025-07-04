using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonPointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
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

    private void Awake()
    {
        originalPosition = foregroundButton.anchoredPosition;
        backgroundPosition = backgroundImage.anchoredPosition;

        wing2OriginalRotation = wingIMG2.eulerAngles;
        wing1OriginalRotation = wingIMG1.eulerAngles;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
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
                    wing1OriginalRotation.z + 30
                ),
                wingsTweenDuration
            )
            .SetEase(wingsEaseType)
            .SetLoops(-1, LoopType.Yoyo);

        wing2Tween = wingIMG2
            .DORotate
            (
                new
                (
                    wing2OriginalRotation.x,
                    wing2OriginalRotation.y,
                    wing2OriginalRotation.z + 30
                ),
                wingsTweenDuration
            )
            .SetEase(wingsEaseType)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        MoveToPosition(originalPosition);
        wing1Tween?.Kill();
        wing2Tween?.Kill();
        wing1Tween = wingIMG1.DORotate(wing1OriginalRotation, wingsTweenDuration);
        wing2Tween = wingIMG2.DORotate(wing2OriginalRotation, wingsTweenDuration);

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveToPosition(backgroundPosition);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MoveToPosition(isHovering
            ? new Vector2(originalPosition.x, originalPosition.y + hoverYOffset)
            : originalPosition);
    }

    private void MoveToPosition(Vector2 targetPosition)
    {
        foregroundButton.DOAnchorPos(targetPosition, buttonTweenDuration, true).SetEase(buttonEaseType);
    }
}
