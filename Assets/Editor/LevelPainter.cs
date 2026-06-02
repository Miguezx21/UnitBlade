using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Pintado automático de niveles desde el menú Tools/UnitBlade.
/// </summary>
public static class LevelPainter
{
    [MenuItem("Tools/UnitBlade/Pintar Level 1 (Castillo)")]
    public static void PaintLevel1()
    {
        Paint(
            "Assets/Art/Tilesets/Castle/atlas_floor.png", "atlas_floor_24",
            "Assets/Art/Tilesets/Castle/atlas_walls.png", "atlas_walls_13");
    }

    [MenuItem("Tools/UnitBlade/Pintar Level 2 (Bosque)")]
    public static void PaintLevel2()
    {
        Paint(
            "Assets/Art/Tilesets/Forest/Floor_04.png", "Floor_04",
            "Assets/Art/Tilesets/Forest/Floor_04.png", "Floor_04");
    }

    static void Paint(string floorPath, string floorSprite, string wallPath, string wallSprite)
    {
        var ground = EnsureTilemap("Tilemap_Ground", 8, 0);
        var plat   = EnsureTilemap("Tilemap_Platforms", 9, 1);
        if (ground == null || plat == null)
        {
            Debug.LogError("[LevelPainter] No se encontró Tilemap_Ground o Tilemap_Platforms en la escena activa.");
            return;
        }

        var floorTile = MakeTile(GetSprite(floorPath, floorSprite), "FloorTile");
        var wallTile  = MakeTile(GetSprite(wallPath, wallSprite), "WallTile");
        if (floorTile == null || wallTile == null)
        {
            Debug.LogError("[LevelPainter] No se cargaron los sprites. Revisa que las texturas estén importadas como Sprite.");
            return;
        }

        ground.ClearAllTiles();
        plat.ClearAllTiles();

        // Suelo continuo x=-6..92, top en y=0 (celdas y=-1..-4)
        FillRect(ground, floorTile, -6, 92, -4, -1);
        // Huecos para saltar
        ClearRect(ground, 26, 29, -1, 0);
        ClearRect(ground, 50, 53, -1, 0);

        // Plataformas
        FillRect(plat, wallTile, 16, 23, 5, 5);
        FillRect(plat, wallTile, 26, 30, 2, 2);
        FillRect(plat, wallTile, 44, 51, 3, 3);
        FillRect(plat, wallTile, 64, 70, 4, 4);

        ground.CompressBounds();
        plat.CompressBounds();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[LevelPainter] Nivel pintado. Guarda con Ctrl+S.");
    }

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

        var parent = go.transform.parent;
        if (parent != null && parent.GetComponent<Grid>() == null)
            parent.gameObject.AddComponent<Grid>();

        var tm = go.GetComponent<Tilemap>() ?? go.AddComponent<Tilemap>();
        var tr = go.GetComponent<TilemapRenderer>() ?? go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = sortingOrder;
        if (go.GetComponent<TilemapCollider2D>() == null) go.AddComponent<TilemapCollider2D>();
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
        return first;
    }
}
