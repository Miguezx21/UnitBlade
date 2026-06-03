using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD reactivo: corazones, elemento activo y runas.
/// Puede vivir como objetos REALES en la escena (editables) o autocrearse en
/// runtime si no existe ninguno. Usa Tools/UnitBlade/Crear HUD en Escena para
/// generarlo de forma editable.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    private Image[] hearts;
    private Image[] runeSlots;
    private Text[] runeNums;
    private Image elementIcon;
    private Text elementText;
    private Font font;

    private readonly string[] runeOrder = { "Pira", "Isa", "Steinn", "Thorn" };

    private Sprite heartSprite;
    private Sprite[] elementSprites; // Pira, Isa, Steinn, Thorn
    private bool spritesLoaded;

    private void LoadSprites()
    {
        if (spritesLoaded) return;
        spritesLoaded = true;
        heartSprite = Resources.Load<Sprite>("HUD/heart");
        elementSprites = new Sprite[4];
        for (int i = 0; i < 4; i++)
            elementSprites[i] = Resources.Load<Sprite>("HUD/" + runeOrder[i]);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Si ya hay un HUD colocado en la escena, no creamos otro.
        if (FindFirstObjectByType<HUDManager>() != null) return;
        var go = new GameObject("HUD");
        go.AddComponent<HUDManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Si el HUD ya está armado en la escena, lo usamos; si no, lo construimos.
        if (!BindByName())
            BuildUI(transform);
    }

    private void Update()
    {
        Refresh();
    }

    private bool BindByName()
    {
        Transform canvas = transform.Find("HUDCanvas");
        if (canvas == null)
        {
            var c = GameObject.Find("HUDCanvas");
            if (c != null) canvas = c.transform;
        }
        if (canvas == null) return false;

        hearts = new Image[3];
        runeSlots = new Image[4];
        runeNums = new Text[4];
        for (int i = 0; i < 3; i++)
        {
            var t = canvas.Find("Heart" + i);
            if (t != null) hearts[i] = t.GetComponent<Image>();
        }
        for (int i = 0; i < 4; i++)
        {
            var t = canvas.Find("Rune" + i);
            if (t != null) runeSlots[i] = t.GetComponent<Image>();
            var n = canvas.Find("RuneNum" + i);
            if (n != null) runeNums[i] = n.GetComponent<Text>();
        }
        var ei = canvas.Find("ElementIcon");
        if (ei != null) elementIcon = ei.GetComponent<Image>();
        var et = canvas.Find("ElementText");
        if (et != null) elementText = et.GetComponent<Text>();

        return hearts[0] != null;
    }

    /// <summary>Construye el HUD como objetos hijos (sirve en runtime y en editor).</summary>
    public Canvas BuildUI(Transform parent)
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvasGO = new GameObject("HUDCanvas");
        canvasGO.transform.SetParent(parent, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        hearts = new Image[3];
        for (int i = 0; i < 3; i++)
            hearts[i] = MakeIcon(canvasGO.transform, "Heart" + i, new Vector2(0, 1),
                new Vector2(70 + i * 80, -60), 64, Color.red);

        elementIcon = MakeIcon(canvasGO.transform, "ElementIcon", new Vector2(0.5f, 1),
            new Vector2(0, -60), 72, Color.white);

        var txtGO = new GameObject("ElementText");
        txtGO.transform.SetParent(canvasGO.transform, false);
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

        runeSlots = new Image[4];
        runeNums = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            runeSlots[i] = MakeIcon(canvasGO.transform, "Rune" + i, new Vector2(1, 1),
                new Vector2(-70 - i * 80, -60), 64, PlayerStats.ColorOf((ElementType)i));
            runeNums[i] = MakeText(canvasGO.transform, "RuneNum" + i, new Vector2(1, 1),
                new Vector2(-70 - i * 80, -100), (i + 1).ToString());
        }

        return canvas;
    }

    private Text MakeText(Transform parent, string name, Vector2 anchor, Vector2 pos, string txt)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = 26;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = txt;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.sizeDelta = new Vector2(40, 30);
        rt.anchoredPosition = pos;
        return t;
    }

    private Image MakeIcon(Transform parent, string name, Vector2 anchor, Vector2 pos, float size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = pos;
        return img;
    }

    private void Refresh()
    {
        LoadSprites();
        var ps = PlayerStats.Instance;
        if (ps != null && hearts != null)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null) continue;
                if (heartSprite != null) hearts[i].sprite = heartSprite;
                bool alive = i < ps.CurrentLives;
                // con imagen de corazon: blanco = vivo, gris oscuro = perdido
                hearts[i].color = alive
                    ? Color.white
                    : new Color(0.18f, 0.18f, 0.18f, 0.7f);
            }

            int e = (int)ps.CurrentElement;
            if (elementIcon != null)
            {
                if (elementSprites != null && elementSprites[e] != null)
                    elementIcon.sprite = elementSprites[e];
                elementIcon.color = Color.white;
            }
            if (elementText != null) elementText.text = ps.CurrentElement.ToString();

            if (runeSlots != null)
                for (int i = 0; i < runeSlots.Length; i++)
                {
                    if (runeSlots[i] == null) continue;
                    if (elementSprites != null && elementSprites[i] != null)
                        runeSlots[i].sprite = elementSprites[i];
                    bool unlocked = ps.IsUnlocked((ElementType)i);
                    // imagen a color si desbloqueada; tenue si no
                    runeSlots[i].color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.22f);

                    if (runeNums != null && runeNums[i] != null)
                    {
                        runeNums[i].text = (i + 1).ToString();
                        runeNums[i].color = unlocked
                            ? Color.white
                            : new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                }
        }
    }
}
