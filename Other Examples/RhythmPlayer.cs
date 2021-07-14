using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmPlayer : MonoBehaviour {
    GameObject[] hitboxes = new GameObject[4];
    GameObject[] notes = new GameObject[4];
    Sprite hitbox0;
    Sprite hitbox1;

    void Start() {
        for (int i = 0; i < 4; i++)
            hitboxes[i] = GameObject.Find("HitBox" + (i + 1));

        notes   = RhythmController.Instance.notes;
        hitbox0 = Resources.Load<Sprite>("Rhythm/hitbox0");
        hitbox1 = Resources.Load<Sprite>("Rhythm/hitbox1");
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.DownArrow))  Highlight(0, true);
        if (Input.GetKeyDown(KeyCode.LeftArrow))  Highlight(1, true);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Highlight(2, true);
        if (Input.GetKeyDown(KeyCode.UpArrow))    Highlight(3, true);
        if (Input.GetKeyUp(KeyCode.DownArrow))    Highlight(0, false);
        if (Input.GetKeyUp(KeyCode.LeftArrow))    Highlight(1, false);
        if (Input.GetKeyUp(KeyCode.RightArrow))   Highlight(2, false);
        if (Input.GetKeyUp(KeyCode.UpArrow))      Highlight(3, false);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name.Contains("Note"))
            Destroy(collision.gameObject);
    }

    void Highlight(int whichHitBox, bool highlight) {
        if (highlight) {
            hitboxes[whichHitBox].GetComponent<SpriteRenderer>().sprite = hitbox1;
            CheckNote(whichHitBox);
        }
        else
            hitboxes[whichHitBox].GetComponent<SpriteRenderer>().sprite = hitbox0;
    }
    void CheckNote(int whichHitBox) {
        if (notes[whichHitBox] != null) {
            Destroy(notes[whichHitBox]);
            notes[whichHitBox] = null;
        }
    }
}
