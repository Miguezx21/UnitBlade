using UnityEngine;

/// <summary>
/// Hace que el fondo acompañe a la cámara para que siempre cubra la pantalla.
/// parallax = 1 → fijo a la cámara (cobertura total).
/// parallax &lt; 1 → efecto de profundidad (se mueve más lento).
/// </summary>
public class BackgroundFollow : MonoBehaviour
{
    public Transform cam;
    [Range(0f, 1f)] public float parallaxX = 1f;
    [Range(0f, 1f)] public float parallaxY = 1f;

    private Vector3 startBg;
    private Vector3 startCam;
    private float zPos;

    private void Start()
    {
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        zPos = transform.position.z;
        startBg = transform.position;
        if (cam != null) startCam = cam.position;
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main != null) { cam = Camera.main.transform; startCam = cam.position; }
            else return;
        }

        Vector3 d = cam.position - startCam;
        transform.position = new Vector3(
            startBg.x + d.x * parallaxX,
            startBg.y + d.y * parallaxY,
            zPos);
    }
}
