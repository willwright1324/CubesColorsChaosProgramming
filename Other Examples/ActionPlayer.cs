using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPlayer : MonoBehaviour {
    Rigidbody2D rb;
    public float speed = 1.5f;
    public int dashSpeed = 5;
    public float dashLength = 0.15f;
    public bool dashCooldown = false;
    bool isDashing;
    Vector2 moveDirection;
    Vector2 dashDirection;

    GameObject audioListener;
    GameObject bullet;
    public bool canShoot = true;
    IEnumerator rotateCoroutine;
    float currentAngle;
    public float rotateSpeed = 600f;
    float fireCooldown;
    float fireRate = 0.2f;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        audioListener = GameObject.Find("AudioListener");
        bullet = Resources.Load("Shooter/PlayerBullet") as GameObject;
    }

    void Update() {
        if (Time.timeScale == 0)
            return;

        float horInput = Input.GetAxisRaw("Horizontal");
        float vertInput = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Action 2")) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerShoot);
            Instantiate(bullet, transform.position + (transform.up * 8 + transform.right * 6), transform.rotation);
        }

        if (!Input.GetButton("Action 2")) {
            fireCooldown = fireRate;
            if (horInput != 0 && vertInput == 0) {
                if (horInput > 0) DoRotate(270);
                else              DoRotate(90);
            }
            if (vertInput != 0 && horInput == 0) {
                if (vertInput > 0) DoRotate(0);
                else               DoRotate(180);
            }
            if (horInput != 0 && vertInput != 0) {
                if      (horInput > 0 && vertInput > 0) DoRotate(315);
                else if (horInput < 0 && vertInput > 0) DoRotate(45);
                else if (horInput < 0 && vertInput < 0) DoRotate(135);
                else if (horInput > 0 && vertInput < 0) DoRotate(225);
            }
        }
        else {
            if (fireCooldown > 0)
                fireCooldown -= Time.deltaTime;
            else {
                AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerShoot);
                Instantiate(bullet, transform.position + (transform.up * 8 + transform.right * 6), transform.rotation);
                fireCooldown = fireRate;
            }
        }

        audioListener.transform.position = transform.position;
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetButtonDown("Action 1")) {
            if (!dashCooldown) {
                AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerDash);
                dashDirection = moveDirection;
                Invoke("DoneDash", dashLength);
                isDashing = true;
            }
        }
    }
    private void FixedUpdate() {
        if (!isDashing) rb.MovePosition((Vector2)transform.position + moveDirection * speed);
        else            rb.MovePosition((Vector2)transform.position + dashDirection * dashSpeed);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag == "Coin") {
            ActionController.Instance.MoveRespawn();
            GameController.Instance.ResetHealth();
        }
        if (collision.tag == "Damage")
            GameController.Instance.DamagePlayer();
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Death")
            ActionController.Instance.Respawn();
    }

    void DoneDash() {
        isDashing = false;
        Invoke("DashCooldown", 0.25f);
        dashCooldown = true;
    }
    void DashCooldown() {
        dashCooldown = false;

        /* Insert where needed
        if (!dashCooldown) {
            Invoke("DashCooldown", 0.5f);
            dashCooldown = true;
        }
        */
    }
    void DoRotate(float angle) {
        if (angle == currentAngle)
            return;

        canShoot = false;
        currentAngle = angle;

        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);
        rotateCoroutine = Rotate(rotation);
        StartCoroutine(rotateCoroutine);
    }
    IEnumerator Rotate(Quaternion rotation) {
        while (Quaternion.Angle(transform.rotation, rotation) > 0.1f) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, Time.deltaTime * rotateSpeed);
            yield return null;
        }
        transform.rotation = rotation;
        canShoot = true;
    }
}
