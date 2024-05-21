// Tadeáš Vykopal, 3.B, PVA, Shadow Slayer

using System.Collections;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    #region VARIABLES
    [Header("Current state variables")] //testing purposes only
    public float currentVelocityX; //testing purposes only
    public float currentGravity; //testing purposes only

    [Header("Movement variables")]
    public bool doConserveMomentum = true;
    public float maxRunSpeed = 20f;
    public float lerpAmount = 1; //whatever that is (it works tho)
    public float runAccelAmount = 10;
    public float runDeccelAmount = 5;
    [Range(0f, 1)] public float accelInAir = 1;
    [Range(0f, 1)] public float deccelInAir = 1;
    [Space(20f)]
    [SerializeField] private Collider2D characterCollider;
    [SerializeField] private Rigidbody2D rb;

    [Header("Jump variables")]
    public float jumpHeight = 10f;
    public int doubleJumps = 3; //not currently in use
    [HideInInspector] public float jumpforce = 1.0f; //is calculated automatically depending on jumpHeight

    [Header("Gravity variables")]
    public float fallGravityMult = 1.8f;
    public float jumpFallGravityMult = 2f;
    public float maxFallSpeed = 40f;
    [HideInInspector] public float originalGravity;
    [HideInInspector] public bool gravityChanged = false;

    [Header("States")]
    public bool isGrounded = false;
    public bool isJumping = false;
    public bool isJumpFalling = false;
    public bool isFalling = false;
    public bool isIdle = false;
    public float idleTransitionZone = 1.5f;
    public bool isRunning = false;
    public bool isGettingDamaged = false;
    public bool isDead = false;
    GameObject currentPlatform;
    [HideInInspector] public bool facingLeft;

    [Header("Timers")]
    public float coyoteTimer = 0.5f;
    [HideInInspector] public float lastOnGroundTime;

    [Header("Ground check variables")]
    public Collider2D groundCheckCollider;
    [SerializeField] private LayerMask ground;

    [Header("Combat variables")]


    [Header("Animation variables")]
    public CharacterAnimator characterAnimator;
    //aren't required, but simplify coding
    const string flash = "flash";
    const string playerAnimIdle = "Player_Idle";
    const string playerAnimRun = "Player_Run";
    const string playerAnimJump = "Player_Jump";
    const string playerAnimFall = "Player_Fall";
    const string playerAnimDie = "Player_Die";
    const string playerAnimAttack1 = "Player_Attack1";
    const string playerAnimAttack2 = "Player_Attack2";
    const string playerAnimAttack3 = "Player_Attack3";
    const string playerAnimAirAttack1 = "Player_AirAttack1";
    const string playerAnimAirAttack2 = "Player_AirAttack2";
    const string playerAnimAirAttack3 = "Player_AirAttack3";
    const string playerAnimAirAttack3Loop = "Player_AirAttack3_loop";
    /*
    Should use this superthing
    Invoke("AttackComplete", animator.GetCurrentAnimatorStateInfo(0).length);
    */
    #endregion

    private void Awake()
    {
        originalGravity = rb.gravityScale;
    }

    public void Update()
    {
        currentVelocityX = rb.velocity.x; //testing purposes only
        currentGravity = rb.gravityScale; //testing purposes only
        lastOnGroundTime -= Time.deltaTime;

        #region STATE CHECKS

        //(not only) GROUND CHECK
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
        //Debug.Log(raycastHit.collider);
        if (raycastHit.collider != null)
        {
            isGrounded = true;
            isJumpFalling = false;
            isFalling = false;
            lastOnGroundTime = coyoteTimer;
            AdjustGravity("original");
        }
        else
        {
            isGrounded = false;
        }

        if (isJumping && rb.velocity.y < 0)
        {
            isJumping = false;
            isJumpFalling = true;
        }

        if (rb.velocity.y < 0 && isJumpFalling == false)
        {
            isFalling = true;
        }

        if (isGrounded == true)
        {
            if (rb.velocity.x > -idleTransitionZone && rb.velocity.x < idleTransitionZone)
            {
                isRunning = false;
                isIdle = true;
            }
            else if (rb.velocity.x != 0)
            {
                isIdle = false;
                isRunning = true;
            }
        }
        else
        {
            isIdle = false;
            isRunning = false;
        }

        #endregion

        #region GRAVITY CHANGES FOR JUMP & JUMP-FALL
        //ENCREASE GRAVITY WHEN FALLING
        if (rb.velocity.y < 0 && isJumpFalling == false)
        {
            AdjustGravity("fallIncrease");
        }
        else if (rb.velocity.y < 0 && isJumpFalling == true)
        {
            AdjustGravity("jumpFallIncrease");
        }
        //LIMIT MAX FALL SPEED OF THE RIGIDBODY
        if (rb.velocity.y <= -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        #endregion

        #region ANIMATION HANDLELER
        if (isIdle == true)
        {
            characterAnimator.ChangeAnimation(playerAnimIdle);
        }
        else if (isRunning == true)
        {
            characterAnimator.ChangeAnimation(playerAnimRun);
        }
        else if (isJumping == true)
        {
            characterAnimator.ChangeAnimation(playerAnimJump);
        }
        else if (isJumpFalling == true || isFalling == true)
        {
            characterAnimator.ChangeAnimation(playerAnimFall);
        }
        else if (isGettingDamaged == true)
        {
            characterAnimator.ChangeAnimation(flash);
            isGettingDamaged = false;
        }
        else if (isDead == true)
        {
            characterAnimator.ChangeAnimation(playerAnimDie);
        }
        #endregion
    }

    #region MOVEMENT
    public void Move(float move)
    {
        float targetSpeed = move * maxRunSpeed;
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount); //Lerp returns a value between two others at a point on a linear scale

        //ACCELRATE CALCULATION
        //Gets an acceleration value based on if we are accelerating (includes turning)
        //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
        float accelRate;
        if (lastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount * accelInAir : runDeccelAmount * deccelInAir;

        //CONSERVE MOMENTUM - if player is moving higher than max speed & is airborne
        if ((doConserveMomentum && Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed)) && (Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed)) && (Mathf.Abs(targetSpeed) > 0.01f) && (lastOnGroundTime < 0))
        {
            accelRate = 0;
        }

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

    public void Jump()
    {
        if (lastOnGroundTime == coyoteTimer)
        {
            jumpforce = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * rb.gravityScale) * -2) * rb.mass;
            rb.AddForce(Vector2.up * jumpforce, ForceMode2D.Impulse);
            isJumping = true;
        }
    }
    #endregion

    #region STATE ADJUSTMENTS
    public void AdjustGravity(string state)
    {
        if (state == "fallIncrease" && gravityChanged == false)
        {
            rb.gravityScale *= fallGravityMult;
            gravityChanged = true;
        }
        else if (state == "jumpFallIncrease" && gravityChanged == false)
        {
            rb.gravityScale *= jumpFallGravityMult;
            gravityChanged = true;
        }
        else if (state == "original" && gravityChanged == true)
        {
            rb.gravityScale = originalGravity;
            gravityChanged = false;
        }
    }

    private void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage()
    {
        isGettingDamaged = true;
    }

    public void Die()
    {
        isDead = true;
        GetComponent<CharacterController2D>().enabled = false;
    }
    #endregion

    #region GO DOWN FROM ONE WAY PLATFORM
    public void GoDownFromPlatform()
    {
        if (currentPlatform != null)
        {
            StartCoroutine(DisableCollision());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("oneWayTag"))
        {
            currentPlatform = collision.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("oneWayTag"))
        {
            currentPlatform = null;
        }
    }

    public IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentPlatform.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(characterCollider, platformCollider);
        yield return new WaitForSeconds(0.25f);
        Physics2D.IgnoreCollision(characterCollider, platformCollider, false);
    }
    #endregion

    #region COMBAT

    public void SwordAttack()
    {
        characterAnimator.ChangeAnimation(playerAnimAttack1);
    }

    #endregion
}

/*
#region GIZMOS
private void OnDrawGizmosSelected()
{
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
}
#endregion
*/