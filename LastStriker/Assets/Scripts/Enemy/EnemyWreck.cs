using UnityEngine;

// Wreck left behind by a destroyed non-rigged enemy: hover units drop and tumble to the ground,
// ground units slump. Then it sinks and is removed.
public class EnemyWreck : MonoBehaviour
{
    public bool falls;
    public float gravity = 14f;
    public float stay = 1f;
    public float sinkTime = 1.2f;
    public float sinkSpeed = 0.8f;
    public float restHeight = 0.3f;

    float groundY;
    Vector3 velocity;
    Vector3 spin;
    float landedAt = -1f;
    float age;

    void Start()
    {
        groundY = transform.position.y - 40f;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 60f))
            groundY = hit.point.y;
        if (falls)
        {
            spin = new Vector3(Random.Range(-220f, 220f), Random.Range(-90f, 90f), Random.Range(-220f, 220f));
        }
        else
        {
            landedAt = 0f;
        }
    }

    void Update()
    {
        age += Time.deltaTime;

        if (falls && landedAt < 0f)
        {
            velocity += Vector3.down * gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(spin * Time.deltaTime, Space.World);
            if (transform.position.y <= groundY + restHeight)
            {
                Vector3 p = transform.position;
                p.y = groundY + restHeight;
                transform.position = p;
                landedAt = age;
            }
            if (age > 6f) Destroy(gameObject);
            return;
        }

        if (!falls && age < 0.6f) transform.Rotate(Vector3.right * 14f * Time.deltaTime, Space.Self);

        float since = age - landedAt;
        if (since > stay) transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
        if (since > stay + sinkTime) Destroy(gameObject);
    }
}
