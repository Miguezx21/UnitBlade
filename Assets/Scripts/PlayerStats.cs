using System;
using UnityEngine;

/// <summary>
/// Estado del jugador: vidas (corazones) y elemento activo de la Unit Blade.
/// Singleton persistente que se autocrea antes de cargar la escena.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public int maxLives = 3;
    public int CurrentLives { get; private set; }
    public ElementType CurrentElement { get; private set; } = ElementType.Pira;

    /// <summary>Se dispara cuando cambian vidas o elemento (para refrescar el HUD).</summary>
    public event Action OnChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("PlayerStats");
            go.AddComponent<PlayerStats>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentLives = maxLives;
    }

    public void TakeDamage(int amount = 1)
    {
        CurrentLives = Mathf.Max(0, CurrentLives - amount);
        OnChanged?.Invoke();
        if (CurrentLives == 0)
            Debug.Log("[UnitBlade] Kaelen ha caído.");
    }

    public void Heal(int amount = 1)
    {
        CurrentLives = Mathf.Min(maxLives, CurrentLives + amount);
        OnChanged?.Invoke();
    }

    /// <summary>Pira está disponible desde el inicio; el resto se desbloquea al recoger su runa.</summary>
    public bool IsUnlocked(ElementType e)
    {
        if (e == ElementType.Pira) return true;
        return GameProgress.Instance != null && GameProgress.Instance.HasRune(e.ToString());
    }

    public void SetElement(ElementType e)
    {
        if (!IsUnlocked(e))
        {
            Debug.Log("[UnitBlade] Elemento bloqueado: " + e + " (necesitas su runa).");
            return;
        }
        CurrentElement = e;
        OnChanged?.Invoke();
    }

    public void CycleElement()
    {
        for (int i = 1; i <= 4; i++)
        {
            var next = (ElementType)(((int)CurrentElement + i) % 4);
            if (IsUnlocked(next))
            {
                CurrentElement = next;
                OnChanged?.Invoke();
                return;
            }
        }
    }

    public static Color ColorOf(ElementType e)
    {
        switch (e)
        {
            case ElementType.Pira:   return new Color(1f, 0.30f, 0.10f);
            case ElementType.Isa:    return new Color(0.20f, 0.60f, 1f);
            case ElementType.Steinn: return new Color(0.30f, 0.80f, 0.30f);
            case ElementType.Thorn:  return new Color(1f, 0.90f, 0.10f);
        }
        return Color.white;
    }
}
