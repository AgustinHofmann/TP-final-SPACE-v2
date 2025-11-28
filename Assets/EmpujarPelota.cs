using UnityEngine;

public class EmpujarPelota : MonoBehaviour
{
    public float fuerzaEmpuje = 10f;
    public float distanciaRayo = 50f;
    public float duracionRayo = 0.5f;
    public float friccion = 0.95f;
    public float alturaMinima = -10f; // Si caen más abajo de esto, respawnean

    private LineRenderer lineRenderer;
    private Camera cam;
    private CharacterController characterController;
    private Rigidbody playerRb;

    private GameObject rayoVisual;

    // Posiciones iniciales
    private Vector3 posicionInicialJugador;
    private Vector3 posicionInicialPelota;
    private GameObject pelota;
    private GameObject jugador;

    void Start()
    {
        // Obtener la cámara
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = GetComponentInChildren<Camera>();
        }
        if (cam == null)
        {
            cam = Camera.main;
        }

        // Buscar el CharacterController o Rigidbody del jugador
        characterController = GetComponentInParent<CharacterController>();
        if (characterController == null)
        {
            playerRb = GetComponentInParent<Rigidbody>();
        }

        // Guardar jugador y su posición inicial
        if (characterController != null)
        {
            jugador = characterController.gameObject;
        }
        else if (playerRb != null)
        {
            jugador = playerRb.gameObject;
        }
        else
        {
            jugador = transform.root.gameObject; // El padre principal
        }

        posicionInicialJugador = jugador.transform.position;

        // Buscar pelota y guardar posición inicial
        pelota = GameObject.FindGameObjectWithTag("Pelota");
        if (pelota != null)
        {
            posicionInicialPelota = pelota.transform.position;
        }

        // Crear LineRenderer
        lineRenderer = cam.gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        // Crear cilindro visual alternativo
        rayoVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rayoVisual.name = "RayoVisual";
        Destroy(rayoVisual.GetComponent<Collider>());

        Renderer rend = rayoVisual.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        rend.material.color = Color.red;

        rayoVisual.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EstaQuieto())
            {
                DispararRayo();
            }
            else
            {
                Debug.Log("¡Debes estar quieto para disparar!");
            }
        }

        // Verificar si cayeron al vacío
        VerificarCaidas();
    }

    void LateUpdate()
    {
        // Frenar la pelota gradualmente
        if (pelota != null)
        {
            Rigidbody rb = pelota.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = rb.velocity * friccion;
                rb.angularVelocity = rb.angularVelocity * friccion;

                if (rb.velocity.magnitude < 0.1f)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    void VerificarCaidas()
    {
        // Verificar jugador
        if (jugador != null && jugador.transform.position.y < alturaMinima)
        {
            Debug.Log("¡Jugador cayó! Respawneando...");
            RespawnearJugador();
        }

        // Verificar pelota
        if (pelota != null && pelota.transform.position.y < alturaMinima)
        {
            Debug.Log("¡Pelota cayó! Respawneando...");
            RespawnearPelota();
        }
    }

    void RespawnearJugador()
    {
        if (characterController != null)
        {
            characterController.enabled = false;
            jugador.transform.position = posicionInicialJugador;
            characterController.enabled = true;
        }
        else
        {
            jugador.transform.position = posicionInicialJugador;

            if (playerRb != null)
            {
                playerRb.velocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
        }
    }

    void RespawnearPelota()
    {
        pelota.transform.position = posicionInicialPelota;

        Rigidbody rb = pelota.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    bool EstaQuieto()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            return false;
        }

        if (characterController != null)
        {
            if (characterController.velocity.magnitude > 0.1f)
            {
                return false;
            }
        }

        if (playerRb != null)
        {
            if (playerRb.velocity.magnitude > 0.1f)
            {
                return false;
            }
        }

        return true;
    }

    void DispararRayo()
    {
        Vector3 origen = cam.transform.position;
        Vector3 direccion = cam.transform.forward;

        Ray rayo = new Ray(origen, direccion);
        RaycastHit golpe;

        Vector3 puntoFinal;

        if (Physics.Raycast(rayo, out golpe, distanciaRayo))
        {
            Debug.Log("El rayo golpeó: " + golpe.collider.name);
            puntoFinal = golpe.point;

            if (golpe.collider.CompareTag("Pelota"))
            {
                Debug.Log("¡Golpeó la pelota!");

                Rigidbody rb = golpe.collider.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddForce(direccion * fuerzaEmpuje, ForceMode.Impulse);
                }
            }
        }
        else
        {
            Debug.Log("El rayo no golpeó nada");
            puntoFinal = origen + direccion * distanciaRayo;
        }

        MostrarRayo(origen, puntoFinal);
    }

    void MostrarRayo(Vector3 inicio, Vector3 fin)
    {
        CancelInvoke("OcultarRayo");
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, inicio);
        lineRenderer.SetPosition(1, fin);

        Vector3 puntoMedio = (inicio + fin) / 2f;
        float distancia = Vector3.Distance(inicio, fin);

        rayoVisual.transform.position = puntoMedio;
        rayoVisual.transform.rotation = Quaternion.FromToRotation(Vector3.up, fin - inicio);
        rayoVisual.transform.localScale = new Vector3(0.02f, distancia / 2f, 0.02f);
        rayoVisual.SetActive(true);

        Invoke("OcultarRayo", duracionRayo);
    }

    void OcultarRayo()
    {
        lineRenderer.enabled = false;
        rayoVisual.SetActive(false);
    }
}