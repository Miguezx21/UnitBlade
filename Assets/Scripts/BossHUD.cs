using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida del boss. Se autocrea en runtime y se muestra
/// solo cuando hay un boss activo.
/// </summary>
public class BossHUD : MonoBehaviour
{
    public static BossHUD Instance { get; private set; }

    private Canvas     _canvas;
    private Image      _barFill;
    private Text       _nameText;
    private int        _maxHP;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<BossHUD>() != null) return;
        var go = new GameObject("BossHUD");
        go.AddComponent<BossHUD>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        _canvas.enabled = false;
    }

    // ── API pública ────────────────────────────────────────

    public void Show(string bossName, int maxHP)
    {
        _maxHP = maxHP;
        if (_nameText != null) _nameText.text = bossName.ToUpper();
        if (_barFill   != null) _barFill.fillAmount = 1f;
        _canvas.enabled = true;
    }

    public void UpdateHP(int currentHP)
    {
        if (_barFill == null) return;
        _barFill.fillAmount = (float)currentHP / _maxHP;
    }

    public void Hide() => _canvas.enabled = false;

    // ── Construcción de la UI ──────────────────────────────

    private void BuildUI()
    {
        var canvasGO = new GameObject("BossHUDCanvas");
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 150;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Fondo de la barra
        var bgGO = new GameObject("BarBG");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        var bgRt = bg.rectTransform;
        bgRt.anchorMin = new Vector2(0.1f, 0f);
        bgRt.anchorMax = new Vector2(0.9f, 0f);
        bgRt.pivot     = new Vector2(0.5f, 0f);
        bgRt.sizeDelta = new Vector2(0, 28);
        bgRt.anchoredPosition = new Vector2(0, 24);

        // Relleno de la barra
        var fillGO = new GameObject("BarFill");
        fillGO.transform.SetParent(bgGO.transform, false);
        _barFill = fillGO.AddComponent<Image>();
        _barFill.color = new Color(0.8f, 0.1f, 0.8f);
        _barFill.type  = Image.Type.Filled;
        _barFill.fillMethod = Image.FillMethod.Horizontal;
        _barFill.fillAmount = 1f;
        var fillRt = _barFill.rectTransform;
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(3, 3);
        fillRt.offsetMax = new Vector2(-3, -3);

        // Nombre del boss
        var txtGO = new GameObject("BossName");
        txtGO.transform.SetParent(canvasGO.transform, false);
        _nameText = txtGO.AddComponent<Text>();
        _nameText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _nameText.fontSize  = 22;
        _nameText.fontStyle = FontStyle.Bold;
        _nameText.alignment = TextAnchor.MiddleCenter;
        _nameText.color     = new Color(0.9f, 0.5f, 1f);
        var txtRt = _nameText.rectTransform;
        txtRt.anchorMin = new Vector2(0.1f, 0f);
        txtRt.anchorMax = new Vector2(0.9f, 0f);
        txtRt.pivot     = new Vector2(0.5f, 0f);
        txtRt.sizeDelta = new Vector2(0, 24);
        txtRt.anchoredPosition = new Vector2(0, 54);
    }
}
