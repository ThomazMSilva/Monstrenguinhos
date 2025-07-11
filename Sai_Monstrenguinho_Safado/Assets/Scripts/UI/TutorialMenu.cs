using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class TutorialMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private UINavigationManager _navigationManager;
        [SerializeField] private List<GameObject> tutorialScreens;
        [SerializeField] private UnityEngine.UI.Button previousButton;
        [SerializeField] private UnityEngine.UI.Button nextButton;
        public UnityEvent OnTutorialClosed;        
        private int currentScreenIndex = 0;

            
        private void Awake()
        {
            StartCoroutine(InitializeValues());
        }

        private System.Collections.IEnumerator InitializeValues()
        {
            while (GameManager.Instance.IsLoading) yield return null;

            GameManager.Instance.SetPause(true);

            SelectTutorial();
            previousButton.onClick.AddListener(PreviousScreen);
            nextButton.onClick.AddListener(NextScreen);
            //closeButton.onClick.AddListener(CloseTutorial);

            EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
            tutorialScreens[currentScreenIndex].SetActive(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SelectTutorial();
        }

        public void NextScreen()
        {
            tutorialScreens[currentScreenIndex].SetActive(false);
            currentScreenIndex++;

            if (currentScreenIndex > 0 && !previousButton.isActiveAndEnabled)
            {
                previousButton.gameObject.SetActive(true);
            }
            else if (currentScreenIndex <= 0)
            {
                previousButton.gameObject.SetActive(false);
                EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
            }

            if (currentScreenIndex >= tutorialScreens.Count)
            {
                currentScreenIndex = 0;
                CloseTutorial();
                return;
            }
            tutorialScreens[currentScreenIndex].SetActive(true);
        }

        public void PreviousScreen()
        {
            tutorialScreens[currentScreenIndex].SetActive(false);
            currentScreenIndex--;

            if (currentScreenIndex > 0 && !previousButton.isActiveAndEnabled)
            {
                previousButton.gameObject.SetActive(true);
            }
            else if (currentScreenIndex <= 0)
            {
                previousButton.gameObject.SetActive(false);
                EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
            }

            //if (currentScreenIndex < 0) currentScreenIndex = tutorialScreens.Count - 1;
            tutorialScreens[currentScreenIndex].SetActive(true);
        }

        public void CloseTutorial()
        {
            GameManager.Instance.Fade(_canvasGroup);
            GameManager.Instance.SetPause(false);
            EventSystem.current.SetSelectedGameObject(null);
            OnTutorialClosed?.Invoke();
        }

        public void OpenTutorial()
        {
            gameObject.SetActive(true);
            tutorialScreens[currentScreenIndex].SetActive(true);
            SelectTutorial();
        }

        public void SelectTutorial()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                var defaultButton = gameObject.GetComponentInChildren<UnityEngine.UI.Selectable>();
                if (defaultButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
                }
            }

        }
    }
}