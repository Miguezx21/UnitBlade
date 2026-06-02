using UnityEngine;

/// <summary>
/// Daño por contacto: si el enemigo toca a Kaelen, le quita un corazón
/// (con tiempo de espera para no vaciar la vida de golpe) y lo empuja.
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    public int damage = 1;
    public float cooldown = 1f;
    public float knockback = 4f;

    private float lastHit = -99f;

    private void OnCollisionStay2D(Collision2D c) { TryHit(c.collider, c.transform); }
    private void OnCollisionEnter2D(Collision2D c) { TryHit(c.collider, c.transform); }
    private void OnTriggerStay2D(Collider2D c) { TryHit(c, c.transform); }
    private void OnTriggerEnter2D(Collider2D c) { TryHit(c, c.transform); }

    private void TryHit(Collider2D other, Transform otherT)
    {
        if (other == null || !other.CompareTag("Player")) return;
        if (Time.time < lastHit + cooldown) return;
        if (PlayerStats.Instance == null) return;

        lastHit = Time.time;
        PlayerStats.Instance.TakeDamage(damage);

        var rb = other.attachedRigidbody;
        if (rb != null)
        {
            float dir = Mathf.Sign(otherT.position.x - transform.position.x);
            if (dir == 0f) dir = 1f;
            rb.linearVelocity = new Vector2(dir * knockback, 3f);
        }
    }
}
