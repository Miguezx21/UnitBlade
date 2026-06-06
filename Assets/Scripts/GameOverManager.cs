using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Maneja la pantalla de Game Over.
/// Se autoinstancia y escucha a PlayerStats para saber cuándo Kaelen muere.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Tiempos")]
    public float deathAnimDuration = 1.5f; // tiempo para que termine la animación IsDead
    public float gameOverDelay     = 1f;   // pausa antes de mostrar el panel

    private Canvas    _canvas;
    private CanvasGroup _panel;
    private bool _triggered;
    private bool _menuActive; // panel visible y aceptando entrada

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<GameOverManager>() != null) return;
        var go = new GameObject("GameOverManager");
        go.AddComponent<GameOverManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        _triggered = false;
        _menuActive = false;
        if (_panel != null)
        {
            _panel.alpha          = 0f;
            _panel.interactable   = false;
            _panel.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        // Fallback de teclado: si el panel está activo, Enter/Espacio = Reintentar.
        if (!_menuActive) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.enterKey.wasPressedThisFrame
            || kb.numpadEnterKey.wasPressedThisFrame
            || kb.spaceKey.wasPressedThisFrame)
            OnRestart();
    }

    // ── API pública ────────────────────────────────────────

    /// <summary>Llamado por PlayerController2D cuando las vidas llegan a 0.</summary>
    public void TriggerGameOver()
    {
        if (_triggered) return;
        _triggered = true;
        StartCoroutine(GameOverSequence());
    }

    // ── Secuencia ──────────────────────────────────────────

    private IEnumerator GameOverSequence()
    {
        // Espera a que termine la animación de muerte
        yield return new WaitForSeconds(deathAnimDuration);

        // Pausa breve antes de mostrar el panel
        yield return new WaitForSeconds(gameOverDelay);

        // Fade in del panel
        yield return StartCoroutine(FadePanel(0f, 1f, 0.5f));

        _panel.interactable   = true;
        _panel.blocksRaycasts = true;
        _menuActive = true; // habilita clic y teclado (Enter/Espacio)
    }

    // ── Botones ────────────────────────────────────────────

    public void OnRestart()
    {
        _menuActive = false;
        if (PlayerStats.Instance != null) PlayerStats.Instance.Revive();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMenu()
    {
        _menuActive = false;
        if (PlayerStats.Instance != null) PlayerStats.Instance.Revive();
        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            SceneManager.LoadScene("MainMenu");
        else
            SceneManager.LoadScene(0);
    }

    // ── UI generada en runtime ─────────────────────────────

    private void BuildUI()
    {
        var canvasGO = new GameObject("GameOverCanvas");
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Fondo semitransparente
        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        _panel = panelGO.AddComponent<CanvasGroup>();

        var bg = panelGO.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        var rt = bg.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        // Texto "GAME OVER"
        var txtGO = new GameObject("GameOverText");
        txtGO.transform.SetParent(panelGO.transform, false);
        var txt = txtGO.AddComponent<Text>();
        txt.text      = "GAME OVER";
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 72;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color     = new Color(0.9f, 0.1f, 0.1f);
        var txtRt = txt.rectTransform;
        txtRt.anchorMin = txtRt.anchorMax = new Vector2(0.5f, 0.6f);
        txtRt.pivot     = new Vector2(0.5f, 0.5f);
        txtRt.sizeDelta = new Vector2(600, 100);
        txtRt.anchoredPosition = Vector2.zero;

        // Botones
        MakeButton(panelGO.transform, "Reintentar", new Vector2(0.5f, 0.42f), OnRestart);
        MakeButton(panelGO.transform, "Menú Principal", new Vector2(0.5f, 0.30f), OnMenu);

        // Pista de teclado
        var hintGO = new GameObject("Hint");
        hintGO.transform.SetParent(panelGO.transform, false);
        var hint = hintGO.AddComponent<Text>();
        hint.text = "(Enter o Espacio para reintentar)";
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 22;
        hint.alignment = TextAnchor.MiddleCenter;
        hint.color = new Color(0.8f, 0.8f, 0.8f);
        var hrt = hint.rectTransform;
        hrt.anchorMin = hrt.anchorMax = new Vector2(0.5f, 0.18f);
        hrt.pivot = new Vector2(0.5f, 0.5f);
        hrt.sizeDelta = new Vector2(600, 40);
        hrt.anchoredPosition = Vector2.zero;

        // Oculto al inicio
        _panel.alpha          = 0f;
        _panel.interactable   = false;
        _panel.blocksRaycasts = false;

        // EventSystem necesario para que los botones respondan (clic de ratón).
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var module = es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            // Asignar las acciones por defecto (si no, el clic no registra en runtime).
            var mi = module.GetType().GetMethod("AssignDefaultActions");
            if (mi != null) mi.Invoke(module, null);
            DontDestroyOnLoad(es);
        }
    }

    private void MakeButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + "Btn");
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.sizeDelta = new Vector2(260, 60);
        rt.anchoredPosition = Vector2.zero;

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var txt = txtGO.AddComponent<Text>();
        txt.text      = label;
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = 32;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color     = Color.white;
        var trt = txt.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
    }

    private IEnumerator FadePanel(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            _panel.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        _panel.alpha = to;
    }
}
