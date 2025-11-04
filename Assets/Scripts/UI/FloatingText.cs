using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float duration = 1f;

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        duration -= Time.deltaTime;
        if (duration <= 0f)
            Destroy(gameObject);
    }
}
