using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Salto")]
    public float jumpForce = 10f;
    public float groundCheckDist = 0.1f;
    public LayerMask groundMask;
    public float coyoteTime = 0.15f;

    [Header("Daño / Rebote")]
    public float bounceForce = 6f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private PlayerAnimator _animCtrl;
    private Collider2D _col;

    private bool _grounded;
    private bool _attacking;
    private bool _takingDamage;
    public bool Dead { get; private set; }

    private float _coyoteTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animCtrl = GetComponent<PlayerAnimator>();
        _col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Dead) return;

        if (PlayerStats.Instance != null && PlayerStats.Instance.CurrentLives <= 0)
        {
            Die();
            return;
        }

        // ── Detección de suelo (raycast desde el borde inferior del collider) ──
        bool wasGrounded = _grounded;
        Vector2 origin = _col != null
            ? new Vector2(transform.position.x, _col.bounds.min.y)
            : (Vector2)transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDist, groundMask);
        _grounded = hit.collider != null;

        // ── Coyote time ────────────────────────────────────
        if (_grounded)
            _coyoteTimer = coyoteTime;          // en suelo: recarga el timer
        else if (wasGrounded && !_grounded)
            _coyoteTimer = coyoteTime;          // acaba de salir del suelo: arranca el timer
        else
            _coyoteTimer -= Time.deltaTime;     // en el aire: cuenta regresiva

        bool canJump = _coyoteTimer > 0f;

        // ── Movimiento y salto (bloqueados si está atacando) ─
        if (!_attacking)
        {
            Movimiento();

            if (!_takingDamage && canJump)
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    bool jumpPressed = kb.spaceKey.wasPressedThisFrame
                                    || kb.wKey.wasPressedThisFrame
                                    || kb.upArrowKey.wasPressedThisFrame;
                    if (jumpPressed)
                    {
                        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                        _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                        _coyoteTimer = 0f;
                        _animCtrl?.OnJump();
                    }
                }
            }
        }

        // ── Ataque ─────────────────────────────────────────
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.zKey.wasPressedThisFrame && !_attacking && _grounded)
            SetAtacando(true);

        // ── Sincronizar animator ───────────────────────────
        _animCtrl?.SetGrounded(_grounded);
        _animCtrl?.SetTakingDamage(_takingDamage);
        _animCtrl?.SetAttacking(_attacking);
        _animCtrl?.SetDead(Dead);
    }

    private void Movimiento()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;

        if (!_takingDamage)
        {
            _rb.linearVelocity = new Vector2(x * moveSpeed, _rb.linearVelocity.y);
            if (_sr != null && x != 0f) _sr.flipX = x < 0f;
        }
    }

    // ── API pública ────────────────────────────────────────

    public void RecibeDanio(Vector2 origenPos, int cantidad)
    {
        if (_takingDamage) return;
        _takingDamage = true;

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.TakeDamage();

        if (PlayerStats.Instance == null || PlayerStats.Instance.CurrentLives > 0)
        {
            Vector2 rebote = new Vector2(transform.position.x - origenPos.x, 0.2f).normalized;
            _rb.AddForce(rebote * bounceForce, ForceMode2D.Impulse);
        }
    }

    public void DesactivaDanio()
    {
        _takingDamage = false;
        _rb.linearVelocity = Vector2.zero;
    }

    public void SetAtacando(bool value)
    {
        _attacking = value;
    }

    private void Die()
    {
        Dead = true;
        SetAtacando(false);
        _rb.linearVelocity = Vector2.zero;
        _rb.simulated = false; // deja de caer al morir

        var atk = GetComponent<PlayerAttack>();
        if (atk != null) atk.enabled = false;

        // Activa animación de muerte
        _animCtrl?.SetDead(true);

        // Lanza la secuencia de Game Over
        GameOverManager.Instance?.TriggerGameOver();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var col = GetComponent<Collider2D>();
        Vector3 origin = col != null
            ? new Vector3(transform.position.x, col.bounds.min.y, 0f)
            : transform.position;
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDist);
    }
}
