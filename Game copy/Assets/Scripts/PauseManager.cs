using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenu;

    [Header("Controls")]
    //We can change this in the inspector
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Music")]
    public AudioSource gameMusic;

    public string sceneToLoad;

    //Is the game Paused? False by default, because we shouldn't be pausing the game on start
    private bool isPaused = false;

    public void Start()
    {
        //When the game starts lets just be sure to hide the pause menu
        pauseMenu.SetActive(false);
    }

    public void Update()
    {
        //If the player presses the "pauseKey" and isPaused is false, call the function "HandleGamePause"
        if (!isPaused && Input.GetKeyDown(pauseKey))
        {
            //Lets pause the game
            HandleGamePause();
        }
        //Nearly the same if statement, but if we ARE paused, lets resume the game
        else if (isPaused && Input.GetKeyDown(pauseKey))
        {
            //Lets resume the game
            HandleGameResume();
        }
    }

    public void BackToMain()
    {
        //Set time to 1 so we dont keep the whole game paused
        Time.timeScale = 1;

        //Called from the menu button
        SceneManager.LoadScene(sceneToLoad);

    }
    public void HandleGamePause()
    {
        //The game is paused
        isPaused = true;

        //Pause music
        gameMusic.Pause();

        //Show pause menu
        pauseMenu.SetActive(true);

        //The simplest way to pause the game is to pause everything
        //Set time to 0
        Time.timeScale = 0;
    }

    public void HandleGameResume()
    {
        //Game is not paused
        isPaused = false;

        //Resume music
        gameMusic.Play();

        //Show pause menu
        pauseMenu.SetActive(false);

        //Unpause the game
        //Set time to 1
        Time.timeScale = 1;
    }

}