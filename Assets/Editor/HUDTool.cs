using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Crea el HUD como objetos reales en la escena para poder editarlo/quitarlo
/// fuera del modo Play.
/// </summary>
public static class HUDTool
{
    [MenuItem("Tools/UnitBlade/Crear HUD en Escena")]
    public static void CreateSceneHUD()
    {
        if (GameObject.Find("HUDCanvas") != null)
        {
            Debug.Log("[HUDTool] Ya existe un HUDCanvas en la escena.");
            return;
        }

        // EventSystem (necesario para UI; lo crea si falta)
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var go = new GameObject("HUD");
        var hud = go.AddComponent<HUDManager>();
        hud.BuildUI(go.transform);

        Undo.RegisterCreatedObjectUndo(go, "Crear HUD");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = go;
        Debug.Log("[HUDTool] HUD creado en la escena. Ya puedes editarlo o borrarlo. Guarda con Ctrl+S.");
    }
}
