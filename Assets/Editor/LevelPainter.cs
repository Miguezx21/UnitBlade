using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Pintado automático de niveles desde el menú Tools/UnitBlade.
/// Usa los atlas atlas_floor.png (suelo) y atlas_walls.png (plataformas).
/// </summary>
public static class LevelPainter
{
    const string FLOOR_PATH = "Assets/Art/Tilesets/Castle/atlas_floor.png";
    const string WALL_PATH  = "Assets/Art/Tilesets/Castle/atlas_walls.png";
    const string FLOOR_SPRITE = "atlas_floor_24"; // baldosa central
    const string WALL_SPRITE  = "atlas_walls_13"; // ladrillo

    [MenuItem("Tools/UnitBlade/Pintar Level 1")]
    public static void PaintLevel1()
    {
        var ground = EnsureTilemap("Tilemap_Ground", 8, 0);
        var plat   = EnsureTilemap("Tilemap_Platforms", 9, 1);
        if (ground == null || plat == null)
        {
            Debug.LogError("[LevelPainter] No se encontró Tilemap_Ground o Tilemap_Platforms en la escena.");
            return;
        }

        var floorTile = MakeTile(GetSprite(FLOOR_PATH, FLOOR_SPRITE), "FloorTile");
        var wallTile  = MakeTile(GetSprite(WALL_PATH,  WALL_SPRITE),  "WallTile");
        if (floorTile == null || wallTile == null)
        {
            Debug.LogError("[LevelPainter] No se pudieron cargar los sprites de los atlas. ¿Importaste atlas_floor.png y atlas_walls.png?");
            return;
        }

        ground.ClearAllTiles();
        plat.ClearAllTiles();

        // --- SUELO continuo de x=-6 a x=92, top en y=0 (celdas y=-1..-4) ---
        FillRect(ground, floorTile, -6, 92, -4, -1);

        // --- Algunos huecos (saltos) para dar plataformeo ---
        ClearRect(ground, 26, 29, -1, 0);   // hueco 1
        ClearRect(ground, 50, 53, -1, 0);   // hueco 2

        // --- PLATAFORMAS flotantes ---
        FillRect(plat, wallTile, 16, 23, 5, 5);   // plataforma para Ignis_02 (20, 6.5)
        FillRect(plat, wallTile, 26, 30, 2, 2);   // escalón sobre hueco 1
        FillRect(plat, wallTile, 44, 51, 3, 3);   // plataforma cofre/runa
        FillRect(plat, wallTile, 64, 70, 4, 4);   // plataforma media

        ground.CompressBounds();
        plat.CompressBounds();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[LevelPainter] Level 1 pintado. Guarda con Ctrl+S.");
    }

    // ---------- Helpers ----------

    static void FillRect(Tilemap tm, TileBase tile, int x0, int x1, int y0, int y1)
    {
        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                tm.SetTile(new Vector3Int(x, y, 0), tile);
    }

    static void ClearRect(Tilemap tm, int x0, int x1, int y0, int y1)
    {
        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                tm.SetTile(new Vector3Int(x, y, 0), null);
    }

    static Tilemap EnsureTilemap(string name, int layer, int sortingOrder)
    {
        var go = GameObject.Find(name);
        if (go == null) return null;
        go.layer = layer;

        // Grid en el padre
        var parent = go.transform.parent;
        if (parent != null && parent.GetComponent<Grid>() == null)
            parent.gameObject.AddComponent<Grid>();

        var tm = go.GetComponent<Tilemap>();
        if (tm == null) tm = go.AddComponent<Tilemap>();

        var tr = go.GetComponent<TilemapRenderer>();
        if (tr == null) tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = sortingOrder;

        var col = go.GetComponent<TilemapCollider2D>();
        if (col == null) col = go.AddComponent<TilemapCollider2D>();

        return tm;
    }

    static Tile MakeTile(Sprite s, string name)
    {
        if (s == null) return null;
        var t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = s;
        t.colliderType = Tile.ColliderType.Grid;
        t.name = name;
        return t;
    }

    static Sprite GetSprite(string path, string spriteName)
    {
        var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        Sprite first = null;
        foreach (var o in objs)
        {
            if (o is Sprite s)
            {
                if (first == null) first = s;
                if (s.name == spriteName) return s;
            }
        }
        return first; // si no encuentra el nombre exacto, usa el primero
    }
}
