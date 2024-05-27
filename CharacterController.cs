// Tadeáš Vykopal, 3.B, PVA, Shadow Slayer

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class CharacterController2D : MonoBehaviour
{
    #region VARIABLES
    [Header("Current state variables")] //testing purposes only
    public float currentVelocityX; //testing purposes only
    public float currentGravity; //testing purposes only

    [Header("Movement variables")]
    public bool doConserveMomentum = true;
    [SerializeField] public float maxRunSpeed = 15f;
    [HideInInspector] public float lerpAmount = 1; //whatever that is (it works tho)
    public float runAccelAmount = 10;
    public float runDeccelAmount = 5;
    [Range(0f, 1)] public float accelInAir = 1;
    [Range(0f, 1)] public float deccelInAir = 1;
    [Space(20f)]
    [SerializeField] private Collider2D characterCollider;
    [SerializeField] private Rigidbody2D rb;
    Vector2 lockPos;

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
    public bool isRunning = false;
    public bool isGettingDamaged = false;
    public bool isDead = false;
    public bool isAttacking = false;
    public bool isChargingAttack = false;
    public bool isPosLocked = false;
    GameObject currentPlatform;
    [HideInInspector] public bool facingLeft;

    [Header("Timers")]
    [SerializeField] public float idleTransitionZone = 1.5f;
    [SerializeField] public float coyoteTimer = 0.1f;
    [SerializeField] public float comboTimer = 0.2f;
    [SerializeField] public float lastOnGroundTime;
    [SerializeField] public float lastAttackedTime;

    [Header("Ground check variables")]
    [SerializeField] private LayerMask ground;
    public Collider2D groundCheckCollider;

    [Header("Combat variables")]
    [SerializeField] Vector2 attackPoint;
    public int combo = 1;
    public float attackRange = 1.3f;
    public float attackChargeDuration = 1f;
    public LayerMask enemyLayer;
    [SerializeField] private Camera mainCamera;
    [SerializeField] public float defaultAttackDamage = 20;
    [SerializeField] public float defaultAttackKnockback = 20;


    [Header("Animation variables")]
    public CharacterAnimator characterAnimator;
    public Animator animator;
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
    const string playerAnimSwordDraw = "Player_SwordDraw";
    #endregion

    private void Awake()
    {
        originalGravity = rb.gravityScale;
        attackPoint = transform.position;
    }

    public void Update()
    {
        currentVelocityX = rb.velocity.x; //testing purposes only
        currentGravity = rb.gravityScale; //testing purposes only
        lastOnGroundTime -= Time.deltaTime;
        lastAttackedTime -= Time.deltaTime;

        #region DEATH POSITIONLOCK
        if ((isDead == true || isPosLocked == true) && isGrounded == true)
        {
            transform.position = lockPos;
        }
        #endregion

        #region STATE CHECKS
        //need a remake into separate methods

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

        if (rb.velocity.y < 0 && isJumpFalling == false && lastOnGroundTime < 0)
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
        if (rb.velocity.y < 0 && isJumpFalling == false && isJumping == false && lastOnGroundTime < 0)
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
        if (isDead == true)
        {
            characterAnimator.ChangeAnimation(playerAnimDie);
        }
        else
        {
            if (isGettingDamaged == true)
            {
                characterAnimator.ChangeAnimation(flash);
                isGettingDamaged = false;
            }
            else if (isAttacking == false)
            {
                if (isIdle == true)
                {
                    characterAnimator.ChangeAnimation(playerAnimIdle);
                }
                else if (isRunning == true && isAttacking == false)
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
            }
            else if (isChargingAttack == true && isJumping == false && isJumpFalling == false)
            {
                characterAnimator.ChangeAnimation(playerAnimSwordDraw);
            }
            else if (isAttacking == true && isJumping == false && isJumpFalling == false)
            {
                if (combo == 1)
                {
                    characterAnimator.ChangeAnimation(playerAnimAttack1);
                }
                else if (combo == 2)
                {
                    characterAnimator.ChangeAnimation(playerAnimAttack2);
                }
                else if (combo == 3)
                {
                    characterAnimator.ChangeAnimation(playerAnimAttack3);
                }
            }
            else if (isAttacking == true && (isJumping == true || isJumpFalling == true || isFalling == true || isGrounded == false))
            {
                if (combo == 1)
                {
                    characterAnimator.ChangeAnimation(playerAnimAirAttack1);
                }
                else if (combo == 2)
                {
                    characterAnimator.ChangeAnimation(playerAnimAirAttack2);
                }
                else if (combo == 3)
                {
                    characterAnimator.ChangeAnimation(playerAnimAirAttack3Loop);
                }
                else
                {
                    characterAnimator.ChangeAnimation(playerAnimAirAttack3);
                }
            }
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

        //CALL FLIP
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
        if (lastOnGroundTime > 0)
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
        defaultAttackKnockback *= -1;
        transform.localScale = scale;
    }

    public IEnumerator Die(float direction)
    {
        yield return new WaitUntil(() => GetComponent<HealthScript>().GetVelocity(direction));
        isDead = true;
        lockPos = transform.position;
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
    //needs to remake into separate script later
    #region SWORD ATTACK
    public IEnumerator SwordAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            if (lastAttackedTime > 0 && combo < 3)
            {
                combo++;
            }
            else
            {
                combo = 1;
            }

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / 3);

            if (FindMouseWorldPos().x >= transform.position.x && facingLeft == true)
            {
                Flip();
            }
            else if (FindMouseWorldPos().x < transform.position.x && facingLeft == false)
            {
                Flip();
            }

            Collider2D[] hit = Physics2D.OverlapCircleAll(FindSwordAttackPoint(), attackRange, enemyLayer);

            foreach (Collider2D enemy in hit)
            {
                enemy.GetComponent<HealthScript>().TakeHit(defaultAttackDamage / 2, defaultAttackKnockback); //damage is divided by 2 cuz it is always dealt twice for some unknown reason
            }

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / 3 * 2);

            isAttacking = false;
            lastAttackedTime = comboTimer;
        }
    }
    #endregion

    #region CHARGED ATTACK

    public IEnumerator ChargedAttack(InputAction.CallbackContext context)
    {
        if (FindMouseWorldPos().x >= transform.position.x && facingLeft == true)
        {
            Flip();
        }
        else if (FindMouseWorldPos().x < transform.position.x && facingLeft == false)
        {
            Flip();
        }

        isPosLocked = true;
        isChargingAttack = true;

        yield return new WaitForSeconds((float)context.duration);

        isPosLocked = false;
        isChargingAttack = false;

        if (context.duration < attackChargeDuration)
        {
            StartCoroutine(SwordAttack());
        }
        else if (context.duration >= attackChargeDuration)
        {

        }
    }

    #endregion


    #region BOW ATTACK
    //coming soon
    #endregion

    private Vector2 FindSwordAttackPoint()
    {
        if (FindMouseWorldPos().x >= transform.position.x)
        {
            attackPoint = new Vector2(transform.position.x + attackRange / 2, transform.position.y);
        }
        else if (FindMouseWorldPos().x < transform.position.x)
        {
            attackPoint = new Vector2(transform.position.x - attackRange / 2, transform.position.y);
        }
        //might add more (up to 8 directions) attack points in the future, but need anims for that
        return attackPoint; //attack point = direction of the attack
    }

    private Vector3 FindMouseWorldPos()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        return mouseWorldPos;
    }

    #endregion

    #region GIZMOS
    /*
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
    */

    private void OnDrawGizmosSelected()
    {
        try
        {
            Gizmos.DrawWireSphere(attackPoint, attackRange);
        }
        catch
        {
            Debug.Log("Failed to draw attack wire sphere");
        }
    }
    #endregion
}