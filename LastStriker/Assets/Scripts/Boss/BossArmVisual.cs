using UnityEngine;

// Boss arm motion: tracks the player within a limited cone, shakes while its attack warning glows,
// and kicks back when the attack fires (detected when the warning tint ends).
public class BossArmVisual : MonoBehaviour
{
    public TintGlow glow;
    public float maxAimAngle = 28f;
    public float aimSpeed = 60f;
    public float shakeAmount = 0.6f;
    public float recoilDistance = 0.45f;
    public float recoilPitch = 10f;
    public float recoilRecover = 5f;

    Transform player;
    Quaternion baseLocalRot;
    Vector3 baseLocalPos;
    Quaternion aimLocalRot;
    float recoil;
    bool wasTinted;

    void Awake()
    {
        baseLocalRot = transform.localRotation;
        baseLocalPos = transform.localPosition;
        aimLocalRot = baseLocalRot;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void LateUpdate()
    {
        if (player != null && transform.parent != null)
        {
            Vector3 dirWorld = (player.position - transform.position).normalized;
            Quaternion want = Quaternion.Inverse(transform.parent.rotation) * Quaternion.LookRotation(dirWorld, Vector3.up);
            float ang = Quaternion.Angle(baseLocalRot, want);
            if (ang > maxAimAngle) want = Quaternion.Slerp(baseLocalRot, want, maxAimAngle / ang);
            aimLocalRot = Quaternion.RotateTowards(aimLocalRot, want, aimSpeed * Time.deltaTime);
        }

        bool tinted = glow != null && glow.IsTinted;
        if (wasTinted && !tinted) recoil = 1f;
        wasTinted = tinted;
        recoil = Mathf.MoveTowards(recoil, 0f, recoilRecover * Time.deltaTime);

        Quaternion rot = aimLocalRot * Quaternion.Euler(-recoilPitch * recoil, 0f, 0f);
        if (tinted) rot *= Quaternion.Euler(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0f);
        transform.localRotation = rot;
        transform.localPosition = baseLocalPos - (rot * Vector3.forward) * (recoilDistance * recoil);
    }
}
