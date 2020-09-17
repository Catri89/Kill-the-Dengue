using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : WorldObject
{
    public int enemyLevel;                  
    public int health = 1;                  //Salud del enemigo(cant de clics para matarlo)
    public float baseSpeed = 2.0f;          //Velocidad de movimiento del enemigo
    public float damageInvincibleTime = 1.0f;   //Tiempo de invencibilidad despues de recibir daño
    public float movementSpeed = 5.0f;      //Velocidad de movimiento del enemigo
    public float baseRotation = 5.0f;       //Velocidad de movimiento del enemigo
    public float rotation = 5.0f;           //Velocidad de movimiento del enemigo
    public int baseDamage;                  //Es la salud que va a sumar/restar el objeto
    public int damage;                      //Es la salud que va a sumar/restar el objeto

    //A este vector siempre se le setea el transform.right porque siempre se mueve en esa direccion
    //Lo que cambia es que cada asteroide tiene una rotacion random, por eso se mueven diferente
    private Vector2 direction;                  //Direccion del enemigo

    private SpriteRenderer m_sprite;            //Sprite actual del enemigo

    public override void Awake() {
        base.Awake();
        m_sprite = GetComponent<SpriteRenderer>();
        SetColor();
        UpdateSoundLevel();
    }

    public override void Start()
    {
        base.Start();
        //Se activa el tiempo de invencibilidad del asteroide por el tiempo de la variable
        StartCoroutine(InvincibleSpawn(invincibleTime));
    }

    private void FixedUpdate() {
        //Se setea el vector de movimiento con el transform.right que es la "derecha" del asteroide
        direction = transform.right;
        float randomRotation = Random.Range(-rotation, rotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + randomRotation), 10.0f * Time.deltaTime);
        MoveEnemy();
    }

    //toma el volumen del soundmanager para aplicarlo al zumbido del enemigo
    public void UpdateSoundLevel() {
        this.GetComponent<AudioSource>().volume = SoundManager.audioSource.volume;
    }

    //Cambia el nivel de dificultad del enemigo
    public void SetEnemyLevel(int newLevel) {
        health = newLevel;
        SetColor();
    }

    //Cambia el color del enemigo dependiendo de la salud del mismo
    private void SetColor() {
        Color32 enemyColor = m_sprite.color;
        switch (health) {
            case 1:
                enemyColor = new Color32(255, 255, 255, 255);
                break;
            case 2:
                enemyColor = new Color32(255, 200, 0, 255);
                break;
            case 3:
                enemyColor = new Color32(255, 100, 0, 255);
                break;
            case 4:
                enemyColor = new Color32(255, 0, 0, 255);
                break;
        }
        m_sprite.color = enemyColor;
    }

    public override void ObjectTouched() {
        if (!invincible) {
            base.ObjectTouched();
            ReduceEnemyHealth();
        }
    }

    private void ReduceEnemyHealth() {
        health--;
        SetColor();
        if(health <= 0) {
            GameItemSpawner.sharedInstance.DestroyGameItem(this);       //Llama al metodo para destruir objetos
        } else {
            StartCoroutine(InvincibleSpawn(damageInvincibleTime));
        }
    }

    //Recalculo de parametros cuando el nivel cambia
    public override void StageChange(int newStage) {
        base.StageChange(newStage);
        movementSpeed = baseSpeed + baseSpeed * newStage / 20;
        damage = baseDamage + baseDamage * newStage / 30;
        rotation = baseRotation + baseRotation * newStage /20;
    }

    //Evento de choque de colliders
    private void OnTriggerEnter2D(Collider2D collision) {
        //Si el enemigo no esta en estado de invencibilidad, colisiono contra otro enemigo y aparte
        //   esta seteada en true el bool para destruccion entre enemigos, éste se destruye
        if (!invincible && collision.CompareTag("Enemy") && GameManager.sharedInstance.asteroidFriendlyFire) {
            GameItemSpawner.sharedInstance.DestroyGameItem(this);
        }
        if (collision.CompareTag("Player")) {
            if (!collision.GetComponent<Character>().invincible) {
                Destroy(this.gameObject);
                SoundManager.PlaySound(character.GetComponent<AudioSource>().clip);
                //character.GetComponent<AudioSource>().Play();
                character.AddHealth(-damage);
            }
        }
    }

    //Con esta corrutina se espera el tiempo seteado y despues se pone en false el bool de invencibilidad
    private IEnumerator InvincibleSpawn(float invincibleTime) {
        invincible = true;
        yield return new WaitForSeconds(invincibleTime);
        invincible = false;
    }

    private void MoveEnemy() {
        //Se averigua si los bordes del asteroide estan fuera del area de juego
        Vector2 boundOut = CameraController.sharedInstance.IsOutCameraBounds(this.m_sprite.bounds);
        //Si boundOut resulto en un vector zero esta dentro de la zona de juego, sino se chequea la posicion y
        //  direccion para mandarlo al sector opuesto asi vuelve a la zona de juego
        if (boundOut != Vector2.zero) {
            MoveOpposite(boundOut);
        }

        //Se mueve el asteroide en su direccion y segun la velocidad
        transform.position = Vector2.Lerp(transform.position, ((Vector2)transform.position + direction), Time.deltaTime * movementSpeed);
    }

    //Se verifica el limite de la zona de juego superado y la direccion, en caso de que corresponda se mueve el asteroide al sector opuesto de la zona de juego
    private void MoveOpposite(Vector2 boundOut) {
        //Si se paso a la derecha de la zona de juego y su movimiento horizontal tiene sentido hacia la derecha, se mueve al opuesto
        if ((boundOut.x > 0 && direction.x > 0)) {
            this.transform.SetPositionAndRotation(new Vector2(CameraController.sharedInstance.GetMinX() - this.m_sprite.bounds.extents.x, transform.position.y), transform.rotation);
        }
        //Si se paso a la izquierda de la zona de juego y su movimiento horizontal tiene sentido hacia la izquierda, se mueve al opuesto
        if ((boundOut.x < 0 && direction.x < 0)) {
            this.transform.SetPositionAndRotation(new Vector2(CameraController.sharedInstance.GetMaxX() + this.m_sprite.bounds.extents.x, transform.position.y), transform.rotation);
        }
        //Si se paso por arriba de la zona de juego y su movimiento vertical tiene sentido hacia arriba, se mueve al opuesto
        if ((boundOut.y > 0 && direction.y > 0)) {
            this.transform.SetPositionAndRotation(new Vector2(transform.position.x, CameraController.sharedInstance.GetMinY() - this.m_sprite.bounds.extents.y), transform.rotation);
        }
        //Si se paso por abajo de la zona de juego y su movimiento vertical tiene sentido hacia abajo, se mueve al opuesto
        if ((boundOut.y < 0 && direction.y < 0)) {
            this.transform.SetPositionAndRotation(new Vector2(transform.position.x, CameraController.sharedInstance.GetMaxY() + this.m_sprite.bounds.extents.y), transform.rotation);
        }
    }


}
