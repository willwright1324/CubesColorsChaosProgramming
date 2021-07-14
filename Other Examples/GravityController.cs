using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour {
    public GameObject player;
    GameObject cam;
    public GameObject camScroll;
    GameObject arrow;
    public GameObject powerCube;
    public Rigidbody2D rb;
    public ConstantForce2D cf;
    public int camDistance = 50;
    bool getComponents;
    IEnumerator flipArrowCoroutine;

    //Boss
    GameObject boss;
    GameObject eyes;
    GameObject flip;
    int switchTime = 8;
    int flipDirection;
    float flipSpeed = 10;

    public static GravityController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }

    void Start() {
        player    = GameObject.FindWithTag("Player");
        cam       = GameObject.FindWithTag("MainCamera");
        camScroll = GameObject.Find("CamScroll");
        arrow     = GameObject.Find("Flip/Arrow");
        powerCube = GameObject.FindWithTag("PowerCube");

        rb = camScroll.GetComponent<Rigidbody2D>();
        cf = camScroll.GetComponent<ConstantForce2D>();

        camScroll.transform.position = new Vector3(player.transform.position.x + camDistance, cam.transform.position.y, cam.transform.position.z);

        boss = GameObject.FindWithTag("Boss");
        eyes = GameObject.Find("Boss/Eyes");
        flip = GameObject.Find("Flip");

        if (GameController.Instance.selectState == SelectState.BOSS) {
            boss.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, player.transform.position.z + 5);
            boss.transform.SetParent(cam.transform);
            InvokeRepeating("DoFlipView", switchTime, switchTime);
        }

        GameController.Instance.DoStartGame(AudioController.Instance.gravityMusic);
    }

    void Update() {
        if (!getComponents) {
            rb = camScroll.GetComponent<Rigidbody2D>();
            cf = camScroll.GetComponent<ConstantForce2D>();
            getComponents = true;
        }
    }
    private void FixedUpdate() {
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, 0, cf.force.x), rb.velocity.y);
    }

    public void DoWinTrigger(GameObject obj) {
        StartCoroutine(WinTrigger(obj));
    }
    IEnumerator WinTrigger(GameObject obj) {
        ConstantForce2D cf = obj.GetComponent<ConstantForce2D>();
        cf.enabled = false;
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        Transform t = obj.GetComponent<Transform>();

        while (t.position.x < powerCube.transform.position.x) {
            rb.velocity = new Vector2(rb.velocity.x - Time.deltaTime * 100, rb.velocity.y);
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, 30, rb.velocity.x), rb.velocity.y);
            yield return null;
        }
        rb.velocity = new Vector2(0, rb.velocity.y);
    }
    public void DoFlipArrow(bool flipped) {
        if (flipArrowCoroutine != null)
            StopCoroutine(flipArrowCoroutine);
        flipArrowCoroutine = FlipArrow(flipped);
        StartCoroutine(flipArrowCoroutine);
    }
    IEnumerator FlipArrow(bool flipped) {
        Quaternion arrowRotation = Quaternion.Euler(Vector3.zero);
        if (flipped)
            arrowRotation = Quaternion.Euler(Vector3.forward * 180);

        while (Quaternion.Angle(arrow.transform.localRotation, arrowRotation) > 0.1f) {
            arrow.transform.localRotation = Quaternion.Lerp(arrow.transform.localRotation, arrowRotation, Time.deltaTime * 20);
            yield return null;
        }
        arrow.transform.localRotation = arrowRotation;
    }

    //Boss
    void DoFlipView() {
        int temp = Mathf.RoundToInt(Random.Range(0, 3));
        while (temp == flipDirection)
            temp = Mathf.RoundToInt(Random.Range(0, 3));
        flipDirection = temp;
        Vector3 eyePos = Vector3.zero;
        switch (flipDirection) {
            case 0:
                eyePos = new Vector3(0, -0.2f, 0);
                break;
            case 1:
                eyePos = new Vector3(-0.2f, 0, 0);
                break;
            case 2:
                eyePos = new Vector3(0, 0.2f, 0);
                break;
            case 3:
                eyePos = new Vector3(0.2f, 0, 0);
                break;
        }
        eyes.transform.localPosition += eyePos;
        StartCoroutine(FlipView(flipDirection));
    }
    IEnumerator FlipView(int direction) {

        yield return new WaitForSeconds(1);
        Quaternion camRotation = Quaternion.Euler(0, 0, direction * 90);
        Quaternion flipRotation = camRotation;
        if (direction == 1 || direction == 3)
            flipRotation = Quaternion.Euler(0, 0, direction * 270);

        while (Quaternion.Angle(cam.transform.localRotation, camRotation) > 0.1f) {
            flip.transform.rotation = Quaternion.Slerp(flip.transform.rotation, flipRotation, Time.deltaTime * flipSpeed);
            cam.transform.localRotation = Quaternion.Slerp(cam.transform.localRotation, camRotation, Time.deltaTime * flipSpeed);
            yield return null;
        }
        flip.transform.rotation = flipRotation;
        cam.transform.localRotation = camRotation;
        eyes.transform.localPosition = Vector3.zero;
    }
}
