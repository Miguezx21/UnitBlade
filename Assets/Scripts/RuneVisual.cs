using UnityEngine;

/// <summary>
/// Feedback visual de una runa: aparece con un "pop", flota suavemente
/// y pulsa su brillo. Se ejecuta automaticamente cuando el objeto se activa.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RuneVisual : MonoBehaviour
{
    [Header("Flotacion")]
    public float floatAmplitude = 0.25f;
    public float floatSpeed = 2f;

    [Header("Brillo (pulso de alpha)")]
    public float pulseSpeed = 3f;
    public float minAlpha = 0.65f;
    public float maxAlpha = 1f;

    [Header("Aparicion")]
    public float spawnPopTime = 0.45f;

    private SpriteRenderer sr;
    private Vector3 baseLocalPos;
    private Vector3 baseScale;
    private float t;
    private float popT;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseLocalPos = transform.localPosition;
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        t = 0f;
        popT = 0f;
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        t += Time.deltaTime;

        // Aparicion tipo "pop" con un pequeno rebote
        if (popT < spawnPopTime)
        {
            popT += Time.deltaTime;
            float k = Mathf.Clamp01(popT / spawnPopTime);
            float overshoot = 1f + 0.25f * Mathf.Sin(k * Mathf.PI);
            transform.localScale = baseScale * (k * overshoot);
        }
        else
        {
            transform.localScale = baseScale;
        }

        // Flotacion vertical
        float y = Mathf.Sin(t * floatSpeed) * floatAmplitude;
        transform.localPosition = baseLocalPos + new Vector3(0f, y, 0f);

        // Pulso de brillo (solo alpha, conserva el color elemental)
        if (sr != null)
        {
            float p = (Mathf.Sin(t * pulseSpeed) + 1f) * 0.5f;
            Color c = sr.color;
            c.a = Mathf.Lerp(minAlpha, maxAlpha, p);
            sr.color = c;
        }
    }
}
