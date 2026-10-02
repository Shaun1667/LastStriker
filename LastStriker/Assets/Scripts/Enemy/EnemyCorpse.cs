using UnityEngine;

// Left behind when an animated enemy dies: holds the death pose, then sinks into the ground and is removed.
public class EnemyCorpse : MonoBehaviour
{
    public Animation anim;
    public string stateName = "death";
    public float stopTime = -1f;
    public float sinkDelay = 2.5f;
    public float lifetime = 4f;
    public float sinkSpeed = 0.8f;

    float age;

    void Update()
    {
        age += Time.deltaTime;

        if (anim != null && stopTime > 0f && !string.IsNullOrEmpty(stateName))
        {
            AnimationState st = anim[stateName];
            if (st != null && st.time >= stopTime)
            {
                st.time = stopTime;
                st.speed = 0f;
            }
        }

        if (age > sinkDelay) transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
        if (age > lifetime) Destroy(gameObject);
    }
}
