using UnityEngine;

// Code-driven motion for VARCO models that have no skeleton (drones, tracked robots):
// turns toward the player, hover bob / tilt or ground rumble, hit flash, and a falling/sinking wreck on death.
[RequireComponent(typeof(EnemyHealth))]
public class EnemyProceduralVisual : MonoBehaviour
{
    public enum MotionType { Ground, Hover }

    [Header("Hierarchy")]
    [Tooltip("Yaw pivot turned toward the player. Detached on death to become the wreck.")]
    public Transform pivot;
    [Tooltip("Model transform that gets bob / tilt offsets.")]
    public Transform model;
    public TintGlow glow;

    [Header("Motion")]
    public MotionType motion = MotionType.Hover;
    public bool facePlayer = true;
    public float turnSpeed = 180f;
    public float bobAmplitude = 0.12f;
    public float bobFrequency = 0.8f;
    public float swayAngle = 4f;
    public float moveTilt = 8f;
    public float rumble = 0.03f;

    [Header("Death")]
    public float wreckStay = 1.0f;
    public float wreckSinkTime = 1.2f;
    [Tooltip("Height of the pivot above the ground when the wreck lands (hover units).")]
    public float wreckRestHeight = 0.3f;

    EnemyHealth health;
    Transform player;
    Vector3 baseLocalPos;
    Quaternion baseLocalRot;
    Vector3 lastPos;
    Vector3 smoothVel;
    float phase;
    bool dead;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
        if (model != null)
        {
            baseLocalPos = model.localPosition;
            baseLocalRot = model.localRotation;
        }
        phase = Random.value * 10f;
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDied -= HandleDied;
        }
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        lastPos = transform.position;
        Face(true);
    }

    void Update()
    {
        if (dead || model == null) return;
        float dt = Mathf.Max(Time.deltaTime, 0.00001f);
        Vector3 vel = (transform.position - lastPos) / dt;
        lastPos = transform.position;
        smoothVel = Vector3.Lerp(smoothVel, vel, 1f - Mathf.Exp(-6f * dt));

        Face(false);

        Vector3 localVel = pivot != null ? pivot.InverseTransformDirection(smoothVel) : smoothVel;
        float t = Time.time + phase;
        Vector3 pos = baseLocalPos;
        Vector3 euler = Vector3.zero;

        if (motion == MotionType.Hover)
        {
            pos.y += Mathf.Sin(t * bobFrequency * 2f * Mathf.PI) * bobAmplitude;
            euler.x = Mathf.Clamp(localVel.z * moveTilt, -25f, 25f) + Mathf.Sin(t * 1.3f) * swayAngle * 0.5f;
            euler.z = Mathf.Clamp(-localVel.x * moveTilt, -25f, 25f) + Mathf.Sin(t * 0.9f) * swayAngle;
        }
        else
        {
            float speed = new Vector3(smoothVel.x, 0f, smoothVel.z).magnitude;
            float k = Mathf.Clamp01(speed / 1.5f);
            pos.y += (Mathf.PerlinNoise(t * 18f, 0.5f) - 0.5f) * rumble * k;
            euler.x = Mathf.Clamp(localVel.z * moveTilt * 0.3f, -5f, 5f);
            euler.z = Mathf.Sin(t * 0.7f) * swayAngle * 0.25f + (Mathf.PerlinNoise(0.5f, t * 14f) - 0.5f) * 2f * k;
        }

        model.localPosition = pos;
        model.localRotation = baseLocalRot * Quaternion.Euler(euler);
    }

    void Face(bool instant)
    {
        if (!facePlayer || pivot == null || player == null) return;
        Vector3 d = player.position - pivot.position;
        d.y = 0f;
        if (d.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(d.normalized, Vector3.up);
        pivot.rotation = instant ? target : Quaternion.RotateTowards(pivot.rotation, target, turnSpeed * Time.deltaTime);
    }

    void HandleDamaged(EnemyHealth h, int amount)
    {
        if (glow != null) glow.Flash();
    }

    void HandleDied(EnemyHealth h)
    {
        if (dead) return;
        dead = true;
        if (pivot == null) return;

        foreach (Collider c in pivot.GetComponentsInChildren<Collider>()) Destroy(c);
        pivot.SetParent(null, true);
        if (glow != null) glow.SetWrecked(new Color(0.35f, 0.33f, 0.32f));

        EnemyWreck wreck = pivot.gameObject.AddComponent<EnemyWreck>();
        wreck.falls = motion == MotionType.Hover;
        wreck.stay = wreckStay;
        wreck.sinkTime = wreckSinkTime;
        wreck.restHeight = wreckRestHeight;
    }
}
