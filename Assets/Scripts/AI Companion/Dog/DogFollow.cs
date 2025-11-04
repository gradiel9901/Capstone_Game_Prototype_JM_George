using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class DogFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float followSpeed = 3f;
    public float stopDistance = 1.5f;
    public float teleportDistance = 10f;

    [Header("Command Settings")]
    public KeyCode commandKey = KeyCode.G;
    public LayerMask targetMask; // Enemy & Dummy layers
    public float commandRange = 20f;

    [Header("Animation Settings")]
    public string speedParameter = "Speed";

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private DogAIAction dogAI;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dogAI = GetComponent<DogAIAction>();

        // Auto-find player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null)
            Debug.LogWarning("⚠️ DogFollow2D: No player found!");
    }

    private void Update()
    {
        HandleFollow();
        HandleCommand();
    }

    private void HandleFollow()
    {
        if (dogAI == null || dogAI.IsBusy) return; // Don't follow while attacking or moving to a command target
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > teleportDistance)
        {
            transform.position = player.position;
            return;
        }

        if (distance > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, followSpeed * Time.deltaTime);

            if (direction.x != 0)
                spriteRenderer.flipX = direction.x < 0;

            animator.SetFloat(speedParameter, 1f);
        }
        else
        {
            animator.SetFloat(speedParameter, 0f);
        }
    }

    private void HandleCommand()
    {
        if (Input.GetKeyDown(commandKey))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos, targetMask);

            if (hit != null)
            {
                Transform target = hit.transform;
                if (dogAI != null && dogAI.IsValidTarget(target))
                {
                    dogAI.StartAttack(target);
                    Debug.Log($"🐕 Dog commanded to attack: {target.name}");
                }
            }
            else
            {
                if (dogAI != null)
                {
                    dogAI.MoveTo(mousePos);
                    Debug.Log("🐾 Dog moving to clicked position.");
                }
            }
        }
    }
}
