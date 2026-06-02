using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controles temporales de prueba (mientras no exista el sistema de combate):
///   1/2/3/4 = elemento Pira/Isa/Steinn/Thorn
///   Q       = ciclar elemento
///   H       = recibir daño (pierde corazón)
///   G       = curar
///   R       = reiniciar runas recolectadas
/// Se autocrea y persiste entre escenas.
/// </summary>
public class InputTester : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<InputTester>() == null)
        {
            var go = new GameObject("InputTester");
            go.AddComponent<InputTester>();
            DontDestroyOnLoad(go);
        }
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        var ps = PlayerStats.Instance;
        if (ps != null)
        {
            if (kb.digit1Key.wasPressedThisFrame) ps.SetElement(ElementType.Pira);
            if (kb.digit2Key.wasPressedThisFrame) ps.SetElement(ElementType.Isa);
            if (kb.digit3Key.wasPressedThisFrame) ps.SetElement(ElementType.Steinn);
            if (kb.digit4Key.wasPressedThisFrame) ps.SetElement(ElementType.Thorn);
            if (kb.qKey.wasPressedThisFrame) ps.CycleElement();
            if (kb.hKey.wasPressedThisFrame) ps.TakeDamage();
            if (kb.gKey.wasPressedThisFrame) ps.Heal();
        }

        if (kb.rKey.wasPressedThisFrame && GameProgress.Instance != null)
            GameProgress.Instance.ResetProgress();
    }
}
