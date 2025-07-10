using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class TutorialMenuHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private UINavigationManager _navigationManager;
        [SerializeField] private List<GameObject> tutorialScreens;
        [SerializeField] private UnityEngine.UI.Button previousButton;
        [SerializeField] private UnityEngine.UI.Button nextButton;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        private int currentScreenIndex = 0;

        private void Start()
        {
            StartCoroutine(InitializeValues());
        }

        private System.Collections.IEnumerator InitializeValues()
        {
            while (GameManager.Instance.IsLoading) yield return null;

            GameManager.Instance.SetPause(true);

            SelectTutorial();
            //_navigationManager.CloseAllPanels();
            previousButton.onClick.AddListener(PreviousScreen);
            nextButton.onClick.AddListener(NextScreen);
            closeButton.onClick.AddListener(CloseTutorial);

            EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
            //_navigationManager.OpenPanel(tutorialScreens[currentScreenIndex]);
            tutorialScreens[currentScreenIndex].SetActive(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SelectTutorial();
        }

        public void NextScreen()
        {
            //_navigationManager.ClosePanel(tutorialScreens[currentScreenIndex]);
            tutorialScreens[currentScreenIndex].SetActive(false);
            currentScreenIndex++;
            if(currentScreenIndex >= tutorialScreens.Count) currentScreenIndex = 0;
            //_navigationManager.OpenPanel(tutorialScreens[currentScreenIndex]);
            tutorialScreens[currentScreenIndex].SetActive(true);
        }

        public void PreviousScreen()
        {
            //_navigationManager.ClosePanel(tutorialScreens[currentScreenIndex]);
            tutorialScreens[currentScreenIndex].SetActive(false);
            currentScreenIndex--;
            if (currentScreenIndex < 0) currentScreenIndex = tutorialScreens.Count - 1 ;
            //_navigationManager.OpenPanel(tutorialScreens[currentScreenIndex]);
            tutorialScreens[currentScreenIndex].SetActive(true);
        }

        public void CloseTutorial()
        {
            /*_navigationManager.ClosePanel(tutorialScreens[currentScreenIndex]);
            _navigationManager.CloseAllPanels();*/
            //gameObject.SetActive(false);
            GameManager.Instance.Fade(_canvasGroup);
            GameManager.Instance.SetPause(false);
            EventSystem.current.SetSelectedGameObject(null);
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