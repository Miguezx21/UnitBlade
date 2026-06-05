using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MorgathBoss : MonoBehaviour
{
    public Transform Player;

    [Header("Vida")]
    public int maxHP = 12;

    [Header("Ataque - Orbes")]
    public GameObject OrbPrefab;
    public Transform  FirePoint;      // punto de disparo (hijo del boss)
    public float orbSpeed    = 5f;
    public float orbLifetime = 5f;
    public float attackRate  = 3f;

    [Header("Ataque - Rayo")]
    public GameObject LightningWarningPrefab;
    public int   lightningDamage = 1;
    public float lightningRate   = 6f;
    public float lightningDelay  = 1f;

    [Header("Elemento")]
    public float switchInterval = 3f;

    [Header("Fase 2 (50% HP)")]
    public float phase2AttackRate     = 1.5f;
    public float phase2SwitchInterval = 1.5f;
    public int   phase2OrbCount       = 3;

    [Header("Activación")]
    public float activationRange = 12f;

    [Header("Victoria")]
    public float victoryDelay = 2.5f;

    // ── Privados ───────────────────────────────────────────
    private int            _hp;
    private bool           _active;
    private bool           _dead;
    private bool           _phase2;
    private bool           _transforming;  // pausa ataques durante transformación
    private ElementType    _currentElement = ElementType.Pira;
    private float          _elementTimer;
    private float          _attackTimer;
    private float          _lightningTimer;
    private int            _attackIndex;   // alterna entre los 3 ataques
    private SpriteRenderer _sr;
    private Animator       _anim;

    private static readonly int TransformingHash  = Animator.StringToHash("IsTransforming");
    private static readonly int AttackingHash     = Animator.StringToHash("IsAttacking");     // golpe melee
    private static readonly int FireAttackHash    = Animator.StringToHash("IsFireAttacking"); // disparo orbes
    private static readonly int LightningHash     = Animator.StringToHash("IsLightning");     // rayo
    private static readonly int IsDeadHash        = Animator.StringToHash("IsDead");

    private void Awake()
    {
        _sr   = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponent<Animator>()        ?? GetComponentInChildren<Animator>();
        _hp   = maxHP;
        ApplyElement();
    }

    private void Start()
    {
        if (Player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) Player = go.transform;
        }
        BossHUD.Instance?.Show("Morgath", maxHP);
    }

    private void Update()
    {
        if (_dead || _transforming || Player == null) return;
        if (PlayerStats.Instance != null && PlayerStats.Instance.CurrentLives <= 0) return;

        if (!_active)
        {
            if (Vector2.Distance(transform.position, Player.position) <= activationRange)
                Activate();
            return;
        }

        FacePlayer();

        // Cambio de elemento
        _elementTimer += Time.deltaTime;
        float interval = _phase2 ? phase2SwitchInterval : switchInterval;
        if (_elementTimer >= interval)
        {
            _elementTimer = 0f;
            NextElement();
        }

        // Alterna entre los 3 ataques en orden: melee → fire → lightning
        _attackTimer += Time.deltaTime;
        float atkRate = _phase2 ? phase2AttackRate : attackRate;
        if (_attackTimer >= atkRate)
        {
            _attackTimer = 0f;
            float dist = Vector2.Distance(transform.position, Player.position);

            switch (_attackIndex % 3)
            {
                case 0:
                    // Melee solo si está cerca, si no hace fire
                    if (dist < 2.5f) StartCoroutine(AttackMelee());
                    else             StartCoroutine(AttackOrbs());
                    break;
                case 1:
                    StartCoroutine(AttackOrbs());
                    break;
                case 2:
                    StartCoroutine(AttackLightning());
                    break;
            }
            _attackIndex++;
        }
    }

    // ── Activación ────────────────────────────────────────
    private void Activate()
    {
        _active = true;
        StartCoroutine(PhaseTransition(isPhase2: false));
    }

    // ── Transición de fase ────────────────────────────────
    private IEnumerator PhaseTransition(bool isPhase2)
    {
        _transforming = true;

        // Animación IsTransforming
        _anim?.SetBool(TransformingHash, true);

        if (isPhase2)
        {
            // Flash blanco intenso x3 para que se note la fase 2
            for (int i = 0; i < 3; i++)
            {
                if (_sr != null) _sr.color = Color.white;
                yield return new WaitForSeconds(0.15f);
                ApplyElement();
                yield return new WaitForSeconds(0.15f);
            }

            // Sacudida de cámara
            StartCoroutine(CameraShake(0.4f, 0.15f));

            // Pausa dramática
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(1.2f);
        }

        _anim?.SetBool(TransformingHash, false);
        _transforming = false;
    }

    // ── Sacudida de cámara ────────────────────────────────
    private IEnumerator CameraShake(float duration, float magnitude)
    {
        var cam = Camera.main;
        if (cam == null) yield break;

        Vector3 orig = cam.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cam.transform.position = orig + (Vector3)Random.insideUnitCircle * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = orig;
    }

    // ── Giro ──────────────────────────────────────────────
    private void FacePlayer()
    {
        float dirX = Player.position.x - transform.position.x;
        transform.localScale = new Vector3(dirX >= 0f ? 1f : -1f, 1f, 1f);
    }

    // ── Elemento ──────────────────────────────────────────
    private void NextElement()
    {
        _currentElement = (ElementType)(((int)_currentElement + 1) % 4);
        ApplyElement();
    }

    private void ApplyElement()
    {
        if (_sr == null) return;
        Color c = PlayerStats.ColorOf(_currentElement);
        c = Color.Lerp(c, Color.black, 0.35f);
        c.a = 1f;
        _sr.color = c;
    }

    public ElementType WeakTo() => (ElementType)(((int)_currentElement + 2) % 4);

    // ── Ataque: melee ─────────────────────────────────────
    private IEnumerator AttackMelee()
    {
        _anim?.SetBool(AttackingHash, true);
        // El daño lo aplica el Animation Event OnMeleeHit()
        yield return new WaitForSeconds(0.8f);
        _anim?.SetBool(AttackingHash, false);
    }

    /// <summary>
    /// Animation Event — llámalo en el frame del golpe de la animación Attack.
    /// En Unity: abre la animación Attack de Morgath → agrega un evento
    /// en el frame del impacto → función: OnMeleeHit
    /// </summary>
    public void OnMeleeHit()
    {
        if (_dead || Player == null) return;

        float dist = Vector2.Distance(transform.position, Player.position);
        if (dist > 2.5f) return; // solo daña si está cerca

        PlayerStats.Instance?.TakeDamage(1);

        // Knockback en la dirección del golpe
        var rb = Player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float dir = Mathf.Sign(Player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(dir * 5f, 4f);
        }

        // Efecto visual: flash en el jugador
        StartCoroutine(MeleeImpactEffect());
    }

    private IEnumerator MeleeImpactEffect()
    {
        var sr = Player?.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color orig = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (sr != null) sr.color = orig;
    }

    // ── Ataque: orbes (Fire Attack) ───────────────────────
    private IEnumerator AttackOrbs()
    {
        if (OrbPrefab == null) yield break;

        _anim?.SetBool(FireAttackHash, true);
        yield return new WaitForSeconds(0.3f);

        Vector2 origin = FirePoint != null
            ? (Vector2)FirePoint.position
            : (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, 0f);

        Vector2 baseDir = ((Vector2)Player.position - origin).normalized;
        int count = _phase2 ? phase2OrbCount : 1;

        for (int i = 0; i < count; i++)
        {
            float angle = count > 1 ? Mathf.Lerp(-25f, 25f, (float)i / (count - 1)) : 0f;
            Vector2 dir = Rotate(baseDir, angle);

            var orb = Instantiate(OrbPrefab, origin, Quaternion.identity);

            var proj = orb.GetComponent<OrbProjectile>();
            if (proj != null)
            {
                proj.SetDirection(dir, orbSpeed, 1, orbLifetime);
            }
            else
            {
                // Fallback: mover directo con Rigidbody2D
                var rb2d = orb.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    rb2d.gravityScale    = 0f;
                    rb2d.linearVelocity  = dir * orbSpeed;
                }
                Destroy(orb, orbLifetime);
            }

            if (count > 1) yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.4f);
        _anim?.SetBool(FireAttackHash, false);
    }

    // ── Ataque: rayo ──────────────────────────────────────
    private IEnumerator AttackLightning()
    {
        _anim?.SetBool(LightningHash, true);
        // El rayo en sí lo dispara el Animation Event OnLightningHit()
        // Solo esperamos a que termine la animación
        yield return new WaitForSeconds(lightningDelay + 0.4f);
        _anim?.SetBool(LightningHash, false);
    }

    /// <summary>
    /// Animation Event — llámalo en el frame del golpe de la animación Lightning.
    /// En Unity: abre la animación Lightning de Morgath → agrega un evento
    /// en el frame del impacto → función: OnLightningHit
    /// </summary>
    public void OnLightningHit()
    {
        if (_dead || Player == null) return;
        Vector3 targetPos = new Vector3(Player.position.x, Player.position.y - 0.3f, 0f);
        LightningStrike.Spawn(targetPos, lightningDelay, lightningDamage, Player);
    }

    // ── Recibir daño ──────────────────────────────────────
    public void TakeDamage(int dmg, ElementType element)
    {
        if (_dead || !_active || _transforming) return;

        bool counter = element == WeakTo();
        int final = counter ? dmg * 3 : dmg;
        _hp = Mathf.Max(0, _hp - final);

        BossHUD.Instance?.UpdateHP(_hp);
        StartCoroutine(FlashHit(counter));

        if (!_phase2 && _hp <= maxHP / 2)
        {
            _phase2 = true;
            StartCoroutine(PhaseTransition(isPhase2: true));
        }

        if (_hp <= 0) StartCoroutine(Die());
    }

    // ── Muerte ────────────────────────────────────────────
    private IEnumerator Die()
    {
        _dead = true;
        _anim?.SetBool(IsDeadHash, true);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        BossHUD.Instance?.Hide();

        yield return new WaitForSeconds(victoryDelay);

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(next < SceneManager.sceneCountInBuildSettings ? next : 0);
    }

    // ── Helpers ───────────────────────────────────────────
    private IEnumerator FlashHit(bool counter)
    {
        if (_sr == null) yield break;
        _sr.color = counter ? Color.yellow : Color.white;
        yield return new WaitForSeconds(0.1f);
        if (!_dead) ApplyElement();
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}
