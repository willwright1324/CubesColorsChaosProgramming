using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour {
    int whichHitBox;
    bool[] longNote = new bool[2];

    void Start() {
        whichHitBox = int.Parse(name.Substring(name.Length - 1)) - 1;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.name.Contains("LongMiddle"))
            return;
        
        if (collision.name.Contains("LongStart"))
            longNote[0] = true;

        if (collision.name.Contains("Note"))
            longNote[1] = true;

        RhythmController.Instance.notes[whichHitBox] = collision.gameObject;
    }
    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.name.Contains("LongMiddle"))
            return;

        if (Vector3.Distance(transform.position, collision.transform.position) < 1)
            RhythmController.Instance.PlaySong();
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.name.Contains("LongMiddle"))
            return;

        if (collision.name.Contains("LongStart"))
            longNote[0] = false;

        if (collision.name.Contains("Note"))
            longNote[1] = false;

        RhythmController.Instance.notes[whichHitBox] = null;
    }
}
