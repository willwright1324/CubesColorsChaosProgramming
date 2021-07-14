using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBouncePlayer : MonoBehaviour {
    GameObject paddleL;
    GameObject paddleR;
    GameObject ball;
    float paddleLRot;
    float paddleRRot;
    public float paddleRest = 30;
    public float paddleSens = 500;
    public float rotationSens = 100;

    void Start() {
        paddleL = GameObject.Find("PivotL");
        paddleR = GameObject.Find("PivotR");
        ball = GameObject.Find("Ball");

        paddleL.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -paddleRest));
        paddleR.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, paddleRest));
        paddleLRot = -paddleRest;
        paddleRRot = paddleRest;
    }

    void Update() {
        if (Time.timeScale == 0 || BallBounceController.Instance.blockCount <= 0)
            return;

        if (Input.GetAxisRaw("Horizontal") != 0)
            transform.Rotate(0, 0, Time.deltaTime * rotationSens * Input.GetAxisRaw("Horizontal"));

        if (Input.GetButton("Action 1")) {
            if (Input.GetButtonDown("Action 1"))
                AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.paddleSwing);

            if (paddleLRot < 0) {
                BallBounceController.Instance.paddleL = true;
                paddleLRot += Time.deltaTime * paddleSens * 2;
            }
            else
                Invoke("HitBufferL", 0.2f);
        }
        else {
            BallBounceController.Instance.paddleL = false;
            if (paddleLRot > -paddleRest)
                paddleLRot -= Time.deltaTime * paddleSens;
        }
        paddleL.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Clamp(paddleLRot, -paddleRest, 0)));

        if (Input.GetButton("Action 2")) {
            if (Input.GetButtonDown("Action 2"))
                AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.paddleSwing);

            if (paddleRRot > 0) {
                BallBounceController.Instance.paddleR = true;
                paddleRRot -= Time.deltaTime * paddleSens * 2;
            }
            else
                Invoke("HitBufferR", 0.2f);
        }
        else {
            BallBounceController.Instance.paddleR = false;
            if (paddleRRot < paddleRest)
                paddleRRot += Time.deltaTime * paddleSens;
        }
        paddleR.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Clamp(paddleRRot, 0, paddleRest)));
    }

    void HitBufferL() { BallBounceController.Instance.paddleL = false; }
    void HitBufferR() { BallBounceController.Instance.paddleR = false; }
}
