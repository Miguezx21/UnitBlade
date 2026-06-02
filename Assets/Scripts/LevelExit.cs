using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Portal de salida: al entrar el jugador, carga la siguiente escena.
/// Si nextScene está vacío, usa el siguiente índice del Build Settings.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    public string nextScene = "";
    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;

        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
            return;
        }

        int idx = SceneManager.GetActiveScene().buildIndex + 1;
        if (idx < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(idx);
        else
            Debug.Log("[UnitBlade] ¡Has completado todos los niveles!");
    }
}
