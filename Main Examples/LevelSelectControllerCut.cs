using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectControllerCut : MonoBehaviour {
    public GameState gameState;
    public SelectState selectState;
    public int currentCube;
    public int[,] levelHowToBoss = null;
    public int[] levelUnlocks = null;
    public int[] levelSelects = null;
    public bool[] cubeCompletes = null;
    public string[] cubeNames = null;
    public bool[] didCutscene = null;

    GameObject colorCube;
    public GameObject[] cubes;
    Text cubeSelectText;
    Text controlsText;
    public float selectCubeCooldown;
    public bool confirmCubeCooldown;

    GameObject cam;
    GameObject camOrbit;
    GameObject transitionBGL;
    GameObject transitionBGR;
    public int camLookSpeed = 8;
    public int camMoveSpeed = 500;
    public int camDistance = 80;
    public int camRotateSpeed = 8;
    public bool camIsLooking;
    public bool camIsMoving;
    public bool camIsRotating;
    IEnumerator lookAtCubeCoroutine;
    IEnumerator rotateCamOrbitCoroutine;

    GameObject devText;
    public bool[] devKeys = new bool[4];
    float keyReset;

    void Start() {
        currentCube    = GameController.Instance.currentCube;
        gameState      = GameController.Instance.gameState;
        selectState    = GameController.Instance.selectState;
        levelHowToBoss = GameController.Instance.levelHowToBoss;
        levelUnlocks   = GameController.Instance.levelUnlocks;
        levelSelects   = GameController.Instance.levelSelects;
        cubeCompletes  = GameController.Instance.cubeCompletes;
        cubeNames      = GameController.Instance.cubeNames;
        didCutscene    = GameController.Instance.didCutscene;

        colorCube = GameObject.Find("ColorCube");
        cubes = new GameObject[cubeNames.Length];
        for (int i = 0; i < cubeNames.Length; i++) {
            cubes[i] = GameObject.Find(cubeNames[i] + "Cube");
            cubes[i].transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        }
        for (int i = 0; i < cubeCompletes.Length; i++) {
            if (cubeCompletes[i] || GameController.Instance.devMode)
                CubeCompleted(i);
        }
        cubeSelectText = GameObject.Find("CubeSelect").GetComponent<Text>();
        controlsText = GameObject.Find("Controls").GetComponent<Text>();

        cam = GameObject.FindWithTag("MainCamera");
        camOrbit = GameObject.Find("CameraOrbit");
        transitionBGL = GameObject.Find("TransitionBGL");
        transitionBGR = GameObject.Find("TransitionBGR");

        devText = GameObject.Find("DevText");
        if (!GameController.Instance.devMode)
            devText.SetActive(false);

        AudioController.Instance.PlayMusic(AudioController.Instance.menuMusic);
        InitCamera();
    }

    void CheckDevMode() {
        foreach (bool b in devKeys) {
            if (!b)
                return;
        }
        GameController.Instance.devMode = true;
        devText.SetActive(true);

        for (int i = 0; i < levelUnlocks.GetLength(0); i++)
            levelUnlocks[i] = 3;
        for (int i = 0; i < cubeCompletes.Length; i++) {
            if (!cubeCompletes[i])
                CubeCompleted(i);
        }
        for (int i = 0; i < cubes.Length; i++) {
            Transform[] c = cubes[i].GetComponentsInChildren<Transform>();
            for (int j = 2; j < c.Length; j++) {
                if (levelUnlocks[i] < j - 1)
                    break;

                c[j].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("General/level" + (j - 1) + "_icon");
            }
        }
    }

    void Update() {
        if (Time.timeScale == 0)
            return;

        colorCube.transform.Rotate(Time.deltaTime * 2, Time.deltaTime * -1.5f, Time.deltaTime * 1);
        foreach (GameObject cube in cubes)
            cube.transform.Rotate(Time.deltaTime * 2, Time.deltaTime * -1.5f, Time.deltaTime * 1);

        // Dev Mode
        if (!GameController.Instance.devMode) {
            keyReset -= Time.deltaTime;
            if (keyReset < 0) {
                for (int i = 0; i < devKeys.Length; i++)
                    devKeys[i] = false;
            }
            if (Input.GetKey(KeyCode.C) 
             || Input.GetKey(KeyCode.U) 
             || Input.GetKey(KeyCode.B) 
             || Input.GetKey(KeyCode.E)) 
            {
                if (Input.GetKey(KeyCode.C)) devKeys[0] = true;
                if (Input.GetKey(KeyCode.U)) devKeys[1] = true;
                if (Input.GetKey(KeyCode.B)) devKeys[2] = true;
                if (Input.GetKey(KeyCode.E)) devKeys[3] = true;
                keyReset = 5;
                CheckDevMode();
            }
        }
        if (GameController.Instance.devMode) {
            if (Input.GetKeyDown(KeyCode.F)) {
                SaveToGameController();
                GameController.Instance.DoLoadScene(SceneManager.sceneCountInBuildSettings - 7);
            }
        }

        // Exit to Menu
        if (Input.GetButtonDown("Cancel")) {
            SaveToGameController();
            GameController.Instance.DoLoadScene(0);
        }

        if (selectState == SelectState.CUBES)
            WatchCube(currentCube);

        // Menu Controls
        if (gameState == GameState.LEVEL_SELECT) {
            // Switch Cube/Level
            if (Input.GetAxisRaw("Horizontal") != 0) {
                if (SelectCubeCooldown() && !camIsMoving) {
                    if (selectState == SelectState.CUBES) {
                        AudioController.Instance.PlaySoundOnce(AudioController.Instance.cameraMove);
                        SelectCube(Input.GetAxisRaw("Horizontal"));
                        if (!confirmCubeCooldown) {
                            Invoke("ConfirmCubeCooldown", 0.5f);
                            confirmCubeCooldown = true;
                        }
                    }
                    if (selectState == SelectState.LEVELS || selectState == SelectState.HOW_TO) {
                        if (levelUnlocks[currentCube] > 0) {
                            AudioController.Instance.PlaySoundOnce(AudioController.Instance.cameraMove);
                            SelectLevel(Input.GetAxisRaw("Horizontal"));
                        }
                    }
                }
            }
            else
                selectCubeCooldown = 0;

            // Select Cube/Level
            if (Input.GetAxisRaw("Action 1") != 0) {
                if (!camIsLooking && !camIsMoving && !camIsRotating && !confirmCubeCooldown) {
                    AudioController.Instance.PlaySoundOnce(AudioController.Instance.selectConfirm);
                    if (selectState == SelectState.CUBES) {
                        if (GameController.Instance.didCutscene[currentCube + 1]) {
                            selectState = SelectState.LEVELS;
                            StartCoroutine(MoveToCube(currentCube));
                        }
                        else {
                            SaveToGameController();
                            GameController.Instance.DoLoadScene(SceneManager.sceneCountInBuildSettings - (4 + currentCube));
                        }
                    }
                    else {
                        if (selectState == SelectState.LEVELS && CheckIfUnlocked()) {
                            gameState = GameState.GAME;
                            SaveToGameController();
                            StartCoroutine(LevelTransition(0, 1 + (currentCube * 4) + levelSelects[currentCube]));
                        }
                        else {
                            if (selectState == SelectState.HOW_TO) {
                                gameState = GameState.GAME;
                                SaveToGameController();
                                StartCoroutine(LevelTransition(0, currentCube * 4 + 1));
                            }
                        }
                    }
                }
            }

            // Go Back
            if (Input.GetAxisRaw("Action 2") != 0) {
                if ((selectState == SelectState.LEVELS || selectState == SelectState.HOW_TO) && !camIsLooking && !camIsMoving && !camIsRotating) {
                    AudioController.Instance.PlaySoundOnce(AudioController.Instance.selectBack);
                    selectState = SelectState.CUBES;
                    StartCoroutine(LookAtCenter());
                }
            }
        }
    }

    // Sets camera to correct position
    void InitCamera() {
        switch (selectState) {
            case SelectState.CUBES:
                cubeSelectText.text = "<- " + cubeNames[currentCube] + " ->";
                SetControlsText(1);
                cam.transform.LookAt(cubes[currentCube].transform.position);
                break;
            default:
                CheckLevelType();
                SetControlsText(0);
                camOrbit.transform.position = cubes[currentCube].transform.position;
                camOrbit.transform.SetParent(cubes[currentCube].transform);
                camOrbit.transform.localRotation = Quaternion.Euler(0, levelSelects[currentCube] * -90, 0);
                cam.transform.position = camOrbit.transform.position + camOrbit.transform.forward * camDistance;
                cam.transform.SetParent(camOrbit.transform);
                cam.transform.localRotation = Quaternion.Euler(0, 180, 0);
                if (GameController.Instance.exitedLevel)
                    StartCoroutine(LevelTransition(1, 0));
                break;
        }
        for (int i = 0; i < cubes.Length; i++) {
            Transform[] c = cubes[i].GetComponentsInChildren<Transform>();
            for (int j = 2; j < c.Length; j++) {
                if (levelUnlocks[i] < j - 1)
                    break;

                c[j].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("General/level" + (j - 1) + "_icon");
            }
        }
        if (GameController.Instance.completedLevel && selectState != SelectState.BOSS)
            StartCoroutine(LevelTransition(1, 0));
        if (selectState == SelectState.BOSS)
            selectState = SelectState.LEVELS;
    }
    void CompleteLevel() {
        if (levelSelects[currentCube] < 3)
            SelectLevel(1);
        else {
            CubeCompleted(currentCube);
            cubeCompletes[currentCube] = true;
        }
        GameController.Instance.completedLevel = false;
        foreach (bool b in cubeCompletes) {
            if (b == false)
                return;
        }

        if (GameController.Instance.devMode)
            return;

        SaveToGameController();
        GameController.Instance.DoLoadScene(SceneManager.sceneCountInBuildSettings - 7);
    }
    // Switch through cubes
    void SelectCube(float direction) {
        if (direction < 0) currentCube = currentCube - 1 < 0 ? 2 : --currentCube;
        else               currentCube = currentCube + 1 > 2 ? 0 : ++currentCube;

        cubeSelectText.text = "<- " + cubeNames[currentCube] + " ->";
    }
    // Switch through levels
    void SelectLevel(float direction) {
        if (direction < 0) levelSelects[currentCube] = levelSelects[currentCube] - 1 < 0 ? 3 : --levelSelects[currentCube];
        else               levelSelects[currentCube] = levelSelects[currentCube] + 1 > 3 ? 0 : ++levelSelects[currentCube];

        if (rotateCamOrbitCoroutine != null)
            StopCoroutine(rotateCamOrbitCoroutine);
        rotateCamOrbitCoroutine = RotateCamOrbit(levelSelects[currentCube]);
        StartCoroutine(rotateCamOrbitCoroutine);
    }
    // Camera looks at selected cube
    void WatchCube(int whichCube) {
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(cubes[whichCube].transform.position - cam.transform.position), Time.smoothDeltaTime * camLookSpeed);
    }
    // Camera moves to selected cube
    IEnumerator MoveToCube(int whichCube) {
        camIsMoving = true;
        cam.transform.SetParent(null);
        camOrbit.transform.SetParent(null);
        camOrbit.transform.position = cubes[whichCube].transform.position;
        camOrbit.transform.SetParent(cubes[whichCube].transform);
        camOrbit.transform.rotation = Quaternion.LookRotation(cam.transform.position - camOrbit.transform.position);
        cam.transform.SetParent(camOrbit.transform);

        CheckIfUnlocked();
        while (Vector3.Distance(cam.transform.position, camOrbit.transform.position) > camDistance) {
            cam.transform.position += cam.transform.forward * Time.smoothDeltaTime * camMoveSpeed;
            yield return null;
        }
        cam.transform.localRotation = Quaternion.Euler(0, 180, 0);
        cam.transform.localPosition = new Vector3(0, 0, cam.transform.localPosition.z);
        
        StartCoroutine(FixCamRotation(levelSelects[whichCube]));
    }
    // Camera looks at level side
    IEnumerator RotateCamOrbit(int whichLevel) {
        camIsRotating = true;
        Quaternion camRotation = Quaternion.Euler(0, whichLevel * -90, 0);

        CheckLevelType();
        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.smoothDeltaTime * camRotateSpeed);
            yield return null;
        }
        camOrbit.transform.localRotation = camRotation;
        camIsRotating = false;
    }
    // Orients camera to cube's last selected level
    IEnumerator FixCamRotation(int whichLevel) {
        camIsMoving = true;
        Quaternion camRotation = Quaternion.Euler(0, whichLevel * -90, 0);

        CheckLevelType();
        while (Quaternion.Angle(camOrbit.transform.localRotation, camRotation) > 0.1f) {
            camOrbit.transform.localRotation = Quaternion.Slerp(camOrbit.transform.localRotation, camRotation, Time.smoothDeltaTime * camRotateSpeed);
            yield return null;
        }
        camOrbit.transform.localRotation = camRotation;
        camIsMoving = false;
    }
    // Camera Orbit points back to center
    IEnumerator LookAtCenter() {
        camIsMoving = true;
        camOrbit.transform.SetParent(null);
        Quaternion camRotation = Quaternion.LookRotation(Vector3.zero - camOrbit.transform.position);

        while (Quaternion.Angle(camOrbit.transform.rotation, camRotation) > 0.1f) {
            camOrbit.transform.rotation = Quaternion.Slerp(camOrbit.transform.rotation, camRotation, Time.smoothDeltaTime * camRotateSpeed);
            yield return null;
        }
        camOrbit.transform.rotation = camRotation;
        SetControlsText(1);
        cubeSelectText.text = "<- " + cubeNames[currentCube] + " ->";
        StartCoroutine(MoveToCenter());
    }
    // Camera moves back to center
    IEnumerator MoveToCenter() {
        cam.transform.SetParent(null);

        while (Vector3.Distance(cam.transform.position, Vector3.zero) > 5) {
            WatchCube(currentCube);
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, Vector3.zero, Time.smoothDeltaTime * camMoveSpeed);
            yield return null;
        }
        cam.transform.position = Vector3.zero;
        camIsMoving = false;
    }
    IEnumerator LevelTransition(int whichTransition, int whichLevel) {
        camIsMoving = true;
        Vector3 movePositionL = transitionBGL.transform.localPosition;
        Vector3 movePositionR = transitionBGR.transform.localPosition;
        int distance = 27;

        if (whichTransition == 0) {
            movePositionL.x = -7.05f;
            movePositionR.x = 7.05f;

            Vector3 camPosition = new Vector3(0, 0, distance);

            while (Vector3.Distance(cam.transform.localPosition, camPosition) > 0.1f) {
                cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, camPosition, Time.smoothDeltaTime * 200);
                yield return null;
            }
            cam.transform.localPosition = camPosition;

            yield return new WaitForSeconds(0.2f);

            while (Vector3.Distance(transitionBGL.transform.localPosition, movePositionL) > 0.1f) {
                transitionBGL.transform.localPosition = Vector3.MoveTowards(transitionBGL.transform.localPosition, movePositionL, Time.smoothDeltaTime * 25);
                transitionBGR.transform.localPosition = Vector3.MoveTowards(transitionBGR.transform.localPosition, movePositionR, Time.smoothDeltaTime * 25);
                yield return null;
            }
            transitionBGL.transform.localPosition = movePositionL;
            transitionBGR.transform.localPosition = movePositionR;

            yield return new WaitForSeconds(0.2f);

            GameController.Instance.DoLoadScene(whichLevel);
        }
        else {
            cam.transform.localPosition = new Vector3(0, 0, distance);
            Vector3 startPositionL = movePositionL;
            Vector3 startPositionR = movePositionR;
            startPositionL.x = -7.05f;
            startPositionR.x = 7.05f;
            transitionBGL.transform.localPosition = startPositionL;
            transitionBGR.transform.localPosition = startPositionR;

            yield return new WaitForSeconds(0.2f);

            while (Vector3.Distance(transitionBGL.transform.localPosition, movePositionL) > 0.1f) {
                transitionBGL.transform.localPosition = Vector3.MoveTowards(transitionBGL.transform.localPosition, movePositionL, Time.smoothDeltaTime * 25);
                transitionBGR.transform.localPosition = Vector3.MoveTowards(transitionBGR.transform.localPosition, movePositionR, Time.smoothDeltaTime * 25);
                yield return null;
            }
            transitionBGL.transform.localPosition = movePositionL;
            transitionBGR.transform.localPosition = movePositionR;

            yield return new WaitForSeconds(0.2f);

            Vector3 camPosition = new Vector3(0, 0, camDistance);

            while (Vector3.Distance(cam.transform.localPosition, camPosition) > 0.1f) {
                cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, camPosition, Time.smoothDeltaTime * 200);
                yield return null;
            }
            cam.transform.localPosition = camPosition;
        }
        camIsMoving = false;
        GameController.Instance.exitedLevel = false;
        if (GameController.Instance.completedLevel)
            CompleteLevel();
    }
    // Prevents fast select when held down
    bool SelectCubeCooldown() {
        if (selectCubeCooldown <= 0) {
            selectCubeCooldown = 0.5f;
            return true;
        }
        else {
            selectCubeCooldown -= Time.deltaTime;
            return false;
        }
    }
    // Prevents selection spam
    void ConfirmCubeCooldown() {
        confirmCubeCooldown = false;

        /* Insert where needed
        if (!confirmCubeCooldown) {
            Invoke("ConfirmCubeCooldown", 0.5f);
            confirmCubeCooldown = true;
        }
        */
    }
    // Checks if selected level is unlocked
    bool CheckIfUnlocked() {
        if (levelSelects[currentCube] == 0) {
            CheckLevelType();
            return true;
        }
        SetControlsText(0);
        if (levelUnlocks[currentCube] >= levelSelects[currentCube]) {
            cubeSelectText.text = "<- Level " + levelSelects[currentCube] + " ->";
            return true;
        }
        else {
            cubeSelectText.text = "<- Locked ->";
            return false;
        }
    }
    void CheckLevelType() {
        if (levelSelects[currentCube] == 0) {
            if (levelUnlocks[currentCube] == 0) {
                SetControlsText(4);
                cubeSelectText.text = "How To Play";
            }
            else {
                SetControlsText(0);
                cubeSelectText.text = "<- How To Play ->";
            }
            selectState = SelectState.HOW_TO;
        }
        else {
            CheckIfUnlocked();
            selectState = SelectState.LEVELS;
        }
    }
    // Change controls
    void SetControlsText(int whichText) {
        switch (whichText) {
            case 0:
                controlsText.text = "Z: Confirm \nX: Back \nArrows: Select";
                break;
            case 1:
                controlsText.text = "Z: Confirm \nEsc: Main Menu \nArrows: Select";
                break;
            case 2:
                controlsText.text = "Z: Select \nX: Back \n\nUp: Level Select";
                break;
            case 3:
                controlsText.text = "Z: Select \nX: Back \n\nDown: Level Select";
                break;
            case 4:
                controlsText.text = "Z: Confirm \nX: Back";
                break;
        }
    }
    void CubeCompleted(int whichCube) {
        string mat = cubes[whichCube].GetComponent<MeshRenderer>().material.name;
        mat = mat.Split(' ')[0];
        if (!mat.Contains("Glow"))
            cubes[whichCube].GetComponent<MeshRenderer>().material = Resources.Load<Material>("FinalBoss/" + mat + "Glow");
    }
    // Saves relevant data to the Game Controller
    void SaveToGameController() {
        GameController.Instance.currentCube = currentCube;
        GameController.Instance.gameState = gameState;
        GameController.Instance.selectState = selectState;
    }
}
