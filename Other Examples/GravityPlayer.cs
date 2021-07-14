using System.Collections;
using UnityEngine;

public class GravityPlayer : MonoBehaviour {
    public Rigidbody2D rb;
    public ConstantForce2D cf;
    Sprite up;
    Sprite down;
    SpriteRenderer sr;
    public float gravity = 1000;
    public bool canFlip = true;
    public bool flipped;

    void Start() {
        rb   = GetComponent<Rigidbody2D>();
        cf   = GetComponent<ConstantForce2D>();
        up   = Resources.Load<Sprite>("Gravity/player_flipW");
        down = Resources.Load<Sprite>("Gravity/player_sideW");
        sr   = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (Time.timeScale == 0)
            return;

        if(Input.GetButton("Action 1") && canFlip) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.switchGravity);
            flipped = !flipped;
            if (flipped) {
                sr.sprite = up;
                rb.velocity = new Vector2(rb.velocity.x, 100);
            }
            else {
                sr.sprite = down;
                rb.velocity = new Vector2(rb.velocity.x, -100);
            }
            canFlip = false;
            GravityController.Instance.DoFlipArrow(flipped);
        }
    }
    private void FixedUpdate() {
        rb.AddForce((flipped ? Vector3.up : Vector3.down) * gravity);
        if (gameObject.transform.position.x < GravityController.Instance.camScroll.transform.position.x - GravityController.Instance.camDistance)
            rb.AddForce(Vector3.right * 110);
        rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, 0, cf.force.x), rb.velocity.y);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Ground")
            canFlip = true;
        if (collision.gameObject.tag == "Death") {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerDamage);
            GameController.Instance.ResetLevel();
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.name == "WinTrigger") {
            GravityController.Instance.DoWinTrigger(gameObject);
            GravityController.Instance.DoWinTrigger(GravityController.Instance.camScroll);
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.name == "Bounds") {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerDeath);
            GameController.Instance.ResetLevel();
        }
    }
}
