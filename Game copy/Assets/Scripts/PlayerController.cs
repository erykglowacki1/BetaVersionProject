using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public MoveForward moveForwardScript;
    public int numberOfLanes = 3;
    public float speed = 10.0f;
    public float floorWidth = 20.0f;
    public float laneWidth;
    private Animator playerAnim;
    private AudioSource audioSource;
    private AudioSource runningAudioSource;
    private AudioSource coinAudioSource;
    private AudioSource powerupAudioSource;
    private AudioSource deathAudioSource;
    public AudioClip runningAudio;
    public AudioClip coinAudio;
    public AudioClip powerupAudio;
    public AudioClip deathAudio;
    public ParticleSystem explosionParticle;
    public ParticleSystem crouchParticle; // Assign in the inspector
    public ParticleSystem uncrouchParticle; // Assign in the inspector
    public ParticleSystem runEffect; // Assign in the inspector
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

    // Double Jump variables
    private bool doubleJumpAvailable = false;
    private float doubleJumpDuration = 5.0f;
    private float doubleJumpTimer = 0.0f;
    private int doubleJumpCount = 1;
    private int currentDoubleJumps = 0;
    // Crouch variables
    private bool isCrouching = false;
    private float originalHeight;

    void Start()
    {
        // Initialize components and variables
        playerRb = GetComponent<Rigidbody>();
        Physics.gravity *= gravityModifier;
        playerAnim = GetComponent<Animator>();

        // Add the following lines to create and configure AudioSource components
        audioSource = gameObject.AddComponent<AudioSource>();
        runningAudioSource = gameObject.AddComponent<AudioSource>();
        coinAudioSource = gameObject.AddComponent<AudioSource>();
        powerupAudioSource = gameObject.AddComponent<AudioSource>();
        deathAudioSource = gameObject.AddComponent<AudioSource>();

        // Configure AudioSource settings
        ConfigureAudioSource(audioSource, null);
        ConfigureAudioSource(runningAudioSource, runningAudio);
        ConfigureAudioSource(coinAudioSource, coinAudio);
        ConfigureAudioSource(powerupAudioSource, powerupAudio);
        ConfigureAudioSource(deathAudioSource, deathAudio);

        laneWidth = floorWidth / numberOfLanes;
        originalHeight = transform.localScale.y;
    }

    void Update()
    {
        // Handle player movement
        HandleMovementInput();

        // Handle jumping
        HandleJumping();

        // Handle crouching
        HandleCrouching();

        // Handle dodge animation
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(PerformDodge());
        }

        // Handle power-up timers
        HandlePowerupTimers();

        // Update particle effects
        UpdateParticleEffects();
    }

    void HandleMovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        // Move left
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0 && !isCrouching)
        {
            currentLane--;
        }

        // Move right
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < numberOfLanes - 1 && !isCrouching)
        {
            currentLane++;
        }

        // Move towards the target lane
        float targetX = currentLane * laneWidth - floorWidth / 2.0f + laneWidth / 2.0f;
        transform.position = Vector3.Lerp(transform.position, new Vector3(targetX, transform.position.y, transform.position.z), Time.deltaTime * speed);

        // Clamp player position within the bounds
        float clampedX = Mathf.Clamp(transform.position.x, -floorWidth / 2.0f + laneWidth / 2.0f, floorWidth / 2.0f - laneWidth / 2.0f);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        // Move forward based on horizontal input
        transform.Translate(Vector3.right * horizontalInput * Time.deltaTime * speed);

        // Play running audio when moving
        if (Mathf.Abs(horizontalInput) > 0.1f && isOnGround && !runningAudioSource.isPlaying)
        {
            runningAudioSource.Play();
        }
        else if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            // Stop running audio when not moving
            runningAudioSource.Stop();
        }
    }

    void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOnGround || (doubleJumpAvailable && doubleJumpTimer > 0))
            {
                // Perform jump
                PerformJump();

                // Disable double jump after the first jump
                if (!isOnGround)
                {
                    doubleJumpAvailable = false;
                    doubleJumpTimer = 0.0f;
                    texts.doubleJumpPowerupText.gameObject.SetActive(false);
                }

                isOnGround = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // Set the "Jump_b" parameter to false in the animator
            playerAnim.SetBool("Jump_b", false);
        }
    }

    void PerformJump()
    {
        // Reset vertical velocity and apply jump force
        playerRb.velocity = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z);
        playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        playerAnim.SetBool("Jump_b", true);
    }

    void HandleCrouching()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Crouch if on the ground
            if (isOnGround)
            {
                isCrouching = true;
                Crouch();
            }
            // Descend more quickly when crouching while in the air
            else if (!isOnGround && !isCrouching)
            {
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

        // Adjust speed based on crouching state
        AdjustSpeedBasedOnCrouch();
    }

    void Crouch()
    {
        // Adjust the player's scale to crouch height
        transform.localScale = new Vector3(transform.localScale.x, originalHeight / 2.0f, transform.localScale.z);

        // Play crouch particle effect
        crouchParticle.Play();
    }

    void Uncrouch()
    {
        // Restore the player's original scale
        transform.localScale = new Vector3(transform.localScale.x, originalHeight, transform.localScale.z);

        // Play uncrouch particle effect
        uncrouchParticle.Play();
    }

    void AdjustSpeedBasedOnCrouch()
    {
        // Adjust speed when continuously crouching
        speed = isCrouching ? 5.0f : 10.0f;
    }

    void HandlePowerupTimers()
    {
        // Handle invincibility power-up timer
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

        // Handle double jump power-up timer
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

    void UpdateParticleEffects()
    {
        // Update runEffect particle system
        if (!isCrouching && !isOnGround && !gameOver)
        {
            if (!runEffect.isPlaying)
            {
                runEffect.Play();
            }
        }
        else
        {
            runEffect.Stop();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Handle collisions with ground, enemies, coins, and power-ups
        HandleCollisions(collision);
    }

    void HandleCollisions(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Player is on the ground
            isOnGround = true;

            // Reset double jump count when grounded
            currentDoubleJumps = 0;

            // Automatically uncrouch when landing on the ground
            if (isCrouching)
            {
                isCrouching = false;
                Uncrouch();
            }
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // Handle collision with enemy
            HandleEnemyCollision();
        }
        else if (collision.gameObject.CompareTag("Coin"))
        {
            // Handle collision with coin
            CollectCoin(collision.gameObject);

            // Play coin audio
            coinAudioSource.Play();
        }
        else if (collision.gameObject.CompareTag("PowerUp"))
        {
            // Handle collision with invincibility power-up
            CollectPowerup(collision.gameObject);

            // Play power-up audio
            powerupAudioSource.Play();
        }
        else if (collision.gameObject.CompareTag("Powerup2"))
        {
            // Handle collision with double jump power-up
            CollectDoubleJump(collision.gameObject);

            // Play power-up audio
            powerupAudioSource.Play();
        }
    }

    void HandleEnemyCollision()
    {
        if (!isInvincible && !gameOver)
        {
            // Player is not invincible and the game is not over
            // Handle game over scenario

            gameOver = true;
            texts.gameoverText.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
            returnMenuButton.gameObject.SetActive(true);

            playerAnim.SetBool("Death_b", true);
            playerAnim.SetInteger("DeathType_int", 1);

            // Play explosion particle effect
            explosionParticle.Play();

            // Play death audio
            deathAudioSource.Play();

            // Stop the forward movement
            moveForwardScript.fire.Stop();
        }
    }

    void CollectCoin(GameObject coin)
    {
        // Increase score and destroy the coin
        texts.score += 100;
        Destroy(coin);
    }

    void CollectPowerup(GameObject powerUp)
    {
        // Activate invincibility power-up
        StartCoroutine(ActivateInvincibility());

        // Set the power-up timer
        powerupTimer = powerupDuration;

        // Update UI for invincibility
        texts.InvincibilityText(powerupTimer);

        // Destroy the power-up object
        Destroy(powerUp);
    }

    IEnumerator ActivateInvincibility()
    {
        // Activate invincibility for a duration
        isInvincible = true;
        yield return new WaitForSeconds(5.0f);
        isInvincible = false;
    }

    void CollectDoubleJump(GameObject powerUp)
    {
        // Activate double jump power-up
        StartCoroutine(ActivateDoubleJump());

        // Destroy the power-up object
        Destroy(powerUp);

        // Update UI for double jump power-up
        texts.DoubleJumpPowerupText(doubleJumpDuration);
    }

    IEnumerator ActivateDoubleJump()
    {
        // Activate double jump for a duration
        doubleJumpAvailable = true;
        doubleJumpTimer = doubleJumpDuration;
        yield return new WaitForSeconds(doubleJumpDuration);
        doubleJumpAvailable = false;
    }

    // Coroutine to animate the collected coin (if needed)
    IEnumerator AnimateCoin(GameObject coin)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalScale = coin.transform.localScale;

        while (elapsed < duration)
        {
            coin.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(coin);
    }

    IEnumerator PerformDodge()
    {
        // Play dodge animation
        playerAnim.SetTrigger("Dodge_trig");

        // Wait for the animation duration
        yield return new WaitForSeconds(playerAnim.GetCurrentAnimatorStateInfo(0).length);

        // End dodge animation
        playerAnim.ResetTrigger("Dodge_trig");
    }

    public void RestartGame()
    {
        // Restart the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnMenu()
    {
        // Return to the main menu
        SceneManager.LoadSceneAsync(0);
    }

    // Helper method to configure AudioSource settings
    private void ConfigureAudioSource(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        // Add any other AudioSource configuration settings here
    }
}
