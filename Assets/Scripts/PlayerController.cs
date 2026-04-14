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

    // Referencias privadas
    private CharacterController controller;
    private Camera cam;
    private float rotacionX = 0f;
    private float bateria;
    private bool linternaEncendida = true;
    private bool estaMoviendo = false;

    // Input System
    private PlayerInput playerInput;
    private InputAction moverAction;
    private InputAction mirarAction;
    private InputAction correrAction;
    private InputAction linternaAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        bateria = duracionBateria;

        // Configurar acciones de input manualmente
        moverAction = new InputAction("Mover", binding: "<Gamepad>/leftStick");
        moverAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        mirarAction = new InputAction("Mirar", binding: "<Mouse>/delta");
        correrAction = new InputAction("Correr", binding: "<Keyboard>/leftShift");
        linternaAction = new InputAction("Linterna", binding: "<Keyboard>/f");

        moverAction.Enable();
        mirarAction.Enable();
        correrAction.Enable();
        linternaAction.Enable();

        linternaAction.performed += _ => ToggleLinterna();

        // Bloquear cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ManejarMovimiento();
        ManejarCamera();
        ManejarLinterna();
        ActualizarRuido();
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

    void OnDestroy()
    {
        moverAction.Disable();
        mirarAction.Disable();
        correrAction.Disable();
        linternaAction.Disable();
    }

    public float GetNivelRuido() => nivelRuido;
    public float GetBateriaNormalizada() => bateria / duracionBateria;
}