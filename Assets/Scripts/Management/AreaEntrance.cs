using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEntrance : MonoBehaviour
{
    [SerializeField] private string transitionName;
    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
            if (audioObj != null)
                audioManager = audioObj.GetComponent<AudioManager>();
            if (audioManager == null)
                audioManager = FindAnyObjectByType<AudioManager>();
        }
    }

    private void Start()
    {
        if (SceneManagement.Instance != null && transitionName == SceneManagement.Instance.SceneTransitionName)
        {
            if (PlayerController.Instance != null)
                PlayerController.Instance.transform.position = this.transform.position;

            if (CameraController.Instance != null)
                CameraController.Instance.SetPlayerCameraFollow();

            if (audioManager != null && audioManager.portalIn != null)
                audioManager.PlaySFX(audioManager.portalIn);

            if (UIFade.Instance != null)
                UIFade.Instance.FadeToClear();
        }
    }
}
