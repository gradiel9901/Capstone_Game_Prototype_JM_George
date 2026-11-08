using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public DemonKing demonKingHealth;
    public GameObject AreaExit;
    public PlayerController playerController;

    private void Awake()
    {
        if (demonKingHealth == null)
        {
            demonKingHealth = FindAnyObjectByType<DemonKing>();
        }

        if (playerController == null)
        {
            playerController = PlayerController.Instance != null ? PlayerController.Instance : FindAnyObjectByType<PlayerController>();
        }
    }

    public void Update()
    {
        if (demonKingHealth == null)
        {
            demonKingHealth = FindAnyObjectByType<DemonKing>();
            if (demonKingHealth == null) return;
        }

        if (demonKingHealth.enemyHealth != null && demonKingHealth.enemyHealth.currentHealth <= 0)
        {
            if (AreaExit != null && !AreaExit.activeSelf)
            {
                AreaExit.SetActive(true);
            }
        }
    }
}
