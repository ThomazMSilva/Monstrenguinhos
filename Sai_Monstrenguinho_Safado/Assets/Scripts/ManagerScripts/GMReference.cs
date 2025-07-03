using UnityEngine;

namespace Assets.Scripts.ManagerScripts
{
    public class GMReference : MonoBehaviour
    {
        public void LoadScene(string scene) => GameManager.Instance.LoadScene(scene);

        public void SetOptionsScreenActive(bool open) => GameManager.Instance.SetOptionsScreenActive(open);

        public void SetCreditsScreenActive(bool active) => GameManager.Instance.SetCreditsScreenActive(active);

        public void SetControlsScreenActive(bool active) => GameManager.Instance.SetControlsScreenActive(active);

        public void SetMenuScreenActive(bool active) => GameManager.Instance.SetMenuScreenActive(active);
    }
}