using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Instancia compartida para acceder desde otras clases
    public static CameraController sharedInstance;

    //variables que contienen los limites maximos y minimos del juego(ya sean los limites de la camara o paneles de la IU)
    private float minX, maxX, minY, maxY;
    //Vector que contiene el centro del area de juego
    private Vector2 gameAreaCenter;

    //Canvas del juego, es lo que controla toda la IU
    public Canvas gameCanvas;
    //Rect del panel de juegos, donde se muestran puntajes, boton de menu, etc.
    public RectTransform gamePanel;

    private DeviceOrientation previousDeviceOrientation;
    private float previousScreenWidth;
    private float previousScreenHeight;

    private void Awake() {
        //Se setea la instancia compartida
        sharedInstance = this;
        //Se setean los limites de la pantalla al iniciar el juego 
        UpdateBounds();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBounds();     //Actualizar limites de juego ante un cambio de resolucion o de orientacion de pantalla
    }

    void UpdateBounds() {
        if (CheckScreenChange()) {  //solo si cambió la orientacion o la resolucion actualiza limites de juego
            //Los limites de ancho se establecen a partir de la posicion en x de la camara(Camera.main.transform.position.x)
            //  a lo cual se le suma/resta el (Camera.main.aspect * Camera.main.orthographicSize)
            //Camera.main.orthographicSize es igual a la mitad de la altura de la camara
            //Camera.main.aspect es el ancho dividido por la altura de la pantalla, ej. cualquier pantalla que sea 16:9 seria 1.78
            //Por eso al multiplicar el orthographicSize * aspect obtenemos la mitad del ancho, lo cual sumado/restado a la posicion
            //  de la camara resulta en los limites maximo y minimos donde la camara llega a mostrar
            minX = (Camera.main.transform.position.x - (Camera.main.aspect * Camera.main.orthographicSize));
            maxX = (Camera.main.transform.position.x + (Camera.main.aspect * Camera.main.orthographicSize));

            //Los limites de altura tienen que ver con lo mismo, parten desde la posicion en y de la camara
            //La diferencia es que aca no hay que multiplicar por el aspect ya que el orthographicSize ya es la mitad de la altura
            minY = (Camera.main.transform.position.y - Camera.main.orthographicSize);
            
            //Este maxY se usaria si no tuvieramos en cuenta el panel superior
            //maxY = (Camera.main.transform.position.y + Camera.main.orthographicSize);

            //La otra diferencia es que en el alto tenemos el limite del panel de juego
            //En primer lugar tenemos el sizeDelta del panel, el cual corresponde al tamaño "predeterminado" del elemento
            // pero al ser un canvas que se ajusta al tamaño de la pantalla a este sizeDelta hay que multiplicarlo por el
            // scaleFactor del canvas, que es basicamente el ratio de escala actual de los elementos del canvas segun la pantalla
            //El problema es que el tamaño que obtenemos es en la medida de la pantalla, la cual tiene el 0,0 abajo a la izquierda,
            //  y cuya medida es en pixeles como la pantalla, por eso a la altura de la pantalla le restamos el tamaño en y del panel
            //Esa resta la metemos en un vector para poder usar el metodo ScreenToWOrldPoint, que basicamente traduce la posicion de
            //  un punto de la pantalla a la unidad de medida del juego
            //Luego de ese metodo solo tenemos que tomar el valor de y
            maxY = Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height - (gamePanel.sizeDelta * gameCanvas.scaleFactor).y)).y;

            gameAreaCenter = new Vector2((maxX + minX) / 2, (maxY + minY) / 2);
            //Se llama al metodo para centrar el respawn area en el medio del area de juego
            //GameItemSpawner.sharedInstance.SetRespawnAreaPosition(new Vector2((maxX + minX) / 2, (maxY + minY) / 2));
        }

        /*
        Muchos comentarios y debugs que me ayudaron a entender como calcular los limites de la camara
         y las posiciones de la IU para tambien usar como limite:
        
        The aspect ratio (width divided by height).
            aspect = width / height
            aspect * height = width
        
        Debug.Log("camera aspect " + Camera.main.aspect);
        Debug.Log("camera size " + Camera.main.orthographicSize.ToString());
        Debug.Log("camera height " + (Camera.main.orthographicSize * 2));
        Debug.Log("camera width " + (Camera.main.aspect * Camera.main.orthographicSize * 2));
        Debug.Log("camera 1/2 height " + Camera.main.orthographicSize);
        Debug.Log("camera 1/2 width " + (Camera.main.aspect * Camera.main.orthographicSize));
        Debug.Log("camera position " + Camera.main.transform.position);
        Debug.Log("gamePanel position " + gamePanel.transform.position);
        Debug.Log("gamePanel position STWP " + Camera.main.ScreenToWorldPoint(gamePanel.transform.position));
        Debug.Log("gamePanel position WTSP " + Camera.main.WorldToScreenPoint(gamePanel.transform.position));
        Debug.Log("screen height " + Screen.height);
        Debug.Log("screen scalefactor " + gameCanvas.scaleFactor);
        Debug.Log("gamePanel sizeDelta * scale " + gamePanel.sizeDelta * gameCanvas.scaleFactor);
        Debug.Log("gamePanel.sizeDelta * scale STWP " + Camera.main.ScreenToWorldPoint(gamePanel.sizeDelta * gameCanvas.scaleFactor));
        Debug.Log("screen height - (gamePanel sizeDelta * scale).y " + (Screen.height - (gamePanel.sizeDelta * gameCanvas.scaleFactor).y));
        Debug.Log("screen height - (gamePanel sizeDelta * scale).y STWP " + Camera.main.ScreenToWorldPoint(new Vector2(0,Screen.height - (gamePanel.sizeDelta * gameCanvas.scaleFactor).y)));
        
        Debug.Log("GameArea min x " + minX);
        Debug.Log("GameArea max x " + maxX);
        Debug.Log("GameArea min y " + minY);
        Debug.Log("GameArea max y " + maxY);
        Debug.Log("GameArea center (" + (maxX + minX)/2 + " , " + (maxY + minY)/2 + ")");
        */

    }

    //Se verifica si hubo un cambio de resolucion o de orientacion de la pantalla
    private bool CheckScreenChange() {
        //Si hubo un cambio en la resolucion o en la orientacion de la pantalla respecto a la anterior verificacion se devuelve true
        if (previousDeviceOrientation != Input.deviceOrientation || previousScreenHeight != Screen.height || previousScreenWidth != Screen.width) {
            previousDeviceOrientation = Input.deviceOrientation;
            previousScreenHeight = Screen.height;
            previousScreenWidth = Screen.width;
            return true;
        }
        return false;
    }

    //Metodo para ver si los bounds informados se encuentran fuera de los limites de la camara en algun sentido
    public Vector2 IsOutCameraBounds(Bounds bounds) {
        /*
        Debug.Log("sprite center (" + bounds.center.x + " , " + bounds.center.y + ")");
        Debug.Log("sprite size (" + bounds.size.x + " , " + bounds.size.y + ")");
        Debug.Log("sprite extents (" + bounds.extents.x + " , " + bounds.extents.y + ")");
        Debug.Log("sprite max (" + bounds.max.x + " , " + bounds.max.y + ")");
        Debug.Log("sprite min (" + bounds.min.x + " , " + bounds.min.y + ")");
        */

        //Se crea en ceros el vector a devolver
        Vector2 boundDifference = Vector2.zero;

        //Si los bordes informados se pasan de algun limite se guarda el valor sobrepasado
        if (bounds.max.x < minX) {
            boundDifference.x = bounds.max.x - minX;
        }
        if (bounds.min.x > maxX) {
            boundDifference.x = bounds.min.x - maxX;
        }
        if (bounds.max.y < minY) {
            boundDifference.y = bounds.max.y - minY;
        }
        if (bounds.min.y > maxY) {
            boundDifference.y = bounds.min.y - maxY;
        }
        //Se devuelve un vector con la diferencia del sobrepaso de un borde para poder utilizar ese dato donde se llamo al metodo
        return boundDifference;
    }

    //Metodo para ver si los bounds informados se encuentran dentro de los limites de la camara
    public bool IsInCameraBounds(Bounds bounds) {
        //Si alguno de los bordes sobrepasa los limites de la pantalla se devuelve true
        if (bounds.min.x < minX && bounds.max.x < maxX && bounds.min.y > minY && bounds.max.y < maxY) {
            return true;
        } else {
            return false;
        }
    }

    //Metodo para ver si la posicion informada se encuentra dentro de los limites de la camara
    public bool IsInCameraBounds(Vector2 position) {
        if (position.x > minX && position.x < maxX && position.y > minY && position.y < maxY) {
            return true;
        } else {
            return false;
        }
    }

    public float GetMaxX() {
        return maxX;
    }

    public float GetMaxY() {
        return maxY;
    }

    public float GetMinX() {
        return minX;
    }

    public float GetMinY() {
        return minY;
    }

    public Vector2 GetCenter() {
        return gameAreaCenter;
    }

}
