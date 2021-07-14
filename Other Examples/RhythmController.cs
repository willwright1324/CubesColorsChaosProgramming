using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmController : MonoBehaviour {
    public AudioClip song;
    public int notefile = 1;
    public float offset = 50;
    public float bpm = 80;
    public float noteSpeed = 100;
    public float time;
    float spb;
    string[] lines = null;
    bool started;
    int lineNum = -1;
    GameObject[] hitboxes = new GameObject[4];
    public GameObject[] notes = new GameObject[4];
    public List<GameObject>[] longNotes = new List<GameObject>[4];
    int waitAmount;
    GameObject player;
    GameObject note;
    GameObject longStart;
    GameObject longMiddle;
    float noteSize;
    float longStartSize;
    float longMiddleSize;

    public static RhythmController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }

    void Start() {
        spb = (60 / bpm) / 4;

        player     = GameObject.FindWithTag("Player");
        note       = Resources.Load("Rhythm/Note") as GameObject;
        longStart  = Resources.Load("Rhythm/LongStart") as GameObject;
        longMiddle = Resources.Load("Rhythm/LongMiddle") as GameObject;

        noteSize       = note.GetComponent<Renderer>().bounds.size.x;
        longStartSize  = longStart.GetComponent<Renderer>().bounds.size.x;
        longMiddleSize = longMiddle.GetComponent<Renderer>().bounds.size.x;

        Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, Camera.main.transform.position.z);

        for (int i = 0; i < 4; i++)
            hitboxes[i] = GameObject.Find("HitBox" + (i + 1));
        for (int i = 0; i < longNotes.Length; i++)
            longNotes[i] = new List<GameObject>();

        TextAsset file = Resources.Load("Rhythm/note_file" + notefile) as TextAsset;
        lines = file.text.Split("\n"[0]);

        InvokeRepeating("PlayNotes", 3, spb);
    }

    private void Update() {
        time += Time.deltaTime;
    }

    public void PlaySong() {
        if (!started) {
            GetComponent<AudioSource>().PlayOneShot(song);
            started = true;
        }
    }
    void StartSong() {
        GetComponent<AudioSource>().PlayOneShot(song);
    }
    void PlayNotes() {
        if (waitAmount > 0) {
            waitAmount--;
            return;
        }

        if (++lineNum >= lines.Length)
            return;

        string[] types = lines[lineNum].Split(' ');
        if (types.Length == 1)
            waitAmount = int.Parse(types[0]) - 1;
        else {
            for (int i = 0; i < types.Length; i++) {
                int type = int.Parse(types[i]);
                if (type != 0) {                       
                    if (type > 1) {
                        GameObject ln = new GameObject("LongNote" + i);
                        GameObject ls = Instantiate(longStart, hitboxes[i].transform.position + hitboxes[i].transform.right * offset, hitboxes[i].transform.rotation);
                        ls.transform.parent = ln.transform;

                        GameObject lm = Instantiate(longMiddle, 
                                                    ls.transform.position + hitboxes[i].transform.right * (longStartSize / 2 + longMiddleSize / 2 - 0.1f), 
                                                    hitboxes[i].transform.rotation);
                        lm.transform.parent = ln.transform;

                        GameObject lm2 = null;
                        int amount = (type - 2) * 16;
                        for (int j = 1; j < amount; j++) {
                            lm2 = Instantiate(longMiddle,
                                              lm.transform.position + hitboxes[i].transform.right * (longMiddleSize - 0.1f),
                                              hitboxes[i].transform.rotation);
                            lm = lm2;
                            lm.transform.parent = ln.transform;
                        }
                        GameObject n = Instantiate(note,
                                                   lm.transform.position + hitboxes[i].transform.right * (longMiddleSize / 2 + noteSize / 2 - 0.1f),
                                                   hitboxes[i].transform.rotation);
                        n.transform.parent = ln.transform;
                    }
                    else
                        Instantiate(note, hitboxes[i].transform.position + hitboxes[i].transform.right * offset, hitboxes[i].transform.rotation);
                }
            }
        }
    }
}
