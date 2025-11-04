using UnityEngine;

public class DamageSource : MonoBehaviour
{
    private int damageAmount;

    private void Start()
    {
        MonoBehaviour currentActiveWeapon = ActiveWeapon.Instance.CurrentActiveWeapon;
        damageAmount = (currentActiveWeapon as IWeapon).GetWeaponInfo().weaponDamage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🔹 1. Handle Enemies
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damageAmount);
            return;
        }

        // 🔹 2. Handle Training Dummy
        DummyTarget dummy = other.GetComponent<DummyTarget>();
        if (dummy != null)
        {
            dummy.TakeDamage(damageAmount);
            return;
        }
    }
}
