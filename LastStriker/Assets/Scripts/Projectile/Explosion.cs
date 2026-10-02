using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    public float duration = 0.25f;
    public float maxScale = 2.5f;
    public Color color = new Color(1f, 0.5f, 0f);

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(transform);
        sphere.transform.localPosition = Vector3.zero;
        Collider col = sphere.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Renderer rend = sphere.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            rend.material = mat;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(0.1f, maxScale, t / duration);
            sphere.transform.localScale = Vector3.one * scale;
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = Mathf.Lerp(1f, 0f, t / duration);
                rend.material.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
