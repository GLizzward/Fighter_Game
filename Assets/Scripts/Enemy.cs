using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour
{
    public int health = 10;
    private Animator animator;
    private Rigidbody2D rb;
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.5f;
    private bool isKnockedBack = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy took damage! Health: " + health);

        if (animator != null)
        {
            animator.SetTrigger("Was Hit");
            Debug.Log("Triggering Was Hit animation.");
        }

        if (!isKnockedBack)
        {
            StartCoroutine(ApplyKnockback());
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PunchHitbox"))
        {
            TakeDamage(5);
        }
        else if (collision.CompareTag("AerialHitbox"))
        {
            TakeDamage(10);  // Higher damage for aerial attack
        }
    }

    private IEnumerator ApplyKnockback()
    {
        isKnockedBack = true;
        rb.velocity = new Vector2(rb.velocity.x, knockbackForce);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }
}