using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vigila un grupo de enemigos. Cuando todos han sido eliminados
/// (destruidos o desactivados), revela la runa asociada.
/// Si la lista de enemigos esta vacia, rastrea automaticamente a sus hijos.
/// </summary>
public class EnemyGroupTracker : MonoBehaviour
{
    [Tooltip("Runa que se activara al limpiar el grupo de enemigos.")]
    public GameObject runeToReveal;

    [Tooltip("Enemigos a vigilar. Si se deja vacio, usa los hijos de este objeto.")]
    public List<GameObject> enemies = new List<GameObject>();

    private bool revealed;

    private void Start()
    {
        if (enemies == null || enemies.Count == 0)
        {
            enemies = new List<GameObject>();
            foreach (Transform child in transform)
                enemies.Add(child.gameObject);
        }

        if (runeToReveal != null)
            runeToReveal.SetActive(false);
    }

    private void Update()
    {
        if (revealed) return;

        if (AllCleared())
        {
            revealed = true;
            if (runeToReveal != null)
                runeToReveal.SetActive(true);
            Debug.Log("[UnitBlade] Grupo eliminado: runa revelada.");
        }
    }

    private bool AllCleared()
    {
        foreach (var e in enemies)
            if (e != null && e.activeInHierarchy)
                return false;
        return true;
    }
}
