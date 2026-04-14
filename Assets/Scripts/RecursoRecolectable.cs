// Scripts/RecursoRecolectable.cs
using UnityEngine;
using System.Collections;

public enum TipoRecurso { Madera, Piedra, Fibra, Agua }

public class RecursoRecolectable : MonoBehaviour, IInteractuable
{
    [Header("Configuración")]
    public TipoRecurso tipo;
    public int cantidadMinima = 1;
    public int cantidadMaxima = 3;
    public float tiempoRecoleccion = 2f;
    public float tiempoRespawn = 60f;

    [Header("Golpes para recolectar")]
    public int golpesNecesarios = 3;

    private int golpesActuales = 0;
    private bool estaDisponible = true;
    private bool recolectando = false;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    public void Interactuar(PlayerController jugador)
    {
        if (!estaDisponible || recolectando) return;
        StartCoroutine(Recolectar(jugador));
    }

    public string GetTextoInteraccion()
    {
        if (!estaDisponible) return "";
        return $"[E] Recolectar {tipo}";
    }

    IEnumerator Recolectar(PlayerController jugador)
    {
        recolectando = true;
        golpesActuales++;

        // Animación de golpe — escala brevemente
        transform.localScale *= 1.1f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale /= 1.1f;

        if (golpesActuales >= golpesNecesarios)
        {
            int cantidad = Random.Range(cantidadMinima, cantidadMaxima + 1);
            jugador.GetInventario().AgregarRecurso(tipo, cantidad);
            Debug.Log($"[Recurso] +{cantidad} {tipo}");

            estaDisponible = false;
            golpesActuales = 0;

            // Desactivar visualmente
            if (rend != null) rend.enabled = false;
            GetComponent<Collider>().enabled = false;

            StartCoroutine(Respawn());
        }

        recolectando = false;
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(tiempoRespawn);
        estaDisponible = true;
        if (rend != null) rend.enabled = true;
        GetComponent<Collider>().enabled = true;
        Debug.Log($"[Recurso] {tipo} respawneado");
    }
}
