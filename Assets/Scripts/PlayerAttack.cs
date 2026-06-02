using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ataque de Kaelen con la Unit Blade.
///   J o clic izquierdo = atacar.
/// Golpea a los enemigos frente a él usando el elemento activo (PlayerStats).
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    public int damage = 1;
    public float range = 1.4f;
    public float cooldown = 0.4f;
    public LayerMask enemyMask;

    private SpriteRenderer sr;
    private Animator anim;
    private float lastAttack = -99f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        bool pressed = (kb != null && kb.jKey.wasPressedThisFrame)
                    || (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (pressed && Time.time >= lastAttack + cooldown)
            Attack();
    }

    private void Attack()
    {
        lastAttack = Time.time;

        if (anim != null) anim.SetBool("IsAttacking", true);
        Invoke(nameof(EndAttackAnim), 0.18f);

        float dir = (sr != null && sr.flipX) ? -1f : 1f;
        Vector2 center = (Vector2)transform.position + new Vector2(dir * range * 0.5f, 0f);

        var elem = PlayerStats.Instance != null
            ? PlayerStats.Instance.CurrentElement
            : ElementType.Pira;

        var hits = Physics2D.OverlapCircleAll(center, range * 0.6f, enemyMask);
        foreach (var h in hits)
        {
            var hp = h.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(damage, elem);
        }
    }

    private void EndAttackAnim()
    {
        if (anim != null) anim.SetBool("IsAttacking", false);
    }
}
