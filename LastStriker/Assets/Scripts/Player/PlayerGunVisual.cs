using UnityEngine;

// First-person rifle view model: points toward the crosshair, kicks on each shot,
// and lowers out of the way while the shield is up (which is also when reloading happens).
public class PlayerGunVisual : MonoBehaviour
{
    public PlayerGun gun;
    public PlayerShield shield;
    public PlayerAim aim;
    public Vector3 restPosition = new Vector3(0.27f, -0.26f, 0.55f);
    public Vector3 modelEulerOffset = Vector3.zero;
    public Vector3 loweredPosition = new Vector3(0.38f, -0.66f, 0.45f);
    public Vector3 loweredEuler = new Vector3(40f, -20f, 12f);
    public float aimDistance = 30f;
    [Tooltip("How far (degrees) the gun may turn toward the crosshair.")]
    public float maxAimAngle = 10f;
    public float aimFollowSpeed = 12f;
    public float lowerSpeed = 6f;
    public float kickBack = 0.045f;
    public float kickPitch = 3.5f;
    public float kickRecover = 18f;

    float lowered;
    float kick;
    Quaternion aimRot = Quaternion.identity;
    Camera cam;

    void OnEnable()
    {
        if (gun != null) gun.OnFired += HandleFired;
    }

    void OnDisable()
    {
        if (gun != null) gun.OnFired -= HandleFired;
    }

    void Start()
    {
        cam = GetComponentInParent<Camera>();
    }

    void HandleFired()
    {
        kick = 1f;
    }

    void LateUpdate()
    {
        bool down = shield != null && shield.IsShieldActive;
        lowered = Mathf.MoveTowards(lowered, down ? 1f : 0f, lowerSpeed * Time.deltaTime);
        float e = lowered * lowered * (3f - 2f * lowered);
        kick = Mathf.MoveTowards(kick, 0f, kickRecover * Time.deltaTime);

        // aim: rotate the gun so it points at the crosshair target
        Quaternion want = Quaternion.identity;
        if (aim != null && cam != null)
        {
            Ray r = aim.GetAimRay();
            Vector3 target = r.origin + r.direction * aimDistance;
            Vector3 gunWorld = cam.transform.TransformPoint(restPosition);
            Vector3 dirLocal = cam.transform.InverseTransformDirection((target - gunWorld).normalized);
            want = Quaternion.LookRotation(dirLocal, Vector3.up);
            float ang = Quaternion.Angle(Quaternion.identity, want);
            if (ang > maxAimAngle) want = Quaternion.Slerp(Quaternion.identity, want, maxAimAngle / ang);
        }
        aimRot = Quaternion.Slerp(aimRot, want, 1f - Mathf.Exp(-aimFollowSpeed * Time.deltaTime));

        Quaternion restRot = aimRot * Quaternion.Euler(modelEulerOffset) * Quaternion.Euler(-kickPitch * kick, 0f, 0f);
        Vector3 restPos = restPosition - (aimRot * Vector3.forward) * (kickBack * kick);

        transform.localPosition = Vector3.Lerp(restPos, loweredPosition, e);
        transform.localRotation = Quaternion.Slerp(restRot, Quaternion.Euler(loweredEuler), e);
    }
}
