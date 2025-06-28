using UnityEngine;

namespace Assets.Scripts.NPCScripts
{
    public class NPCBehaviour : MonoBehaviour
    {
        public ClientAttributes Attributes;
        public float tolerance;
        public UnityEngine.Events.UnityEvent<NPCBehaviour> onDestroy;

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnDestroy() => onDestroy?.Invoke(this);
    }
}
