using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Construye el HUD por código (sin assets de UI) y lo refresca de forma reactiva:
///  - Corazones de vida (arriba izquierda)
///  - Indicador del elemento activo de la espada (arriba centro)
///  - Ranuras de runas recolectadas (arriba derecha)
/// Se autocrea en cada escena.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    private Image[] hearts;
    private Image[] runeSlots;
    private Image elementIcon;
    private Text elementText;
    private Sprite squareSprite;
    private Font font;

    private readonly string[] runeOrder = { "Pira", "Isa", "Steinn", "Thorn" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("HUD");
            go.AddComponent<HUDManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        squareSprite = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

        BuildUI();
    }

    private void Update()
    {
        Refresh();
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("HUDCanvas");
        canvasGO.transform.SetParent(transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Corazones (arriba izquierda)
        hearts = new Image[3];
        for (int i = 0; i < 3; i++)
            hearts[i] = MakeIcon(canvasGO.transform, new Vector2(0, 1),
                new Vector2(70 + i * 80, -60), 64, Color.red);

        // Indicador de elemento (arriba centro)
        elementIcon = MakeIcon(canvasGO.transform, new Vector2(0.5f, 1),
            new Vector2(0, -60), 72, Color.white);

        var txtGO = new GameObject("ElementText");
        txtGO.transform.SetParent(canvasGO.transform);
        elementText = txtGO.AddComponent<Text>();
        elementText.font = font;
        elementText.fontSize = 30;
        elementText.fontStyle = FontStyle.Bold;
        elementText.alignment = TextAnchor.MiddleCenter;
        elementText.color = Color.white;
        var rt = elementText.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -110);
        rt.sizeDelta = new Vector2(360, 44);

        // Ranuras de runas (arriba derecha)
        runeSlots = new Image[4];
        for (int i = 0; i < 4; i++)
            runeSlots[i] = MakeIcon(canvasGO.transform, new Vector2(1, 1),
                new Vector2(-70 - i * 80, -60), 64, PlayerStats.ColorOf((ElementType)i));
    }

    private Image MakeIcon(Transform parent, Vector2 anchor, Vector2 pos, float size, Color color)
    {
        var go = new GameObject("Icon");
        go.transform.SetParent(parent);
        var img = go.AddComponent<Image>();
        img.sprite = squareSprite;
        img.color = color;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = pos;
        return img;
    }

    private void Refresh()
    {
        var ps = PlayerStats.Instance;
        if (ps != null && hearts != null)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                bool alive = i < ps.CurrentLives;
                hearts[i].color = alive
                    ? new Color(1f, 0.15f, 0.2f)
                    : new Color(0.18f, 0.18f, 0.18f, 0.6f);
            }
            if (elementIcon != null) elementIcon.color = PlayerStats.ColorOf(ps.CurrentElement);
            if (elementText != null) elementText.text = ps.CurrentElement.ToString();
        }

        if (ps != null && runeSlots != null)
        {
            for (int i = 0; i < runeSlots.Length; i++)
            {
                bool unlocked = ps.IsUnlocked((ElementType)i);
                Color c = PlayerStats.ColorOf((ElementType)i);
                c.a = unlocked ? 1f : 0.22f;
                runeSlots[i].color = c;
            }
        }
    }
}
