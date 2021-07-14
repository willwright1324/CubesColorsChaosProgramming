using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crusher : MonoBehaviour {
    public float startDelay;
    public float crushSpeed = 30f;
    public float retractSpeed = 30f;
    public float crushPause = 0.2f;
    public float retractPause = 0.2f;
    float maxLength;
    float minLength;
    Vector3 pos;
    public AudioSource audioSource;

    void Start() {
        maxLength = transform.lossyScale.y;
        minLength = transform.lossyScale.x;
        pos = transform.position;
        audioSource = GetComponent<AudioSource>();
        Invoke("DoRetract", startDelay);
    }

    void DoCrush() { StartCoroutine(Crush()); }
    void DoRetract() { StartCoroutine(Retract()); }

    IEnumerator Crush() {
        audioSource.PlayOneShot(AudioController.Instance.crusherActivate);
        while (transform.localScale.y < maxLength) {           
            transform.localScale += new Vector3(0, Time.deltaTime * crushSpeed, 0);
            transform.position += transform.up * Time.deltaTime * crushSpeed;
            yield return null;
        }
        audioSource.PlayOneShot(AudioController.Instance.crusherSmash);
        transform.position = pos;
        transform.localScale = new Vector3(transform.localScale.x, maxLength, transform.localScale.z);
        Invoke("DoRetract", crushPause);
    }
    IEnumerator Retract() {
        while (transform.localScale.y > minLength) {
            transform.localScale -= new Vector3(0, Time.deltaTime * retractSpeed, 0);
            transform.position -= transform.up * Time.deltaTime * retractSpeed;
            yield return null;
        }
        transform.localScale = new Vector3(transform.localScale.x, minLength, transform.localScale.z);
        Invoke("DoCrush", retractPause);
    }
}
