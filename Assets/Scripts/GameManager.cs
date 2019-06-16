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
        public Text scoreText, bombTallyText;

        public void SetTallyText(int currNoOfBombs, int noOfBombsAllowed)
        {
            bombTallyText.text = currNoOfBombs.ToString() + " / " + noOfBombsAllowed.ToString();
        }
    }

    public HUD hud;

    [Space]
    public int noOfEnemies;
    public int enemyKillReward;
    [HideInInspector]
    public bool gameBeingPlayed;

    public int noOfBombsAllowed = 1;

    [HideInInspector]
    public int currNoOfBombs;

    public delegate void OnGameEvents();
    public static event OnGameEvents onGameOver, onGameStart;

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
        //PlayerController.onPowerupCollected += onPowerupCollected;
    }
    private void OnDisable()
    {
        GridManager.onGameStart -= OnGridLoaded;
    }

    public void onPowerupCollected(Powerups type)
    {
        Debug.Log("collected: " + type);
        switch(type)
        {
            case Powerups.BombPower:
                noOfBombsAllowed++;
                currNoOfBombs++;
                hud.SetTallyText(currNoOfBombs, noOfBombsAllowed);
                break;

            case Powerups.SpeedPower:
                break;

            default:
                Debug.Log("Invalid power type");
                break;
        }
    }

    public void OnBombExploded()
    {
        currNoOfBombs++;
        currNoOfBombs = Mathf.Clamp(currNoOfBombs, 0, noOfBombsAllowed);
        hud.SetTallyText(currNoOfBombs, noOfBombsAllowed);
    }

    public void OnBombDropped()
    {
        currNoOfBombs--;
        hud.SetTallyText(currNoOfBombs, noOfBombsAllowed);
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
        GridManager.instance.StartGame();

        currNoOfEnemies = noOfEnemies;
        currScore = 0;
        mainMenu.SetActive(false);
        gameOverScreen.SetActive(false);
        hud.SetActive(true);
        hud.scoreText.text = currScore.ToString();
        gameBeingPlayed = true;
        noOfBombsAllowed = 1;
        currNoOfBombs = 1;
        hud.SetTallyText(currNoOfBombs, noOfBombsAllowed);

        if(onGameStart!=null)
        {
            onGameStart();
        }
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
