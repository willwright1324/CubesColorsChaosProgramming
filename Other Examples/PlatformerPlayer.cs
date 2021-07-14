using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerPlayer : MonoBehaviour {
    PlatformerCamera bcam;
    Rigidbody2D rb;
    BoxCollider2D bc;
    public float speed = 120;
    float speedMax;
    public float jumpForce = 200;
    float jumpForceMax;
    public float springForce = 350;
    public float deceleration = 60;
    public bool canJump;

    void Start() {
        bcam = Camera.main.GetComponent<PlatformerCamera>();
        rb = GetComponent<Rigidbody2D>();
        speedMax = speed;
        jumpForceMax = jumpForce;
    }

    private void Update() {
        if (Input.GetAxisRaw("Horizontal") != 0) {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (Input.GetAxisRaw("Horizontal") > 0) {
                sr.flipX = false;
            }
            else
                sr.flipX = true;
        }
        if (Input.GetButton("Action 1") && canJump && Input.GetAxisRaw("Vertical") >= 0) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerJump);
            bcam.followY = false;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false;
        }
    }
    private void FixedUpdate() {
        rb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * speed, 0), ForceMode2D.Impulse);
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -speed, speed), Mathf.Clamp(rb.velocity.y, -jumpForce, jumpForce));
        rb.velocity = new Vector2(rb.velocity.x / (Time.deltaTime * deceleration), rb.velocity.y);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Platform" && transform.position.y > collision.gameObject.transform.position.y) {
            bcam.followY = true;
            canJump = true;
            jumpForce = jumpForceMax;
        }
    }
    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.tag == "Platform") {
            if (Input.GetButtonDown("Action 1") && Input.GetAxisRaw("Vertical") < 0) {
                canJump = false;
                bc = collision.gameObject.GetComponent<BoxCollider2D>();
                bc.enabled = false;
                Invoke("EnablePlatform", 0.25f);
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Platform")
            bcam.followY = false;
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Ground") {
            bcam.followY = true;
            canJump = true;
            jumpForce = jumpForceMax;
        }
        if (collision.gameObject.tag == "Spring") {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.springBoost);
            bcam.followY = true;
            jumpForce = springForce;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Death") {
            GameController.Instance.DamagePlayer();
            GameController.Instance.RespawnPlayer();
            bcam.Invoke("Refocus", 1f);
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Ground")
            bcam.followY = false;
    }

    void EnablePlatform() {
        bc.enabled = true;
    }
}
