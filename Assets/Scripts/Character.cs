using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public enum Actions { Left, Right, Up, Down, Stand } //Enum de acciones del personaje

    private Vector2 direction;                          //Vector de direccion
    private Vector2 destiny;                            //Vector de destino
    private Animator m_animator;                        //Animator controller
    public float movementSpeed = 5f;                    //Velocidad de movimiento del personaje
    [SerializeField]
    private int health = 0;                             //Salud actual
    private bool resting = false;                       //Bandera para saber si esta descansando
    private Actions prevAction = Actions.Stand;         //Auxiliar para saber la accion previa realizada
    private Actions currentAction = Actions.Stand;      //Accion actual
    private float moveAreaMaxX, moveAreaMaxY, moveAreaMinX, moveAreaMinY;   //limites del area de movimiento

    public ParticleSystem invinsibleAura;               //objeto que tiene el particle system del aura de invencibilidad
    public bool invincible = true;                      //Booleano para saber si el item esta en estado de invencibilidad o no
    public float invincibleTime;                        //tiempo paso en invencibilidad el personaje
    public float invincibleTimeLimit;                   //tiempo que se seteo invencibilidad para el personaje
    public float blinkScale = 0.75f;                    //Porcentaje del tiempo limite que tiene que pasar para que empiece a destellar
    private bool isBlinking = false;                    //Bool para saber si esta destellando

    private void Awake() {
        //Se setea el componente animador a la variable de manejo
        m_animator = GetComponent<Animator>();
        //Se inicializa el destino con la posicion del personaje
        destiny = transform.position;
        invinsibleAura = GetComponentInChildren<ParticleSystem>();
    }

    private void Start() {
        SetMoveArea();
        StartCoroutine(TakeABreath(2.0f));  //Al comenzar el personaje queda unos segundos en espera
    }

    private void FixedUpdate() {
        if (GameManager.sharedInstance.gamestate == GameManager.gameStates.Playing) {
            CheckInvincibility();
            UpdateAnimation();          //Actualiza la animacion del personaje segun su direccion
            if (ReachedDestination()) {     //Se verifica si el personaje llego a su ultimo destino
                float timeToRest = Random.Range(1.0f, 3.5f);
                if (!resting) StartCoroutine(TakeABreath(1.5f)); //Si llego y no esta descansando, descansa
                //DecideNextDirection();      //Crear nuevo destino aleatorio del personaje
                //DecideDirectionLenght();    //Se fija que tan lejos va a ir o cuanto esperar
            }
            MoveCharacter();                //Mueve fisicamente al personaje
        }

    }

    //Se fija si el jugador tiene seteada la invencibilidad y por cuanto tiempo
    public void CheckInvincibility() {
        if (invincible) {
            if (invincibleTime<= invincibleTimeLimit) {
                //Si el tiempo activo no llego al limite se suma el delta al activo
                invincibleTime += Time.deltaTime;
            } else {
                //Si se llego al limite se detiene el sistema de particulas
                invincibleTime = 0;
                invinsibleAura.Stop();
                invincible = false;
            }
        }
    }

    public void SetInvinsibility(float time) {
        if (!invincible) {
            invincible = true;
            invincibleTimeLimit = time;
            invinsibleAura.Play();
        }
    }

    //Corrutina de destello
    IEnumerator Blink(float time) {
        //Se setea tiempo limite de destello el tiempo actual mas el tiempo limite de aparicion
        var endTime = Time.time + time;
        //Se setea el bool de que esta destellando para no volver a llamar a la corrutina
        isBlinking = true;
        //Durante el tiempo de destello se realiza
        while (Time.time < endTime) {
            //Se deshabilita y habilita el sprite render por 0.3 segundos en loop
            //m_renderer.enabled = false;
            yield return new WaitForSeconds(0.3f);
            //m_renderer.enabled = true;
            yield return new WaitForSeconds(0.3f);
        }
        isBlinking = false;
    }

    private void SetMoveArea() {
        float spriteBoundsSize = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        moveAreaMaxX = CameraController.sharedInstance.GetMaxX() - spriteBoundsSize;
        moveAreaMinX = CameraController.sharedInstance.GetMinX() + spriteBoundsSize;
        moveAreaMaxY = CameraController.sharedInstance.GetMaxY() - spriteBoundsSize;
        moveAreaMinY = CameraController.sharedInstance.GetMinY() + spriteBoundsSize;
    }

    //Verifica si el personaje llego al destino que tenia fijado
    private bool ReachedDestination() {
        //Como soy tonto y me dio paja pensar si habia una forma mejor de hacerlo quedó asi este if
        //Si la posicion es mayor en el eje en el que se esta moviendo, es que se alcanzo el destino,
        //  y se setea la posicion del destino por si se paso
        if (
            (direction.x < 0 && transform.position.x < destiny.x)
         || (direction.x > 0 && transform.position.x > destiny.x)
         || (direction.y < 0 && transform.position.y < destiny.y)
         || (direction.y > 0 && transform.position.y > destiny.y)
            ) {
            transform.SetPositionAndRotation(destiny, transform.rotation);
            //Debug.Log("reached destiny");
            return true;
        }
        return false;
    }

    //Corrutina para dejar quieto al personaje un tiempo despues de haberse movido
    //  luego de lo cual calcula el proximo destino
    private IEnumerator TakeABreath(float timeToRest) {
        resting = true;             //Seteamos el bool de descansando, para que se quede quieto un momento
        direction = Vector2.zero;   //Ponemos direccion en cero para que la animacion se detenga
        UpdateAnimation();          //Actualiza la animacion del personaje segun su direccion
        yield return new WaitForSeconds(timeToRest);
        resting = false;            //Despues de esperar, se decide el proximo destino
        DecideNextDirection();      //Crear nuevo destino aleatorio del personaje
        DecideDirectionLenght();    //Se fija que tan lejos va a ir o cuanto esperar
    }

    //Genera una accion random para el proximo destino del personaje
    private void DecideNextDirection() {
        //random de un enum
        //Para elegir un valor aleatorio del enum se castea el (Enum)Random, ya que los valores de los enums estan asociados a un entero,
        //  y pueden ser referenciados con el casteo del (Enum)int
        // Se usa el System.Enum.GetValues(typeof(Enum)) para obtener un vector con los valores del enum, usando el length de
        //  ese vector como tope maximo del random
        //currentAction = (Actions)Random.Range(0, System.Enum.GetValues(typeof(Actions)).Length);
        //

        prevAction = currentAction;
        //Se genera un vector con las acciones validas segun la ultima accion y la posicion del personaje
        Actions[] validActions = CheckValidActions();
        //Se elige una accion al azar del vector de acciones validas, para que sea la actual
        currentAction = validActions[Random.Range(0, validActions.Length)];
        //Debug.Log("new action    " + currentAction.ToString());
    }

    //Se genera un vector con las acciones validas
    private Actions[] CheckValidActions() {
        //Primero se genera como lista de Actions ya que no sabemos la longitud que tendra
        List<Actions> validActions = new List<Actions>();
        //Se recorren todas las acciones del enum
        foreach(Actions action in System.Enum.GetValues(typeof(Actions))) {
            //Si la accion recorrida es la previa ya se descarta
            if(action != prevAction) {
                //Si la posicion en el eje correspondiente a la accion recorrida +- 0.5f es >< al limite
                //  de movimiento, se toma como valida esa accion
                //Se suma/resta 0.5f para que el movimiento minimo tenga esa longitud y que no parezca
                //  que el personaje realiza  movimientos feos
                switch (action) {
                    case Actions.Left:
                        if (transform.position.x - 0.5f > moveAreaMinX) validActions.Add(action);
                        break;
                    case Actions.Right:
                        if (transform.position.x + 0.5f < moveAreaMaxX) validActions.Add(action);
                        break;
                    case Actions.Up:
                        if (transform.position.y + 0.5f < moveAreaMaxY) validActions.Add(action);
                        break;
                    case Actions.Down:
                        if (transform.position.y - 0.5f > moveAreaMinX) validActions.Add(action);
                        break;
                    case Actions.Stand:     
                        //En el caso de Stand al no haber movimiento solo se valida que no sea la accion previa
                        validActions.Add(action);
                        break;
                }
            }
        }
        //Se devuelve el listado de Actions transformado en Array
        return validActions.ToArray();
    }

    //Decide hasta donde va a caminar
    private void DecideDirectionLenght() {
        //Para cada accion se genera, en base al eje de la accion, un destino random en ese eje donde
        //  el valor minimo del random sera la posicion en el eje +- 0.5f, y el mayor el limite sobre ese eje
        switch (currentAction) {
            case Actions.Left:
                direction = Vector2.left * 0.5f;
                destiny = new Vector2(Random.Range(transform.position.x - 0.5f, moveAreaMinX), transform.position.y);
                m_animator.Play("LeftAnim");
                break;
            case Actions.Right:
                direction = Vector2.right * 0.5f;
                destiny = new Vector2(Random.Range(transform.position.x + 0.5f, moveAreaMaxX), transform.position.y);
                m_animator.Play("RightAnim");
                break;
            case Actions.Up:
                direction = Vector2.up * 0.5f;
                destiny = new Vector2(transform.position.x, Random.Range(transform.position.y + 0.5f, moveAreaMaxY));
                m_animator.Play("UpAnim");
                break;
            case Actions.Down:
                direction = Vector2.down * 0.5f;
                destiny = new Vector2(transform.position.x, Random.Range(transform.position.y - 0.5f, moveAreaMinY));
                m_animator.Play("DownAnim");
                break;
            case Actions.Stand:
                //En el caso de stand solo se llama a la corrutina de descanso del personaje
                StartCoroutine(TakeABreath(2.0f));
                break;
        }
    }

    private void MoveCharacter() {
        //Se mueve al personaje de forma fluida(Lerp) desde la posicion actual hacia la posicion
        //  mas la direccion(siempre es un medio vector)
        transform.position = Vector2.Lerp(transform.position, (Vector2)transform.position + direction, Time.deltaTime * movementSpeed);
    }

    private void UpdateAnimation() {
        //Se setean los parametros 'x' e 'y' del animator para que se actualice la animacion 
        m_animator.SetInteger("x", Mathf.RoundToInt(direction.normalized.x));
        m_animator.SetInteger("y", Mathf.RoundToInt(direction.normalized.y));
        //Debug.Log("animator (" + m_animator.GetInteger("x") + " , " + m_animator.GetInteger("y") + ")");
    }

    ///---------------- METODOS PUBLICOS ------------------------///

    //Metodo para que se sumen salud al contador
    public int GetHealth() {
        return health;
    }

    //Metodo para que se sumen salud al contador
    public void AddHealth(int number) {
        health += number;
        if (health > 100) health = 100;
        if (health < 0) health = 0;
    }

    //Metodo para setear el contador de salud a un numero especifico
    public void SetHealth(int number) {
        if (number <= 100) {
            health = number;
        } else {
            health = 100;
        }

    }
}
