using System.Collections;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    private string currentAnim;
    private SpriteRenderer spriteRend;
    public float flashDuration = 1.0f;
    public Color flashColor = Color.white;
    protected Color originalColor;

    private void Awake()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        originalColor = spriteRend.color;
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimation(string newAnim)
    {
        if (currentAnim != newAnim && newAnim == "flash")
        {
            StartCoroutine(Flash(spriteRend, flashDuration, flashColor));
        }
        else if (currentAnim != newAnim)
        {
            animator.Play(newAnim);
            currentAnim = newAnim;
        }
    }

    private IEnumerator Flash(SpriteRenderer spriteRend, float duration, Color color)
    {
        spriteRend.color = color;
        yield return new WaitForSeconds(duration);
        spriteRend.color = originalColor;
    }
}
