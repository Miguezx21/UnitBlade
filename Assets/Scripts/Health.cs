using System.Collections;
using UnityEngine;

/// <summary>
/// Vida de un enemigo. Recibe daño con un elemento; si coincide con su
/// debilidad, el daño se triplica (counter elemental de la matriz de Unit Blade).
/// Al morir se desactiva para que EnemyGroupTracker revele la runa.
/// </summary>
public class Health : MonoBehaviour
{
    public int maxHP = 3;
    public bool hasWeakness = true;
    public ElementType weakTo = ElementType.Isa;

    private int hp;
    private SpriteRenderer sr;
    private Animator anim;
    private bool dead;

    private void Awake()
    {
        hp = maxHP;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int dmg, ElementType element)
    {
        if (dead) return;

        int final = dmg;
        bool countered = hasWeakness && element == weakTo;
        if (countered) final *= 3;

        hp -= final;
        Debug.Log("[UnitBlade] " + name + " recibe " + final + (countered ? " (¡COUNTER!)" : "") + " | HP: " + Mathf.Max(0, hp));

        if (gameObject.activeInHierarchy) StartCoroutine(Flash(countered));

        if (hp <= 0) Die();
    }

    private void Die()
    {
        dead = true;
        if (anim != null) anim.SetBool("IsDead", true);
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
        Destroy(gameObject, 0.4f);
    }

    private IEnumerator Flash(bool countered)
    {
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = countered ? Color.yellow : Color.white;
        yield return new WaitForSeconds(0.08f);
        if (sr != null) sr.color = original;
    }
}
