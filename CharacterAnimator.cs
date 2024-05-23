using System.Collections;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private string currentAnim;
    [SerializeField] private SpriteRenderer spriteRend;
    [SerializeField] public float flashDuration = 1.0f;
    [SerializeField] public Color flashColor = Color.white;
    protected Color originalColor;
    private float fadeTimer = 0f;
    private float alpha;
    public bool isDead = false;
    [SerializeField] float fadeDuration = 3f;

    private void Awake()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        originalColor = spriteRend.color;
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (isDead == true)
        {
            fadeTimer += Time.deltaTime;
            alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            spriteRend.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            if (fadeTimer >= fadeDuration)
            {
                Destroy(gameObject);
            }
        }
    }

    public void JustDied()
    {
        isDead = true;
    }

    public void ChangeAnimation(string newAnim)
    {
        if (currentAnim != "Player_Die")
        {
            if (newAnim == "flash")
            {
                StartCoroutine(Flash(spriteRend, flashDuration, flashColor));
            }
            else if (currentAnim != newAnim)
            {
                animator.Play(newAnim);
                currentAnim = newAnim;
            }
        } 
        else
        {
            JustDied();
        }
    }

    private IEnumerator Flash(SpriteRenderer spriteRend, float duration, Color color)
    {
        spriteRend.color = color;
        yield return new WaitForSeconds(duration);
        spriteRend.color = originalColor;
    }
}