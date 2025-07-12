using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class DebugLosses : MonoBehaviour
    {
        [SerializeField] private NPCScripts.NPCManager npcManager;
        [SerializeField] private List<GameObject> visualFeedbacks = new();
        private int currentFeedbacks = 0;
        [SerializeField] private float tweenDuration = .5f;
        [SerializeField] private float fadeDuration = 2f;
        [SerializeField] private Ease easeType = Ease.OutSine;

        public void ClearFeedbacks()
        {
            foreach (var feedback in visualFeedbacks)
            {
                feedback/*.transform.GetChild(0).gameObject*/.SetActive(true);
            }

            currentFeedbacks = 0;
        }

        public void AddFeedback()
        {
            currentFeedbacks++;
            if (currentFeedbacks > visualFeedbacks.Count) return;

            var star = visualFeedbacks[^currentFeedbacks];

            if (star == null || !star.activeSelf) return;

            var particles = star.GetComponentInChildren<ParticleSystem>();

            if (particles != null) particles.Play();

            var starParent = star.transform.parent;

            starParent.rotation = Quaternion.identity;

            var originalPosition = star.transform.position;

            starParent
            .DORotate(new(15, 0, 0), tweenDuration)
            .SetEase(easeType)
            .OnComplete
            (
                () => 
                {
                    star.transform.parent = starParent.parent;
                    
                    var rb = star.GetComponent<Rigidbody>();
                    rb.isKinematic = false;
                    rb.useGravity = true;
                        
                    var rend = star.GetComponent<SpriteRenderer>();
                    rend
                    .DOFade(0, fadeDuration)
                    .OnComplete
                    (   
                        () =>
                        { 
                            star.SetActive(false);
                            rb.useGravity = false;
                            rb.isKinematic = true;
                            var c = rend.material.color;
                            c.a = 1;
                            rend.material.color = c;
                            star.transform.parent = starParent;
                            star.transform.position = originalPosition;
                        }
                    );
                }
            );
        }


        /*[SerializeField] private Vector3 defaultOffset = Vector3.right;
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
        }*/

    }
}