using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Block") {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.ballBounce);
            collision.gameObject.GetComponent<Block>().Break();
        }   
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.name == "Player")
            GameController.Instance.DamagePlayer();
        if (collision.gameObject.name == "PaddleLUp"
         || collision.gameObject.name == "PaddleLDown"
         || collision.gameObject.name == "PaddleRUp"
         || collision.gameObject.name == "PaddleRDown") 
        {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.ballBounce);
            BallBounceController.Instance.HitBall(collision.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.name == "Bounds")
            BallBounceController.Instance.WrapBall();
    }
}
