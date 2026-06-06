using System.Collections.Generic;
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
    private static readonly int IsDeadHash    = Animator.StringToHash("IsDead");
    private static readonly int HitHash       = Animator.StringToHash("Hit");
    private static readonly int ElementHash   = Animator.StringToHash("Element"); // 0=Pira 1=Isa 2=Steinn 3=Thorn

    // Solo intentamos escribir parámetros que existan realmente en el controller.
    private readonly HashSet<int> _params = new HashSet<int>();

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb   = GetComponent<Rigidbody2D>();
        CacheParams();
    }

    private void CacheParams()
    {
        _params.Clear();
        if (_anim.runtimeAnimatorController == null) return;
        foreach (var p in _anim.parameters) _params.Add(p.nameHash);
    }

    private void Update()
    {
        if (_params.Contains(SpeedHash))
            _anim.SetFloat(SpeedHash, Mathf.Abs(_rb.linearVelocity.x));

        // El ataque cambia según la runa equipada (color de la espada).
        if (_params.Contains(ElementHash) && PlayerStats.Instance != null)
            _anim.SetInteger(ElementHash, (int)PlayerStats.Instance.CurrentElement);
    }

    public void SetGrounded(bool value)  { if (_params.Contains(GroundedHash))  _anim.SetBool(GroundedHash, value); }
    public void SetAttacking(bool value) { if (_params.Contains(AttackingHash)) _anim.SetBool(AttackingHash, value); }
    public void SetAxeMode(bool value)   { if (_params.Contains(AxeModeHash))   _anim.SetBool(AxeModeHash, value); }
    public void SetTakingDamage(bool value) { if (value && _params.Contains(HitHash)) _anim.SetTrigger(HitHash); }
    public void SetDead(bool value)      { if (_params.Contains(IsDeadHash))    _anim.SetBool(IsDeadHash, value); }
    public void OnJump()                 { if (_params.Contains(JumpTrigHash))  _anim.SetTrigger(JumpTrigHash); }
}
