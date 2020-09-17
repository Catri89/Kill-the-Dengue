using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    float screenHeight = Screen.height, screenWidth = Screen.width;

    Transform mainMenuPanel, settingsPanel;

    // Start is called before the first frame update
    void Start() {
        //Me fijo si ya mostramos el tutorial de los google play services
        if (!PlayerPrefs.HasKey("GPS Tuto")) {
            //Primero seteo como listo el tutorial
            PlayerPrefs.SetString("GPS Tuto", "Listo");
            ShowGPSTutorial();
            //Muestro tutorial de uso de los GPS
            //  Mostrar boton para hacer el sign in
            //  Mostrar como se va a ver cuando se loguea
            //  Mostrar botones de logros y lista de puntajes
        }
        InitializePanels();
        Debug.Log("Version: " + Application.version);
        this.transform.Find("VersionText").GetComponent<Text>().text = "v" + Application.version;
    }

    public void ShowGPSTutorial() {

    }

    public void InitializePanels() {
        mainMenuPanel = this.transform.Find("MainMenuPanel").transform;
        settingsPanel = this.transform.Find("SettingsPanel").transform;
        SetButtons();
        mainMenuPanel.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
    }

    public void SetButtons() {

        Button startButton = mainMenuPanel.transform.Find("StartButton").GetComponent<Button>();
        startButton.onClick.AddListener(StartGame);
        Button settingsButton = mainMenuPanel.transform.Find("SettingsButton").GetComponent<Button>();
        settingsButton.onClick.AddListener(SettingsButton);
        Button backSettingsButton = settingsPanel.transform.Find("BackSettingsButton").GetComponent<Button>();
        backSettingsButton.onClick.AddListener(BackSettingsButton);
        /*        Toggle soundToggle = optionsPanel.transform.Find("SoundPanel").Find("SoundToggle").GetComponent<Toggle>();
                        soundToggle.onValueChanged.AddListener(GameManager.sharedInstance.ChangeAudio);
                        Slider soundSlider = optionsPanel.transform.Find("SoundPanel").Find("SoundSlider").GetComponent<Slider>();
                        soundSlider.onValueChanged.AddListener(GameManager.sharedInstance.SetAudioVolume);
                        */
        Button exitButton = mainMenuPanel.transform.Find("ExitButton").GetComponent<Button>();
        exitButton.onClick.AddListener(Application.Quit);
    }

    public void SettingsButton() {
        if (!settingsPanel.gameObject.activeSelf) { 
            settingsPanel.gameObject.SetActive(true);
            //settingsPanel.GetComponent<Animator>().Play("EnterCamera");
            //settingsPanel.position = new Vector2(0, settingsPanel.position.y);
        }
    }

    public void BackSettingsButton() {
        //settingsPanel.GetComponent<Animator>().Play("OutCamera");
        settingsPanel.gameObject.SetActive(false);
        //settingsPanel.SetPositionAndRotation(new Vector2(820, 0), settingsPanel.rotation);
    }

    public void StartGame() {
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator LoadGame() {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene("GameScene");

    }

    public void LoadEmpty() {
        SceneManager.LoadScene("EmptyScene");
    }

    public void ExitGame() {
        Application.Quit();
    }
}
