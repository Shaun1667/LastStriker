using UnityEngine;
using System;
using System.Collections;

// Drives a rigged VARCO model (legacy Animation clips) from the enemy's gameplay events:
// idle/walk loop, attack on telegraph, guard pose for shield robots, hit reactions and a death corpse.
[RequireComponent(typeof(EnemyHealth))]
public class EnemyModelAnimator : MonoBehaviour
{
    [Serializable]
    public class ClipSlot
    {
        public AnimationClip clip;
        public float start = 0f;
        [Tooltip("-1 = play to the end of the clip")]
        public float end = -1f;
        public float speed = 1f;
    }

    [Header("Hierarchy")]
    [Tooltip("Yaw pivot that turns the model toward the player. Detached on death to become the corpse.")]
    public Transform pivot;
    [Tooltip("Object that holds the Animation component (clip paths are relative to it).")]
    public Transform model;
    public float turnSpeed = 240f;

    [Header("Clips")]
    public ClipSlot idle = new ClipSlot();
    public WrapMode idleWrap = WrapMode.Loop;
    public ClipSlot walk = new ClipSlot();
    public ClipSlot attack = new ClipSlot();
    public ClipSlot guard = new ClipSlot();
    public ClipSlot hit = new ClipSlot();
    public ClipSlot death = new ClipSlot();

    [Header("Feel")]
    public float hitCooldown = 0.6f;
    public float crossFade = 0.15f;
    public Color telegraphGlow = new Color(1f, 0.12f, 0.05f);
    public float telegraphGlowIntensity = 3f;
    public float corpseStayAfterDeath = 0.8f;
    public float corpseSinkTime = 1.5f;

    Animation anim;
    EnemyHealth health;
    EnemyBase attacker;
    EnemyShield shield;
    Transform player;
    Renderer[] renderers;
    MaterialPropertyBlock mpb;
    float lockUntil;
    float lastHitTime = -99f;
    bool guarding;
    bool dead;
    Vector3 lastPos;
    string baseState = "";
    Coroutine glowRoutine;

    static readonly int EmissiveFactorId = Shader.PropertyToID("emissiveFactor");
    static readonly int EmissiveTexId = Shader.PropertyToID("emissiveTexture");

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        attacker = GetComponent<EnemyBase>();
        shield = GetComponent<EnemyShield>();

        if (model != null)
        {
            anim = model.GetComponent<Animation>();
            if (anim == null) anim = model.gameObject.AddComponent<Animation>();
            anim.playAutomatically = false;
            anim.cullingType = AnimationCullingType.AlwaysAnimate;
            AddState("idle", idle, idleWrap);
            AddState("walk", walk, WrapMode.Loop);
            AddState("attack", attack, WrapMode.ClampForever);
            AddState("guard", guard, WrapMode.ClampForever);
            AddState("hit", hit, WrapMode.ClampForever);
            AddState("death", death, WrapMode.ClampForever);
            renderers = model.GetComponentsInChildren<Renderer>();
        }
        mpb = new MaterialPropertyBlock();

        health.OnDamaged += HandleDamaged;
        health.OnDied += HandleDied;
        if (attacker != null) attacker.OnTelegraph += HandleTelegraph;
        if (shield != null) shield.OnGuardChanged += HandleGuard;
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
            health.OnDied -= HandleDied;
        }
        if (attacker != null) attacker.OnTelegraph -= HandleTelegraph;
        if (shield != null) shield.OnGuardChanged -= HandleGuard;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        lastPos = transform.position;
        FacePlayer(true);

        if (anim != null && anim["idle"] != null && !guarding)
        {
            anim.Play("idle");
            anim["idle"].time = UnityEngine.Random.Range(0f, anim["idle"].length);
            baseState = "idle";
        }
    }

    void AddState(string name, ClipSlot slot, WrapMode wrap)
    {
        if (slot == null || slot.clip == null) return;
        anim.AddClip(slot.clip, name);
        AnimationState st = anim[name];
        st.wrapMode = wrap;
        st.speed = slot.speed;
    }

    float SlotEnd(string name, ClipSlot slot)
    {
        AnimationState st = anim[name];
        float len = st != null ? st.length : 0f;
        return slot.end > 0f ? Mathf.Min(slot.end, len) : len;
    }

    float SlotDuration(string name, ClipSlot slot)
    {
        return Mathf.Max(0.05f, (SlotEnd(name, slot) - slot.start) / Mathf.Max(0.01f, slot.speed));
    }

    bool PlaySlot(string name, ClipSlot slot, float fade)
    {
        if (anim == null || anim[name] == null) return false;
        anim.CrossFade(name, fade, PlayMode.StopAll);
        AnimationState st = anim[name];
        st.time = slot.start;
        st.speed = slot.speed;
        baseState = "";
        return true;
    }

    void Update()
    {
        if (dead || anim == null) return;

        FacePlayer(false);

        Vector3 delta = transform.position - lastPos;
        delta.y = 0f;
        float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
        lastPos = transform.position;

        if (guarding) return;

        // Hold the end frame of segments that stop before the clip ends.
        if (Time.time < lockUntil) return;

        string desired = (speed > 0.3f && anim["walk"] != null) ? "walk" : "idle";
        if (anim[desired] == null) return;
        if (baseState != desired || !anim.IsPlaying(desired))
        {
            anim.CrossFade(desired, 0.25f, PlayMode.StopAll);
            anim[desired].speed = desired == "idle" ? idle.speed : walk.speed;
            baseState = desired;
        }
    }

    void LateUpdate()
    {
        if (dead || anim == null || Time.time >= lockUntil) return;
        // Clamp one-shot segments that end before the clip does.
        ClampSegment("attack", attack);
        ClampSegment("hit", hit);
    }

    void ClampSegment(string name, ClipSlot slot)
    {
        AnimationState st = anim[name];
        if (st == null || !anim.IsPlaying(name)) return;
        float end = SlotEnd(name, slot);
        if (st.time > end) st.time = end;
    }

    void FacePlayer(bool instant)
    {
        if (pivot == null || player == null) return;
        Vector3 dir = player.position - pivot.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);
        pivot.rotation = instant ? target : Quaternion.RotateTowards(pivot.rotation, target, turnSpeed * Time.deltaTime);
    }

    void HandleTelegraph(float duration)
    {
        if (dead) return;
        if (PlaySlot("attack", attack, crossFade))
        {
            lockUntil = Time.time + SlotDuration("attack", attack);
        }
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(Glow(duration));
    }

    void HandleGuard(bool on)
    {
        if (dead) return;
        guarding = on;
        if (on)
        {
            if (PlaySlot("guard", guard, 0.2f)) lockUntil = float.MaxValue;
        }
        else
        {
            lockUntil = 0f;
            baseState = "";
        }
    }

    void HandleDamaged(EnemyHealth h, int amount)
    {
        if (dead || guarding) return;
        if (Time.time < lockUntil) return;               // don't cut an attack short
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;
        if (PlaySlot("hit", hit, 0.05f))
        {
            lockUntil = Time.time + SlotDuration("hit", hit);
        }
    }

    void HandleDied(EnemyHealth h)
    {
        if (dead) return;
        dead = true;
        SetGlow(Color.black);
        if (pivot == null) return;

        // Strip hitboxes so the corpse can't be shot or block raycasts.
        foreach (Collider c in pivot.GetComponentsInChildren<Collider>()) Destroy(c);

        pivot.SetParent(null, true);
        float deathTime = 0.5f;
        if (anim != null && anim["death"] != null)
        {
            PlaySlot("death", death, 0.1f);
            deathTime = SlotDuration("death", death);
        }
        EnemyCorpse corpse = pivot.gameObject.AddComponent<EnemyCorpse>();
        corpse.anim = anim;
        corpse.stateName = "death";
        corpse.stopTime = death.clip != null ? SlotEnd("death", death) : -1f;
        corpse.sinkDelay = deathTime + corpseStayAfterDeath;
        corpse.lifetime = corpse.sinkDelay + corpseSinkTime;
    }

    IEnumerator Glow(float duration)
    {
        SetGlow(telegraphGlow * telegraphGlowIntensity);
        yield return new WaitForSeconds(duration);
        SetGlow(Color.black);
        glowRoutine = null;
    }

    void SetGlow(Color c)
    {
        if (renderers == null) return;
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissiveFactorId, c);
            mpb.SetTexture(EmissiveTexId, Texture2D.whiteTexture);
            r.SetPropertyBlock(mpb);
        }
    }
}
