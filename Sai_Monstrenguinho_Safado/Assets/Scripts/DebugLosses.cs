using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class DebugLosses : MonoBehaviour
    {
        [SerializeField] private NPCScripts.NPCManager npcManager;
        [SerializeField] private List<GameObject> visualFeedbacks = new();
        [SerializeField] private Vector3 defaultOffset = Vector3.right;
        private int currentFeedbacks = 0;

        public void ClearFeedbacks()
        {
            foreach (var feedback in visualFeedbacks)
            {
                feedback.transform.GetChild(0).gameObject.SetActive(false);
            }

            currentFeedbacks = 0;
        }

        public void AddFeedback()
        {
            currentFeedbacks++;
            for (int i = 0; i < currentFeedbacks && i < visualFeedbacks.Count; i++)
            {
                visualFeedbacks[i].transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            int count = visualFeedbacks.Count;
            if (count == 0) { Debug.LogError("Nao tem feedback visual de falha serializado"); }
            if (visualFeedbacks[0].transform.childCount < 1) { Debug.LogError("feedback visual nao tem filho"); }

            if(count < npcManager.MaximumFailedClients)
            {
                var lastOnList = visualFeedbacks[count - 1];
                for (int i = count; i <= npcManager.MaximumFailedClients; i++)
                {
                    var newVisualFeedback = Instantiate
                        (
                        lastOnList,
                        lastOnList.transform.position + defaultOffset * (i - (count - 1)),
                        lastOnList.transform.rotation,
                        lastOnList.transform.parent
                        );
                    newVisualFeedback.transform.GetChild(0).gameObject.SetActive(false);
                    visualFeedbacks.Add(newVisualFeedback);
                }
            }
        }

    }
}