using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BombermanCore;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [System.Serializable]
    public class MainMenu : UiScreen
    {
        public GameObject startButton;
    }

    public MainMenu mainMenu;

    [System.Serializable]
    public class GameOverScreen : UiScreen
    {
        public Text messageText, scoreText;
        public GameObject restartButton;

        public void SetGameOverScreen(string message, string score)
        {
            messageText.text = message;
            scoreText.text = score;
        }
    }

    public GameOverScreen gameOverScreen;

    [System.Serializable]
    public class HUD : UiScreen
    {
        public Text scoreText;
    }

    public HUD hud;

    [Space]
    public int noOfEnemies;
    public int enemyKillReward;
    [HideInInspector]
    public bool gameBeingPlayed;

    public delegate void OnGameOver();
    public static event OnGameOver onGameOver;

    int currNoOfEnemies, currScore;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        mainMenu.SetActive(true);
        hud.SetActive(false);
        gameOverScreen.SetActive(false);
        gameBeingPlayed = false;
    }

    private void OnEnable()
    {
        GridManager.onGameStart += OnGridLoaded;
    }

    private void OnDisable()
    {
        GridManager.onGameStart -= OnGridLoaded;
    }

    //We used this event to know when grid loading is complete.
    void OnGridLoaded(Vector3? pos = null)
    {

    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void GameOver(GameOverReasons reason)
    {
        if (onGameOver != null)
            onGameOver();

        gameBeingPlayed = false;
        hud.SetActive(false);
        gameOverScreen.SetActive(true);
        string msg = (reason == GameOverReasons.PlayerDead) ? "You lost." : "YOU WON!!";
        gameOverScreen.SetGameOverScreen(msg, currScore.ToString());
    }

    public void GameStart()
    {
        currNoOfEnemies = noOfEnemies;
        currScore = 0;
        mainMenu.SetActive(false);
        gameOverScreen.SetActive(false);
        hud.SetActive(true);
        hud.scoreText.text = currScore.ToString();
        gameBeingPlayed = true;

        GridManager.instance.StartGame();
    }

    public void OnEnemyKilled()
    {
        ChangeScore(enemyKillReward);
        currNoOfEnemies--;
        if(currNoOfEnemies <= 0)
        {
            GameOver(GameOverReasons.AllEnemiesDead);
        }
    }

    void ChangeScore(int delta)
    {
        currScore += delta;
        hud.scoreText.text = currScore.ToString();
    }

    public void ButtonClickListener(GameObject go)
    {
        if(go == mainMenu.startButton)
        {
            GameStart();   
        }

        if (go == gameOverScreen.restartButton)
        {
            GameStart();
            //GridManager.instance.Restart();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
