using System.Collections;
using UnityEngine;

/// <summary>
/// Alerta visual que aparece en el suelo antes de que caiga el rayo de Morgath.
/// Se crea por código, no necesita prefab asignado.
/// </summary>
public class LightningWarning : MonoBehaviour
{
    public float duration = 1f; // ajustable en el prefab o desde código

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null)
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = CreateCircleSprite();
            transform.localScale = new Vector3(3f, 0.5f, 1f);
        }
        _sr.sortingOrder = 10;
    }

    private void Start()
    {
        StartCoroutine(Blink(duration));
    }

    /// <summary>
    /// Crea el warning en la posición dada y lo destruye tras la duración.
    /// </summary>
    public static LightningWarning Spawn(Vector3 position, float duration)
    {
        var go = new GameObject("LightningWarning");
        go.transform.position = position;
        var w = go.AddComponent<LightningWarning>();
        w.StartCoroutine(w.Blink(duration));
        return w;
    }

    private IEnumerator Blink(float duration)
    {
        float elapsed  = 0f;
        float blinkRate = 0.15f; // velocidad del parpadeo
        bool  visible  = true;

        while (elapsed < duration)
        {
            elapsed    += blinkRate;
            visible     = !visible;

            // Parpadeo que se acelera al final para dar urgencia
            float t = elapsed / duration;
            blinkRate = Mathf.Lerp(0.15f, 0.05f, t);

            if (_sr != null)
                _sr.color = visible
                    ? new Color(1f, 0.2f, 0f, 0.85f)  // naranja-rojo visible
                    : new Color(1f, 1f, 0f, 0.3f);     // amarillo tenue

            yield return new WaitForSeconds(blinkRate);
        }

        // Flash blanco final justo antes del golpe
        if (_sr != null) _sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);

        Destroy(gameObject);
    }

    // Genera un sprite de círculo en runtime
    private Sprite CreateCircleSprite()
    {
        int size    = 64;
        var tex     = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius   = size / 2f;

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            float alpha = dist < radius ? 1f : 0f;
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
