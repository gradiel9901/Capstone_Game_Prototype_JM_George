using UnityEngine;

public class DogAIAction : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float attackRange = 1.2f;
    public int damage = 10;
    public float attackCooldown = 1f;

    private float attackTimer = 0f;
    private Transform currentTarget;
    private Vector2 moveTarget;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public bool IsBusy => currentTarget != null || isMoving;
    private bool isMoving = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        if (currentTarget != null)
        {
            HandleAttackTarget();
        }
        else if (isMoving)
        {
            HandleMoveCommand();
        }
    }

    // ✅ Called from DogFollow.cs when clicking an enemy/dummy
    public void StartAttack(Transform target)
    {
        currentTarget = target;
        isMoving = false;
    }

    // ✅ Called from DogFollow.cs when clicking empty ground
    public void MoveTo(Vector2 destination)
    {
        moveTarget = destination;
        currentTarget = null;
        isMoving = true;
    }

    private void HandleMoveCommand()
    {
        Vector2 direction = (moveTarget - (Vector2)transform.position);
        if (direction.magnitude < 0.1f)
        {
            isMoving = false;
            animator.SetFloat("Speed", 0f);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
        animator.SetFloat("Speed", 1f);

        if (direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;
    }

    private void HandleAttackTarget()
    {
        if (currentTarget == null)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        if (distance > attackRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
            animator.SetFloat("Speed", 1f);
        }
        else
        {
            animator.SetFloat("Speed", 0f);

            if (attackTimer <= 0f)
            {
                AttackOnce();
                attackTimer = attackCooldown;
            }
        }
    }

    private void AttackOnce()
    {
        if (currentTarget == null) return;

        // Check for enemy or dummy
        var enemy = currentTarget.GetComponent<EnemyHealth>();
        var dummy = currentTarget.GetComponent<DummyTarget>();

        if (enemy != null)
            enemy.TakeDamage(damage);
        else if (dummy != null)
            dummy.TakeDamage(damage);
    }

    public bool IsValidTarget(Transform target)
    {
        return target.CompareTag("Enemy") || target.CompareTag("Dummy");
    }
}
