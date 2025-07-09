using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCameraHandler : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private int activePriority = 12;
    [SerializeField] private int inactivePriority = 8;

    public void SetVCActive() => _virtualCamera.Priority = activePriority;
    public void SetVCInactive() => _virtualCamera.Priority = inactivePriority;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.P)) SetVCActive();
        if(Input.GetKey(KeyCode.O)) SetVCInactive();
    }
}
