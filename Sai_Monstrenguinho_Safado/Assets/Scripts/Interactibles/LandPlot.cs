using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class LandPlot : MonoBehaviour, IInteractible
    {
        [SerializeField] private GameObject selectionDisplay;

        public void Interact()
        {
            Debug.Log("Interagiu com terreno");
        }

        public void Select()
        {
            selectionDisplay.SetActive(true);
        }

        public void Deselect()
        {
            selectionDisplay.SetActive(false);
        }
    }
}