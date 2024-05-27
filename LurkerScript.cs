using UnityEditor.Build;
using UnityEngine;

public class LurkerScript : MonoBehaviour
{
    [SerializeField] private GameObject wpL;
    [SerializeField] private GameObject wpR;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform targetPoint;
    [SerializeField] public float defaultAttackDamage = 40;
    [SerializeField] public float defaultAttackKnockback = 40;
    public float speed = 10f;
    public float runAccelAmount = 10;
    public float runDeccelAmount = 10;
    private bool facingLeft;
    public bool colDis = false;
    public bool isGrounded = false;
    public Collider2D groundCheckCollider;
    [SerializeField] private LayerMask ground;
    //doesnt need Animator, cuz it only has 1 anim

    void Start()
    {
        targetPoint = wpR.transform;
    }

    void Update()
    {
        float raycastDistance = groundCheckCollider.bounds.extents.y + 0.01f; //raycastDistance is equal to a half of the groundCheckCollider (it is slightly bigger, so it doesn't bug)
        RaycastHit2D raycastHit = Physics2D.BoxCast(groundCheckCollider.bounds.center, groundCheckCollider.bounds.size, 0f, Vector2.down, raycastDistance, ground);
        #region RAY DRAWOUT (may or may not work, hard to tell)
        /*
        Color rayColor;
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(groundCheckCollider.bounds.center + new Vector3(groundCheckCollider.bounds.extents.x, 0), Vector2.down * raycastDistance, rayColor, 1, false);
        Debug.DrawRay(groundCheckCollider.bounds.center - new Vector3(groundCheckCollider.bounds.extents.x, 0), Vector2.down * raycastDistance, rayColor, 1, false);
        Debug.DrawRay(groundCheckCollider.bounds.center - new Vector3(groundCheckCollider.bounds.extents.x, groundCheckCollider.bounds.extents.y), Vector2.right * (groundCheckCollider.bounds.extents.x * 2f), rayColor, 1, false);
        */
        #endregion

        if (raycastHit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        Vector2 point = targetPoint.position - transform.position;
        if (targetPoint == wpR.transform)
        {
            Move(1);
        }
        else
        {
            Move(-1);
        }

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f && targetPoint == wpR.transform)
        {
            targetPoint = wpL.transform;
        }
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f && targetPoint == wpL.transform)
        {
            targetPoint = wpR.transform;
        }
    }

    public void Move(float move)
    {
        if (isGrounded == true)
        {
            float targetSpeed = move * speed;
            targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, 1); //Lerp returns a value between two others at a point on a linear scale

            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount; //ACCELRATE CALCULATION

            float speedDif = targetSpeed - rb.velocity.x; //Calculate difference between current velocity and desired velocity
            float movement = speedDif * accelRate; //Calculate force along x-axis to apply to the player
            rb.AddForce(movement * Vector2.right, ForceMode2D.Force); //Convert this to a vector and apply to rigidbody

            //FLIP
            if (move < 0 && !facingLeft)
            {
                Flip();
            }
            else if (move > 0 && facingLeft)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        defaultAttackKnockback *= -1;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && colDis == false)
        {
            collision.gameObject.GetComponent<HealthScript>().TakeHit(defaultAttackDamage / 2, defaultAttackKnockback); //damage is divided by 2 cuz it is always dealt twice for some unknown reason
        }
    }
}
