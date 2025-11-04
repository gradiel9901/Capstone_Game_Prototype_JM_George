using UnityEngine;

public class DogController : MonoBehaviour
{
    public Transform player;
    public DogAIAction aiAction;
    public float followSpeed = 3f;
    public float stopDistance = 2f;
    public float attackRange = 1.2f;
    public string[] targetTags = { "Enemy", "Dummy" };

    private Transform target;

    private void Update()
    {
        if (player == null || aiAction == null) return;

        FindClosestTarget();

        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if (distanceToTarget > attackRange)
                aiAction.MoveTo(target.position);
            else
                aiAction.StartAttack(target);
        }
        else
        {
            FollowPlayer();
        }
    }

    void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > stopDistance)
            aiAction.MoveTo(player.position);
    }

    void FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (string tag in targetTags)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in targets)
            {
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = obj.transform;
                }
            }
        }

        target = closest;
    }
}
