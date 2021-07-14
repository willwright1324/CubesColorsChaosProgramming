using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour {
    public int cutscene = 1;
    string[] text = null;
    int index = -1;

    Text sceneText;
    Image sceneImage;

    void Start() {
        GameController.Instance.Init();
        sceneText = GameObject.Find("SceneText").GetComponent<Text>();
        sceneImage = GameObject.Find("SceneImage").GetComponent<Image>();

        CanvasScaler cs = GameObject.Find("MainCanvas").GetComponent<CanvasScaler>();
        Rect r = sceneImage.gameObject.GetComponent<RectTransform>().rect;
        r.width = cs.referenceResolution.x;
        r.height = cs.referenceResolution.y;

        TextAsset file = Resources.Load("Cutscenes/" + cutscene + "/cutscene_text" + cutscene) as TextAsset;
        text = file.text.Split("\n"[0]);
        AudioController.Instance.PlayMusic(AudioController.Instance.puzzleMusic);
        Next();
    }

    void Update() {
        if (Time.timeScale == 0)
            return;

        if (Input.GetButtonDown("Action 1")) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.selectConfirm);
            Next();
        }
    }

    void Next() {
        index++;
        if (index < text.Length) {
            sceneText.text = text[index];
            sceneImage.sprite = Resources.Load<Sprite>("Cutscenes/" + cutscene + "/image" + index);
        }
        else {
            switch (cutscene) {
                case 1:
                case 2:
                case 3:
                case 4:
                    GameController.Instance.didCutscene[cutscene - 1] = true;
                    GameController.Instance.DoLoadScene(SceneManager.sceneCountInBuildSettings - 2);
                    break;
                case 5:
                    GameController.Instance.DoLoadScene(SceneManager.sceneCountInBuildSettings - 1);
                    break;
                case 6:
                    GameController.Instance.gameState = GameState.LEVEL_SELECT;
                    GameController.Instance.selectState = SelectState.CUBES;
                    GameController.Instance.DoLoadScene(0);
                    break;
            }
        }
    }
}
