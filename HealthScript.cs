using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public CharacterController2D characterController;
    public CharacterAnimator characterAnimator;
    public float maxHealth;
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
            try
            {
                characterController.TakeDamage();
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
                //Create a fade-away animation for enemies/characters without character controller (mo�n� bude fungovat kdy� dostanou character controller script a bude je pou��vat jen tento script a fade-away nebo problik�n� se vytvo�� v characterAnimatoru) same goes for flash
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
}
