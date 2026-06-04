using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

/// <summary>
/// Construye el AnimatorController de Kaelen a partir de los clips del .ase
/// (Idle, Walking, Jump, Dead, Attack) y lo asigna al Kaelen de la escena.
/// </summary>
public static class KaelenSetup
{
    const string ASE = "Assets/Art/Characters/Kaelen/KAELENfinal.ase";
    const string CTRL = "Assets/Animations/Kaelen/Kaelen_Aseprite.controller";

    [MenuItem("Tools/UnitBlade/Configurar Kaelen (Aseprite)")]
    public static void Setup()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(ASE);
        if (assets == null || assets.Length == 0)
        {
            Debug.LogError("[KaelenSetup] No se encontró " + ASE +
                " importado. Asegúrate de que el paquete '2D Aseprite' esté instalado y el .ase importado.");
            return;
        }

        var clips = new Dictionary<string, AnimationClip>();
        Sprite firstSprite = null;
        foreach (var a in assets)
        {
            if (a is AnimationClip c) clips[c.name] = c;
            if (a is Sprite s && firstSprite == null) firstSprite = s;
        }

        AnimationClip Get(params string[] names)
        {
            foreach (var n in names) if (clips.ContainsKey(n)) return clips[n];
            return null;
        }

        var idle = Get("Idle");
        var walk = Get("Walking", "Walk");
        var jump = Get("Jump");
        var dead = Get("Dead", "Death");
        var attack = Get("Attack");

        if (idle == null)
        {
            Debug.LogError("[KaelenSetup] No se encontraron clips en el .ase. Clips hallados: " + clips.Count);
            return;
        }

        var ac = AnimatorController.CreateAnimatorControllerAtPath(CTRL);
        ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ac.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ac.AddParameter("IsAttacking", AnimatorControllerParameterType.Bool);
        ac.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

        var sm = ac.layers[0].stateMachine;

        var sIdle = sm.AddState("Idle"); sIdle.motion = idle;
        sm.defaultState = sIdle;
        var sWalk = sm.AddState("Walk"); sWalk.motion = walk != null ? walk : idle;

        var t1 = sIdle.AddTransition(sWalk);
        t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        t1.hasExitTime = false; t1.duration = 0.05f;
        var t2 = sWalk.AddTransition(sIdle);
        t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        t2.hasExitTime = false; t2.duration = 0.05f;

        if (jump != null)
        {
            var sJump = sm.AddState("Jump"); sJump.motion = jump;
            var aj = sm.AddAnyStateTransition(sJump);
            aj.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            aj.duration = 0; aj.canTransitionToSelf = false;
            var jb = sJump.AddTransition(sIdle);
            jb.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            jb.hasExitTime = false; jb.duration = 0.05f;
        }

        if (attack != null)
        {
            var sAtk = sm.AddState("Attack"); sAtk.motion = attack;
            var aa = sm.AddAnyStateTransition(sAtk);
            aa.AddCondition(AnimatorConditionMode.If, 0, "IsAttacking");
            aa.duration = 0; aa.canTransitionToSelf = false;
            var ab = sAtk.AddTransition(sIdle);
            ab.hasExitTime = true; ab.exitTime = 0.9f; ab.duration = 0.05f;
        }

        if (dead != null)
        {
            var sDead = sm.AddState("Dead"); sDead.motion = dead;
            var ad = sm.AddAnyStateTransition(sDead);
            ad.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
            ad.duration = 0; ad.canTransitionToSelf = false;
        }

        AssetDatabase.SaveAssets();

        // Asignar a Kaelen en la escena activa
        var kael = GameObject.Find("Kaelen");
        if (kael != null)
        {
            var sr = kael.GetComponent<SpriteRenderer>();
            if (sr != null && firstSprite != null) sr.sprite = firstSprite;
            var an = kael.GetComponent<Animator>();
            if (an != null) an.runtimeAnimatorController = ac;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[KaelenSetup] Controller creado y asignado a Kaelen. Guarda con Ctrl+S.");
        }
        else
        {
            Debug.LogWarning("[KaelenSetup] Controller creado, pero no hay 'Kaelen' en la escena. " +
                "Asígnalo manualmente al Animator de Kaelen: " + CTRL);
        }
    }
}
