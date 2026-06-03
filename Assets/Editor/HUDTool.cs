using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Crea el HUD editable en la escena con las imágenes ya asignadas y números 1-4.
/// </summary>
public static class HUDTool
{
    const string HUD_DIR = "Assets/Resources/HUD/";
    static readonly string[] RUNE = { "Pira", "Isa", "Steinn", "Thorn" };

    [MenuItem("Tools/UnitBlade/Crear HUD en Escena")]
    public static void CreateSceneHUD()
    {
        // Limpia HUD y EventSystem previos (evita duplicados y el error de Input System)
        var oldHud = GameObject.Find("HUD");
        if (oldHud != null) Object.DestroyImmediate(oldHud);
        var oldCanvas = GameObject.Find("HUDCanvas");
        if (oldCanvas != null && oldCanvas.transform.parent == null) Object.DestroyImmediate(oldCanvas);
        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es != null) Object.DestroyImmediate(es.gameObject);

        var go = new GameObject("HUD");
        var hud = go.AddComponent<HUDManager>();
        var canvas = hud.BuildUI(go.transform);

        AssignSprites(canvas.transform);

        Undo.RegisterCreatedObjectUndo(go, "Crear HUD");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = go;
        Debug.Log("[HUDTool] HUD creado con imágenes y números. Guarda con Ctrl+S.");
    }

    static Sprite Load(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(HUD_DIR + name + ".png");
    }

    static void AssignSprites(Transform canvas)
    {
        var heart = Load("heart");
        for (int i = 0; i < 3; i++)
        {
            var img = Find(canvas, "Heart" + i);
            if (img != null && heart != null) img.sprite = heart;
        }
        var ei = Find(canvas, "ElementIcon");
        if (ei != null) { var s = Load("Isa"); if (s != null) ei.sprite = s; }
        for (int i = 0; i < 4; i++)
        {
            var img = Find(canvas, "Rune" + i);
            if (img != null) { var s = Load(RUNE[i]); if (s != null) img.sprite = s; }
        }
    }

    static Image Find(Transform canvas, string name)
    {
        var t = canvas.Find(name);
        return t != null ? t.GetComponent<Image>() : null;
    }
}
