using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

/// <summary>
/// El .ase de Kaelen NO tiene tags, así que Unity lo importa como un único clip
/// con TODOS los frames (de ahí el "loop de todas las animaciones").
/// Esta herramienta corta los frames sueltos (Frame_0..Frame_N) en clips
/// separados (Idle/Walk/Jump/Attack/Hit/Dead), construye el AnimatorController
/// con transiciones correctas y lo asigna al Kaelen de la escena activa.
///
/// >>> Si alguna animación no calza, ajusta los rangos de abajo y vuelve a ejecutar. <<<
/// Rangos = índice de frame inicial y final (INCLUSIVE).
/// </summary>
public static class KaelenSetup
{
    const string ASE      = "Assets/Art/Characters/Kaelen/KAELENfinal.ase";
    const string CLIP_DIR = "Assets/Animations/Kaelen/Clips";
    const string CTRL     = "Assets/Animations/Kaelen/Kaelen_Aseprite.controller";
    const float  FPS      = 10f;

    // ── Rangos de frames por animación (AJUSTABLES) ──────────────────────────
    // (start, end) inclusivos sobre los Frame_0..Frame_23 del .ase.
    static readonly (string name, int start, int end, bool loop)[] RANGES =
    {
        ("Idle",   0,  3,  true),
        ("Walk",   4,  8,  true),
        ("Hit",    9,  11, false),
        ("Attack", 12, 15, false),
        ("Jump",   16, 19, false),
        ("Dead",   20, 23, false),
    };

    [MenuItem("Tools/UnitBlade/Configurar Kaelen (Aseprite)")]
    public static void Setup()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(ASE);
        if (assets == null || assets.Length == 0)
        {
            Debug.LogError("[KaelenSetup] No se encontró/importó " + ASE +
                ". Verifica que el paquete '2D Aseprite' esté instalado.");
            return;
        }

        // Recolectar sprites "Frame_N" ordenados por N.
        var sprites = assets.OfType<Sprite>()
            .Where(s => s.name.StartsWith("Frame_"))
            .OrderBy(s => ParseIndex(s.name))
            .ToList();

        if (sprites.Count == 0)
        {
            // Fallback: cualquier sprite, en el orden que venga.
            sprites = assets.OfType<Sprite>().ToList();
        }

        if (sprites.Count == 0)
        {
            Debug.LogError("[KaelenSetup] El .ase no generó sprites. Revisa el import (importHiddenLayers).");
            return;
        }

        Debug.Log($"[KaelenSetup] {sprites.Count} frames detectados.");

        // Crear carpeta de clips.
        EnsureFolder(CLIP_DIR);

        // Construir un clip por rango.
        var clips = new Dictionary<string, AnimationClip>();
        foreach (var r in RANGES)
        {
            int start = Mathf.Clamp(r.start, 0, sprites.Count - 1);
            int end   = Mathf.Clamp(r.end,   0, sprites.Count - 1);
            if (end < start) (start, end) = (end, start);

            var clip = BuildClip(r.name, sprites, start, end, r.loop);
            clips[r.name] = clip;
        }

        // ── AnimatorController (REUTILIZA el asset para conservar su GUID) ────
        // Así TODAS las escenas que referencian este controller se actualizan
        // a la vez, sin tener que re-asignar Kaelen en cada escena.
        var ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(CTRL);
        if (ac == null)
        {
            ac = AnimatorController.CreateAnimatorControllerAtPath(CTRL);
        }
        else
        {
            // Limpiar parámetros y estados previos.
            for (int i = ac.parameters.Length - 1; i >= 0; i--)
                ac.RemoveParameter(ac.parameters[i]);
            var sm0 = ac.layers[0].stateMachine;
            foreach (var t in sm0.anyStateTransitions.ToArray())
                sm0.RemoveAnyStateTransition(t);
            foreach (var st in sm0.states.ToArray())
                sm0.RemoveState(st.state);
        }
        ac.AddParameter("Speed",       AnimatorControllerParameterType.Float);
        ac.AddParameter("IsGrounded",  AnimatorControllerParameterType.Bool);
        ac.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
        ac.AddParameter("IsDead",      AnimatorControllerParameterType.Bool);
        ac.AddParameter("JumpTriger",  AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Hit",         AnimatorControllerParameterType.Trigger);

        var sm = ac.layers[0].stateMachine;

        var sIdle = sm.AddState("Idle"); sIdle.motion = clips["Idle"];
        sm.defaultState = sIdle;
        var sWalk = sm.AddState("Walk"); sWalk.motion = clips["Walk"];
        var sJump = sm.AddState("Jump"); sJump.motion = clips["Jump"];
        var sAtk  = sm.AddState("Attack"); sAtk.motion = clips["Attack"];
        var sHit  = sm.AddState("Hit"); sHit.motion = clips["Hit"];
        var sDead = sm.AddState("Dead"); sDead.motion = clips["Dead"];

        // Idle <-> Walk (por velocidad)
        var iw = sIdle.AddTransition(sWalk);
        iw.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        iw.hasExitTime = false; iw.duration = 0.05f;
        var wi = sWalk.AddTransition(sIdle);
        wi.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        wi.hasExitTime = false; wi.duration = 0.05f;

        // Jump por TRIGGER (one-shot) → evita el loop continuo.
        var anyJump = sm.AddAnyStateTransition(sJump);
        anyJump.AddCondition(AnimatorConditionMode.If, 0, "JumpTriger");
        anyJump.duration = 0.02f; anyJump.canTransitionToSelf = false;
        // Jump → Idle cuando vuelve a tocar suelo.
        var jumpEnd = sJump.AddTransition(sIdle);
        jumpEnd.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        jumpEnd.hasExitTime = false; jumpEnd.duration = 0.05f;

        // Attack mientras IsAttacking; sale solo al terminar / soltar.
        var anyAtk = sm.AddAnyStateTransition(sAtk);
        anyAtk.AddCondition(AnimatorConditionMode.If, 0, "IsAttacking");
        anyAtk.duration = 0.02f; anyAtk.canTransitionToSelf = false;
        var atkEnd = sAtk.AddTransition(sIdle);
        atkEnd.AddCondition(AnimatorConditionMode.IfNot, 0, "IsAttacking");
        atkEnd.hasExitTime = true; atkEnd.exitTime = 0.8f; atkEnd.duration = 0.05f;

        // Hit por trigger, vuelve a Idle al terminar.
        var anyHit = sm.AddAnyStateTransition(sHit);
        anyHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        anyHit.duration = 0.02f; anyHit.canTransitionToSelf = false;
        var hitEnd = sHit.AddTransition(sIdle);
        hitEnd.hasExitTime = true; hitEnd.exitTime = 0.9f; hitEnd.duration = 0.05f;

        // Dead: estado final (sin salida).
        var anyDead = sm.AddAnyStateTransition(sDead);
        anyDead.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
        anyDead.duration = 0.02f; anyDead.canTransitionToSelf = false;

        EditorUtility.SetDirty(ac);
        AssetDatabase.SaveAssets();

        // ── Asignar a Kaelen en la escena activa ─────────────────────────────
        var kael = GameObject.Find("Kaelen");
        if (kael != null)
        {
            var sr = kael.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = sprites[0];
            var an = kael.GetComponent<Animator>();
            if (an == null) an = kael.AddComponent<Animator>();
            an.runtimeAnimatorController = ac;
            EditorUtility.SetDirty(kael);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[KaelenSetup] OK: clips + controller creados y asignados a Kaelen. Guarda con Ctrl+S.");
        }
        else
        {
            Debug.LogWarning("[KaelenSetup] Controller listo, pero no hay 'Kaelen' en la escena. " +
                "Asigna manualmente " + CTRL + " al Animator de Kaelen.");
        }
    }

    static AnimationClip BuildClip(string name, List<Sprite> sprites, int start, int end, bool loop)
    {
        var clip = new AnimationClip { frameRate = FPS };

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        int count = end - start + 1;
        var keys = new ObjectReferenceKeyframe[count];
        for (int i = 0; i < count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time  = i / FPS,
                value = sprites[start + i]
            };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string path = $"{CLIP_DIR}/Kaelen_{name}.anim";
        if (System.IO.File.Exists(path)) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static int ParseIndex(string n)
    {
        int us = n.LastIndexOf('_');
        return (us >= 0 && int.TryParse(n.Substring(us + 1), out int v)) ? v : 0;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}
