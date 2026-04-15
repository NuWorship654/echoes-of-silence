// Scripts/UIJuego.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class UIJuego : MonoBehaviour
{
    [Header("HUD principal")]
    [SerializeField] private TextMeshProUGUI textoCiclo;
    [SerializeField] private TextMeshProUGUI textoInteraccion;
    [SerializeField] private TextMeshProUGUI textoArmaEquipada;

    [Header("Inventario recursos")]
    [SerializeField] private GameObject panelInventario;
    [SerializeField] private TextMeshProUGUI textoMadera;
    [SerializeField] private TextMeshProUGUI textoPiedra;
    [SerializeField] private TextMeshProUGUI textoFibra;
    [SerializeField] private TextMeshProUGUI textoAgua;

    [Header("Panel crafting")]
    [SerializeField] private GameObject panelCrafting;
    [SerializeField] private Transform contenedorRecetas;
    [SerializeField] private GameObject prefabBotonReceta;

    private PlayerController jugador;
    private Inventario inventario;
    private InventarioArmas inventarioArmas;

    private InputAction abrirCrafting;
    private InputAction abrirInventario;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            jugador = playerObj.GetComponent<PlayerController>();
            inventario = playerObj.GetComponent<Inventario>();
            inventarioArmas = playerObj.GetComponent<InventarioArmas>();

            if (inventarioArmas == null)
                inventarioArmas = playerObj.AddComponent<InventarioArmas>();
        }

        abrirCrafting = new InputAction("AbrirCrafting", binding: "<Keyboard>/tab");
        abrirInventario = new InputAction("AbrirInventario", binding: "<Keyboard>/i");

        abrirCrafting.performed += _ => ToggleCrafting();
        abrirInventario.performed += _ => ToggleInventario();

        abrirCrafting.Enable();
        abrirInventario.Enable();

        if (panelCrafting != null) panelCrafting.SetActive(false);
        if (panelInventario != null) panelInventario.SetActive(false);
    }

    void Update()
    {
        ActualizarHUD();
        ActualizarInventarioUI();
    }

    void OnDestroy()
    {
        abrirCrafting?.Disable();
        abrirInventario?.Disable();
    }

    void ActualizarHUD()
    {
        if (textoCiclo != null && GameManager.Instance != null)
            textoCiclo.text = GameManager.Instance.GetTiempoFormateado();

        if (textoInteraccion != null && jugador != null)
            textoInteraccion.text = jugador.HayObjetoEnMira() ? jugador.GetTextoInteraccion() : "";

        if (textoArmaEquipada != null && inventarioArmas != null)
        {
            var arma = inventarioArmas.GetArmaEquipada();
            textoArmaEquipada.text = arma.HasValue ? $"Arma: {arma.Value}" : "Sin arma";
        }
    }

    void ActualizarInventarioUI()
    {
        if (inventario == null) return;
        if (textoMadera != null) textoMadera.text = $"Madera: {inventario.GetCantidad(TipoRecurso.Madera)}";
        if (textoPiedra != null) textoPiedra.text = $"Piedra: {inventario.GetCantidad(TipoRecurso.Piedra)}";
        if (textoFibra != null) textoFibra.text = $"Fibra: {inventario.GetCantidad(TipoRecurso.Fibra)}";
        if (textoAgua != null) textoAgua.text = $"Agua: {inventario.GetCantidad(TipoRecurso.Agua)}";
    }

    void ToggleCrafting()
    {
        if (panelCrafting == null) return;
        bool abierto = !panelCrafting.activeSelf;
        panelCrafting.SetActive(abierto);

        Cursor.lockState = abierto ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = abierto;

        if (abierto) RefrescarRecetas();
    }

    void ToggleInventario()
    {
        if (panelInventario != null)
            panelInventario.SetActive(!panelInventario.activeSelf);
    }

    void RefrescarRecetas()
    {
        if (contenedorRecetas == null || prefabBotonReceta == null) return;

        foreach (Transform hijo in contenedorRecetas)
            Destroy(hijo.gameObject);

        if (CraftingSystem.Instance == null || inventario == null) return;

        foreach (var receta in CraftingSystem.Instance.recetas)
        {
            bool disponible = CraftingSystem.Instance.PuedeCraftear(receta, inventario);
            GameObject boton = Instantiate(prefabBotonReceta, contenedorRecetas);

            TextMeshProUGUI textoBoton = boton.GetComponentInChildren<TextMeshProUGUI>();
            if (textoBoton != null)
                textoBoton.text = $"{receta.resultado} x{receta.cantidadResultado}\n{receta.GetDescripcion()}";

            Button btn = boton.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = disponible;
                var recetaLocal = receta;
                btn.onClick.AddListener(() => OnCraftear(recetaLocal));
            }
        }
    }

    void OnCraftear(RecetaCrafting receta)
    {
        if (CraftingSystem.Instance.Craftear(receta, inventario, inventarioArmas))
        {
            RefrescarRecetas();
            ActualizarInventarioUI();
        }
    }
}
