using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectAutoScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 10f;
    public float scrollAcceleration = 2f;
    public float initialScrollDelay = 0.5f;
    public float repeatScrollRate = 0.1f;

    [Header("Input Settings")]
    public float inputDeadzone = 0.1f;

    private bool mouseOver = false;
    private List<Selectable> m_Selectables = new List<Selectable>();
    private ScrollRect m_ScrollRect;
    private Vector2 m_NextScrollPosition = Vector2.up;

    private DefaultInputActions inputActions;
    private Vector2 currentNavigationInput;
    private float holdTimer;
    private bool isScrollingContinuously;

    void Awake()
    {
        m_ScrollRect = GetComponent<ScrollRect>();
        inputActions = new DefaultInputActions();
    }

    void OnEnable()
    {
        RefreshSelectables();
        inputActions.UI.Enable();
        inputActions.UI.Navigate.performed += OnNavigatePerformed;
        inputActions.UI.Navigate.canceled += OnNavigateCanceled;
    }

    void OnDisable()
    {
        inputActions.UI.Navigate.performed -= OnNavigatePerformed;
        inputActions.UI.Navigate.canceled -= OnNavigateCanceled;
        inputActions.UI.Disable();
    }

    void Start()
    {
        RefreshSelectables();
        ScrollToSelected(true);
    }

    void Update()
    {
        HandleContinuousScrolling();
        UpdateScrollPosition();
    }

    private void RefreshSelectables()
    {
        m_Selectables.Clear();
        if (m_ScrollRect && m_ScrollRect.content)
        {
            m_ScrollRect.content.GetComponentsInChildren(m_Selectables);
        }
    }

    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        currentNavigationInput = context.ReadValue<Vector2>();

        if (currentNavigationInput.sqrMagnitude > inputDeadzone * inputDeadzone)
        {
            ScrollToSelected(false);
            holdTimer = 0f;
            isScrollingContinuously = false;
        }
    }

    private void OnNavigateCanceled(InputAction.CallbackContext context)
    {
        currentNavigationInput = Vector2.zero;
        holdTimer = 0f;
        isScrollingContinuously = false;
    }

    private void HandleContinuousScrolling()
    {
        if (currentNavigationInput.sqrMagnitude <= inputDeadzone * inputDeadzone)
        {
            holdTimer = 0f;
            isScrollingContinuously = false;
            return;
        }

        holdTimer += Time.deltaTime;

        if (!isScrollingContinuously)
        {
            if (holdTimer >= initialScrollDelay)
            {
                isScrollingContinuously = true;
                holdTimer = 0f; 
            }
        }
        else
        {
            if (holdTimer >= repeatScrollRate)
            {
                ScrollToSelected(false);
                holdTimer = 0f;
            }
        }
    }

    private void UpdateScrollPosition()
    {
        if (!mouseOver)
        {
            float currentSpeed = scrollSpeed;

            if (isScrollingContinuously)
            {
                currentSpeed *= scrollAcceleration;
            }

            m_ScrollRect.normalizedPosition = Vector2.Lerp(
                m_ScrollRect.normalizedPosition,
                m_NextScrollPosition,
                currentSpeed * Time.deltaTime
            );
        }
        else
        {
            m_NextScrollPosition = m_ScrollRect.normalizedPosition;
        }
    }

    void ScrollToSelected(bool quickScroll)
    {
        if (m_Selectables.Count == 0) return;

        int selectedIndex = -1;
        Selectable selectedElement = EventSystem.current.currentSelectedGameObject ?
            EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null;

        if (selectedElement)
        {
            selectedIndex = m_Selectables.IndexOf(selectedElement);
        }

        if (selectedIndex > -1)
        {
            float normalizedPosition = 1 - (selectedIndex / ((float)m_Selectables.Count - 1));

            if (quickScroll)
            {
                m_ScrollRect.normalizedPosition = new Vector2(0, normalizedPosition);
                m_NextScrollPosition = m_ScrollRect.normalizedPosition;
            }
            else
            {
                m_NextScrollPosition = new Vector2(0, normalizedPosition);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        ScrollToSelected(false);
    }
}