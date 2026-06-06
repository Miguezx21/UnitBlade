using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Configura las runas de la escena para que aparezcan al matar a los enemigos
/// CERCANOS (asignación automática por distancia en X). Así puedes añadir o mover
/// enemigos libremente y solo volver a ejecutar esta herramienta.
///
/// - Cada enemigo (tag "Enemy") se asigna a la runa más cercana.
/// - La runa queda oculta hasta que mueren todos sus enemigos asignados.
/// - El runeId se fija según el nombre (Runa_Pira -> "Pira", etc.).
/// </summary>
public static class RuneSetupTool
{
    [MenuItem("Tools/UnitBlade/Configurar Runas por Cercania")]
    public static void Setup()
    {
        // 1) Quitar EnemyGroupTracker antiguos (desactivaban la runa y rompían el flujo).
        foreach (var old in Object.FindObjectsByType<EnemyGroupTracker>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(old);

        // 2) Runas (incluye inactivas).
        var runes = Object.FindObjectsByType<RunePickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (runes.Length == 0)
        {
            Debug.LogWarning("[RuneSetup] No hay runas (RunePickup) en la escena.");
            return;
        }

        // 3) Enemigos activos por tag.
        GameObject[] enemies;
        try { enemies = GameObject.FindGameObjectsWithTag("Enemy"); }
        catch { enemies = new GameObject[0]; Debug.LogWarning("[RuneSetup] No existe el tag 'Enemy'."); }

        // 4) Preparar tracker en cada runa.
        var trackers = new Dictionary<RunePickup, RuneRevealTracker>();
        foreach (var r in runes)
        {
            r.gameObject.SetActive(true);

            string id = r.gameObject.name.Replace("Runa_", "").Replace("Rune_", "").Trim();
            if (!string.IsNullOrEmpty(id)) r.runeId = id;

            var t = r.GetComponent<RuneRevealTracker>();
            if (t == null) t = r.gameObject.AddComponent<RuneRevealTracker>();
            t.enemies = new List<GameObject>();
            trackers[r] = t;
            EditorUtility.SetDirty(r);
        }

        // 5) Asignar cada enemigo a la runa más cercana (en X).
        foreach (var e in enemies)
        {
            RunePickup nearest = null;
            float best = float.MaxValue;
            foreach (var r in runes)
            {
                float d = Mathf.Abs(e.transform.position.x - r.transform.position.x);
                if (d < best) { best = d; nearest = r; }
            }
            if (nearest != null) trackers[nearest].enemies.Add(e);
        }

        // 6) Reporte.
        foreach (var kv in trackers)
            Debug.Log($"[RuneSetup] {kv.Key.name} (id={kv.Key.runeId}) ← {kv.Value.enemies.Count} enemigos asignados.");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[RuneSetup] Listo. Guarda con Ctrl+S. (Vuelve a ejecutar si mueves/añades enemigos.)");
    }
}
