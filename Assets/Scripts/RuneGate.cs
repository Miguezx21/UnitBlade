using UnityEngine;

/// <summary>
/// Puerta/barrera que solo se abre si el jugador ya posee la runa requerida.
/// Implementa la progresion: cada nivel exige las runas de los anteriores.
/// </summary>
public class RuneGate : MonoBehaviour
{
    [Tooltip("Runa necesaria para abrir: Pira, Isa, Steinn, Thorn. Vacio = sin requisito.")]
    public string requiredRune = "";

    [Tooltip("Objeto visual de la barrera (se atenua al abrir). Vacio = este mismo objeto.")]
    public GameObject gateVisual;

    private Collider2D col;

    private void Start()
    {
        col = GetComponent<Collider2D>();

        if (!string.IsNullOrEmpty(requiredRune) &&
            GameProgress.Instance != null &&
            GameProgress.Instance.HasRune(requiredRune))
        {
            Open();
        }
    }

    public void Open()
    {
        if (col != null) col.enabled = false;

        var target = gateVisual != null ? gateVisual : gameObject;
        var sr = target.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.15f;
            sr.color = c;
        }
        Debug.Log("[UnitBlade] Puerta abierta (runa " + requiredRune + ").");
    }
}
