using UnityEditor.Build;
using UnityEngine;

public class LurkerScript : MonoBehaviour
{
    [SerializeField] private GameObject wpL;
    [SerializeField] private GameObject wpR;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform targetPoint;
    public float speed = 20f;
    public float runAccelAmount = 10;
    public float runDeccelAmount = 10;
    private bool facingLeft;
    //doesnt need Animator, cuz it only has 1 anim

    void Start()
    {
        targetPoint = wpR.transform;
    }

    void Update()
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

    private void Flip()
    {
        facingLeft = !facingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
