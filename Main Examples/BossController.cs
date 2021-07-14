using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour {
    enum BossState { ATTACK, CENTER, CUBE}
    public bool playerMove = true;
    public bool bossDamage = true;
    GameObject player;
    GameObject camOrbit;
    GameObject cam;
    GameObject ground;
    GameObject shadow;
    GameObject boss;
    GameObject bossOrbit;
    GameObject face;
    GameObject armPivotL;
    GameObject armPivotR;
    GameObject handL;
    GameObject handR;
    GameObject colorCube;
    GameObject spin;
    GameObject planet;
    GameObject cubeShadow;

    Text bossHealth;
    public List<GameObject> healthBars;
    public GameObject lastAttack;
    public List<GameObject> attackList;
    public List<GameObject> attackPicks;
    string[] attackNames = { "Racing", "Shooter", "Rhythm", "Platformer", "Gravity", "Maze", "Ball", "Puzzle" };
    public int currentSide;
    public float flipTime = 0.2f;
    public float flipSpeed = 500f;
    public Transform[] sidePositions = new Transform[6];
    Vector3 spinDirection;
    public float bossSpeed = 30f;
    IEnumerator chargePlayerCoroutine;
    IEnumerator lookAtObjectCoroutine;
    IEnumerator rotateArmXCoroutine;
    IEnumerator extendHandLCoroutine;
    IEnumerator extendHandRCoroutine;

    public static BossController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }

    void Start() {
        GameController.Instance.gameState = GameState.GAME;
        GameController.Instance.selectState = SelectState.BOSS;
        GameController.Instance.InitHealth();
        player    = GameObject.FindWithTag("Player");
        camOrbit  = GameObject.Find("CameraOrbit");
        cam       = Camera.main.gameObject;
        ground    = GameObject.FindWithTag("Ground");
        shadow    = GameObject.Find("BossShadow");
        boss      = GameObject.Find("Boss");
        bossOrbit = GameObject.Find("BossOrbit");
        face      = GameObject.Find("Face");
        armPivotL = GameObject.Find("ArmPivotL");
        armPivotR = GameObject.Find("ArmPivotR");
        handL     = GameObject.Find("HandL");
        handR     = GameObject.Find("HandR");
        colorCube = GameObject.Find("ColorCube");
        spin      = GameObject.Find("Spin");
        planet    = GameObject.Find("Planet");

        bossHealth = GameObject.Find("BossHealth").GetComponent<Text>();
        RectTransform[] bh = bossHealth.gameObject.GetComponentsInChildren<RectTransform>();

        foreach (string s in attackNames) {
            foreach (RectTransform t in bh) {
                if (t.name.Contains(s)) {
                    healthBars.Add(t.gameObject);
                    break;
                }
            }
        }

        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        for (int i = 0; i < 8; i++) {
            foreach (GameObject cube in cubes) {
                if (cube.name.Contains(attackNames[i]))
                    attackList.Add(cube);
            }
        }

        Transform[] tempPositions = GameObject.Find("Sides").GetComponentsInChildren<Transform>();
        for (int i = 0; i < sidePositions.Length; i++) {
            for (int j = 0; j < tempPositions.Length; j++) {
                if (tempPositions[j].name.Contains(i + "")) {
                    sidePositions[i] = tempPositions[j];
                }
            }
        }

        AudioController.Instance.PlayMusic(AudioController.Instance.bossMusic);
        Invoke("NextAttack", 3f);
    }

    void Update() {
        if (Time.timeScale == 0 || boss == null)
            return;

        planet.transform.Rotate(Time.deltaTime * -0.4f, Time.deltaTime * 0.6f, Time.deltaTime * -0.2f);

        float shadowY = 1000;
        if (Physics.Raycast(boss.transform.position, Vector3.down, out RaycastHit hit)) {
            if (hit.collider.tag == "Ground") {
                shadowY = ground.transform.position.y + 0.45f;
            }
        }
        shadow.transform.position = new Vector3(boss.transform.position.x, shadowY, boss.transform.position.z);
        shadow.transform.rotation = boss.transform.rotation;
    }

    void DoMoveToCube() {
        StartCoroutine(MoveToCube());
    }
    public void DamageBoss() {
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.bossDamage);
        if (chargePlayerCoroutine != null) {
            face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face2");
            StopCoroutine(chargePlayerCoroutine);
            chargePlayerCoroutine = null;
            boss.GetComponent<Renderer>().material = Resources.Load<Material>("FinalBoss/ObstacleBlack");
            Destroy(cubeShadow);

            GameObject hb = null;
            string attackName = lastAttack.name.Split('C')[0];
            foreach (GameObject go in healthBars) {
                if (go.name.Contains(attackName)) {
                    hb = go;
                    healthBars.Remove(go);
                    break;
                }
            }
            Destroy(hb);

            for (int i = 0; i < healthBars.Count; i++)
                StartCoroutine(HealthMove(healthBars[i], 80 + (i * 45)));

            if (attackPicks.Count >= 8) {
                Destroy(boss);
                Destroy(shadow);
                GameController.Instance.Invoke("CompleteLevel", 2f);
            }
            else {
                DoRotateArmX(180, 20);
                Invoke("NextAttack", 2f);
            }
        }
    }
    void NextAttack() {
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face0");
        bossDamage = true;

        Vector3 upSide = sidePositions[currentSide].position;
        Vector3 nextSide = Vector3.zero;
        spinDirection = Vector3.zero;
        GameObject attack = attackList[(Mathf.RoundToInt(Random.Range(0, 8)))];

        if (attackPicks.Count == 0)
            attack = attackList[(Mathf.RoundToInt(Random.Range(0, 4)))];
        else {
            if (attackPicks.Count < 8) {
                while (attackPicks.Contains(attack))
                    attack = attackList[(Mathf.RoundToInt(Random.Range(0, 8)))];

                CubeInfo ci = attack.GetComponent<CubeInfo>();
                int[] sides = new int[3];

                sides[0] = ci.side1;
                sides[1] = ci.side2;
                sides[2] = ci.side3;

                for (int i = 0; i < sides.Length; i++) {
                    if (currentSide != sides[i]) {
                        if (currentSide <= 2 && currentSide + 3 == sides[i])
                            continue;
                        else {
                            if (currentSide > 2 && currentSide - 3 == sides[i])
                                continue;
                        }
                        currentSide = sides[i];
                        break;
                    }
                }

                nextSide = sidePositions[currentSide].position;
                spin.transform.LookAt(nextSide);
                spinDirection = Vector3.right;
                colorCube.transform.SetParent(spin.transform);
            }
        }
        lastAttack = attack;
        attackPicks.Add(lastAttack);

        if (spinDirection != Vector3.zero) StartCoroutine(SlamUp());
        else                               StartCoroutine(MoveToCube());
    }
    void DoAttack() {
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face1");
        int attack = Random.Range(0, 3);
        List<GameObject> attacks = GetAttackRange();
        switch (attack) {
            case 0:
                StartCoroutine(ShootAttack());
                break;
            case 1:
                StartCoroutine(SpikeSpawn(attacks[0], attacks[1]));
                break;
            case 2:
                StartCoroutine(CrushAttack(attacks[0], attacks[1]));
                break;
        }
    }
    List<GameObject> GetAttackRange() {
        List<GameObject> attacks = new List<GameObject>();
        foreach (GameObject go in attackList) {
            if (go == lastAttack)
                continue;

            CubeInfo ci = go.GetComponent<CubeInfo>();
            int[] sides = { ci.side1, ci.side2, ci.side3 };
            foreach (int i in sides) {
                if (i == currentSide)
                    attacks.Add(go);
            }
        }
        Vector3 currentPos = lastAttack.transform.position;
        foreach (GameObject go in attacks) {
            if (currentPos.x != go.transform.position.x && currentPos.z != go.transform.position.z) {
                attacks.Remove(go);
                break;
            }
        }
        return attacks;
    }
    void DoLookAtObject(GameObject gObject, float speed) {
        if (lookAtObjectCoroutine != null)
            StopCoroutine(lookAtObjectCoroutine);
        lookAtObjectCoroutine = LookAtObject(gObject, speed);
        StartCoroutine(lookAtObjectCoroutine);
    }
    void DoRotateArmX(int rotation, float speed) {
        if (rotateArmXCoroutine != null)
            StopCoroutine(rotateArmXCoroutine);
        rotateArmXCoroutine = RotateArmX(rotation, speed);
        StartCoroutine(rotateArmXCoroutine);
    }
    IEnumerator HealthMove(GameObject whichBar, float whichX) {
        Transform barTransform = whichBar.GetComponent<RectTransform>().transform;

        while (barTransform.localPosition.x > whichX) {
            barTransform.localPosition -= barTransform.right * Time.deltaTime * 60;
            yield return null;
        }
        Vector3 barPos = barTransform.localPosition;
        barPos.x = whichX;
        barTransform.localPosition = barPos;
    }
    IEnumerator SlamUp() {
        int oppositeSide = currentSide;
        Vector3 newPos = sidePositions[oppositeSide].position + Vector3.up * 40;

        DoRotateArmX(180, 5);
        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        boss.transform.position = newPos;
        DoLookAtObject(colorCube, 10);

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(SlamDown(sidePositions[oppositeSide].position));
    }
    IEnumerator SlamDown(Vector3 oppositeSide) {
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face1");
        Vector3 slamPos = oppositeSide + Vector3.up * 10;

        DoRotateArmX(270, 15);
        while (Vector3.Distance(boss.transform.position, slamPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, slamPos, Time.deltaTime * bossSpeed * 2);
            yield return null;
        }
        boss.transform.position = slamPos;
        StartCoroutine(FlipCube(spinDirection));
    }
    IEnumerator FlipCube(Vector3 flipDirection) {
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face0");
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.cubeFlip);
        ground.SetActive(false);

        float flipCount = 0;
        while (flipCount < flipTime) {
            spin.transform.localRotation *= Quaternion.Euler(flipDirection * Time.deltaTime * flipSpeed);
            flipCount += Time.deltaTime;
            yield return null;
        }
        flipCount = flipTime;
        Vector3 upDirection = Vector3.right * -90;
        
        while (flipCount < flipTime + 0.1f) {
            spin.transform.localRotation *= Quaternion.Euler(flipDirection * Time.deltaTime * flipSpeed);
            flipCount += Time.deltaTime;
            yield return null;
        }
        spin.transform.localRotation = Quaternion.Euler(upDirection);
        colorCube.transform.SetParent(null);
        ground.SetActive(true);

        StartCoroutine(MoveUp());
    }
    IEnumerator MoveUp() {
        Vector3 newPos = boss.transform.position + Vector3.up * 40;

        DoRotateArmX(0, 10);
        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(1);

        StartCoroutine(MoveToCube());
    }
    IEnumerator MoveToCube() {
        DoLookAtObject(lastAttack, 10);
        Vector3 newPos = lastAttack.transform.position + Vector3.up * 20;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(1);

        StartCoroutine(PowerUp());
    }
    IEnumerator PowerUp() {
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face1");
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.bossPowerUp);
        Vector3 newPos = lastAttack.transform.position;

        DoRotateArmX(180, 15);
        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed * 2);
            yield return null;
        }

        cubeShadow = Instantiate(Resources.Load("FinalBoss/CubeShadow") as GameObject, newPos, Quaternion.identity);
        boss.GetComponent<Renderer>().material = lastAttack.GetComponent<Renderer>().material;
        Vector3 playerPos = player.transform.position;
        playerPos.y = boss.transform.position.y;
        boss.transform.LookAt(playerPos);

        yield return new WaitForSeconds(1);

        StartCoroutine(MoveToTop());
    }
    IEnumerator MoveToTop() {
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face0");
        Vector3 newPos = lastAttack.transform.position + Vector3.up * 15;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        DoLookAtObject(player, 10);
        DoRotateArmX(0, 10);

        yield return new WaitForSeconds(2);

        DoAttack();
    }
    IEnumerator ShootAttack() {
        DoRotateArmX(270, 15);

        GameObject bullet = Resources.Load("FinalBoss/Bullet") as GameObject;
        float spinTime = 0;
        float shootTime = 0;
        while (spinTime < 5) {
            spinTime += Time.deltaTime;
            shootTime += Time.deltaTime;

            if (shootTime > 0.1) {
                AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.bossAttack);

                GameObject b = Instantiate(bullet, handL.transform.position + handL.transform.up * -5, Quaternion.identity);
                b.GetComponent<ConstantForce>().force = boss.transform.forward * 50;
                Destroy(b, 10f);

                b = Instantiate(bullet, handR.transform.position + handR.transform.up * -5, Quaternion.identity);
                b.GetComponent<ConstantForce>().force = boss.transform.forward * 50;
                Destroy(b, 10f);

                shootTime = 0;
            }
            boss.transform.rotation *= Quaternion.Euler(boss.transform.up * Time.deltaTime * 200);
            yield return null;
        }
        StartCoroutine(StartCharge());
    }
    IEnumerator SpikeSpawn(GameObject start, GameObject end) {
        DoLookAtObject(start, 10);
        DoRotateArmX(270, 15);

        yield return new WaitForSeconds(1f);

        Vector3 endPos = end.transform.position;
        endPos.y = boss.transform.position.y;
        Quaternion bossRotation = Quaternion.LookRotation((endPos - boss.transform.position).normalized);

        GameObject attackSpawn = Resources.Load("FinalBoss/AttackSpawn") as GameObject;
        float flipTime = 5f;
        bool flipped = true;
        float angleSize = 90 / 10;
        float nextAngle = 90;
        while (Quaternion.Angle(boss.transform.rotation, bossRotation) > 1f) {
            flipTime += Time.deltaTime;
            if (flipTime > 0.2f) {
                if (flipped) DoRotateArmX(260, 15);
                else         DoRotateArmX(280, 15);

                flipped = !flipped;
                flipTime = 0;
            }
                
            if (Quaternion.Angle(boss.transform.rotation, bossRotation) < nextAngle) {
                GameObject a = Instantiate(attackSpawn, boss.transform.position - (Vector3.up * 4.5f) + (boss.transform.forward * Random.Range(15, 30)), boss.transform.rotation);
                StartCoroutine(SpikeAttack(a));
                nextAngle -= angleSize;
            }
            boss.transform.rotation = Quaternion.Slerp(boss.transform.rotation, bossRotation, Time.deltaTime * 2);
            yield return null;
        }
        boss.transform.rotation = bossRotation;

        DoRotateArmX(270, 15);
        StartCoroutine(StartCharge());
    }
    IEnumerator SpikeAttack(GameObject attackSpawn) {
        yield return new WaitForSeconds(2.5f);

        Transform[] t = attackSpawn.GetComponentsInChildren<Transform>();
        GameObject spike = null;
        foreach (Transform tr in t) {
            if (tr.name.Contains("Spike"))
                spike = tr.gameObject;
        }
        spike.GetComponent<MeshRenderer>().enabled = true;
        spike.GetComponent<BoxCollider>().enabled = true;

        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.bossAttack);
        Vector3 endPos = Vector3.up * Random.Range(-2, 4);
        while (spike.transform.localPosition.y < endPos.y) {
            spike.transform.localPosition += Vector3.up * Time.deltaTime * 50;
            yield return null;
        }
        spike.transform.localPosition = endPos;

        yield return new WaitForSeconds(1f);

        Destroy(attackSpawn, 0.2f);
    }
    IEnumerator CrushAttack(GameObject start, GameObject end) {
        DoLookAtObject(start, 10);
        DoRotateArmX(270, 15);

        yield return new WaitForSeconds(1f);

        Vector3 startPos = start.transform.position;
        startPos.y = boss.transform.position.y;
        Quaternion bossRotationS = Quaternion.LookRotation((startPos - boss.transform.position).normalized);

        Vector3 endPos = end.transform.position;
        endPos.y = boss.transform.position.y;
        Quaternion bossRotationE = Quaternion.LookRotation((endPos - boss.transform.position).normalized);

        Quaternion bossRotation = bossRotationE;

        float handX = 0.7f;
        float handZ = 1f;
        while (handL.transform.localScale.x < handX && handL.transform.localScale.z < handZ) {
            if (handL.transform.localScale.x < handX)
                handL.transform.localScale = handR.transform.localScale += new Vector3(Time.deltaTime, 0, 0);
            if (handL.transform.localScale.z < handZ)
                handL.transform.localScale = handR.transform.localScale += new Vector3(0, 0, Time.deltaTime);
            yield return null;
        }
        handL.transform.localScale = new Vector3(handX, handL.transform.localScale.y, handZ);
        handR.transform.localScale = new Vector3(handX, handR.transform.localScale.y, handZ);

        float extendLength = 7;
        StartCoroutine(ExtendHand(handL, extendLength, new Vector3(-0.5f, -0.6f, 0), true));

        yield return new WaitUntil(() => handL.transform.localScale.y == extendLength);

        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.bossAttack);

        int attackCount = 0;
        bool flipArm = true;
        float turnTimer = 0;
        while (attackCount < 2 && Quaternion.Angle(boss.transform.rotation, bossRotation) > 1f) {
            if (flipArm) {
                if (handL.transform.localScale.y == extendLength)
                    StopExtendHandL(ExtendHand(handL, extendLength, new Vector3(-0.5f, -0.6f, 0), false));
                if (handR.transform.localScale.y == 0.5f)
                    StopExtendHandR(ExtendHand(handR, extendLength, new Vector3(0.5f, -0.6f, 0), true));
                if (handR.transform.localScale.y == extendLength && handL.transform.localScale.y == 0.5f) {
                    AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.bossAttack);
                    flipArm = false;
                }
            }
            else {
                if (handR.transform.localScale.y == extendLength)
                    StopExtendHandR(ExtendHand(handR, extendLength, new Vector3(0.5f, -0.6f, 0), false));
                if (handL.transform.localScale.y == 0.5f)
                    StopExtendHandL(ExtendHand(handL, extendLength, new Vector3(-0.5f, -0.6f, 0), true));
                if (handL.transform.localScale.y == extendLength && handR.transform.localScale.y == 0.5f) {
                    AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.bossAttack);
                    flipArm = true;
                }
            }

            turnTimer += Time.deltaTime;
            if (turnTimer > 5) {
                bossRotation = bossRotation == bossRotationE ? bossRotationS : bossRotationE;
                attackCount++;
                turnTimer = 0;
            }
            boss.transform.rotation = Quaternion.Slerp(boss.transform.rotation, bossRotation, Time.deltaTime * 0.4f);
            yield return null;
        }
        StopExtendHandL(ExtendHand(handL, extendLength, new Vector3(-0.5f, -0.6f, 0), false));
        StopExtendHandR(ExtendHand(handR, extendLength, new Vector3(0.5f, -0.6f, 0), false));

        yield return new WaitUntil(() => handL.transform.localScale.y == 0.5f && handR.transform.localScale.y == 0.5f);

        handX = 0.5f;
        handZ = 0.5f;
        while (handL.transform.localScale.x > handX && handL.transform.localScale.z > handZ) {
            if (handL.transform.localScale.x > handX)
                handL.transform.localScale = handR.transform.localScale -= new Vector3(Time.deltaTime, 0, 0);
            if (handL.transform.localScale.z > handZ)
                handL.transform.localScale = handR.transform.localScale -= new Vector3(0, 0, Time.deltaTime);
            yield return null;
        }
        handL.transform.localScale = new Vector3(handX, handL.transform.localScale.y, handZ);
        handR.transform.localScale = new Vector3(handX, handR.transform.localScale.y, handZ);

        yield return new WaitForSeconds(1f);

        StartCoroutine(StartCharge());
    }
    void StopExtendHandL(IEnumerator c) {
        if (extendHandLCoroutine != null)
            StopCoroutine(extendHandLCoroutine);
        extendHandLCoroutine = c;
        StartCoroutine(extendHandLCoroutine);
    }
    void StopExtendHandR(IEnumerator c) {
        if (extendHandRCoroutine != null)
            StopCoroutine(extendHandRCoroutine);
        extendHandRCoroutine = c;
        StartCoroutine(extendHandRCoroutine);
    }
    IEnumerator ExtendHand(GameObject hand, float maxLength, Vector3 startPos, bool extend) {
        float extendSpeed = 5;
        float minLength = 0.5f;
        float extendSpeedR = 2;
        
        if (hand == handR)
            extendSpeedR *= -1;

        if (extend) {
            while (hand.transform.localScale.y < maxLength) {
                hand.transform.localScale += new Vector3(0, Time.deltaTime * extendSpeed, 0);
                hand.transform.localPosition += hand.transform.forward * Time.deltaTime * extendSpeedR;
                yield return null;
            }
            hand.transform.localScale = new Vector3(hand.transform.localScale.x, maxLength, hand.transform.localScale.z);
        }
        else {
            while (hand.transform.localScale.y > minLength) {
                hand.transform.localScale -= new Vector3(0, Time.deltaTime * extendSpeed, 0);
                hand.transform.localPosition -= hand.transform.forward * Time.deltaTime * extendSpeedR;
                yield return null;
            }
            hand.transform.localScale = new Vector3(hand.transform.localScale.x, minLength, hand.transform.localScale.z);
            hand.transform.localPosition = startPos;
        }
    }
    IEnumerator StartCharge() {
        DoLookAtObject(player, 20);

        yield return new WaitForSeconds(1);

        DoRotateArmX(160, 15);
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face0");
        Vector3 newPos = boss.transform.position + boss.transform.forward * -5;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(1);

        chargePlayerCoroutine = ChargePlayer();
        StartCoroutine(chargePlayerCoroutine);
    }
    IEnumerator ChargePlayer() {
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face1");
        DoRotateArmX(270, 15);
        bossDamage = false;
        Vector3 newPos = boss.transform.position + boss.transform.forward * 50;

        while (Vector3.Distance(boss.transform.position, newPos) > 1) {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, newPos, Time.deltaTime * bossSpeed * 1);
            yield return null;
        }
        DoLookAtObject(lastAttack, 10);
        StartCoroutine(MoveBack());
    }
    IEnumerator MoveBack() {
        face.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("FinalBoss/boss_face0");
        Vector3 bossPos = boss.transform.position;
        bossPos.y = bossOrbit.transform.position.y;
        bossOrbit.transform.LookAt(bossPos);
        boss.transform.parent = bossOrbit.transform;
        Vector3 attackPos = lastAttack.transform.position;
        Vector3 orbitPos = bossOrbit.transform.position;
        attackPos.y = orbitPos.y;

        Quaternion orbitRotation = Quaternion.LookRotation(attackPos - orbitPos);
        Vector3 lastPos = lastAttack.transform.position;
        lastPos.y = boss.transform.position.y;
        while (Quaternion.Angle(bossOrbit.transform.rotation, orbitRotation) > 0.1f) {
            boss.transform.LookAt(lastPos);
            bossOrbit.transform.rotation = Quaternion.Slerp(bossOrbit.transform.rotation, orbitRotation, Time.deltaTime * 2f);
            yield return null;
        }
        bossOrbit.transform.rotation = orbitRotation;
        boss.transform.parent = null;
        DoLookAtObject(lastAttack, 10);
        StartCoroutine(MoveToTop());
    }
    IEnumerator LookAtObject(GameObject gObject, float speed) {
        Vector3 gObjectPos = gObject.transform.position;
        gObjectPos.y = boss.transform.position.y;
        Quaternion bossRotation = Quaternion.LookRotation((gObjectPos - boss.transform.position).normalized);

        while (Quaternion.Angle(boss.transform.rotation, bossRotation) > 0.1f) {
            boss.transform.rotation = Quaternion.Slerp(boss.transform.rotation, bossRotation, Time.deltaTime * speed);
            yield return null;
        }
        boss.transform.rotation = bossRotation;
    }
    IEnumerator RotateArmX(int rotation, float speed) {
        Quaternion armRotation = Quaternion.Euler(rotation, 0, 0);

        while (Quaternion.Angle(armPivotL.transform.localRotation, armRotation) > 0.1f) {
            armPivotL.transform.localRotation = Quaternion.Slerp(armPivotL.transform.localRotation, armRotation, Time.deltaTime * speed);
            armPivotR.transform.localRotation = Quaternion.Slerp(armPivotR.transform.localRotation, armRotation, Time.deltaTime * speed);
            yield return null;
        }
        armPivotL.transform.localRotation = armRotation;
        armPivotR.transform.localRotation = armRotation;
    }
}
