using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MenuPrincipalIfActive : MonoBehaviour
    {
        [SerializeField] private UINavigationManager _navigationManager;

        private void Start()
        {
            StartCoroutine(WaitForEndLoading());
        }

        private IEnumerator WaitForEndLoading()
        {
            while (GameManager.Instance.IsLoading) yield return null;
            _navigationManager.OpenPanel(gameObject);
        }
    }
}