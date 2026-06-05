using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class OrbProjectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private int _damage;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
    }

    /// <summary>Llamado por MageEnemy al instanciar el orbe (igual que BulletScript.SetDirection).</summary>
    public void SetDirection(Vector3 direction, float speed, int damage, float lifetime)
    {
        _damage = damage;

        Vector2 dir = ((Vector2)direction).normalized;
        _rb.linearVelocity = dir * speed;

        // El sprite apunta a la izquierda por defecto
        // → flipX cuando va a la derecha
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = dir.x < 0f;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.TakeDamage(_damage);

        // Knockback en la dirección del orbe
        var rb = other.attachedRigidbody;
        if (rb != null)
        {
            Vector2 dir = _rb.linearVelocity.normalized;
            rb.linearVelocity = new Vector2(dir.x * 4f, 3f);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Player"))
            Destroy(gameObject);
    }
}
