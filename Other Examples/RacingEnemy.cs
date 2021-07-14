using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingEnemy : MonoBehaviour {
    public int track = 1;
    GameObject enemyPath;
    public Transform[] paths;
    public int pathIndex = 0;
    public float speed = 1000f;
    public float turnSpeed = 200f;

    void Start() {
        enemyPath = GameObject.Find("EnemyPath" + track);
        paths = enemyPath.GetComponentsInChildren<Transform>();
        Transform[] tempPaths = enemyPath.GetComponentsInChildren<Transform>();
        paths = new Transform[tempPaths.Length - 1];

        for (int i = 0; i < paths.Length; i++) {
            for (int j = 0; j < tempPaths.Length; j++) {
                string name = tempPaths[j].name;
                if (name.Contains("" + i) && !name.Contains("Enemy")) {
                    paths[i] = tempPaths[j];
                    break;
                }
            }
        }

        DoDrive();
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.name == "RaceStart(Clone)")
            RacingController.Instance.EnemyLap();
    }
    void DoDrive() {
        if (RacingController.Instance.raceOver)
            return;

        StartCoroutine(Drive());
    }
    IEnumerator Drive() {
        while (Vector3.Distance(transform.position, paths[pathIndex].transform.position) > 5) {
            transform.position = Vector3.LerpUnclamped(transform.position, transform.position + transform.up * 0.1f, Time.deltaTime * speed);

            Vector3 target = paths[pathIndex].position - transform.position;
            float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg - 90;
            Quaternion enemyRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, enemyRotation, Time.deltaTime * turnSpeed);
            yield return null;
        }
        pathIndex = pathIndex + 1 >= paths.Length ? 0 : ++pathIndex;
        DoDrive();
    }
}
