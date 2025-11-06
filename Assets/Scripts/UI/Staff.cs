using System.Collections.Generic;
using UnityEngine;

public class Staff : MonoBehaviour, IWeapon
{
    [Header("Weapon Settings")]
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject magicLaser;
    [SerializeField] private Transform magicLaserSpawnPoint;
    [SerializeField] private float targetSearchRadius = 8f; // How far the staff looks for targets

    private Animator myAnimator;
    private Transform currentTarget;
    private float targetRefreshTimer = 0f;
    private float targetRefreshRate = 0.25f; // Check for targets every 0.25s

    private readonly int ATTACK_HASH = Animator.StringToHash("Attack");

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        AutoTargetClosestEnemy();
        FacePlayerDirection();
    }

    public void Attack()
    {
        myAnimator.SetTrigger(ATTACK_HASH);
    }

    public void SpawnStaffProjectileAnimEvent()
    {
        GameObject newLaser = Instantiate(magicLaser, magicLaserSpawnPoint.position, Quaternion.identity);

        // Fire toward the target if available, else forward based on player facing direction
        Vector2 dir;

        if (currentTarget != null)
        {
            dir = (currentTarget.position - magicLaserSpawnPoint.position).normalized;
        }
        else
        {
            dir = PlayerController.Instance.FacingLeft ? Vector2.left : Vector2.right;
        }

        newLaser.transform.right = dir;
        newLaser.GetComponent<MagicLaser>().UpdateLaserRange(weaponInfo.weaponRange);
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    private void AutoTargetClosestEnemy()
    {
        targetRefreshTimer -= Time.deltaTime;
        if (targetRefreshTimer > 0) return;
        targetRefreshTimer = targetRefreshRate;

        Collider2D[] hits = Physics2D.OverlapCircleAll(PlayerController.Instance.transform.position, targetSearchRadius);

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy") || hit.CompareTag("Dummy"))
            {
                float dist = Vector2.Distance(PlayerController.Instance.transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hit.transform;
                }
            }
        }

        currentTarget = closest;
    }

    /// <summary>
    /// Makes the staff face wherever the player is facing.
    /// </summary>
    private void FacePlayerDirection()
    {
        if (PlayerController.Instance.FacingLeft)
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, targetSearchRadius);
    }
}
