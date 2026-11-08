using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class ScreenShakeManager : Singleton<ScreenShakeManager>
{
    private CinemachineImpulseSource source;

    protected override void Awake() {
        base.Awake();
        source = GetComponent<CinemachineImpulseSource>();
        if (source == null)
        {
            source = FindAnyObjectByType<CinemachineImpulseSource>();
        }
    }

    public void ShakeScreen() {
        if (source != null)
        {
            source.GenerateImpulse();
        }
    }
}
