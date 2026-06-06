using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Crea la escena MainMenu con el menú funcional, cablea sprites y sonidos,
/// y la registra como escena 0 en Build Settings.
/// </summary>
public static class MainMenuTool
{
    const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Tools/UnitBlade/Crear Escena Menu Principal")]
    public static void CreateMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.10f);
        cam.orthographic = true;
        camGO.tag = "MainCamera";

        // EventSystem (Input System nuevo)
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        // AudioManager + clips
        var audioGO = new GameObject("AudioManager");
        var audio = audioGO.AddComponent<AudioManager>();
        audio.sfxSword     = LoadClip("Assets/Sounds/MINECRAFT SWORD SOUND EFFECT - FREE.mp3");
        audio.sfxJump      = LoadClip("Assets/Sounds/Roblox jump Sound effect.mp3");
        audio.sfxFire      = LoadClip("Assets/Sounds/Fire Explosion Sound effect  Fire Blast Sound Effect #3 @WsnSolutions.mp3");
        audio.sfxIce       = LoadClip("Assets/Sounds/Ice Crack Freeze Sound Effect.mp3");
        audio.sfxRock      = LoadClip("Assets/Sounds/ROCKS AND STONES IMPACT - (Sound Effect).mp3");
        audio.sfxLightning = LoadClip("Assets/Sounds/Lightning sound  thunder and lightning sound effects.mp3");
        audio.sfxParry     = LoadClip("Assets/Sounds/Parry Sound Effects.mp3");
        // menuMusic / levelMusic / bossMusic se asignan cuando cargues las pistas.

        // Menú
        var menuGO = new GameObject("MainMenu");
        var menu = menuGO.AddComponent<MainMenuController>();
        menu.background = LoadFirstSprite(
            "Assets/Art/Backgrounds/Dark Gothic Castle.png",
            "Assets/Art/Backgrounds/HR_Dark Gothic Castle.png",
            "Assets/Art/Backgrounds/forestbg.jpg",
            "Assets/Art/Backgrounds/backgroundboss.jpg");
        menu.kaelen     = LoadAseSprite("Assets/Art/Characters/Kaelen/KAELENfinal.ase");
        menu.runeThron  = LoadSprite("Assets/Resources/HUD/Thorn.png");
        menu.runePira   = LoadSprite("Assets/Resources/HUD/Pira.png");
        menu.runeIsa    = LoadSprite("Assets/Resources/HUD/Isa.png");
        menu.runeSteinn = LoadSprite("Assets/Resources/HUD/Steinn.png");

        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        RegisterAsFirstScene();

        Debug.Log("[MainMenuTool] MainMenu creado y registrado como escena 0. " +
                  "Asigna las pistas de música al AudioManager cuando las tengas.");
    }

    static void RegisterAsFirstScene()
    {
        var list = new List<EditorBuildSettingsScene>();
        list.Add(new EditorBuildSettingsScene(SCENE_PATH, true));
        foreach (var s in EditorBuildSettings.scenes)
            if (s.path != SCENE_PATH) list.Add(s);
        EditorBuildSettings.scenes = list.ToArray();
    }

    static AudioClip LoadClip(string path)
    {
        var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (c == null) Debug.LogWarning("[MainMenuTool] No se encontró audio: " + path);
        return c;
    }

    static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // Devuelve el primer sprite (Frame_0) de un .ase importado.
    static Sprite LoadAseSprite(string path)
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(path);
        Sprite first = null;
        foreach (var a in all)
        {
            if (a is Sprite s)
            {
                if (s.name == "Frame_0") return s;
                if (first == null) first = s;
            }
        }
        return first;
    }

    static Sprite LoadFirstSprite(params string[] paths)
    {
        foreach (var p in paths)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (s != null) return s;
        }
        Debug.LogWarning("[MainMenuTool] Ningún fondo importado como Sprite; el menú usará color sólido. " +
                         "Marca alguna imagen de Backgrounds como 'Sprite (2D and UI)'.");
        return null;
    }
}
