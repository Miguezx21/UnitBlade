using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator _anim;
    private Rigidbody2D _rb;

    private static readonly int SpeedHash     = Animator.StringToHash("Speed");
    private static readonly int GroundedHash  = Animator.StringToHash("IsGrounded");
    private static readonly int AttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int AxeModeHash   = Animator.StringToHash("IsAxeMode");
    private static readonly int JumpTrigHash  = Animator.StringToHash("JumpTriger");
    private static readonly int IsDeadHash    = Animator.StringToHash("IsDead"); // typo original del Animator Controller

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb   = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _anim.SetFloat(SpeedHash, Mathf.Abs(_rb.linearVelocity.x));
    }

    public void SetGrounded(bool value)  => _anim.SetBool(GroundedHash,  value);
    public void SetAttacking(bool value) => _anim.SetBool(AttackingHash, value);
    public void SetAxeMode(bool value)   => _anim.SetBool(AxeModeHash,   value);
    public void SetTakingDamage(bool value) { /* sin parámetro en el Animator por ahora */ }
    public void SetDead(bool value)
    {
        foreach (var p in _anim.parameters)
            if (p.nameHash == IsDeadHash) { _anim.SetBool(IsDeadHash, value); return; }
        // Si no existe el parámetro IsDead en el Animator, lo ignora silenciosamente
    }
    public void OnJump() => _anim.SetTrigger(JumpTrigHash);
}
