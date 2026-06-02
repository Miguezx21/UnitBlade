using UnityEngine;

/// <summary>
/// La cámara sigue suavemente al jugador (tag Player).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smooth = 5f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    private void Start()
    {
        TryFindTarget();
    }

    private void LateUpdate()
    {
        if (target == null) { TryFindTarget(); if (target == null) return; }
        Vector3 goal = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, goal, smooth * Time.deltaTime);
    }

    private void TryFindTarget()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) target = p.transform;
    }
}
