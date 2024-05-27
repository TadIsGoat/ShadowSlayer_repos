using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public CharacterController2D characterController;
    public CharacterAnimator characterAnimator;
    public Rigidbody2D rb;
    public Collider2D col;
    public float maxHealth;
    public float deathDuration = 5f;
    public float currentHealth { get; private set; }

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeHit(float damage, float knockback)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        Vector2 knockbackForce = knockback * Vector2.right;
        knockbackForce.y = knockback / 10;
        rb.AddForce(knockbackForce, ForceMode2D.Impulse);

        if (currentHealth > 0)
        {
            currentHealth -= damage;
            try
            {
                if (characterController != null)
                {
                    characterController.isGettingDamaged = true;
                }
            }
            catch
            {
                characterAnimator.ChangeAnimation("flash");
            }
        }
        else if (currentHealth <= 0)
        {
            currentHealth = 0;
            try
            {
                StartCoroutine(characterController.Die(knockback));
            }
            catch
            {
                StartCoroutine(Die(knockback));
            }
        }
    }

    public void GainHealth(float health)
    {
        currentHealth = Mathf.Clamp(currentHealth + health, 0, maxHealth);
        if (currentHealth > 0)
        {
            currentHealth += health;
        }
    }

    public IEnumerator Die(float direction) //for enemies
    {
        rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
        col.sharedMaterial = null;

        yield return new WaitUntil(() => GetVelocity(direction));

        GetComponent<LurkerScript>().enabled = false;
        GetComponent<LurkerScript>().colDis = true;
        yield return new WaitForSeconds(deathDuration);

        characterAnimator.JustDied();
    }

    public bool GetVelocity(float direction)
    {
        if (direction < 0 && rb.velocity.x >= direction / 4)
        {
            return true;
        }
        else if (direction > 0 && rb.velocity.x <= direction / 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}