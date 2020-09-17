using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameItemSpawner : MonoBehaviour
{
    //Instancia compartida para acceder desde otras clases
    public static GameItemSpawner sharedInstance;
    //Prefab de cada gameitem para instanciar
    public GameObject enemyPrefab;              //prefab del virus
    public GameObject[] collectablePrefabs;     //prefab de los collectables

    //Variables de juego
    public WorldObject[] worldObjects;                      //array con todos los objetos creados
    public int maxEnemies = 30;                             //Cantidad maxima de enemigos en juego
    public int startingEnemies = 10;                        //Cantidad inicial de enemigos a spawnear 
    public float enemySpawnTime = 0.0f;                     //Variable para controlar el tiempo de creacion de enemigos nuevos
    public float enemySpawnFrequency = 5.0f;                //Frecuencia con que se van a crear nuevos enemigos
    public float enemyBaseSpawnFrequency = 5.0f;            //Frecuencia base con que se van a crear nuevos enemigos
    public float minEnemySpawnFrequency = 0.5f;             //Frecuencia minima con que se van a crear nuevos enemigos
    public float collectableSpawnTime = 0.0f;               //Variable para controlar el tiempo de creacion de coleccionables
    public float collectableSpawnFrequency = 5.0f;          //Frecuencia con que se van a crear items coleccionables
    public float collectableBaseSpawnFrequency = 5.0f;      //Frecuencia base con que se van a crear items coleccionables
    public float maxCollectableSpawnFrequency = 10.0f;      //Frecuencia con que se van a crear items coleccionables

    public float miniBossChance = 0.02f;                    //Probabilidad de spawnear un enemigo especial

    private CircleCollider2D safetySpawnAreaCollider;       //Bordes del area de seguridad del personaje
    private Bounds safetySpawnAreaBounds;       //Bordes del area de seguridad del personaje

    private AudioSource audioSource;            //Sonido de destruccion de un enemigo

    private void Awake() {
        //Se setea la instancia compartida
        sharedInstance = this;
        //Se toma el area de respawneo del jugador, para tener en cuenta a la hora de crear asteroides
        safetySpawnAreaCollider = GameObject.Find("SafetySpawnArea").GetComponent<CircleCollider2D>();
        safetySpawnAreaBounds = safetySpawnAreaCollider.bounds;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Se crean los enemigos iniciales
        //SpawnInitialEnemies();
        //Se toma el componente del audio para su posterior uso
        audioSource = GetComponent<AudioSource>();
    }

    //Creacion de los asteroides iniciales
    public void SpawnInitialEnemies() {
        //Segun la variable seteada es la cantidad
        for (int i = 0; i < startingEnemies; i++) {
            CreateEnemy();   //Se generan todos en el mayor tamaño para que sea mas facil al inicio
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Si el juego esta en estado jugando, llama a las creaciones de item
        if (GameManager.sharedInstance.gamestate == GameManager.gameStates.Playing) {
            worldObjects = GetComponentsInChildren<WorldObject>();
            SpawnEnemy();
            SpawnCollectable();
        }
    }

    //Recalcula los parametros de juego cuando cambia el nivel
    public void StageChange(int newStage) {
        miniBossChance += 0.01f;
        maxEnemies = Mathf.RoundToInt(10 + newStage / 2);
        enemySpawnFrequency = 5 - 0.25f * newStage;
        if (enemySpawnFrequency < minEnemySpawnFrequency) enemySpawnFrequency = minEnemySpawnFrequency;
        collectableSpawnFrequency = 5 + 1f * newStage;
        if (collectableSpawnFrequency > maxCollectableSpawnFrequency) collectableSpawnFrequency = maxCollectableSpawnFrequency;
        foreach(WorldObject gameItem in this.GetComponentsInChildren<WorldObject>()) {
            gameItem.StageChange(newStage);
        }
    }

    //Actualiza el volumen de los enemigos spawneados
    public void UpdateEnemiesVolume() {
        worldObjects = GetComponentsInChildren<WorldObject>();
        foreach (Enemy enemy in worldObjects) {
            if (enemy) {
                enemy.UpdateSoundLevel();
            }
        }
    }

    //Detiene el sonido de los enemigos
    public void StopEnemiesSound() {
        //try {
            worldObjects = GetComponentsInChildren<WorldObject>();
            foreach (Enemy enemy in worldObjects) {
                if (enemy) {
                    enemy.GetComponent<AudioSource>().mute = true;
                }
            }
        //}
        //catch (System.Exception) {
        //    Debug.Log("Error de StopEnemiesSound");
            //throw;
        //}
    }

    //Reinicia el sonido de los enemigos
    public void StartEnemiesSound() {
        worldObjects = GetComponentsInChildren<WorldObject>();
        foreach (Enemy enemy in worldObjects) {
            if (enemy) {
                enemy.GetComponent<AudioSource>().mute = false;
            }
        }
    }

    private void SpawnEnemy() {
        //Se fija la cantidad de enemigos hijos que tiene el manager para ver si no se llego al limite
        if (this.transform.GetComponentsInChildren<Enemy>().Length < maxEnemies) {
            //Si el tiempo de creacion de enemigos llego al tiempo seteado
            if (enemySpawnTime >= enemySpawnFrequency) {
                //Se vuelve a cero el contador y se llama al creador de enemigos
                enemySpawnTime = 0;
                CreateEnemy();
            } else {
                //Sino se suma el deltatime al tiempo de creacion
                enemySpawnTime += Time.deltaTime;
            }
        }
    }

    private void SpawnCollectable() {
        //Si el tiempo de creacion de colleccionables llego al tiempo seteado
        if (collectableSpawnTime >= collectableSpawnFrequency) {
            //Se vuelve a cero el contador y se llama al creador de colleccionables
            collectableSpawnTime = 0;
            CreateCollectable();
        } else {
            //Sino se suma el deltatime al tiempo de creacion
            collectableSpawnTime += Time.deltaTime;
        }
    }

    //Crea un nuevo collectable en pantalla que queda como hijo del itemspawner
    private void CreateCollectable() {
        //Se genera un vector de posicion para randomizar
        Vector2 rndPosition = new Vector2(0, 0) {
            //Se generan la coordenadas x e y random, dentro del minimo y del maximo area de juego
            x = Random.Range(CameraController.sharedInstance.GetMinX(), CameraController.sharedInstance.GetMaxX()),
            y = Random.Range(CameraController.sharedInstance.GetMinY(), CameraController.sharedInstance.GetMaxY())
        };

        //Se elige al azar el collectable a generar
        GameObject collectableType = collectablePrefabs[Random.Range(0,collectablePrefabs.Length)];

        //Se instancia el nuevo collectable a partir del prefab elegido al azar, en la posicion al azar,
        //  y seteandolo como hijo del manager
        GameObject newCollectable = Instantiate(collectableType, rndPosition, new Quaternion(0,0,0,0), this.transform);
    }

    //Metodo de creacion de enemigos, generandolos fuera del area de seguridad del personaje
    private void CreateEnemy() {
        //Se genera la rotacion random del asteroide
        Quaternion rndRotation = Quaternion.Euler(0, 0, Random.Range(0, 359));
        //Se genera un vector de posicion para randomizar
        Vector2 rndPosition = new Vector2(0, 0);
        safetySpawnAreaBounds = safetySpawnAreaCollider.bounds;

        //Este ciclo se ejecutara generando posiciones aleatorias hasta que una este fuera del respawn area
        do {
            //Se generan la coordenadas x e y random, dentro del minimo y del maximo area de juego
            rndPosition.x = Random.Range(CameraController.sharedInstance.GetMinX(), CameraController.sharedInstance.GetMaxX());
            rndPosition.y = Random.Range(CameraController.sharedInstance.GetMinY(), CameraController.sharedInstance.GetMaxY());
            //Debug.Log("posicion valida? " + respawnArea.Contains(rndPosition));

        } while (safetySpawnAreaBounds.Contains(rndPosition));
        //Se instancia el nuevo asteroide a partir del prefab que tiene el manager, usando la posicion y rotacion random generadas,
        //  y seteandolo como hijo del manager
        GameObject newEnemy = Instantiate(enemyPrefab, rndPosition, rndRotation, this.transform);
        if (Random.Range(0f, 1f) < miniBossChance) {
            newEnemy.GetComponent<Enemy>().SetEnemyLevel(Random.Range(1, 5));
        }
    }

    //Este metodo de creacion de asteroides acepta el tamaño, la posicion y la rotacion como parametros
    //Actualmente se esta usando cuando se destruye un asteroide para crear sus partes derivadas,
    public void CreateAsteroid(Vector2 position, Quaternion rotation) {
        //Se instancia el nuevo asteroide a partir del prefab que tiene el manager, usando la posicion y rotacion de parametro,
        //  y seteandolo como hijo del manager
        GameObject newAsteroid = Instantiate(enemyPrefab, position, rotation, this.transform);
    }

    //Metodo para destruir todos los objetos spawneados en la partida
    public void DestroyCurrentGameItems() {
        //WorldObject[] currentGameItems = this.GetComponentsInChildren<WorldObject>();
        WorldObject[] currentGameItems;
        currentGameItems = worldObjects;
        foreach(WorldObject gameItem in currentGameItems) {
            if (gameItem) {
                Destroy(gameItem.gameObject);
            }
        }
    }

    //Metodo para destruir asteroides
    public void DestroyGameItem(WorldObject gameItem) {
        //Luego de generar las partes nuevas(si corresponde) reproduce el sonido de la explosion
        SoundManager.PlaySound(audioSource.clip);
        //Y destruye el gameobject del asteroide a destruir
        Destroy(gameItem.gameObject);
    }

    //Metodo para generar partes a partir de la destruccion de un asteroide
    private void CreateParts(Vector2 position, int parts) {
        //Segun la cantidad de partes a generar, se setea una rotacion base para generar la random de cada parte
        //-Si son 2 partes la rotacion base sera de 180
        //-Si son 4 partes la rotacion base sera de 90
        float baseRotation = 360 / parts;
        //Ciclo que genera las 2/4 partes nuevas
        for (int i = 0; i < parts; i++) {
            //La rotacion se generara aleatoria entre la base de rotacion y la cantidad de partes
            //Al poner como rango inferior del random la rotacion base * i, hacemos que esta vaya cambiando entre   000-180/00-090-180-270
            //En el rango superior a la misma formula se le suma la rotacion base, para que se genere entre         180-360/90-180-270-360
            Quaternion partRotation = Quaternion.Euler(0, 0, Random.Range((baseRotation * i), (baseRotation * i) + baseRotation));
            //Se llama al metodo de creacion de asteroides con el tamaño menor al del asteroide original, con la posicion del original y
            //  con la rotacion random
            CreateAsteroid(position, partRotation);
        }

        //De esta forma las partes nuevas van a estar rotadas de la siguiente manera:
        //-Si son 2 partes:
        //--La rotacion base sera de 180, entonces cada parte -->
        //--1ra: El angulo random sera entre 0 y 180
        //--2da: El angulo random sera entre 180 y 360
        //-Si son 4 partes:
        //--La rotacion base sera de 900, entonces cada parte -->
        //--1ra: El angulo random sera entre 0 y 90
        //--2da: El angulo random sera entre 90 y 180
        //--3ra: El angulo random sera entre 180 y 270
        //--4ta: El angulo random sera entre 270 y 360
    }
    /*
    //Este metodo hace que el respawn area del jugador quede en el centro del area de juego
    public void SetRespawnAreaPosition(Vector2 position) {
        playerSafeArea.transform.position = position;
    }*/
}
