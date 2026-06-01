using UnityEngine;

/// <summary>
/// Al entrar el jugador en el trigger de la runa, la registra en GameProgress
/// y desaparece. Requiere un Collider2D marcado como Trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RunePickup : MonoBehaviour
{
    [Tooltip("Identificador de la runa: Pira, Isa, Steinn o Thorn.")]
    public string runeId = "Pira";

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameProgress.Instance != null)
            GameProgress.Instance.CollectRune(runeId);

        // Aqui se podria instanciar un VFX de recoleccion.
        Destroy(gameObject);
    }
}
