using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Se coloca sobre la propia runa. Mantiene la runa OCULTA (sin renderer ni
/// collider) hasta que mueren todos los enemigos asignados; entonces la revela.
/// A diferencia de desactivar el GameObject, este componente sigue corriendo
/// porque el objeto permanece activo. La lista de enemigos la asigna la
/// herramienta "Configurar Runas por Cercanía".
/// </summary>
public class RuneRevealTracker : MonoBehaviour
{
    [Tooltip("Enemigos que deben morir para revelar esta runa.")]
    public List<GameObject> enemies = new List<GameObject>();

    [Tooltip("Si no hay enemigos asignados, mostrar la runa de inmediato.")]
    public bool revealIfEmpty = false;

    private bool _revealed;
    private bool _armed;
    private SpriteRenderer _sr;
    private Collider2D _col;
    private RuneVisual _visual;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _visual = GetComponent<RuneVisual>();

        _armed = enemies != null && enemies.Count > 0;
        SetVisible(_armed ? false : revealIfEmpty);

        Debug.Log($"[RuneReveal] '{name}' vigilando {(enemies != null ? enemies.Count : 0)} enemigos. " +
                  $"armed={_armed}. (Si armed=False, no se le asignaron enemigos: revisa tags 'Enemy' y vuelve a correr la herramienta.)");
    }

    private void Update()
    {
        if (_revealed || !_armed) return;

        foreach (var e in enemies)
            if (e != null && e.activeInHierarchy) return; // aún quedan vivos

        _revealed = true;
        SetVisible(true);
        Debug.Log("[UnitBlade] Runa revelada: " + name);
    }

    private void SetVisible(bool v)
    {
        if (_sr != null) _sr.enabled = v;
        if (_col != null) _col.enabled = v;
        if (_visual != null) _visual.enabled = v; // al activarse, dispara el "pop"
    }
}
