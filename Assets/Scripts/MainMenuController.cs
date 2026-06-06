using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú principal de Unit Blade construido por código (uGUI).
/// Botones: JUGAR, RUNAS, INSTRUCCIONES, HISTORIA. Cada panel tiene "Volver".
/// Los sprites (fondo + runas) se asignan desde el Inspector o la herramienta de editor.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Sprites (asignados por la herramienta)")]
    public Sprite background;
    public Sprite kaelen;      // Personaje para el menú
    public Sprite runeThron;   // Thurisaz (rayo)
    public Sprite runePira;    // Kenaz (fuego)
    public Sprite runeIsa;     // Hielo
    public Sprite runeSteinn;  // Piedra

    [Header("Primer nivel")]
    public string firstScene = "Level_01_Castle";

    private Font _font;
    private Canvas _canvas;
    private GameObject _panelRunas, _panelInstr, _panelHist, _mainGroup;

    private static readonly Color GOLD = new Color(0.95f, 0.75f, 0.25f);
    private static readonly Color PANEL_BG = new Color(0.05f, 0.06f, 0.10f, 0.94f);

    // ── Lore corregido de las runas ──────────────────────────────────────────
    private static readonly (string title, string sub, string desc)[] RUNES =
    {
        ("RUNA THRON (THURISAZ)", "Rayo · Fuerza de Thor",
         "Runa de la fuerza reactiva. Thron es la espina que hiere y el gigante que protege. " +
         "Invoca la fuerza de Thor para desatar rayos y desmantelar defensas. Como la espina que hiere, " +
         "su poder es rápido y decisivo."),

        ("RUNA PIRA (KENAZ)", "Fuego · Iluminación y transformación",
         "Runa de la iluminación y la transformación. Pira es la antorcha que ilumina el camino y el fuego " +
         "que quema para purificar. Representa la purificación del fuego, transformando lo viejo en nuevo y " +
         "proporcionando calor y luz. Es el fuego de la forja que da forma a la Unit Blade."),

        ("RUNA ISA (HIELO)", "Hielo · Quietud y barrera",
         "Runa de la quietud y la barrera. Isa es el hielo que detiene el flujo y la quietud que permite la " +
         "introspección. Crea una barrera que detiene el tiempo y el movimiento, permitiendo la observación " +
         "antes de actuar. Como el hielo que congela, detiene todas las cosas."),

        ("RUNA STEINN (PIEDRA)", "Piedra · Estabilidad y fundamento",
         "Runa de la estabilidad y el fundamento. Steinn es la piedra firme que resiste el tiempo y la tierra " +
         "que sostiene. Representa la inmutabilidad y la fuerza de la tierra. Proporciona un fundamento sólido " +
         "sobre el cual construir, permitiendo la resistencia contra las fuerzas que intentan movernos."),
    };

    private const string INSTRUCTIONS =
        "CONTROLES\n" +
        "•  A / D  o  ← →   —   Moverse\n" +
        "•  Espacio  o  W   —   Saltar\n" +
        "•  J  o  Clic izq.  —   Atacar\n" +
        "•  Z   —   Ataque secundario\n" +
        "•  1 · 2 · 3 · 4   —   Cambiar runa (elemento)\n\n" +
        "OBJETIVO\n" +
        "Avanza por el Castillo, el Bosque y la Cripta hasta enfrentar a Morgath. " +
        "Recoge las runas para desbloquear nuevos elementos: cada puzle y enemigo exige el poder correcto.\n\n" +
        "REGLAS\n" +
        "•  Los corazones son tu vida; al perderlos todos, reapareces.\n" +
        "•  Usa Pira (fuego), Isa (hielo), Steinn (piedra) y Thron (rayo) según la situación.\n" +
        "•  Cada runa rompe un tipo de defensa distinto.";

    private const string STORY =
        "LA UNIT BLADE\n\n" +
        "En las tierras del norte, donde el invierno nunca duerme, las antiguas runas vikingas guardaban el " +
        "equilibrio del mundo. Forjada en el fuego de Pira y templada en el hielo de Isa, la Unit Blade fue " +
        "creada para contener el poder de los elementos en una sola hoja.\n\n" +
        "Kaelen, joven guardián de la forja, despierta para encontrar su hogar en ruinas: el hechicero Morgath " +
        "ha robado las runas para corromper su poder y desatar la tormenta de Thron sobre el reino.\n\n" +
        "Sin más que su valor y una hoja incompleta, Kaelen debe recuperar las cuatro runas —Pira, Isa, Steinn " +
        "y Thron— atravesando el Castillo, el Bosque y la Cripta, para reforjar la Unit Blade y enfrentar a " +
        "Morgath antes de que la oscuridad lo consuma todo.";

    private void Awake()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.menuMusic);
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("MenuCanvas");
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Fondo
        var bg = NewImage(canvasGO.transform, "Background");
        Stretch(bg.rectTransform);
        if (background != null) { bg.sprite = background; bg.color = Color.white; bg.preserveAspect = false; }
        else bg.color = new Color(0.08f, 0.09f, 0.14f);
        // Oscurecedor para legibilidad
        var dim = NewImage(canvasGO.transform, "Dim");
        Stretch(dim.rectTransform);
        dim.color = new Color(0, 0, 0, 0.35f);

        // Grupo principal (título + botones)
        _mainGroup = new GameObject("Main");
        _mainGroup.transform.SetParent(canvasGO.transform, false);
        Stretch(_mainGroup.AddComponent<RectTransform>());

        // Kaelen a la izquierda (como en el arte de referencia)
        if (kaelen != null)
        {
            var hero = NewImage(_mainGroup.transform, "Kaelen");
            hero.sprite = kaelen; hero.color = Color.white; hero.preserveAspect = true;
            var hrt = hero.rectTransform;
            hrt.anchorMin = hrt.anchorMax = hrt.pivot = new Vector2(0f, 0f);
            hrt.anchoredPosition = new Vector2(120, 60);
            hrt.sizeDelta = new Vector2(620, 900);
        }

        // Sombra/recuadro para el título
        var title = NewText(_mainGroup.transform, "Title", "UNIT BLADE", 120, GOLD);
        title.fontStyle = FontStyle.Bold;
        Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(60, -150), new Vector2(1300, 170));

        // Botones (centro-derecha)
        MakeMenuButton("JUGAR",        -40, OnPlay);
        MakeMenuButton("RUNAS",       -160, () => Show(_panelRunas));
        MakeMenuButton("INSTRUCCIONES",-280, () => Show(_panelInstr));
        MakeMenuButton("HISTORIA",    -400, () => Show(_panelHist));

        // Recuadro de runas (siempre visible, arriba a la derecha)
        BuildRuneSidebar(_mainGroup.transform);

        // Paneles
        _panelRunas = BuildRunesPanel(canvasGO.transform);
        _panelInstr = BuildTextPanel(canvasGO.transform, "INSTRUCCIONES", INSTRUCTIONS);
        _panelHist  = BuildTextPanel(canvasGO.transform, "HISTORIA", STORY);
        _panelRunas.SetActive(false);
        _panelInstr.SetActive(false);
        _panelHist.SetActive(false);
    }

    // ── Acciones ─────────────────────────────────────────────────────────────
    private void OnPlay()
    {
        if (Application.CanStreamedLevelBeLoaded(firstScene))
            SceneManager.LoadScene(firstScene);
        else
            SceneManager.LoadScene(1); // fallback: primera escena tras el menú
    }

    private void Show(GameObject panel)
    {
        _mainGroup.SetActive(false);
        panel.SetActive(true);
    }

    private void Back(GameObject panel)
    {
        panel.SetActive(false);
        _mainGroup.SetActive(true);
    }

    // ── Constructores de UI ──────────────────────────────────────────────────
    private GameObject BuildTextPanel(Transform parent, string header, string body)
    {
        var panel = NewPanel(parent, "Panel_" + header);
        var title = NewText(panel.transform, "Header", header, 70, GOLD);
        title.fontStyle = FontStyle.Bold;
        Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(1400, 90));

        ScrollArea(panel.transform, out RectTransform contentRT);
        var txt = NewText(contentRT, "Body", body, 34, Color.white);
        txt.alignment = TextAnchor.UpperLeft;
        var rt = txt.rectTransform;
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(40, 0); rt.offsetMax = new Vector2(-40, 0);
        var fitter = txt.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        FitContent(contentRT, txt);

        MakeButton(panel.transform, "VOLVER", -60, () => Back(panel), fromBottom: true);
        return panel;
    }

    private GameObject BuildRunesPanel(Transform parent)
    {
        var panel = NewPanel(parent, "Panel_Runas");
        var title = NewText(panel.transform, "Header", "RUNAS", 70, GOLD);
        title.fontStyle = FontStyle.Bold;
        Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(1400, 90));

        ScrollArea(panel.transform, out RectTransform contentRT);
        var vlg = contentRT.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 30; vlg.padding = new RectOffset(40, 40, 20, 40);
        vlg.childControlHeight = true; vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
        var csf = contentRT.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Sprite[] icons = { runeThron, runePira, runeIsa, runeSteinn };
        for (int i = 0; i < RUNES.Length; i++)
            BuildRuneEntry(contentRT, icons[i], RUNES[i]);

        MakeButton(panel.transform, "VOLVER", -60, () => Back(panel), fromBottom: true);
        return panel;
    }

    private void BuildRuneEntry(Transform parent, Sprite icon, (string title, string sub, string desc) r)
    {
        var row = new GameObject("Rune_" + r.title);
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>();
        var le = row.AddComponent<LayoutElement>(); le.minHeight = 170;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 24; hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.UpperLeft;

        var iconGO = NewImage(row.transform, "Icon");
        if (icon != null) { iconGO.sprite = icon; iconGO.color = Color.white; }
        else iconGO.color = new Color(1, 1, 1, 0.15f);
        iconGO.preserveAspect = true;
        var ile = iconGO.gameObject.AddComponent<LayoutElement>();
        ile.preferredWidth = 120; ile.preferredHeight = 120; ile.minWidth = 120;

        var textCol = new GameObject("Text");
        textCol.transform.SetParent(row.transform, false);
        textCol.AddComponent<RectTransform>();
        var vlg = textCol.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4; vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        var t1 = NewText(textCol.transform, "T", r.title, 36, GOLD); t1.fontStyle = FontStyle.Bold;
        t1.alignment = TextAnchor.UpperLeft;
        var t2 = NewText(textCol.transform, "S", r.sub, 26, new Color(0.8f, 0.85f, 0.95f));
        t2.alignment = TextAnchor.UpperLeft; t2.fontStyle = FontStyle.Italic;
        var t3 = NewText(textCol.transform, "D", r.desc, 28, Color.white);
        t3.alignment = TextAnchor.UpperLeft;
        foreach (var t in new[] { t1, t2, t3 })
        {
            var e = t.gameObject.AddComponent<LayoutElement>();
            e.flexibleWidth = 1;
        }
    }

    // Botón del menú principal: anclado al centro, apilado por 'y'.
    private void MakeMenuButton(string label, float y, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(_mainGroup.transform, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.12f, 0.14f, 0.20f, 0.92f);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(460, 96);
        rt.anchoredPosition = new Vector2(0, y);

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = GOLD;
        colors.pressedColor = new Color(0.85f, 0.65f, 0.2f);
        btn.colors = colors;

        var txt = NewText(go.transform, "Label", label, 42, Color.white);
        txt.fontStyle = FontStyle.Bold;
        Stretch(txt.rectTransform);
    }

    // Recuadro de runas siempre visible (arriba a la derecha), estilo referencia.
    private void BuildRuneSidebar(Transform parent)
    {
        var box = NewImage(parent, "RuneSidebar");
        box.color = new Color(0.05f, 0.06f, 0.10f, 0.78f);
        var brt = box.rectTransform;
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(1f, 1f);
        brt.anchoredPosition = new Vector2(-40, -40);
        brt.sizeDelta = new Vector2(560, 560);

        var vlg = box.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 14; vlg.padding = new RectOffset(20, 20, 18, 18);
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperLeft;

        Sprite[] icons = { runeThron, runePira, runeIsa, runeSteinn };
        for (int i = 0; i < RUNES.Length; i++)
        {
            var row = new GameObject("R" + i);
            row.transform.SetParent(box.transform, false);
            row.AddComponent<RectTransform>();
            var le = row.AddComponent<LayoutElement>(); le.minHeight = 120;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14; hlg.childControlWidth = true; hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var ic = NewImage(row.transform, "Icon");
            if (icons[i] != null) { ic.sprite = icons[i]; ic.color = Color.white; }
            else ic.color = new Color(1, 1, 1, 0.15f);
            ic.preserveAspect = true;
            var ile = ic.gameObject.AddComponent<LayoutElement>();
            ile.preferredWidth = 90; ile.preferredHeight = 90; ile.minWidth = 90;

            var col = new GameObject("T"); col.transform.SetParent(row.transform, false);
            col.AddComponent<RectTransform>();
            var cvl = col.AddComponent<VerticalLayoutGroup>();
            cvl.childControlWidth = true; cvl.childControlHeight = true;
            cvl.childForceExpandWidth = true; cvl.childForceExpandHeight = false;
            var cle = col.AddComponent<LayoutElement>(); cle.flexibleWidth = 1;

            var t1 = NewText(col.transform, "N", RUNES[i].title, 26, GOLD);
            t1.fontStyle = FontStyle.Bold; t1.alignment = TextAnchor.UpperLeft;
            var t2 = NewText(col.transform, "S", RUNES[i].sub, 20, new Color(0.85f, 0.88f, 0.95f));
            t2.alignment = TextAnchor.UpperLeft;
        }
    }

    // ── Helpers de UI ────────────────────────────────────────────────────────
    private GameObject NewPanel(Transform parent, string name)
    {
        var panel = NewImage(parent, name).gameObject;
        Stretch(panel.GetComponent<RectTransform>());
        panel.GetComponent<Image>().color = PANEL_BG;
        return panel;
    }

    private RectTransform ScrollArea(Transform parent, out RectTransform content)
    {
        var viewGO = new GameObject("Viewport");
        viewGO.transform.SetParent(parent, false);
        var viewRT = viewGO.AddComponent<RectTransform>();
        viewRT.anchorMin = new Vector2(0.08f, 0.16f);
        viewRT.anchorMax = new Vector2(0.92f, 0.82f);
        viewRT.offsetMin = viewRT.offsetMax = Vector2.zero;
        viewGO.AddComponent<RectMask2D>();
        var scroll = viewGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewGO.transform, false);
        content = contentGO.AddComponent<RectTransform>();
        content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1); content.offsetMin = content.offsetMax = Vector2.zero;

        scroll.viewport = viewRT;
        scroll.content = content;
        return viewRT;
    }

    private void FitContent(RectTransform content, Text txt)
    {
        // Asegura que el contenido tenga altura al menos la del texto.
        var le = content.gameObject.GetComponent<LayoutElement>() ?? content.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 800;
    }

    private Button MakeButton(Transform parent, string label, float y, UnityEngine.Events.UnityAction onClick, bool fromBottom = false)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.12f, 0.14f, 0.20f, 0.95f);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, fromBottom ? 0f : 0.5f);
        rt.sizeDelta = new Vector2(420, 90);
        rt.anchoredPosition = new Vector2(0, fromBottom ? -y : y); // fromBottom: y negativo => arriba del borde
        if (fromBottom) rt.anchoredPosition = new Vector2(0, 60);

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        var colors = btn.colors;
        colors.highlightedColor = GOLD;
        colors.pressedColor = new Color(0.8f, 0.6f, 0.2f);
        btn.colors = colors;

        var txt = NewText(go.transform, "Label", label, 40, Color.white);
        txt.fontStyle = FontStyle.Bold;
        Stretch(txt.rectTransform);
        return btn;
    }

    private Image NewImage(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.rectTransform.sizeDelta = new Vector2(100, 100);
        return img;
    }

    private Text NewText(Transform parent, string name, string content, int size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = _font;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.text = content;
        t.rectTransform.sizeDelta = new Vector2(400, 60);
        return t;
    }

    private void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private void Anchor(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }
}
