using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public enum GameState { LEVEL_SELECT, GAME, PAUSED }
public enum SelectState { CUBES, LEVELS, HOW_TO, BOSS }
public class GameController : MonoBehaviour {
    /* 
     * Racing:      0
     * Gravity:     1
     * Action:      2
     */

    public GameState gameState;
    public SelectState selectState;
    public GameObject pauseUI;
    public GameObject startUI;
    GameObject fade;
    Text countdownText;
    public int currentCube;
    public int[,] levelHowToBoss = new int[3, 2];
    public int[] levelUnlocks = new int[3];
    public int[] levelSelects = new int[3];
    public bool[] cubeCompletes = new bool[3];
    public string[] cubeNames = { "Racing", "Gravity", "Action" };
    public bool[] didCutscene = new bool[6];
    public bool devMode;
    GameData gd;
    /*
    public int[,] levelHowToBoss = new int[8, 2];
    public int[] levelUnlocks = new int[8];
    public int[] levelSelects = new int[8];
    public string[] cubeNames = {"Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "BallBounce", "Puzzle"};
    */
    public bool completedLevel;
    public bool exitedLevel;
    public float startTimer;
    public AudioClip startMusic;
    IEnumerator startGameCoroutine;

    GameObject[] coins;
    GameObject player;
    GameObject door;
    GameObject respawn;
    Rigidbody2D rb;
    Transform[] playerHealth;
    Text coinScore;
    public int healthCount;
    public int coinAmount;

    // Singleton
    private static GameController instance = null;
    public static GameController Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<GameController>();
                if (instance == null) {
                    GameObject gc = Resources.Load("General/GameController") as GameObject;
                    gc = Instantiate(gc);
                    instance = gc.GetComponent<GameController>();
                    DontDestroyOnLoad(gc);
                }
            }
            return instance;
        }
    }
    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }
    }
    private void OnEnable() {
        UnityEngine.Cursor.visible = false;
        pauseUI = GameObject.Find("PauseUI");
        pauseUI.SetActive(false);
        startUI = GameObject.Find("StartUI");
        fade = GameObject.Find("Fade");
        countdownText = GameObject.Find("Countdown").GetComponent<Text>();
        startUI.SetActive(false);
        Load();
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(StartScene());

        int scene = SceneManager.GetActiveScene().buildIndex;
        if (scene == 0 || scene == SceneManager.sceneCountInBuildSettings - 1)
            return;

        int cube = scene / 4;
        int level = scene % 4;
        if (cube > -1 && cube < 3) {
            switch (level) {
                case 1:
                    selectState = SelectState.HOW_TO;
                    break;
                default:
                    selectState = SelectState.LEVELS;
                    break;
            }
            gameState = GameState.GAME;
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        StartCoroutine(StartScene());
    }
    private void Update() {
        // Quick Reset
        /* 
        if (Input.GetKeyDown(KeyCode.R)) {
            AudioController.Instance.audioMusic.Stop();
            gameState = GameState.LEVEL_SELECT;
            selectState = SelectState.CUBES;
            levelHowToBoss = new int[3, 2];
            levelSelects = new int[3];
            currentCube = 0;
            pauseUI.SetActive(false);
            DoLoadScene(0);
        } */

        if (Input.GetButtonDown("Cancel")) {
            if (gameState == GameState.GAME) {
                if (selectState == SelectState.HOW_TO) {
                    gameState = GameState.LEVEL_SELECT;
                    exitedLevel = true;
                    AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.selectBack);
                    DoLoadScene(SceneManager.sceneCountInBuildSettings - 2);
                }
                else {
                    pauseUI.SetActive(true);
                    gameState = GameState.PAUSED;
                    AudioController.Instance.audioSound.Pause();
                    Time.timeScale = 0f;
                }
            }
            else {
                if (gameState == GameState.PAUSED) {
                    if (startTimer > 0) {
                        if (startGameCoroutine != null)
                            StopCoroutine(startGameCoroutine);
                        startGameCoroutine = StartGame(startMusic, startTimer);
                        StartCoroutine(startGameCoroutine);
                    }
                    else
                        Time.timeScale = 1f;
                    pauseUI.SetActive(false);
                    gameState = GameState.GAME;
                }
            }
        }
        if (Input.GetButtonDown("Action 2")) {
            if (gameState == GameState.PAUSED) {
                if (startGameCoroutine != null) {
                    StopCoroutine(startGameCoroutine);
                    startTimer = 0;
                    startMusic = null;
                    AudioController.Instance.audioSound.Stop();
                    countdownText.text = "3";
                    startUI.SetActive(false);
                }
                Time.timeScale = 1f;
                pauseUI.SetActive(false);
                gameState = GameState.LEVEL_SELECT;
                if (selectState == SelectState.BOSS)
                    selectState = SelectState.CUBES;
                exitedLevel = true;
                DoLoadScene(SceneManager.sceneCountInBuildSettings - 2);
            }
            else {
                if (gameState == GameState.GAME && selectState == SelectState.HOW_TO && Input.GetButton("Action 1")) {
                    levelHowToBoss[currentCube, 0] = 1;
                    gameState = GameState.GAME;
                    selectState = SelectState.LEVELS;
                    if (levelUnlocks[currentCube] == 0) {
                        levelSelects[currentCube] = 1;
                        levelUnlocks[currentCube] = 1;
                    }
                    DoLoadScene(2 + (currentCube * 4));
                }
            }
        }
    }
    public void Init() {}
    public void InitPlayer() {
        player = GameObject.FindWithTag("Player");
        respawn = GameObject.FindWithTag("Respawn");
        respawn.transform.position = player.transform.position;
    }
    public void Save() {
        GameData gd = new GameData();
        gd.gameState      = gameState;
        gd.selectState    = selectState;
        gd.currentCube    = currentCube;
        gd.levelHowToBoss = levelHowToBoss;
        gd.levelUnlocks   = levelUnlocks;
        gd.levelSelects   = levelSelects;
        gd.cubeCompletes  = cubeCompletes;
        gd.didCutscene    = didCutscene;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + Path.DirectorySeparatorChar + "SaveData.gd");
        bf.Serialize(file, gd);
        file.Close();
    }
    public void Load() {
        if (File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "SaveData.gd")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + Path.DirectorySeparatorChar + "SaveData.gd", FileMode.Open);
            GameData gd = new GameData();
            gd = (GameData)bf.Deserialize(file);
            file.Close();

            gameState      = gd.gameState;
            selectState    = gd.selectState;
            currentCube    = gd.currentCube;
            levelHowToBoss = gd.levelHowToBoss;
            levelUnlocks   = gd.levelUnlocks;
            levelSelects   = gd.levelSelects;
            cubeCompletes  = gd.cubeCompletes;
            didCutscene    = gd.didCutscene;
        }
    }
    public void DeleteSave() {
        gameState = GameState.LEVEL_SELECT;
        selectState = SelectState.CUBES;
        currentCube = 0;
        levelHowToBoss = new int[3, 2];
        levelUnlocks = new int[3];
        levelSelects = new int[3];
        cubeCompletes = new bool[3];
        didCutscene = new bool[6];
        devMode = false;
        Save();
    }
    // Initializes health when needed
    public void InitHealth() {
        playerHealth = GameObject.Find("PlayerHealth").GetComponentsInChildren<Transform>();
        healthCount = playerHealth.Length - 1;
    }
    // Initializes coins and door when needed
    public void InitCoins() {
        coins = GameObject.FindGameObjectsWithTag("Coin");
        door = GameObject.FindWithTag("Door");
        coinScore = GameObject.Find("CoinScore").GetComponent<Text>();
        coinAmount = coins.Length;
        coinScore.text = "Coins: 0 / " + coinAmount;
    }
    // Countdown for game
    public void DoStartGame(AudioClip ac) {
        if (AudioController.Instance.audioMusic.isPlaying)
            AudioController.Instance.audioMusic.Stop();
        startMusic = ac;
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.countdown);
        Time.timeScale = 0;

        if (startGameCoroutine != null)
            StopCoroutine(startGameCoroutine);
        startGameCoroutine = StartGame(ac, 0);
        StartCoroutine(startGameCoroutine);
    }
    // Gives player full health
    public void ResetHealth() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.healthReset);
        healthCount = playerHealth.Length - 1;
        foreach (Transform h in playerHealth)
            h.gameObject.SetActive(true);
    }
    // Player takes damage
    public void DamagePlayer() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerDamage);
        healthCount--;
        if (healthCount <= 0) ResetLevel();
        else                  playerHealth[healthCount + 1].gameObject.SetActive(false);
    }
    // Player respawns
    public void RespawnPlayer() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerDeath);
        player.SetActive(false);
        player.transform.position = respawn.transform.position;
        Invoke("EnablePlayer", 1f);
    }
    void EnablePlayer() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.respawn);
        player.SetActive(true);
    }
    // Player collects coin
    public void CollectCoin() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerCollect);
        coinAmount--;
        coinScore.text = "Coins: " + (coins.Length - coinAmount) + " / " + coins.Length;
    }
    // Player opens door
    public void OpenDoor() {
        if (coinAmount == 0)
            Destroy(door);
    }
    // Player completes level
    public void CompleteLevel() {
        completedLevel = true;
        gameState = GameState.LEVEL_SELECT;
        if (levelUnlocks[currentCube] == levelSelects[currentCube] && levelUnlocks[currentCube] < 3)
            levelUnlocks[currentCube]++;
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.winTune);
        DoLoadScene(SceneManager.sceneCountInBuildSettings - (selectState == SelectState.BOSS ? 8 : 2));
    }
    // Resets level
    public void ResetLevel() {
        DoLoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void DoLoadScene(int scene) {
        StartCoroutine(LoadScene(scene));
    }
    IEnumerator LoadScene(int scene) {
        fade.SetActive(true);
        fade.transform.localPosition = Vector3.up * 540;
        float fadeY = 0;
        Time.timeScale = 0;

        while (fade.transform.localPosition.y > fadeY) {
            fade.transform.localPosition += Vector3.down * Time.unscaledDeltaTime * 1000;
            yield return null;
        }
        fade.transform.localPosition = Vector3.zero;
        Time.timeScale = 1;

        SceneManager.LoadScene(scene);
    }
    // Countdown
    IEnumerator StartGame(AudioClip ac, float time) {
        AudioController.Instance.audioSound.UnPause();
        startUI.SetActive(true);
        startTimer = time;

        while (startTimer < 4) {
            if (gameState != GameState.PAUSED) {
                startTimer += Time.unscaledDeltaTime;
                if (startTimer >= 1) {
                    if (startTimer < 3)
                        countdownText.text = (3 - (int)startTimer) + "";
                    else
                        countdownText.text = "Go!";
                }
            }
            yield return null;
        }
        startTimer = 0;
        Time.timeScale = 1;
        countdownText.text = "3";
        startUI.SetActive(false);
        AudioController.Instance.PlayMusic(ac);
    }
    IEnumerator StartScene() {
        fade.SetActive(true);
        fade.transform.localPosition = Vector3.zero;
        float fadeY = -540;
        Time.timeScale = 0;

        while (fade.transform.localPosition.y > fadeY) {
            fade.transform.localPosition += Vector3.down * Time.unscaledDeltaTime * 1000;
            yield return null;
        }
        fade.transform.localPosition = Vector3.down * fadeY;
        if (gameState != GameState.GAME || selectState == SelectState.HOW_TO || selectState == SelectState.BOSS)
            Time.timeScale = 1;
    }
}
