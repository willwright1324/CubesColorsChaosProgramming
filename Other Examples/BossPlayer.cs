using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPlayer : MonoBehaviour {
    GameObject camOrbit;
    GameObject cam;
    GameObject playerTarget;
    GameObject arm;
    GameObject respawn;
    GameObject shadow;
    GameObject ground;
    Rigidbody rb;

    float baseSpeed;
    public float speed = 1f;
    public float speedMax = 12f;
    public float deceleration = 1.2f;
    public float jumpHeight = 10f;
    public float jumpForce = 60f;
    public float gravityForce = -60f;
    public float forwardSpeed = 15f;
    public float rotateSpeed = 50f;
    public bool canJump = true;

    Vector3 playerPos;
    Vector3 targetPos;
    Vector3 gravity;
    IEnumerator moveCamCoroutine;

    void Start() {
        GameController.Instance.InitPlayer();
        camOrbit     = GameObject.Find("CameraOrbit");
        cam          = Camera.main.gameObject;
        playerTarget = GameObject.Find("PlayerTarget");
        arm          = GameObject.Find("PlayerModel/ArmR");
        respawn      = GameObject.FindWithTag("Respawn");
        shadow       = GameObject.Find("PlayerShadow");
        ground       = GameObject.FindWithTag("Ground");
        rb           = GetComponent<Rigidbody>();
        baseSpeed    = speed;
    }

    void Update() {
        if (Time.timeScale == 0)
            return;

        if (transform.position.y < camOrbit.transform.position.y) {
            GameController.Instance.DamagePlayer();
            GameController.Instance.RespawnPlayer();
        }

        if (Input.GetButtonDown("Action 1") && canJump) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerJump);
            canJump = false;
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        if (BossController.Instance.playerMove) {
            camOrbit.transform.rotation *= Quaternion.Euler(0, -Input.GetAxisRaw("Horizontal") * Time.deltaTime * rotateSpeed, 0);
            playerTarget.transform.position += playerTarget.transform.forward * -Input.GetAxisRaw("Vertical") * Time.deltaTime * forwardSpeed;
            Vector3 targetLocalPos = playerTarget.transform.localPosition;
            playerTarget.transform.localPosition = new Vector3(targetLocalPos.x, targetLocalPos.y, Mathf.Clamp(targetLocalPos.z, 5, 17));
        }

        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(transform.position - cam.transform.position), Time.deltaTime * rotateSpeed / 10);
        respawn.transform.position = targetPos = playerTarget.transform.position;
        playerPos = transform.position;
        playerPos.y = targetPos.y = 0;

        float shadowY = 1000;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit)) {
            if (hit.collider.tag == "Ground")
                shadowY = ground.transform.position.y + 0.45f;
        }
        shadow.transform.position = new Vector3(transform.position.x, shadowY, transform.position.z);
        shadow.transform.rotation = transform.rotation;
    }
    private void FixedUpdate() {
        if (!BossController.Instance.playerMove)
            return;

        if (Vector3.Distance(playerPos, targetPos) > 1f)
            rb.velocity += (targetPos - playerPos).normalized * speed;
        else
            rb.velocity = new Vector3(rb.velocity.x / deceleration, rb.velocity.y, rb.velocity.z / deceleration);
        rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -speedMax, speedMax), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -speedMax, speedMax));
    }
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Trigger")
            gravity = other.gameObject.transform.up * gravityForce;
        if (other.gameObject.tag == "Enemy") {
            BossController.Instance.DamageBoss();
            rb.velocity = Vector3.up * 10;
            rb.AddForce(transform.up * jumpForce / 2, ForceMode.Impulse);
        }
        if (other.gameObject.tag == "Damage")
            GameController.Instance.DamagePlayer();
    }
    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Ground") {
            canJump = true;
            BossController.Instance.playerMove = true;
        }
        if (collision.gameObject.tag == "Cube") {
            rb.AddForce((transform.position - collision.contacts[0].point).normalized * 100, ForceMode.Impulse);
            BossController.Instance.playerMove = false;
        }
        if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Damage") {
            if (BossController.Instance.bossDamage)
                GameController.Instance.DamagePlayer();
        }
    }

    IEnumerator Jump() {
        float height = transform.position.y + jumpHeight;

        while (transform.position.y < height) {
            rb.MovePosition(transform.position + transform.up * Time.deltaTime * jumpForce * Mathf.Max((height - transform.position.y), 1));
            yield return null;
        }
        StartCoroutine(Fall(height));
    }
    IEnumerator Fall(float height) {
        while (!canJump) {
            rb.MovePosition(transform.position - transform.up * Time.deltaTime * jumpForce * Mathf.Max((height - transform.position.y), 1));
            yield return null;
        }
    }
    IEnumerator PunchUp() {
        while (arm.transform.localRotation != Quaternion.Euler(100, 0, 0)) {
            arm.transform.localRotation = Quaternion.RotateTowards(arm.transform.localRotation, Quaternion.Euler(100, 0, 0), Time.deltaTime * 100f);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(PunchDown());
    }
    IEnumerator PunchDown() {
        while (arm.transform.localRotation != Quaternion.Euler(0, 0, 0)) {
            arm.transform.localRotation = Quaternion.RotateTowards(arm.transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * 100f);
            yield return null;
        }
    }
    IEnumerator MoveCam(Quaternion whichRotation) {
        while (Quaternion.Angle(camOrbit.transform.localRotation, whichRotation) > 0.1f) {
            transform.rotation = camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, whichRotation, Time.smoothDeltaTime * rotateSpeed);
            yield return null;
        }
    }
}
