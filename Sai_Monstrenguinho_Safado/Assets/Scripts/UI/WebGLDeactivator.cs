using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class WebGLDeactivator : MonoBehaviour
    {
#if UNITY_WEBGL
        void Start()
        {
            gameObject.SetActive(false);
        }
#endif
    }
}