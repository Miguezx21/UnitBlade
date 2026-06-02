using UnityEngine;

/// <summary>
/// Demostración visual del jefe final: Morgath alterna su elemento cada cierto
/// tiempo y se tinta con el color correspondiente. (Regla de oro: solo recibe
/// daño con el elemento OPUESTO; aquí se muestra de forma visual).
/// </summary>
public class MorgathBoss : MonoBehaviour
{
    public float switchInterval = 3f;

    private SpriteRenderer sr;
    private float timer;
    private ElementType current = ElementType.Pira;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Apply();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            current = (ElementType)(((int)current + 1) % 4);
            Apply();
        }
    }

    private void Apply()
    {
        if (sr != null)
        {
            Color c = PlayerStats.ColorOf(current);
            c = Color.Lerp(c, Color.black, 0.35f); // aura oscura
            c.a = 1f;
            sr.color = c;
        }
    }

    /// <summary>El elemento con el que SÍ recibe daño (el opuesto al actual).</summary>
    public ElementType WeakTo()
    {
        return (ElementType)(((int)current + 2) % 4);
    }
}
