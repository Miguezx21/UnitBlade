using UnityEngine;

/// <summary>
/// Puzzle elemental: una barrera/cristal que solo se disipa cuando el jugador
/// la toca teniendo el elemento correcto equipado en la Unit Blade.
/// Demuestra la "salsa" mecánica (transmutación obligatoria).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ElementalPuzzle : MonoBehaviour
{
    [Tooltip("Elemento necesario para disipar la barrera.")]
    public ElementType requiredElement = ElementType.Isa;

    [Tooltip("Objeto visual de la barrera. Vacío = este mismo objeto.")]
    public GameObject visual;

    private bool solved;

    private void Start()
    {
        ApplyTint(0.8f);
    }

    private void OnTriggerEnter2D(Collider2D other) { TrySolve(other); }
    private void OnTriggerStay2D(Collider2D other) { TrySolve(other); }

    private void TrySolve(Collider2D other)
    {
        if (solved) return;
        if (!other.CompareTag("Player")) return;
        if (PlayerStats.Instance == null) return;

        if (PlayerStats.Instance.CurrentElement == requiredElement)
        {
            solved = true;
            ApplyTint(0.08f);
            // desactiva TODOS los colliders (sólido + trigger) para dejar pasar
            foreach (var col in GetComponents<Collider2D>())
                col.enabled = false;
            Debug.Log("[UnitBlade] Barrera disipada con " + requiredElement + ".");
        }
    }

    private void ApplyTint(float alpha)
    {
        var t = visual != null ? visual : gameObject;
        var sr = t.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = PlayerStats.ColorOf(requiredElement);
            c.a = alpha;
            sr.color = c;
        }
    }
}
