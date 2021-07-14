using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RacingController : MonoBehaviour {
    public int track = 1;
    public int laps = 0;
    public int currentLap = 0;
    public int currentEnemyLap;
    GameObject player;
    GameObject temps;
    GameObject raceStart;
    GameObject trackPieces;
    GameObject respawn;
    public GameObject[,] tracks;
    public int[,] trackCoords = null;
    int startX;
    int startY;
    int trackCount;
    public bool raceOver;
    Text lap;
    Text enemyLap;

    GameObject enemy;
    GameObject enemyPath;
    GameObject obstacles;

    public static RacingController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }

    void Start() {
        player    = GameObject.FindWithTag("Player");
        raceStart = Instantiate(Resources.Load("Racing/RaceStart") as GameObject);
        respawn   = GameObject.FindWithTag("Respawn");
        lap       = GameObject.Find("MainCanvas/Lap").GetComponent<Text>();
        enemyLap  = GameObject.Find("MainCanvas/EnemyLap").GetComponent<Text>();
        enemy     = GameObject.FindWithTag("Enemy");
        enemyPath = GameObject.Find("EnemyPath" + track);
        obstacles = GameObject.Find("Obstacles" + track);
        lap.text = "Lap: 0 / " + laps;
        enemyLap.text = "EnemyLap: 0 / " + laps;

        float trackSize = (Resources.Load("Racing/StraightTrack") as GameObject).transform.lossyScale.x;

        TextAsset file = Resources.Load("Racing/racing_track" + track) as TextAsset;
        string[] lines = file.text.Split("\n"[0]);
        tracks = new GameObject[lines.Length, lines.Length];
        trackCoords = new int[lines.Length, lines.Length];

        temps = new GameObject("Temps");
        trackPieces = new GameObject("Tracks");

        int lineNum = -1;

        foreach (string line in lines) {
            lineNum++;
            string[] types = line.Split(' ');
            int length = types.Length;
            for (int i = 0; i < length; i++) {
                if (types[i].StartsWith("0")) {
                    trackCoords[i, lineNum] = -1;
                    continue;
                }
                GameObject t = new GameObject("Temp");
                int rot = int.Parse(types[i].Substring(1));
                t.transform.SetParent(temps.transform);

                if (types[i].StartsWith("S") || types[i].StartsWith("B")) {
                    t = Instantiate(Resources.Load("Racing/StraightTrack") as GameObject);
                    t.transform.SetParent(trackPieces.transform);
                }
                if (types[i].StartsWith("L") || types[i].StartsWith("R")) {
                    t = Instantiate(Resources.Load("Racing/CornerTrack") as GameObject);
                    t.transform.SetParent(trackPieces.transform);

                    if (types[i].StartsWith("L")) {
                        t.transform.localScale = new Vector2(-t.transform.localScale.x, t.transform.localScale.y);
                        rot = -rot;
                    }
                }
                t.transform.rotation = Quaternion.Euler(0, 0, 90 * rot);
                float size = t.transform.lossyScale.y * 10;
                t.transform.position = new Vector3((player.transform.position.x - ((length * size) / 2) + (size / 2) + (i * size)), 
                                                    player.transform.position.y + ((length * size) / 2) - (size / 2) - (lineNum * size), 
                                                    player.transform.position.z + 1);

                Track tt = t.GetComponent<Track>();
                tt.coordX = i;
                tt.coordY = lineNum;
                tracks[i, lineNum] = t;
                if (types[i].StartsWith("B")) {
                    startX = i;
                    startY = lineNum;
                }
            }
        }
        GameObject start = tracks[startX, startY];
        enemy.transform.rotation     = player.transform.rotation = start.transform.rotation;
        raceStart.transform.position = start.transform.position + -start.transform.forward * 0.1f + start.transform.up * 10;
        respawn.transform.position   = player.transform.position = start.transform.position + -start.transform.forward * 2 + -start.transform.right * 30 + -start.transform.up * 15;
        obstacles.transform.position = enemy.transform.position = enemyPath.transform.position = player.transform.position + Vector3.right * 60;
        Destroy(temps);
        trackCount = tracks.Length;

        GameController.Instance.DoStartGame(AudioController.Instance.racingMusic);
    }

    public void CheckLap() {
        if (raceOver)
            return;

        if (currentLap > 0) {
            foreach (int track in trackCoords) {
                if (track == 0)
                    return;
            }
        }
        if (currentLap < laps) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.lapComplete);
            lap.text = "Lap: " + ++currentLap + " / " + laps;
            for (int i = 0; i < trackCoords.GetLength(0); i++) {
                for (int j = 0; j < trackCoords.GetLength(1); j++) {
                    if (trackCoords[i, j] == 1)
                        trackCoords[i, j] = 0;
                }
            }
        }
        else {
            raceOver = true;
            lap.text = "You Win!";
            enemyLap.text = "Enemy: Lose!";
            GameObject.FindWithTag("PowerCube").transform.position = raceStart.transform.position + -raceStart.transform.forward * 30 + raceStart.transform.up * 100;
        }
    }
    public void EnemyLap() {
        if (raceOver)
            return;

        if (currentEnemyLap < laps)
            enemyLap.text = "EnemyLap: " + ++currentEnemyLap + " / " + laps;
        else {
            enemyLap.text = "Enemy: Win!";
            lap.text = "You Lose!";
            raceOver = true;
            Invoke("Lose", 3f);
        }
            
    }
    void Lose() {
        GameController.Instance.ResetLevel();
    }
}
