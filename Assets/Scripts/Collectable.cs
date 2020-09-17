using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : WorldObject
{
    private SpriteRenderer m_renderer;
    float activeTime = 0.0f;            //Tiempo que el item estuvo en pantalla
    public int healthRecover;           //Es la salud que va a recuperar el collectable
    public float invincibilityTime;     //Es el tiempo de invencibilidad que le otorga al personaje
    public float baseTimeLimit = 10.0f; //Tiempo base en que el item queda en pantalla para ser atrapado
    public float timeLimit = 10.0f;     //Tiempo en que el item queda en pantalla para ser atrapado
    private float minTimeLimit = 2.5f;  //Tiempo minimo en que el item queda en pantalla para ser atrapado
    public float blinkScale = 0.75f;    //Porcentaje del tiempo limite que tiene que pasar para que empiece a destellar
    private bool isBlinking = false;    //Bool para saber si esta destellando

    public override void Awake() {
        base.Awake();
        //Se setea el sprite renderer para el efecto de destello
        m_renderer = GetComponent<SpriteRenderer>();
    }

    public override void Start() {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //CheckTouch();
        if (activeTime <= timeLimit) {
            //Si el tiempo activo no llego al limite se suma el delta al activo
            activeTime += Time.deltaTime;
        } else {
            //Si se llego al limite se destruye el item
            Destroy(this.gameObject);
        }
        //Si el tiempo activo llego al porcentaje de tiempo limite requerido para destellar, 
        //  y no esta destellando actualmente
        if (activeTime >= timeLimit * blinkScale && !isBlinking) {
            //Se llama a la corrutina de destello
            StartCoroutine(Blink());
        }
    }

    //Accion para cuando el objeto es tocado
    public override void ObjectTouched() {
        base.ObjectTouched();
        character.SetInvinsibility(invincibilityTime);  //Setea la invencibilidad del jugador por un tiempo
        character.AddHealth(healthRecover);             //Se suma la salud correspondiente por el objeto
        Destroy(this.gameObject);                       //Destruye el objeto
    }

    //Recalculo de parametros cuando el nivel cambia
    public override void StageChange(int newStage) {
        base.StageChange(newStage);
        timeLimit = baseTimeLimit - newStage/10;
        if (timeLimit < minTimeLimit) timeLimit = minTimeLimit;

    }

    //Corrutina de destello
    IEnumerator Blink() {
        //Se setea tiempo limite de destello el tiempo actual mas el tiempo limite de aparicion
        var endTime = Time.time + timeLimit;
        //Se setea el bool de que esta destellando para no volver a llamar a la corrutina
        isBlinking = true;
        //Durante el tiempo de destello se realiza
        while (Time.time < endTime) {
            //Se deshabilita y habilita el sprite render por 0.3 segundos en loop
            m_renderer.enabled = false;
            yield return new WaitForSeconds(0.3f);
            m_renderer.enabled = true;
            yield return new WaitForSeconds(0.3f);
        }
        isBlinking = false;
    }
}
