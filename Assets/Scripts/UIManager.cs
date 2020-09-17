using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Instancia compartida para acceder desde otras clases
    public static UIManager sharedInstance;

    //Elementos de la IU para manejar durannte el juego
    public Slider healthSlider;             //Muestra cantidad de asteroides destruidos en la IU
    public Text healthSliderText;           //Muestra cantidad de asteroides destruidos en la IU
    public Text pointsText;                 //Muestra puntos acumulados en la IU
    public Text timeElapsedText;            //Muestra tiempo transcurrido
    public Text stageText;                  //Muestra el nivel actual de juego
    public RectTransform gameOverPanel;     //Mensaje de juego finalizado
    public RectTransform gamePanel;         //Panel de datos del juego
    public RectTransform menuPanel;         //Panel de pausa
    public RectTransform backButton;        //Boton de vuelta al juego del panel de pausa
    public RectTransform restartButton;     //Boton de reinicio de juego del panel de pausa
    public RectTransform soundPanel;        //Panel de opciones de sonido del panel de pausa
    public RectTransform extendPanel;       //Panel de pregunta de extension con video
    public RectTransform continuePanel;     //Panel de continuacion de juego
    public Canvas gameCanvas;               //Canvas controlador del UI

    private void Awake() {
        sharedInstance = this;    
    }

    // Update is called once per frame
    void Update()
    {
        switch (GameManager.sharedInstance.gamestate) {
            case GameManager.gameStates.GameOver:
                menuPanel.GetChild(0).GetChild(0).GetComponent<Text>().text = "Fin de la partida";
                menuPanel.gameObject.SetActive(true);       //Se activa el menu con la opcion de reiniciar
                continuePanel.gameObject.SetActive(false);  //Se desactiva el panel de continuacion
                extendPanel.gameObject.SetActive(false);    //Se desactiva el menu de pregunta de extension con video
                restartButton.gameObject.SetActive(true);   //Se desactiva el boton de reinicio de juego del panel de pausa
                backButton.gameObject.SetActive(false);     //Se desactiva el boton de volver al juego del panel de pausa
                soundPanel.gameObject.SetActive(false);     //Se desactiva el panel de opciones de sonido
                break;
            case GameManager.gameStates.Pause:
                menuPanel.GetChild(0).GetChild(0).GetComponent<Text>().text = "Menu de Pausa";
                continuePanel.gameObject.SetActive(false);  //Se desactiva el panel de continuacion
                extendPanel.gameObject.SetActive(false);    //Se desactiva el menu de pregunta de extension con video
                backButton.gameObject.SetActive(true);      //Se activa el boton de volver al juego del panel de pausa
                restartButton.gameObject.SetActive(false);  //Se desactiva el boton de reinicio de juego del panel de pausa
                soundPanel.gameObject.SetActive(true);      //Se activa el panel de opciones de sonido
                break;
            case GameManager.gameStates.Playing:
                continuePanel.gameObject.SetActive(false);  //Se desactiva el panel de continuacion
                extendPanel.gameObject.SetActive(false);    //Se desactiva el menu de pregunta de extension con video
                menuPanel.gameObject.SetActive(false);      //Se activa el menu por si estaba en gameover
                break;
            case GameManager.gameStates.AskExtend:
                extendPanel.gameObject.SetActive(true);     //Se activa el menu de pregunta de extension con video
                menuPanel.gameObject.SetActive(false);      //Se desactiva el menu por si estaba en gameover
                break;
            case GameManager.gameStates.Continue:
                continuePanel.gameObject.SetActive(true);   //Se activa el panel de continuacion
                extendPanel.gameObject.SetActive(false);    //Se desactiva el menu de pregunta de extension con video
                break;
            default:
                break;
        }
        /*
        if (GameManager.sharedInstance.gamestate == GameManager.gameStates.GameOver) {
            gameOverPanel.sizeDelta = new Vector2(gameOverPanel.sizeDelta.x, (Screen.height - gamePanel.sizeDelta.y * gameCanvas.scaleFactor) / gameCanvas.scaleFactor);
            gameOverPanel.gameObject.SetActive(true);   //Se activa el panel con el cartel de gameover
        }
        */
    }

    public void SetStageText(float newValue) {
        stageText.text = newValue.ToString();
    }

    public void SetPointsText(float newValue) {
        pointsText.text = newValue.ToString();
    }

    public void SetHealthSlider(float newValue) {
        healthSlider.value = newValue;
        healthSliderText.text = (healthSlider.value / healthSlider.maxValue).ToString("P0");
    }

    public void SetTimeElapsed(float newValue) {
        timeElapsedText.text = newValue.ToString("F2");
    }
}
