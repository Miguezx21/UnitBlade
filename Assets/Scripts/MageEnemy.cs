using UnityEngine;

public class MageEnemy : MonoBehaviour
{
    public Transform Player;
    public GameObject OrbPrefab;

    public float DetectionRange = 6f;
    public float FireRate       = 2f;
    public int   OrbDamage      = 1;
    public float OrbSpeed       = 5f;
    public float OrbLifetime    = 4f;

    private float   _lastShoot;
    private Animator _anim;

    // Nombres de parámetros del Animator del mago
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int IsWalkingHash   = Animator.StringToHash("IsWalking");

    void Start()
    {
        _anim = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

        if (Player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) Player = go.transform;
        }
    }

    void Update()
    {
        if (Player == null) return;
        if (PlayerStats.Instance != null && PlayerStats.Instance.CurrentLives <= 0)
        {
            if (_anim != null) _anim.SetBool(IsAttackingHash, false);
            return;
        }

        // ── Girar hacia el jugador ──────────────────────────
        float dirX = Player.position.x - transform.position.x;
        transform.localScale = new Vector3(dirX >= 0f ? 1f : -1f, 1f, 1f);

        // ── Disparar si está en rango ───────────────────────
        float distance = Vector2.Distance(transform.position, Player.position);
        bool inRange = distance < DetectionRange;

        if (inRange && Time.time > _lastShoot + FireRate)
        {
            Shoot();
            _lastShoot = Time.time;
        }

        // ── Animaciones ─────────────────────────────────────
        if (_anim != null)
        {
            _anim.SetBool(IsWalkingHash,   false);   // el mago no camina
            _anim.SetBool(IsAttackingHash, inRange);  // ataca cuando ve al jugador
        }
    }

    private void Shoot()
    {
        if (OrbPrefab == null) return;

        // Dirección real hacia el jugador (no solo horizontal)
        Vector2 spawnPos = transform.position;
        Vector2 targetPos = Player.position;
        Vector2 dir = (targetPos - spawnPos).normalized;

        // Spawn del orbe ligeramente adelante del mago
        Vector2 offset = dir * 0.6f;
        GameObject orb = Instantiate(OrbPrefab, spawnPos + offset, Quaternion.identity);
        orb.GetComponent<OrbProjectile>()?.SetDirection(dir, OrbSpeed, OrbDamage, OrbLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    }
}
