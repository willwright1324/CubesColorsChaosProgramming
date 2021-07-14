using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerCamera : MonoBehaviour {
    GameObject player;
    GameObject camLeft;
    GameObject camRight;
    public float offsetX;
    public float offsetY;
    public float offsetZ;
    public float border = 1.5f;
    public int[] sides = new int[4];
    public float speed = 500f;
    public bool followY;
    public float buffer = 15;
    public float followSide;
    public float followX;
    public float followSens = 1f;

    void Start() {
        player   = GameObject.FindWithTag("Player");
        camLeft  = GameObject.Find("Player/CamLeft");
        camRight = GameObject.Find("Player/CamRight");

        transform.position = new Vector3(player.transform.position.x + offsetX, player.transform.position.y + offsetY, transform.position.z + offsetZ);
    }

    void LateUpdate() {
        if (!player.activeSelf)
            return;

        Vector3 playerPos = player.transform.position;
        Vector3 camPos = transform.position;
        float camX = camPos.x;
        float camY = camPos.y;
        if (Input.GetAxisRaw("Horizontal") != 0) {
            followX += Input.GetAxisRaw("Horizontal") * Time.deltaTime * followSens;
            followX = Mathf.Clamp(followX, -1, 1);
        }

        if (sides[0] == 0) {
            if (followX == 1)
                camX = camRight.transform.position.x;
        }
        if (sides[2] == 0) {
            if (followX == -1)
                camX = camLeft.transform.position.x;
        }
        if ((playerPos.y > camPos.y && sides[1] == 0) || (playerPos.y < camPos.y && sides[3] == 0)) {
            if (followY)
                camY = playerPos.y;
        }
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(camX, camY, camPos.z), Time.deltaTime * speed);
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "CameraBounds") {
            string name = collision.gameObject.name;
            if      (name.Contains("Right")) sides[0] = 1;
            else if (name.Contains("Up"))    sides[1] = 1;
            else if (name.Contains("Left"))  sides[2] = 1;
            else if (name.Contains("Down"))  sides[3] = 1;
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "CameraBounds") {
            string name = collision.gameObject.name;
            if      (name.Contains("Right")) sides[0] = 0;
            else if (name.Contains("Up"))    sides[1] = 0;
            else if (name.Contains("Left"))  sides[2] = 0;
            else if (name.Contains("Down"))  sides[3] = 0;
        }
    }

    public void Refocus() {
        transform.position = new Vector3(player.transform.position.x + offsetX, player.transform.position.y + offsetY, transform.position.z + offsetZ);
    }


}
