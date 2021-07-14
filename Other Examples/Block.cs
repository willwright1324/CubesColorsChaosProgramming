using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {
    int hits = 3;
    SpriteRenderer mat;

    void Start() {
        mat = GetComponent<SpriteRenderer>();
    }

    public void Break() {
        switch (--hits) {
            case 2:
                mat.material = Resources.Load<Material>("BallBounce/Break2");
                break;
            case 1:
                mat.material = Resources.Load<Material>("BallBounce/Break3");
                break;
            case 0:
                AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.blockBreak);
                BallBounceController.Instance.DestroyBlock();
                Destroy(gameObject);
                break;
        }
    }
}
