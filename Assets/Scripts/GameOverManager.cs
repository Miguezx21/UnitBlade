using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
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
        if (_panel != null)
        {
            _panel.alpha          = 0f;
            _panel.interactable   = false;
            _panel.blocksRaycasts = false;
        }
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
    }

    // ── Botones ────────────────────────────────────────────

    public void OnRestart()
    {
        if (PlayerStats.Instance != null) PlayerStats.Instance.Revive();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

        // Botón Reintentar
        MakeButton(panelGO.transform, "Reintentar", new Vector2(0.5f, 0.4f), OnRestart);

        // Oculto al inicio
        _panel.alpha          = 0f;
        _panel.interactable   = false;
        _panel.blocksRaycasts = false;

        // EventSystem necesario para que los botones respondan
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            // Agrega el módulo correcto según el Input System activo
            var moduleType = System.Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (moduleType != null)
                es.AddComponent(moduleType);
            else
                es.AddComponent<StandaloneInputModule>();
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
