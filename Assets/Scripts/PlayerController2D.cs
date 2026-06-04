using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento básico de Kaelen: caminar (A/D o flechas) y saltar (Espacio/W/Arriba).
/// Usa el nuevo Input System. Actualiza el Animator (Speed, IsGrounded).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundRadius = 0.25f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private bool grounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private bool isDead;

    private void Update()
    {
        // Muerte al quedarse sin corazones
        if (isDead) return;
        if (PlayerStats.Instance != null && PlayerStats.Instance.CurrentLives <= 0)
        {
            Die();
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        float x = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;

        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);

        if (x != 0f && sr != null) sr.flipX = x < 0f;

        // Detección de suelo
        if (groundCheck != null)
            grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
        else
            grounded = Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        bool jumpPressed = kb.spaceKey.wasPressedThisFrame
                        || kb.wKey.wasPressedThisFrame
                        || kb.upArrowKey.wasPressedThisFrame;

        if (jumpPressed && grounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(x) * moveSpeed);
            anim.SetBool("IsGrounded", grounded);
        }
    }

    private void Die()
    {
        isDead = true;
        if (anim != null) anim.SetBool("IsDead", true);
        if (rb != null) rb.linearVelocity = Vector2.zero;

        var atk = GetComponent<PlayerAttack>();
        if (atk != null) atk.enabled = false;

        Debug.Log("[UnitBlade] Kaelen ha muerto. Reapareciendo...");
        Invoke(nameof(Respawn), 2.5f);
    }

    private void Respawn()
    {
        if (PlayerStats.Instance != null) PlayerStats.Instance.Revive();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
    }
}
