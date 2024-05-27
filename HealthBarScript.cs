using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private HealthScript healthScript;
    [SerializeField] private Image currentHealth;

    void Update()
    {
        currentHealth.fillAmount = healthScript.currentHealth / 100;
    }
}