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

    public void SetElement(ElementType e)
    {
        CurrentElement = e;
        OnChanged?.Invoke();
    }

    public void CycleElement()
    {
        CurrentElement = (ElementType)(((int)CurrentElement + 1) % 4);
        OnChanged?.Invoke();
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
