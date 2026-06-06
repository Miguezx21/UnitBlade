using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public int damage = 1;
    public float range = 1.4f;
    public float cooldown = 0.4f;

    private SpriteRenderer _sr;
    private PlayerAnimator _animCtrl;   // ← ahora usamos el componente
    private float _lastAttack = -99f;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _animCtrl = GetComponent<PlayerAnimator>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        bool pressed = (kb != null && kb.jKey.wasPressedThisFrame)
                    || (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (pressed && Time.time >= _lastAttack + cooldown)
            Attack();
    }

    private void Attack()
    {
        _lastAttack = Time.time;
        _animCtrl?.SetAttacking(true);
        AudioManager.Instance?.Sword();
        Invoke(nameof(EndAttack), 0.35f);   // duración del clip Attack

        float dir = (_sr != null && _sr.flipX) ? -1f : 1f;
        Vector2 center = (Vector2)transform.position + new Vector2(dir * range * 0.5f, 0f);

        var elem = PlayerStats.Instance != null
            ? PlayerStats.Instance.CurrentElement
            : ElementType.Pira;

        foreach (var h in Physics2D.OverlapCircleAll(center, range * 0.6f))
        {
            if (!h.CompareTag("Enemy") && !h.CompareTag("Boss")) continue;

            // Enemigo normal
            h.GetComponent<Health>()?.TakeDamage(damage, elem);

            // Boss Morgath (tiene su propio sistema de vida)
            h.GetComponent<MorgathBoss>()?.TakeDamage(damage, elem);
        }
    }

    private void EndAttack() => _animCtrl?.SetAttacking(false);
}