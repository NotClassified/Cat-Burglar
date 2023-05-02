using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMovement : MonoBehaviour
{
    Rigidbody2D rb;


    [Header("Jumping")]
    [SerializeField] float minJumpHeight;
    [SerializeField] float maxJumpHeight;

    [SerializeField] float minJumpCharge;
    [SerializeField] float maxJumpCharge;

    [SerializeField] float jumpGravityScale;
    [SerializeField] float fallGravityScale;

    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] float groundCheckLength;
    [SerializeField] LayerMask groundLayerMask;

    [SerializeField] Slider jumpIndicator;
    bool isJumping;

    [Header("Forward Movement")]
    [SerializeField] float maxSpeed;
    [SerializeField] float obstacleSpeedReduction;
    [SerializeField] float acceleration;
    float forwardSpeed;

    [Header("Damage")]
    [SerializeField] GameObject damageIndicator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    private void Start()
    {
        jumpIndicator.gameObject.SetActive(false);
        damageIndicator.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Jump());
        }

        forwardSpeed = Mathf.Lerp(forwardSpeed, maxSpeed, Time.deltaTime * acceleration);
        transform.position += Vector3.right * forwardSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    private void FixedUpdate()
    {
        if (rb.velocity.y > 0)
        {
            rb.gravityScale = jumpGravityScale;
        }
        else
        {
            rb.gravityScale = fallGravityScale;
        }

        if (transform.position.y < -5f)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    bool isGrounded()
    {
        if (Physics2D.Raycast(transform.position + groundCheckOffset, Vector2.down, groundCheckLength, groundLayerMask))
        {
            return true;
        }
        return false;
    }

    IEnumerator Jump()
    {
        isJumping = true;

        jumpIndicator.gameObject.SetActive(true);
        float jumpChargeTime = 0;
        float chargePercent = 0;
        while (Input.GetKey(KeyCode.Space) && jumpChargeTime < maxJumpCharge)
        {
            jumpChargeTime += Time.deltaTime;
            chargePercent = (jumpChargeTime - minJumpCharge) / (maxJumpCharge - minJumpCharge);

            jumpIndicator.value = chargePercent;
            yield return null;
        }

        while (!Input.GetKeyUp(KeyCode.Space)) //wait for jump button release
            yield return null;
        if (isGrounded())
        {
            float jumpHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, chargePercent);

            float jumpForce = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * jumpGravityScale) * -2) * rb.mass;

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            isJumping = false;

            while (isGrounded()) //wait for player to get air
                yield return null;

            while (!isGrounded()) //wait for player to land
                yield return null;

            if (!isJumping) //hide jump indicator if player isn't holding jump button
            {
                jumpIndicator.gameObject.SetActive(false);
            }
        }
        else
            jumpIndicator.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Obstacle"))
        {
            DamagePlayer();
        }
    }

    void DamagePlayer()
    {
        if (forwardSpeed - obstacleSpeedReduction > 0)
        {
            forwardSpeed -= obstacleSpeedReduction;
        }
        else if (forwardSpeed != 0)
            forwardSpeed = 0;

        damageIndicator.SetActive(true);
        Invoke("HideDamageIndicator", .1f);
    }
    void HideDamageIndicator() => damageIndicator.SetActive(false);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        //ground check:
        Gizmos.DrawRay(transform.position + groundCheckOffset, Vector3.down * groundCheckLength);
    }
}
