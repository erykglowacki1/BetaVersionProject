using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Texts : MonoBehaviour
{
    public int score;
    public int highScore;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameoverText;
    public TextMeshProUGUI invincibilityText;
    public TextMeshProUGUI doubleJumpPowerupText;
    public TextMeshProUGUI highScoreText;
    private PlayerController PlayerControllerScript;
    
    public TextMeshProUGUI menuSelect;
    
    void Start()
    {

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        score = 0;
        scoreText.text = "Score: " + score;
        highScoreText.text = "High Score: " + highScore;
        PlayerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
        StartCoroutine(IncreaseScoreCoroutine());
    }

    IEnumerator IncreaseScoreCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (!PlayerControllerScript.gameOver) {
                UpdateScore(5);

            }
        }
    }

    private void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Collision with Enemy detected");
            if (score > highScore)
            {
                highScore = score;
                highScoreText.text = "High Score: " + highScore;
                Debug.Log("New High Score: " + highScore);

            }
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            GameOver();
        }
    }

    public void GameOver()
    {
        gameoverText.gameObject.SetActive(true);
        menuSelect.gameObject.SetActive(true);
    }

    public void InvincibilityText(float remainingTime)
    {
        if (invincibilityText != null) 
        {
            invincibilityText.gameObject.SetActive(true);
            invincibilityText.text = "Invincibility: " + Mathf.Ceil(remainingTime).ToString();
        }
    }

    public void DoubleJumpPowerupText(float remainingTime)
    {
        if (doubleJumpPowerupText != null)
        {
            doubleJumpPowerupText.gameObject.SetActive(true);
           
        }
    }
}
