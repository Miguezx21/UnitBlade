using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lleva el registro de las runas recolectadas por el jugador.
/// Persiste entre escenas (singleton + PlayerPrefs) y se autocrea
/// antes de cargar cualquier escena, por lo que NO necesita colocarse
/// manualmente en la jerarquia.
/// </summary>
public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    private readonly HashSet<string> runes = new HashSet<string>();
    private const string SaveKey = "UB_Runes";

    // Estado de runas al INICIO del nivel actual (para revertir al reintentar).
    private readonly HashSet<string> _levelStartSnapshot = new HashSet<string>();
    private int _lastSceneIndex = -999;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("GameProgress");
            go.AddComponent<GameProgress>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Inicio limpio cada sesión: el jugador comienza sin runas y las recoge jugando.
        PlayerPrefs.DeleteKey(SaveKey);
        runes.Clear();
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        if (s.buildIndex == _lastSceneIndex)
        {
            // Misma escena recargada = REINTENTO: revertir runas al inicio del nivel.
            runes.Clear();
            foreach (var r in _levelStartSnapshot) runes.Add(r);
            Save();
            Debug.Log("[UnitBlade] Reintento: runas restauradas al inicio del nivel (total: " + runes.Count + ").");
        }
        else
        {
            // Nivel nuevo (o avance): guardar el estado actual como punto de partida.
            _levelStartSnapshot.Clear();
            foreach (var r in runes) _levelStartSnapshot.Add(r);
            _lastSceneIndex = s.buildIndex;
        }
    }

    public bool HasRune(string id)
    {
        return !string.IsNullOrEmpty(id) && runes.Contains(id);
    }

    public void CollectRune(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (runes.Add(id))
        {
            Save();
            Debug.Log("[UnitBlade] Runa recolectada: " + id + " (total: " + runes.Count + ")");
        }
    }

    public int RuneCount => runes.Count;

    public void ResetProgress()
    {
        runes.Clear();
        Save();
        Debug.Log("[UnitBlade] Progreso de runas reiniciado.");
    }

    private void Save()
    {
        PlayerPrefs.SetString(SaveKey, string.Join(",", runes));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        runes.Clear();
        string s = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(s)) return;
        foreach (var r in s.Split(','))
            if (!string.IsNullOrEmpty(r)) runes.Add(r);
    }
}
