using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {
    public GameObject[] buttons = new GameObject[4];
    public GameObject select;
    public GameObject creditsScreen;
    GameObject confirmation;
    Text selectText;
    public int selection;
    public int menuMode;
    int selectionRange;

    void Start() {
        GameController.Instance.Init();
        select = GameObject.Find("Select");
        creditsScreen = GameObject.Find("CreditsScreen");
        creditsScreen.SetActive(false);
        confirmation = GameObject.Find("Confirmation");
        confirmation.SetActive(false);

        selectText = select.GetComponent<Text>();

        selectionRange = buttons.Length - 1;
        select.transform.position = buttons[0].transform.position;

        AudioController.Instance.PlayMusic(AudioController.Instance.menuMusic);
    }

    void Update() {
        if (Time.timeScale == 0)
            return;

        if (Input.GetButtonDown("Vertical") && menuMode == 0) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.selectMove);
            Select(Input.GetAxisRaw("Vertical"));
        }
        if (Input.GetButtonDown("Action 1")) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.selectConfirm);
            switch (menuMode) {
                case 1:
                    creditsScreen.SetActive(false);
                    menuMode = 0;
                    return;
                case 2:
                    GameController.Instance.Save();
                    Application.Quit();
                    return;
                case 3:
                    GameController.Instance.DeleteSave();
                    confirmation.SetActive(false);
                    menuMode = 0;
                    return;
            }
            switch (buttons[selection].name) {
                case "Play":
                    if (!GameController.Instance.didCutscene[0])
                        GameController.Instance.DoLoadScene(SceneManager.sceneCountInBuildSettings - 3);
                    else
                        GameController.Instance.DoLoadScene(SceneManager.sceneCountInBuildSettings - 2);
                    break;
                case "Credits":
                    menuMode = 1;
                    creditsScreen.SetActive(true);
                    break;
                case "Quit":
                    if (menuMode == 0) {
                        menuMode = 2;
                        confirmation.SetActive(true);
                    }
                    break;
                case "Delete":
                    if (menuMode == 0) {
                        menuMode = 3;
                        confirmation.SetActive(true);
                    }
                    break;
            }
        }
        if (Input.GetButtonDown("Action 2") && (menuMode == 2 || menuMode == 3)) {
            AudioController.Instance.audioSound.PlayOneShot(AudioController.Instance.selectBack);
            confirmation.SetActive(false);
            menuMode = 0;
        }
    }
    void Select(float direction) {
        if (direction > 0) selection = selection - 1 < 0 ? selectionRange : --selection;
        else               selection = selection + 1 > selectionRange ? 0 : ++selection;

        selectText.text = "<";
        for (int i = 0; i < buttons[selection].GetComponent<Text>().text.Length + 2; i++)
            selectText.text += " ";
        selectText.text += ">";
        select.transform.position = buttons[selection].transform.position;
    }
}
