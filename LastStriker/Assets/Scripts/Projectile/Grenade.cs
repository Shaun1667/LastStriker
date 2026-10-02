
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float travelTime = 1.2f;
    public int damage = 40;
    public int destroyScoreValue = 200;
    public GameObject explosionPrefab;

    [Header("Trajectory")]
    [Tooltip("Peak height of the arc above the straight line (m).")]
    public float arcHeight = 1.2f;
    [Tooltip("Where the grenade ends up, measured in front of the camera (m).")]
    public float endDistance = 1.8f;
    [Tooltip("Vertical offset of the end point from the camera centre (negative = below the crosshair line).")]
    public float endHeight = -0.35f;

    [Header("Keep On Screen")]
    [Tooltip("Pushes the grenade back inside the camera view so it can always be shot.")]
    public bool keepInView = true;
    [Range(0.5f, 1f)] public float maxViewportY = 0.8f;
    [Range(0f, 0.3f)] public float viewportMarginX = 0.06f;

    Vector3 startPos;
    Vector3 targetPos;
    float timer;
    bool exploded;
    Transform targetPlayer;
    Camera viewCamera;

    public void Launch(Transform player)
    {
        targetPlayer = player;
        viewCamera = player != null ? player.GetComponent<Camera>() : null;
        if (viewCamera == null) viewCamera = Camera.main;
        startPos = transform.position;
        if (viewCamera != null)
        {
            Transform c = viewCamera.transform;
            targetPos = c.position + c.forward * endDistance + c.up * endHeight;
        }
        else
        {
            targetPos = player.position;
        }
        timer = 0f;
    }

    void Update()
    {
        if (exploded) return;
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / travelTime);
        Vector3 p = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * arcHeight;
        if (keepInView) p = ClampToView(p);
        transform.position = p;

        if (t >= 1f)
        {
            Explode(true);
        }
    }

    Vector3 ClampToView(Vector3 p)
    {
        if (viewCamera == null) return p;
        Vector3 vp = viewCamera.WorldToViewportPoint(p);
        if (vp.z <= viewCamera.nearClipPlane) return p;
        bool changed = false;
        if (vp.y > maxViewportY) { vp.y = maxViewportY; changed = true; }
        if (vp.x < viewportMarginX) { vp.x = viewportMarginX; changed = true; }
        if (vp.x > 1f - viewportMarginX) { vp.x = 1f - viewportMarginX; changed = true; }
        return changed ? viewCamera.ViewportToWorldPoint(vp) : p;
    }

    public void Shoot()
    {
        if (exploded) return;
        Explode(false);
    }

    void Explode(bool hitPlayer)
    {
        exploded = true;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("explosion");

        if (hitPlayer)
        {
            if (targetPlayer != null)
            {
                PlayerHealth ph = targetPlayer.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(damage, true);
            }
        }
        else
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(destroyScoreValue);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            GameObject fx = new GameObject("ExplosionFX");
            fx.transform.position = transform.position;
            fx.AddComponent<Explosion>();
        }

        Destroy(gameObject);
    }
}
