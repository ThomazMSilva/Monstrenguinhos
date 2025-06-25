using UnityEngine;

namespace Assets.Scripts.Interactibles
{
    public class Interactible : MonoBehaviour
    {
        [SerializeField] private GameObject selectionDisplay;

        public virtual void Interact(object sender = null) { UnityEngine.Debug.LogWarning("Interagivel nao contem implementacao para \'Interact()\'"); }

        public virtual void Select(object sender = null) 
        {
            selectionDisplay.SetActive(true);

            UnityEngine.Debug.LogWarning("Interagivel nao contem implementacao para \'Select()\'"); 
        }

        public virtual void Deselect(object sender = null) 
        {
            selectionDisplay.SetActive(false);

            UnityEngine.Debug.LogWarning("Interagivel nao contem implementacao para \'Deselect()\'"); 
        }

    }
}