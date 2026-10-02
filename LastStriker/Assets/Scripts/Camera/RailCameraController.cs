
using UnityEngine;

// Camera shake as an additive offset: removed early each frame and re-applied in LateUpdate,
// so it never fights the rail mover or snaps the camera back to a stale position.
[DefaultExecutionOrder(-1000)]
public class RailCameraController : MonoBehaviour
{
    float shakeTimeLeft;
    float shakeMagnitude;
    Vector3 appliedOffset;

    public void Shake(float duration, float magnitude)
    {
        shakeTimeLeft = Mathf.Max(shakeTimeLeft, duration);
        shakeMagnitude = magnitude;
    }

    void Update()
    {
        if (appliedOffset != Vector3.zero)
        {
            transform.position -= appliedOffset;
            appliedOffset = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        if (shakeTimeLeft <= 0f || Time.timeScale <= 0f) return;
        shakeTimeLeft -= Time.deltaTime;
        Vector2 r = UnityEngine.Random.insideUnitCircle * shakeMagnitude;
        appliedOffset = transform.right * r.x + transform.up * r.y;
        transform.position += appliedOffset;
    }
}
