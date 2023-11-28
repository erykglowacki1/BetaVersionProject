using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public int numberOfLanes = 3;
    public float speed = 10.0f;
    public float floorWidth = 20.0f;
    public float laneWidth;
    private Animator playerAnim;
    private float currentLane = 1;
    private Rigidbody playerRb;
    public float jumpForce;
    public bool isOnGround = true;
    public float gravityModifier;
    public bool gameOver = false;
    public Texts texts;
    public Button restartButton;
    public Button returnMenuButton;
    private bool isInvincible = false;
    private float powerupDuration = 5.0f;
    private float powerupTimer;
    


    private int doubleJumpCount = 1;  // Set to 1 for initial double jump ability
    private int currentDoubleJumps = 0;  // Keep track of the number of double jumps used

    // Add these variables
    private bool doubleJumpAvailable = false;
    private float doubleJumpDuration = 5.0f;
    private float doubleJumpTimer = 0.0f;

    private bool isCrouching = false;
    private float originalHeight;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        Physics.gravity *= gravityModifier;
        playerAnim = GetComponent<Animator>();

        laneWidth = floorWidth / numberOfLanes;
        originalHeight = transform.localScale.y;
    }
    

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0 && !isCrouching)
        {
            currentLane--;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < numberOfLanes - 1 && !isCrouching)
        {
            currentLane++;
        }

        float targetX = currentLane * laneWidth - floorWidth / 2.0f + laneWidth / 2.0f;
        transform.position = Vector3.Lerp(transform.position, new Vector3(targetX, transform.position.y, transform.position.z), Time.deltaTime * speed);

        float clampedX = Mathf.Clamp(transform.position.x, -floorWidth / 2.0f + laneWidth / 2.0f, floorWidth / 2.0f - laneWidth / 2.0f);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        transform.Translate(Vector3.right * horizontalInput * Time.deltaTime * speed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOnGround || (doubleJumpAvailable && doubleJumpTimer > 0))
            {
                playerRb.velocity = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z);
                playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                playerAnim.SetBool("Jump_b", true);

                if (!isOnGround)
                {
                    doubleJumpAvailable = false;
                    doubleJumpTimer = 0.0f;
                    texts.doubleJumpPowerupText.gameObject.SetActive(false);
                    playerAnim.SetBool("Jump_b", false);
                }

                isOnGround = false;
                
            }
            
        }

        if (Input.GetKeyUp(KeyCode.Space)) {
            playerAnim.SetBool("Jump_b", false);
        }
        

        // Check for crouch input
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (isOnGround)
            {
                // Crouch if on the ground
                isCrouching = true;
                Crouch();
            }
            else if (!isOnGround && !isCrouching)
            {
                // Descend more quickly when crouching while in the air
                playerRb.velocity = new Vector3(playerRb.velocity.x, -jumpForce, playerRb.velocity.z);
                isCrouching = true;
                Crouch();
            }
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow) && isCrouching)
        {
            // Uncrouch when crouch key is released
            isCrouching = false;
            Uncrouch();
        }

        if (isCrouching)
        {
            // Additional actions when continuously crouching
            // Example: Decrease speed when continuously crouching
            speed /= 2.0f;
        }
        else
        {
            // Restore speed when not crouching
            speed = 10.0f;
        }

        if (isInvincible)
        {
            powerupTimer -= Time.deltaTime;
            powerupTimer = Mathf.Max(0, powerupTimer);

            texts.InvincibilityText(powerupTimer);
            if (powerupTimer <= 0)
            {
                isInvincible = false;
                texts.invincibilityText.gameObject.SetActive(false);
                gameOver = false;
            }
        }

        if (!isOnGround && doubleJumpTimer > 0)
        {
            doubleJumpTimer -= Time.deltaTime;

            if (doubleJumpTimer <= 0)
            {
                doubleJumpAvailable = false;
                texts.doubleJumpPowerupText.gameObject.SetActive(false);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Ground"))
    {
        isOnGround = true;
        currentDoubleJumps = 0;  // Reset double jump count when grounded
        if (isCrouching)
        {
            // Automatically uncrouch when landing on the ground
            isCrouching = false;
            Uncrouch();
        }
    }

    if (collision.gameObject.CompareTag("Enemy"))
    {
        if (!isInvincible && !gameOver)
        {
            gameOver = true;
            texts.gameoverText.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
            returnMenuButton.gameObject.SetActive(true);
                playerAnim.SetBool("Death_b", true);
                playerAnim.SetInteger("DeathType_int", 1);
        }
    }

    if (collision.gameObject.CompareTag("Coin"))
    {
        CollectCoin(collision.gameObject);
    }
    else if (collision.gameObject.CompareTag("PowerUp"))
    {
        CollectPowerup(collision.gameObject);
    }
    else if (collision.gameObject.CompareTag("Powerup2"))
    {
        CollectDoubleJump(collision.gameObject);
    }
}

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnMenu()
    {
        SceneManager.LoadSceneAsync(0);
    }

    private void CollectCoin(GameObject Coin)
    {
        texts.score += 100;
        Destroy(Coin);
    }

    private void CollectPowerup(GameObject PowerUp)
    {
        StartCoroutine(ActivateInvincibility());
        powerupTimer = powerupDuration;
        texts.InvincibilityText(powerupTimer);
        Destroy(PowerUp);
    }

    IEnumerator ActivateInvincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(5.0f);
        isInvincible = false;
    }

    private void CollectDoubleJump(GameObject PowerUp)
    {
        StartCoroutine(ActivateDoubleJump());
        Destroy(PowerUp);
        texts.DoubleJumpPowerupText(doubleJumpDuration);
    }

    IEnumerator ActivateDoubleJump()
    {
        doubleJumpAvailable = true;
        doubleJumpTimer = doubleJumpDuration;
        yield return new WaitForSeconds(doubleJumpDuration);
        doubleJumpAvailable = false;
    }

    // Add these functions for crouch functionality
    private void Crouch()
    {
        // Adjust the player's scale to crouch height
        transform.localScale = new Vector3(transform.localScale.x, originalHeight / 2.0f, transform.localScale.z);

        // Additional actions when crouching
        Debug.Log("Player is crouching!");
    }

    private void Uncrouch()
    {
        // Restore the player's original scale
        transform.localScale = new Vector3(transform.localScale.x, originalHeight, transform.localScale.z);

        // Additional actions when uncrouching
        Debug.Log("Player is uncrouching!");
    }
}
