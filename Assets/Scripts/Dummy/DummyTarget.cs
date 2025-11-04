using UnityEngine;
using TMPro;

public class DummyTarget : MonoBehaviour
{
    [Header("Dummy Settings")]
    [SerializeField] private int maxHealth = 1000;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material hitMaterial;
    [SerializeField] private float hitFlashTime = 0.1f;

    [Header("Floating Damage")]
    [SerializeField] private TMP_Text floatingTextPrefab;
    [SerializeField] private Transform floatingTextSpawnPoint;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private float flashTimer;

    // 🧊 Reference to Warrior (Quest Giver)
    [SerializeField] private NPCInteraction warriorNPC;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && defaultMaterial != null)
            spriteRenderer.material = defaultMaterial;
    }

    private void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0 && spriteRenderer != null)
                spriteRenderer.material = defaultMaterial;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (spriteRenderer != null && hitMaterial != null)
        {
            spriteRenderer.material = hitMaterial;
            flashTimer = hitFlashTime;
        }

        ShowFloatingText(damage);

        // ✅ Report to Warrior Quest
        if (warriorNPC != null)
        {
            warriorNPC.RegisterDamage(damage);
        }

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth; // Reset for testing
        }
    }

    private void ShowFloatingText(int damage)
    {
        if (floatingTextPrefab != null)
        {
            TMP_Text floatText = Instantiate(floatingTextPrefab, floatingTextSpawnPoint.position, Quaternion.identity, transform);
            floatText.text = damage.ToString();
            Destroy(floatText.gameObject, 1f);
        }
    }
}
