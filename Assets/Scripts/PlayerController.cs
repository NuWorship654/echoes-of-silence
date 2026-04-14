// Scripts/PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 3f;
    public float velocidadCarrera = 5f;

    [Header("Cámara")]
    public float sensibilidadMouse = 2f;
    public float limiteVertical = 80f;

    [Header("Sistema de Ruido")]
    public float ruidoReposo = 0f;
    public float ruidoCaminar = 0.3f;
    public float ruidoCorrer = 0.8f;
    public float nivelRuido = 0f;

    [Header("Linterna")]
    public Light linterna;
    public float duracionBateria = 120f;

    [Header("Interacción")]
    public float distanciaInteraccion = 3f;
    public LayerMask capaInteraccion;

    private CharacterController controller;
    private Camera cam;
    private float rotacionX = 0f;
    private float bateria;
    private bool linternaEncendida = true;
    private bool estaMoviendo = false;

    private PlayerInput playerInput;
    private InputAction moverAction;
    private InputAction mirarAction;
    private InputAction correrAction;
    private InputAction linternaAction;
    private InputAction interactuarAction;

    private Inventario inventario;
    private IInteractuable objetoEnMira;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        inventario = GetComponent<Inventario>();
        if (inventario == null)
            inventario = gameObject.AddComponent<Inventario>();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        bateria = duracionBateria;

        moverAction = new InputAction("Mover", binding: "<Gamepad>/leftStick");
        moverAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        mirarAction = new InputAction("Mirar", binding: "<Mouse>/delta");
        correrAction = new InputAction("Correr", binding: "<Keyboard>/leftShift");
        linternaAction = new InputAction("Linterna", binding: "<Keyboard>/f");
        interactuarAction = new InputAction("Interactuar", binding: "<Keyboard>/e");

        moverAction.Enable();
        mirarAction.Enable();
        correrAction.Enable();
        linternaAction.Enable();
        interactuarAction.Enable();

        linternaAction.performed += _ => ToggleLinterna();
        interactuarAction.performed += _ => Interactuar();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ManejarMovimiento();
        ManejarCamera();
        ManejarLinterna();
        ActualizarRuido();
        DetectarObjetoEnMira();
    }

    void ManejarMovimiento()
    {
        Vector2 input = moverAction.ReadValue<Vector2>();
        bool corriendo = correrAction.IsPressed();
        float velocidadActualVal = corriendo ? velocidadCarrera : velocidad;

        Vector3 movimiento = transform.right * input.x + transform.forward * input.y;
        controller.Move(movimiento * velocidadActualVal * Time.deltaTime);

        if (!controller.isGrounded)
            controller.Move(Vector3.down * 9.8f * Time.deltaTime);

        estaMoviendo = movimiento.magnitude > 0.1f;
    }

    void ManejarCamera()
    {
        Vector2 delta = mirarAction.ReadValue<Vector2>();

        rotacionX -= delta.y * sensibilidadMouse * Time.deltaTime * 30f;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);

        cam.transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * delta.x * sensibilidadMouse * Time.deltaTime * 30f);
    }

    void ToggleLinterna()
    {
        if (linterna == null) return;
        linternaEncendida = !linternaEncendida;
        linterna.enabled = linternaEncendida;
    }

    void ManejarLinterna()
    {
        if (linternaEncendida && linterna != null)
        {
            bateria -= Time.deltaTime;
            if (bateria <= 0)
            {
                bateria = 0;
                linternaEncendida = false;
                linterna.enabled = false;
            }
        }
    }

    void ActualizarRuido()
    {
        bool corriendo = correrAction.IsPressed();

        if (!estaMoviendo)
            nivelRuido = ruidoReposo;
        else if (corriendo)
            nivelRuido = ruidoCorrer;
        else
            nivelRuido = ruidoCaminar;
    }

    void DetectarObjetoEnMira()
    {
        Ray rayo = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(rayo, out hit, distanciaInteraccion, capaInteraccion))
        {
            IInteractuable interactuable = hit.collider.GetComponent<IInteractuable>();
            if (interactuable != null)
            {
                objetoEnMira = interactuable;
                return;
            }
        }

        objetoEnMira = null;
    }

    void Interactuar()
    {
        if (objetoEnMira != null)
            objetoEnMira.Interactuar(this);
    }

    void OnDestroy()
    {
        moverAction.Disable();
        mirarAction.Disable();
        correrAction.Disable();
        linternaAction.Disable();
        interactuarAction.Disable();
    }

    public float GetNivelRuido() => nivelRuido;
    public float GetBateriaNormalizada() => bateria / duracionBateria;
    public Inventario GetInventario() => inventario;
    public string GetTextoInteraccion() => objetoEnMira?.GetTextoInteraccion() ?? "";
    public bool HayObjetoEnMira() => objetoEnMira != null;
}
