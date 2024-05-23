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

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth > 0)
        {
            currentHealth -= damage;
            Debug.Log("Damage taken: " + damage);
            try
            {
                if (characterController != null)
                {
                    characterController.TakeDamage();
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
                characterController.Die();
            }
            catch
            {
                Die();
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

    public void Die() //for enemies
    {
        GetComponent<LurkerScript>().enabled = false;
        rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
        col.sharedMaterial = null;
        Invoke(nameof(WhitherAway), deathDuration);

        //Create a fade-away animation for enemies/characters without character controller (možná bude fungovat když dostanou character controller script a bude je používat jen tento script a fade-away nebo problikání se vytvoøí v characterAnimatoru) same goes for flash
    }

    public void WhitherAway()
    {
        characterAnimator.JustDied();
    }
}