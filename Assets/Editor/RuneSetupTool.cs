using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Configura las runas para que aparezcan al matar a un TIPO de enemigo.
/// - Agrupa los enemigos por tipo (prefijo del nombre: "Ignis_01" -> "Ignis").
/// - Cada tipo completo se asigna a UNA runa (todos los Ignis a una, todos los
///   Glacius a otra, etc.), eligiendo la runa más cercana al grupo y evitando
///   que dos tipos caigan en la misma runa cuando hay suficientes runas.
/// - La runa queda oculta hasta que muere TODO su tipo de enemigo asignado.
/// Vuelve a ejecutar si añades/mueves enemigos.
/// </summary>
public static class RuneSetupTool
{
    [MenuItem("Tools/UnitBlade/Configurar Runas por Tipo")]
    public static void Setup()
    {
        // 1) Quitar trackers antiguos.
        foreach (var old in Object.FindObjectsByType<EnemyGroupTracker>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(old);

        // 2) Runas (incluye inactivas).
        var runes = Object.FindObjectsByType<RunePickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (runes.Length == 0) { Debug.LogWarning("[RuneSetup] No hay runas (RunePickup) en la escena."); return; }

        // 3) Enemigos por tag.
        GameObject[] enemies;
        try { enemies = GameObject.FindGameObjectsWithTag("Enemy"); }
        catch { Debug.LogWarning("[RuneSetup] No existe el tag 'Enemy'."); return; }
        if (enemies.Length == 0) { Debug.LogWarning("[RuneSetup] No hay objetos con tag 'Enemy'."); }

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

        // 5) Agrupar enemigos por TIPO y calcular su posición media (X).
        var byType = new Dictionary<string, List<GameObject>>();
        foreach (var e in enemies)
        {
            string type = TypeOf(e.name);
            if (!byType.ContainsKey(type)) byType[type] = new List<GameObject>();
            byType[type].Add(e);
        }
        var centroidX = new Dictionary<string, float>();
        foreach (var kv in byType)
        {
            float sx = 0f;
            foreach (var g in kv.Value) sx += g.transform.position.x;
            centroidX[kv.Key] = sx / kv.Value.Count;
        }

        // 6) Asignación tipo -> runa: emparejado por distancia, evitando repetir runa.
        var pairs = new List<(float d, string type, RunePickup rune)>();
        foreach (var type in byType.Keys)
            foreach (var r in runes)
                pairs.Add((Mathf.Abs(centroidX[type] - r.transform.position.x), type, r));
        pairs.Sort((a, b) => a.d.CompareTo(b.d));

        var typeToRune = new Dictionary<string, RunePickup>();
        var usedRunes = new HashSet<RunePickup>();
        foreach (var p in pairs)
        {
            if (typeToRune.ContainsKey(p.type) || usedRunes.Contains(p.rune)) continue;
            typeToRune[p.type] = p.rune;
            usedRunes.Add(p.rune);
        }
        // Tipos sobrantes (más tipos que runas): a la runa más cercana aunque se comparta.
        foreach (var type in byType.Keys)
        {
            if (typeToRune.ContainsKey(type)) continue;
            RunePickup nr = null; float best = float.MaxValue;
            foreach (var r in runes)
            {
                float d = Mathf.Abs(centroidX[type] - r.transform.position.x);
                if (d < best) { best = d; nr = r; }
            }
            typeToRune[type] = nr;
        }

        // 7) Llenar trackers.
        foreach (var kv in byType)
            trackers[typeToRune[kv.Key]].enemies.AddRange(kv.Value);

        // 8) Reporte.
        foreach (var kv in trackers)
        {
            var types = new List<string>();
            foreach (var bt in byType)
                if (typeToRune[bt.Key] == kv.Key) types.Add($"{bt.Key} x{bt.Value.Count}");
            Debug.Log($"[RuneSetup] {kv.Key.name} (id={kv.Key.runeId}) ← {kv.Value.enemies.Count} enemigos " +
                      $"[{(types.Count > 0 ? string.Join(", ", types) : "ninguno")}]");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[RuneSetup] Listo (por tipo). Guarda con Ctrl+S.");
    }

    /// <summary>Extrae el tipo desde el nombre: "Ignis_03 (1)" -> "Ignis".</summary>
    static string TypeOf(string name)
    {
        if (string.IsNullOrEmpty(name)) return "?";
        int cut = name.Length;
        foreach (char sep in new[] { '_', ' ', '(' })
        {
            int i = name.IndexOf(sep);
            if (i >= 0 && i < cut) cut = i;
        }
        return name.Substring(0, cut).Trim();
    }
}
