using UnityEngine;

/// <summary>
/// Barrera elemental: bloquea el paso (collider sólido) hasta que el jugador
/// se acerca teniendo el elemento correcto equipado. Entonces se disipa y
/// desactiva sus colliders para dejar pasar.
/// Usa detección por distancia (robusto, sin depender de la matriz de física).
/// </summary>
public class ElementalPuzzle : MonoBehaviour
{
    [Tooltip("Elemento necesario para disipar la barrera.")]
    public ElementType requiredElement = ElementType.Pira;

    [Tooltip("Distancia a la que el jugador puede disiparla.")]
    public float activateDistance = 2f;

    [Tooltip("Objeto visual. Vacío = este mismo objeto.")]
    public GameObject visual;

    private bool solved;
    private Transform player;

    private void Start()
    {
        ApplyTint(0.85f);
    }

    private void Update()
    {
        if (solved) return;
        if (PlayerStats.Instance == null) return;
        if (PlayerStats.Instance.CurrentElement != requiredElement) return;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        if (Vector2.Distance(player.position, transform.position) <= activateDistance)
            Solve();
    }

    private void Solve()
    {
        solved = true;
        ApplyTint(0.06f);
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;
        Debug.Log("[UnitBlade] Barrera disipada con " + requiredElement + ".");
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
