using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : Singleton<CameraController>
{
    private CinemachineCamera cinemachineCamera;
#pragma warning disable CS0618
    private CinemachineVirtualCamera cinemachineVirtualCamera;
#pragma warning restore CS0618

    public void SetPlayerCameraFollow() {
        if (PlayerController.Instance == null) return;

        // Try modern Unity 6 CinemachineCamera first
        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Target.TrackingTarget = PlayerController.Instance.transform;
            return;
        }

        // Fallback for backwards-compatible CinemachineVirtualCamera
#pragma warning disable CS0618
        cinemachineVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        if (cinemachineVirtualCamera != null)
        {
            cinemachineVirtualCamera.Follow = PlayerController.Instance.transform;
        }
#pragma warning restore CS0618
    }
}
