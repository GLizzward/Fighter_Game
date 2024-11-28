using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    float horizontalInput;
    float moveSpeed = 5f;
    float jumpPower = 7f;
    bool isGrounded = false;
    bool isDashing = false;

    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    private float dashTime;

    // Reference to PunchHitbox and AerialHitbox
    public GameObject punchHitbox;
    public GameObject aerialHitbox;
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Ensure PunchHitbox and AerialHitbox are found in the children
        if (punchHitbox == null)
        {
            punchHitbox = transform.Find("PunchHitbox")?.gameObject;
            if (punchHitbox == null)
            {
                Debug.LogError("PunchHitbox not found as child object.");
            }
        }

        if (aerialHitbox == null)
        {
            aerialHitbox = transform.Find("AerialHitbox")?.gameObject;
            if (aerialHitbox == null)
            {
                Debug.LogError("AerialHitbox not found as child object.");
            }
        }

        // Start with both hitboxes hidden
        if (punchHitbox != null)
        {
            punchHitbox.SetActive(false);
        }
        if (aerialHitbox != null)
        {
            aerialHitbox.SetActive(false);
        }
    }

    void Update()
    {
        if (!isDashing)
        {
            horizontalInput = Input.GetAxis("Horizontal");

            // Handle Jump
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                isGrounded = false;
                animator.SetBool("isJumping", !isGrounded);
            }

            // Trigger Aerial Attack when "K" is pressed in the air
            if (Input.GetKeyDown(KeyCode.K) && !isGrounded)  // Check if in the air
            {
                Debug.Log("Aerial Attack Triggered");
                PlayAerialAttack();  // Trigger the aerial attack animation
            }

            // Trigger Yoga Kick animation when "K" is pressed and grounded
            if (Input.GetKeyDown(KeyCode.K) && isGrounded)
            {
                PlayKick();
            }

            // Trigger Yoga Medium Punch animation when "P" is pressed
            if (Input.GetKeyDown(KeyCode.P))
            {
                PlayPunch();
            }

            // Trigger Yoga Spear when "J" is pressed and grounded
            if (Input.GetKeyDown(KeyCode.J) && isGrounded)
            {
                PlaySpear();
            }

            // Trigger Dash
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                StartDash();
            }
        }

        if (isDashing)
        {
            DashUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
            animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
            animator.SetFloat("yVelocity", rb.velocity.y);
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTime = dashDuration;

        // Always dash forward in the current facing direction
        float dashDirection = transform.localScale.x > 0 ? 1 : 1;
        rb.velocity = new Vector2(dashDirection * dashSpeed, rb.velocity.y);

        // Trigger dash animation
        animator.SetTrigger("Dash");
    }

    private void DashUpdate()
    {
        dashTime -= Time.deltaTime;

        if (dashTime <= 0)
        {
            isDashing = false;
            rb.velocity = new Vector2(0, rb.velocity.y); // Stop dash movement
        }
    }

    private void PlayKick()
    {
        animator.SetTrigger("Kick");
    }

    private void PlayPunch()
    {
        animator.SetTrigger("Punch");

        // Activate PunchHitbox after a delay at the end of the punch animation
        if (punchHitbox != null)
        {
            StartCoroutine(ActivateHitboxAtEndOfAnimation(punchHitbox));
        }
    }

    private void PlaySpear()
    {
        animator.SetTrigger("Spear");
    }

    // New function for Aerial Attack
    private void PlayAerialAttack()
    {
        animator.SetTrigger("AerialAttack");

        // Activate the AerialHitbox during the aerial attack animation
        if (aerialHitbox != null)
        {
            StartCoroutine(ActivateAerialHitboxAtEndOfAnimation());
        }
    }

    // Coroutine to activate PunchHitbox at the end of the punch animation
    private IEnumerator ActivateHitboxAtEndOfAnimation(GameObject hitbox)
    {
        // Wait for the animation to reach the right time (adjust the time to match the punch animation duration)
        float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration * 0.9f); // Wait until 90% of the animation is done

        // Now activate the hitbox
        hitbox.SetActive(true);
        StartCoroutine(DeactivateHitbox(hitbox));
    }

    // Coroutine to activate AerialHitbox at the right time during the aerial attack animation
    private IEnumerator ActivateAerialHitboxAtEndOfAnimation()
    {
        // Wait for the aerial attack animation to play for a certain duration
        float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration * 0.6f);  // Adjust as needed

        // Activate AerialHitbox
        if (aerialHitbox != null)
        {
            aerialHitbox.SetActive(true);
            StartCoroutine(DeactivateHitbox(aerialHitbox));
        }
    }

    // Coroutine to deactivate the hitbox after a brief period
    private IEnumerator DeactivateHitbox(GameObject hitbox)
    {
        yield return new WaitForSeconds(0.5f);
        hitbox.SetActive(false);
    }

    // Combined OnTriggerEnter2D method to handle both collisions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ground detection
        if (collision.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isJumping", false);
        }

        // Collision detection for PunchHitbox and AerialHitbox hitting enemies
        if (collision.CompareTag("Enemy"))
        {
            if (punchHitbox.activeSelf)
            {
                Debug.Log("Enemy hit by PunchHitbox!");
                collision.gameObject.GetComponent<EnemyScript>().TakeDamage(5); // Adjust damage as needed
            }
            if (aerialHitbox.activeSelf)
            {
                Debug.Log("Enemy hit by AerialHitbox!");
                collision.gameObject.GetComponent<EnemyScript>().TakeDamage(10); // Adjust damage for aerial attack
            }
        }
    }
}