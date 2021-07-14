using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundedCamera : MonoBehaviour {
    GameObject player;
    public float offsetX;
    public float offsetY;
    public float offsetZ;
    public float border = 1.75f;
    public int[] sides = new int[4];

    void Start() {
        player = GameObject.FindWithTag("Player");
        transform.position = new Vector3(player.transform.position.x + offsetX, player.transform.position.y + offsetY, transform.position.z + offsetZ);
    }

    void LateUpdate() {
        if (player.activeSelf == false)
            return;

        Vector3 playerPos = player.transform.position;
        Vector3 camPos = transform.position;

        if ((playerPos.x > camPos.x && sides[0] == 0) || (playerPos.x < camPos.x && sides[2] == 0))
            transform.position += new Vector3(playerPos.x - camPos.x, 0, 0);
        if ((playerPos.y > camPos.y && sides[1] == 0) || (playerPos.y < camPos.y && sides[3] == 0))
            transform.position += new Vector3(0, playerPos.y - camPos.y, 0);
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
            if      (name.Contains("Right"))sides[0] = 0;
            else if (name.Contains("Up"))   sides[1] = 0;
            else if (name.Contains("Left")) sides[2] = 0;
            else if (name.Contains("Down")) sides[3] = 0;
        }
    }
}