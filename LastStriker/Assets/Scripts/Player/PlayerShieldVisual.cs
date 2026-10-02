using UnityEngine;

// First-person shield view model: rests low in the bottom-left corner and rises while the shield is held.
public class PlayerShieldVisual : MonoBehaviour
{
    public PlayerShield shield;
    public Vector3 loweredPosition = new Vector3(-0.46f, -0.62f, 0.72f);
    public Vector3 loweredEuler = new Vector3(28f, 18f, -12f);
    public Vector3 raisedPosition = new Vector3(-0.17f, -0.2f, 0.62f);
    public Vector3 raisedEuler = new Vector3(2f, 8f, -3f);
    public float raiseSpeed = 7f;
    public float lowerSpeed = 4f;
    public float bobAmount = 0.006f;
    public float bobSpeed = 1.6f;

    float t;

    void LateUpdate()
    {
        bool up = shield != null && shield.IsShieldActive;
        t = Mathf.MoveTowards(t, up ? 1f : 0f, (up ? raiseSpeed : lowerSpeed) * Time.deltaTime);
        float e = t * t * (3f - 2f * t);

        Vector3 pos = Vector3.Lerp(loweredPosition, raisedPosition, e);
        pos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmount * (1f - e);
        transform.localPosition = pos;
        transform.localRotation = Quaternion.Slerp(Quaternion.Euler(loweredEuler), Quaternion.Euler(raisedEuler), e);
    }
}
