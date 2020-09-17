using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GooglePlayGames;

public class GameManager : MonoBehaviour
{
    //Instancia compartida para acceder desde otras clases
    public static GameManager sharedInstance;
    //Enum con todos los estados en los que se puede encontrar el juego
    public enum gameStates {Menu, Pause, GameOver, Playing, AskExtend, Continue}

    //Elementos de la IU para manejar durannte el juego
    public RectTransform gameOverPanel;         //Mensaje de juego finalizado
    public RectTransform gamePanel;             //Panel de datos del juego
    public Canvas gameCanvas;                   //Canvas de manejo de UI
    public GameObject gameTuto;                 //Panel de tutorial de juego

    //Variables de juego
    private int stage = 1;                      //Nivel actual de la partida
    private const float MAX_HEALTH = 100.0f;    //Salud maxima
    private float timeElapsed;                  //Tiempo transcurrido de la partida
    private int points = 0;                     //Puntos acumulados en la partida
    private float stageTime = 0;                //Tiempo transcurrido del nivel
    private int stagePoints = 0;                //Puntos acumulados en el nivel
    public float stageTimeLimit = 5.0f;         //Tiempo para pasar de nivel
    public int stagePointsLimit = 200;          //Puntos necesarios para pasar de nivel
    public bool asteroidFriendlyFire = true;    //Bool para saber si los asteroides se destruyen entre si
    public gameStates gamestate;                //Estado actual del juego
    private Character character;                //jugador
    public GameObject[] touchsDisplay;          //Objeto para mostrar el toque
    [SerializeField]
    private bool extendedGame = false;          //Para saber si ya extendio el juego usando video
    public int healthExtention = 50;            //Cuanta salud se recupera con el video
    public bool showTrail = false;              //bool para mostrar touchs


    //Variables de prueba
    public bool sinCanvas = false;              //Para probar el juego sin canvas


    private void Awake() {
        sharedInstance = this;      //Se asigna la instancia compartida
        character = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Antes de iniciar se verifica si hay que mostrar el tutorial
        //Si todavia no se mostro el tutorial de juego se muestra
        if (!PlayerPrefs.HasKey("GameTuto")) {
            //Lo primero que se hace es setear el playerprefs para que no vuelva a mostrarse
            PlayerPrefs.SetInt("GameTuto", 1);
            //Se activa el gameobject con el tutorial
            gameTuto.SetActive(true);
        } else {
            gameTuto.SetActive(false);
            //Se inicializa el juego si no se muestra el tutorial
            StartGame();
        }
    }

    //Inicializacion del juego
    public void StartGame() {
        GPSManager.sharedInstance.UnlockAchievement(GPGSIds.achievement_primer_partida);
        GameItemSpawner.sharedInstance.SpawnInitialEnemies();   //Crea los enemigos iniciales
        extendedGame = false;               //Setea en falso que se haya extendido el juego
        character.SetHealth((int)MAX_HEALTH);    //Se asigna la salud maxima
        gamestate = gameStates.Playing;     //se marca el estado del juego como jugando
        timeElapsed = 0.0f;                 //se asigna el tiempo transcurrido en cero
        Time.timeScale = 1;                 //se configura la escala de tiempo en 1 por si antes estaba en 0
    }
    
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        showTrail = true;
#endif
        if (gamestate == gameStates.Playing) {  //Solo si el juego esta en juego
            ShowTouchTrail();                   //Si esta en el editor de unity muestra los touch
            Time.timeScale = 1;                 //Seteamos la escala del tiempo en 1 para despausar
            CheckTouchs();                      //A ver si con esto arreglo el touch sobre los bichos
            CheckTime();                        //Se chequea y actualiza el tiempo de juego
            UpdateStage();                      //Verifica si se llego al objetivo para pasar de nivel
            UpdateTexts();                      //Se actualizan los textos de la IU
            CheckGameOver();                    //Se realizan las validaciones para ver si termino el juego
        } else {
            Time.timeScale = 0;                 //Se setea en cero la escala de tiempo para que deje de transcurrir
        }
    }

    //Metodo para ver si el toque de pantalla fue sobre un objeto
    private void CheckTouchs() {
        if (Input.touchCount > 0) {
            foreach (Touch touch in Input.touches) {
                //Debug.Log("touch id     " + touch.fingerId);
                //Debug.Log("touch pos    " + touch.position);
                //Debug.Log("touch rawpos " + touch.rawPosition);
                //Debug.Log("touch phase  " + touch.phase);
                if (touch.phase == TouchPhase.Began) {
                    Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    foreach (WorldObject worldObject in GameItemSpawner.sharedInstance.worldObjects) {
                        if (worldObject) {
                            if (worldObject.GetComponent<Collider2D>().bounds.Contains(touchPosition)) {
                                worldObject.ObjectTouched();
                            }
                        }
                    }
                }
            }
        }
    }

    //Metodo para mostrar el trazo del toque
    private void ShowTouchTrail() {
        if (showTrail) {
            for (var i = 0; i < touchsDisplay.Length; i++) {
                //if (Input.touches.Length > i && Input.GetTouch(i).phase == TouchPhase.Began) {
                if (Input.touches.Length > i) {
                    touchsDisplay[i].SetActive(true);
                    touchsDisplay[i].transform.position = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                } else {
                    touchsDisplay[i].SetActive(false);
                    if (i == 0) touchsDisplay[0].GetComponent<TrailRenderer>().Clear();
                }
            }
        }
    }

    private void UpdateStage() {
        if(stageTime >= stageTimeLimit || stagePoints >= stagePointsLimit ) {
            NextStage();
        } else {
            stageTime += Time.deltaTime;
        }
    }

    private void CheckGameOver() {
        //Chequea si el personaje perdio toda su salud
        if (character.GetHealth() <= 0) {
            GameItemSpawner.sharedInstance.StopEnemiesSound();
            if (extendedGame) {
                GameOver();
            } else {
                gamestate = gameStates.AskExtend;
            }
        }
    }

    public void ExtendGame() {
        character.AddHealth(healthExtention);
        GameItemSpawner.sharedInstance.DestroyCurrentGameItems();
        gamestate = gameStates.Continue;
        extendedGame = true;
    }

    public void ContinueGame() {
        gamestate = gameStates.Playing;
    }

    //se suma el tiempo pasado en el ultimo frame al tiempo transcurrido
    private void CheckTime() {
        timeElapsed += Time.deltaTime;          
    }

    //Se actualizan textos de la IU
    private void UpdateTexts() {
        if (!sinCanvas) {
            UIManager.sharedInstance.SetHealthSlider(character.GetHealth());
            UIManager.sharedInstance.SetTimeElapsed(timeElapsed);
            UIManager.sharedInstance.SetPointsText(points);
            UIManager.sharedInstance.SetStageText(stage);
        }
    }

    public void GameOver() {
        //Se cargan los puntos totales de la partida en la tabla de puntajes de google
        GPSManager.sharedInstance.UpdateLeaderBoard(GPGSIds.leaderboard_puntuaciones_mximas, points);
        //Se carga el tiempo total de la partida en la tabla de tiempos de google
        GPSManager.sharedInstance.UpdateLeaderBoard(GPGSIds.leaderboard_mejores_tiempos, timeElapsed * 1000);
        //Se desbloquea el logro de primer contagio
        GPSManager.sharedInstance.UnlockAchievement(GPGSIds.achievement_primer_contagio);

        gamestate = gameStates.GameOver;            //Se setea el estado de juego "Fin de la partida"
        timeElapsed = 0;                            //Se pone el tiempo restante en 0 por si habia quedado negativo
    }

    //Abre el menu de pausa
    public void PauseMenu() {
        if (gamestate == gameStates.Playing) {
            GameItemSpawner.sharedInstance.StopEnemiesSound();
            gamestate = gameStates.Pause;           //Se setea el estado de juego "Pausa"
        }
    }

    //Abre el menu de pausa
    public void PauseGame() {
        gamestate = gameStates.Pause;               //Se setea el estado de juego "Pausa"
    }

    //Cierra el menu de pausa y vuelve al juego
    public void UnpauseGame() {
        GameItemSpawner.sharedInstance.UpdateEnemiesVolume();
        GameItemSpawner.sharedInstance.StartEnemiesSound();
        gamestate = gameStates.Playing;             //Se setea el estado de juego "En juego"
    }

    //Se hace un "hard reset" del juego
    public void RestartGame() {
        SceneManager.LoadScene("GameScene");        //Se recarga la escena de juego para inicializar todo
    }

    //Se vuelve al menu principal
    public void MainMenu() {
        SceneManager.LoadScene("MainMenu");       //Se recarga la escena de juego para inicializar todo
    }

    //Metodo para que se sumen puntos al contador
    public void AddPoints(int number) {
        points += number;           //Suma cantidad de puntos al global
        stagePoints += number;      //Suma cantidad de puntos a puntos del nivel
    }

    //Metodo para setear el contador de puntos a un numero especifico
    public void SetPoints(int number) {
        points = number;
    }

    //Metodo para sumar un nivel
    public void NextStage() {
        stage++;
        GameItemSpawner.sharedInstance.StageChange(stage);
        stageTime = 0;
        stagePoints = 0;
    }

    //Metodo para setear el nivel actual de la partida
    public void SetStage(int newStage) {
        stage = newStage;
        stageTime = 0;
        stagePoints = 0;
    }

    //Metodo para obtener el nivel actual de la partida
    public int GetStage() {
        return stage;
    }

    /*
    //Esto esta para mostrar la posicion del mouse
    private void OnGUI() {
        Vector3 point = new Vector3();
        Event currentEvent = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = currentEvent.mousePosition.x;
        mousePos.y = Camera.main.pixelHeight - currentEvent.mousePosition.y;

        point = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));


        GUILayout.BeginArea(new Rect(20, Screen.height -100, 250, 120));
        GUILayout.Label("Screen pixels: " + Camera.main.pixelWidth + ":" + Camera.main.pixelHeight);
        GUILayout.Label("Mouse position: " + mousePos);
        GUILayout.Label("Mouse click: " + mousePos);
        GUILayout.Label("World position: " + point.ToString("F3"));
        GUILayout.EndArea();

    }*/
}
