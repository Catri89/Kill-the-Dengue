using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    //Componentes del objeto
    private Collider2D m_collider;       //El collider del enemigo
    
    //Parametros del gameItem
    public float invincibleTime = 2.0f;         //Tiempo de invencibilidad para que no se destruya apenas creado
    public bool invincible = true;              //Booleano para saber si el item esta en estado de invencibilidad o no
    public int gameitemBasePointsValue;         //Puntaje que va a dar al jugador este item
    private int gameitemPointsValue;            //Puntaje que va a dar al jugador este item

    public Character character;                 //jugador

    public virtual void Awake() {
        //Se setea el collider del enemigo a su variable de manejo
        m_collider = GetComponent<Collider2D>();
        character = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();
    }

    public virtual void Start() {
        StageChange(GameManager.sharedInstance.GetStage());
    }

    //Se chequea si el toque en la pantalla fue sobre este asteroide
    //Por ahora el llamado a este metodo esta comentado ya que funcionaria el metodo OnMouseDown con los toques
    /*
    public virtual void CheckTouch() {
        if (Input.touchCount > 0) {
            foreach (Touch touch in Input.touches) {
                if (touch.phase == TouchPhase.Began) {
                    Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    if (m_collider.bounds.Contains(touchPosition)) {
                        ObjectTouched();
                    }
                }
            }
        }
    }*/

    //Metodo para que cada heredero de worldobject haga diferentes cosas
    public virtual void ObjectTouched() {
        GameManager.sharedInstance.AddPoints(gameitemPointsValue);  //Se suman los puntos correspondientes al objeto
    }

    public virtual void StageChange(int newStage) {
        gameitemPointsValue = gameitemBasePointsValue * newStage;
        //Aca cada gameitem autoajusta sus parametros segun el nivel que cambio
    }

    public void SetPointsValue(int newPointValue) {
        gameitemPointsValue = newPointValue;
    }

    /*
    //Se setea la velocidad del asteroide segun su tamaño
    private void UpdateSpeed() {
        //Este calculo se hace asi para que a menor tamaño del asteroide sea mas rapido
        movementSpeed = (3 - (int)itemSize) * GameItemManager.sharedInstance.baseSpeed;
    }
    */
}
