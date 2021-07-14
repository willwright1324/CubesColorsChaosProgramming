using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour {
    public string folderName = "Circle";
    public int boardSize = 4;

    public GameObject[,] boardPositionGrid;
    public GameObject[,] puzzlePieceGrid;
    GameObject boardPositions;
    GameObject puzzlePieces;
    GameObject cam;
    GameObject puzzlePiece;
    GameObject cursor;
    GameObject enemy;
    GameObject eye;
    GameObject arm;
    GameObject block;
    public float attackTime = 3f;
    public int cursorX;
    public int cursorY;
    public int emptyX;
    public int emptyY;
    int attackX;
    int attackY;
    public bool canSelect = true;
    public bool canMove = true;
    bool isAttacking;
    bool won;
    float xMargin = 0;
    float yMargin = 0;

    public static PuzzleController Instance { get; private set; } = null;
    private void Awake() { Instance = this; }

    void Start() {
        GameController.Instance.InitHealth();
        boardPositions = GameObject.Find("BoardPositions");
        puzzlePieces   = GameObject.Find("PuzzlePieces");
        cam            = GameObject.FindWithTag("MainCamera");
        puzzlePiece    = Resources.Load("Puzzle/PuzzlePiece") as GameObject;
        cursor         = GameObject.Find("Cursor");
        enemy          = GameObject.FindWithTag("Enemy");
        eye            = GameObject.Find("Enemy/Eye");
        arm            = GameObject.Find("Enemy/Eye/Arm");
        block          = GameObject.FindWithTag("Damage");

        GameObject puzzleP = Resources.Load("Puzzle/PuzzlePiece") as GameObject;
        Bounds puzzleSize = puzzleP.GetComponent<Renderer>().bounds;
        cursor.transform.localScale = puzzleP.transform.localScale;

        Vector3 camPos = cam.transform.position;
        boardPositionGrid = new GameObject[boardSize, boardSize];
        puzzlePieceGrid = new GameObject[boardSize, boardSize];

        int amount = 0;
        for (int y = 0; y < boardSize; y++) {
            for (int x = 0; x < boardSize; x++) {
                amount++;
                GameObject bp = new GameObject();
                bp.name = "BoardPosition|" + amount;
                bp.transform.position = new Vector3(camPos.x + (puzzleSize.size.x + xMargin) * x, camPos.y - (puzzleSize.size.y + yMargin) * y, camPos.z + 200);
                bp.transform.SetParent(boardPositions.transform);
                boardPositionGrid[x, y] = bp;

                GameObject pp = Instantiate(Resources.Load("Puzzle/PuzzlePiece") as GameObject);
                pp.name = "PuzzlePiece|" + amount;
                pp.transform.position = bp.transform.position;
                pp.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Puzzle/" + folderName + "/image_" + amount);
                pp.transform.SetParent(puzzlePieces.transform);
                puzzlePieceGrid[x, y] = pp;
            }
        }
        GameObject image = GameObject.Find("Image");
        image.GetComponent<Image>().sprite = Resources.Load<Sprite>("Puzzle/" + folderName + "/image_0");

        int destroyX = Random.Range(0, boardSize);
        int destroyY = Random.Range(0, boardSize);
        Destroy(puzzlePieceGrid[destroyX, destroyY]);
        emptyX = destroyX;
        emptyY = destroyY;

        int[] pick = { 1, -1 };
        for (int i = 0; i < 100; i++) {
            int randPick = Random.Range(0, 2);
            int randAxis = Random.Range(0, 2);
            int checkX = 0;
            int checkY = 0;

            if (randAxis == 0) checkX = pick[randPick];
            else               checkY = pick[randPick];

            if (emptyX + checkX < 0 || emptyX + checkX > boardSize - 1)
                checkX = -checkX;
            if (emptyY + checkY < 0 || emptyY + checkY > boardSize - 1)
                checkY = -checkY;

            int newX = emptyX + checkX;
            int newY = emptyY + checkY;

            puzzlePieceGrid[emptyX, emptyY] = puzzlePieceGrid[newX, newY];
            puzzlePieceGrid[newX, newY] = null;
            puzzlePieceGrid[emptyX, emptyY].transform.position = boardPositionGrid[emptyX, emptyY].transform.position;

            emptyX = newX;
            emptyY = newY;
        }

        cursorX = emptyX;
        cursorY = emptyY;
        if (emptyX - 1 >= 0) cursorX--;
        else                 cursorX++;

        cursor.transform.position = new Vector3(boardPositionGrid[cursorX, emptyY].transform.position.x, boardPositionGrid[cursorX, emptyY].transform.position.y, cursor.transform.position.z);
        enemy.transform.position  = new Vector3(boardPositionGrid[emptyX, emptyY].transform.position.x, boardPositionGrid[emptyX, emptyY].transform.position.y, cursor.transform.position.z - 1);
        cam.transform.position    = new Vector2(boardPositionGrid[0, 0].transform.position.x - (puzzleSize.size.x / 2) + (puzzleSize.size.x * boardSize) / 2, 
                                                boardPositionGrid[0, 0].transform.position.y + (puzzleSize.size.y / 2) - (puzzleSize.size.y * boardSize) / 2);

        GameController.Instance.DoStartGame(AudioController.Instance.puzzleMusic);
    }

    void Update() {
        if (Time.timeScale == 0)
            return;

        if (attackTime > 0)
            attackTime -= Time.deltaTime;
        else {
            if (!isAttacking && !won) {
                isAttacking = true;
                int tempX = emptyX - cursorX;
                int tempY = emptyY - cursorY;
                attackX = cursorX;
                attackY = cursorY;
                if (tempX != 0) {
                    eye.transform.localPosition = new Vector3(-0.6f * tempX, 0, -0.1f);
                    eye.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                }
                if (tempY != 0) {
                    eye.transform.localPosition = new Vector3(0, 0.6f * tempY, -0.1f);
                    eye.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                Invoke("DoAttack", 1);
            }
        }
        if (canSelect && canMove && !won) {
            if (Input.GetButtonDown("Horizontal")) {
                canSelect = false;
                MoveX(Input.GetAxisRaw("Horizontal"));
            }
            else {
                if (Input.GetButtonDown("Vertical")) {
                    canSelect = false;
                    MoveY(Input.GetAxisRaw("Vertical"));
                }
                else {
                    if (Input.GetButtonDown("Action 1") && !isAttacking) {
                        canSelect = false;
                        canMove = false;
                        DoMovePiece();
                    }
                }
            }
        }
        if (won) {
            if (Input.GetButtonDown("Horizontal")) {
                canSelect = false;
                WinMoveX(Input.GetAxisRaw("Horizontal"));
            }
            else {
                if (Input.GetButtonDown("Vertical")) {
                    canSelect = false;
                    WinMoveY(Input.GetAxisRaw("Vertical"));
                }
                else {
                    if (Input.GetButtonDown("Action 1"))
                        CheckCube();
                }
            }
        }
    }

    void CheckBoard() {
        bool clear = true;
        for (int y = 0; y < boardSize; y++) {
            for (int x = 0; x < boardSize; x++) {
                if (!(x == emptyX && y == emptyY)) {
                    string board = boardPositionGrid[x, y].name;
                    board = board.Split('|')[1];
                    string piece = puzzlePieceGrid[x, y].name;
                    piece = piece.Split('|')[1];
                    if (!(board.Equals(piece))) {
                        clear = false;
                        break;
                    }
                }
            }
            if (!clear)
                break;
        }
        if (clear) {
            Destroy(enemy);
            GameObject.FindWithTag("PowerCube").transform.position = boardPositionGrid[emptyX, emptyY].transform.position;
            won = true;
        }          
    }
    void DoAttack() {
        StartCoroutine(Attack());
    }
    public void DamagePlayer() {
        if (attackX == cursorX && attackY == cursorY) {
            GameController.Instance.DamagePlayer();
        }
    }
    void MoveX(float direction) {
        if (direction == 0) {
            canSelect = true;
            return;
        }
        float newX = emptyX + direction;

        if (newX >= 0 && newX <= boardSize - 1) {
            cursorX = (int)newX;
            cursorY = emptyY;
            StartCoroutine(MoveCursor());
        }
        else
            canSelect = true;
    }
    void MoveY(float direction) {
        if (direction == 0) {
            canSelect = true;
            return;
        }
        float newY = emptyY - direction;

        if (newY >= 0 && newY <= boardSize - 1) {
            cursorY = (int)newY;
            cursorX = emptyX;
            StartCoroutine(MoveCursor());
        }
        else
            canSelect = true;
    }
    void WinMoveX(float direction) {
        if (direction == 0) {
            canSelect = true;
            return;
        }
        float newX = cursorX + direction;

        if (newX >= 0 && newX <= boardSize - 1) {
            cursorX = (int)newX;
            StartCoroutine(MoveCursor());
        }
        else
            canSelect = true;
    }
    void WinMoveY(float direction) {
        if (direction == 0) {
            canSelect = true;
            return;
        }
        float newY = cursorY - direction;

        if (newY >= 0 && newY <= boardSize - 1) {
            cursorY = (int)newY;
            StartCoroutine(MoveCursor());
        }
        else
            canSelect = true;
    }
    void CheckCube() {
        if (cursorX == emptyX && cursorY == emptyY)
            GameController.Instance.CompleteLevel();
    }
    void DoMovePiece() {
        int tempX = cursorX;
        int tempY = cursorY;
        cursorX = emptyX;
        cursorY = emptyY;
        emptyX = tempX;
        emptyY = tempY;
        StartCoroutine(MovePiece());
    }
    IEnumerator Attack() {
        Quaternion armRotation = Quaternion.Euler(Vector2.zero);

        while (Quaternion.Angle(arm.transform.localRotation, armRotation) > 0.1f) {
            arm.transform.localRotation = Quaternion.Slerp(arm.transform.localRotation, armRotation, Time.deltaTime * 10);
            yield return null;
        }
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.obstacleAppear);
        arm.transform.localRotation = armRotation;
        block.transform.position = boardPositionGrid[attackX, attackY].transform.position + Vector3.back * 1;

        yield return new WaitForSeconds(0.5f);

        block.transform.position = new Vector3(500, 500, 0);
        arm.transform.localRotation = Quaternion.Euler(new Vector2(90, 0));
        eye.transform.localPosition = Vector3.back * 0.1f;
        attackTime = 5f;
        isAttacking = false;
    }
    IEnumerator MoveCursor() {
        if (enemy != null)
            enemy.transform.position = new Vector3(boardPositionGrid[emptyX, emptyY].transform.position.x, boardPositionGrid[emptyX, emptyY].transform.position.y, cursor.transform.position.z - 1);

        Vector3 cursorPosition = new Vector3(boardPositionGrid[cursorX, cursorY].transform.position.x, boardPositionGrid[cursorX, cursorY].transform.position.y, cursor.transform.position.z);
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.playerMove);

        while (Vector3.Distance(cursor.transform.position, cursorPosition) > 1) {
            cursor.transform.position = Vector3.MoveTowards(cursor.transform.position, cursorPosition, Time.deltaTime * 500);
            yield return null;
        }
        cursor.transform.position = cursorPosition;
        canSelect = true;
    }
    IEnumerator MovePiece() {
        Vector3 piecePosition = boardPositionGrid[cursorX, cursorY].transform.position;
        StartCoroutine(MoveCursor());
        AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.tileSwitch);

        while (Vector3.Distance(puzzlePieceGrid[emptyX, emptyY].transform.position, piecePosition) > 1) {
            puzzlePieceGrid[emptyX, emptyY].transform.position = Vector3.MoveTowards(puzzlePieceGrid[emptyX, emptyY].transform.position, piecePosition, Time.deltaTime * 500);
            yield return null;
        }
        puzzlePieceGrid[emptyX, emptyY].transform.position = piecePosition;
        puzzlePieceGrid[cursorX, cursorY] = puzzlePieceGrid[emptyX, emptyY];
        puzzlePieceGrid[emptyX, emptyY] = null;
        canMove = true;
        CheckBoard();
    }
}
