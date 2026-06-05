using System.Collections;
using UnityEngine;

/// <summary>
/// Efecto visual de rayo que cae del cielo generado por código.
/// MorgathBoss lo instancia via Animation Event.
/// </summary>
public class LightningStrike : MonoBehaviour
{
    /// <summary>
    /// Crea el efecto completo: alerta en suelo → rayo del cielo → impacto.
    /// </summary>
    public static void Spawn(Vector3 targetPos, float warningDuration, int damage, Transform player)
    {
        var go = new GameObject("LightningStrike");
        var ls = go.AddComponent<LightningStrike>();
        ls.StartCoroutine(ls.Sequence(targetPos, warningDuration, damage, player));
    }

    private IEnumerator Sequence(Vector3 targetPos, float warningDuration, int damage, Transform player)
    {
        // ── 1. Alerta en el suelo (círculo parpadeante) ────
        var warning = CreateWarningCircle(targetPos);
        yield return Blink(warning, warningDuration);
        Destroy(warning);

        // ── 2. Rayo cae del cielo ──────────────────────────
        var bolt = CreateBolt(targetPos);
        yield return Flash(bolt, 0.12f);
        Destroy(bolt);

        // ── 3. Destello de impacto ─────────────────────────
        var impact = CreateImpact(targetPos);
        yield return Flash(impact, 0.1f);
        Destroy(impact);

        // ── 4. Daño al jugador si está cerca ───────────────
        if (player != null && Vector2.Distance(player.position, targetPos) < 1.5f)
        {
            PlayerStats.Instance?.TakeDamage(damage);
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 6f);
        }

        Destroy(gameObject);
    }

    // ── Círculo de alerta en el suelo ──────────────────────
    private GameObject CreateWarningCircle(Vector3 pos)
    {
        var go = new GameObject("Warning");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeCircleSprite(64);
        sr.color        = new Color(1f, 0.3f, 0f, 0.7f);
        sr.sortingOrder = 5;
        go.transform.localScale = new Vector3(2.5f, 0.4f, 1f);
        return go;
    }

    // ── Rayo vertical ──────────────────────────────────────
    private GameObject CreateBolt(Vector3 targetPos)
    {
        var go = new GameObject("Bolt");

        // Empieza desde arriba de la pantalla hasta el suelo
        float height = 10f;
        go.transform.position = targetPos + Vector3.up * (height / 2f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeBoxSprite();
        sr.color        = new Color(0.9f, 0.95f, 1f, 0.95f); // blanco azulado
        sr.sortingOrder = 10;
        go.transform.localScale = new Vector3(0.3f, height, 1f);

        // Borde exterior más ancho y semitransparente
        var glow = new GameObject("Glow");
        glow.transform.SetParent(go.transform, false);
        var srGlow = glow.AddComponent<SpriteRenderer>();
        srGlow.sprite       = MakeBoxSprite();
        srGlow.color        = new Color(0.5f, 0.7f, 1f, 0.35f);
        srGlow.sortingOrder = 9;
        glow.transform.localScale = new Vector3(3f, 1f, 1f);

        return go;
    }

    // ── Destello de impacto ────────────────────────────────
    private GameObject CreateImpact(Vector3 pos)
    {
        var go = new GameObject("Impact");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeCircleSprite(64);
        sr.color        = new Color(1f, 1f, 0.6f, 0.9f);
        sr.sortingOrder = 11;
        go.transform.localScale = new Vector3(2f, 2f, 1f);
        return go;
    }

    // ── Corrutinas de animación ────────────────────────────
    private IEnumerator Blink(GameObject target, float duration)
    {
        if (target == null) yield break;
        var sr     = target.GetComponent<SpriteRenderer>();
        float t    = 0f;
        float rate = 0.12f;
        bool  vis  = true;

        while (t < duration)
        {
            t    += rate;
            vis   = !vis;
            rate  = Mathf.Lerp(0.12f, 0.04f, t / duration); // acelera al final
            if (sr != null)
                sr.color = vis
                    ? new Color(1f, 0.3f, 0f, 0.85f)
                    : new Color(1f, 0.9f, 0f, 0.25f);
            yield return new WaitForSeconds(rate);
        }
    }

    private IEnumerator Flash(GameObject target, float duration)
    {
        if (target == null) yield break;
        var sr = target.GetComponent<SpriteRenderer>();
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (sr != null)
            {
                float alpha = 1f - (t / duration);
                var c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            yield return null;
        }
    }

    // ── Generadores de sprites ─────────────────────────────
    private Sprite MakeCircleSprite(int size)
    {
        var tex    = new Texture2D(size, size);
        var center = new Vector2(size / 2f, size / 2f);
        float r    = size / 2f;
        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
            tex.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), center) < r
                ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }

    private Sprite MakeBoxSprite()
    {
        var tex = new Texture2D(4, 4);
        for (int x = 0; x < 4; x++)
        for (int y = 0; y < 4; y++)
            tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4);
    }
}
