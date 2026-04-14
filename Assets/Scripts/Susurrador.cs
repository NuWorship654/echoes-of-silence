// Scripts/Susurrador.cs
using UnityEngine;
using UnityEngine.AI;

public class Susurrador : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidadPatrulla = 1.5f;
    public float velocidadPersecucion = 4f;
    public float distanciaDeteccion = 10f;
    public float distanciaAtaque = 1.5f;
    public float umbralRuido = 0.2f;

    [Header("Referencias")]
    public Transform jugador;
    public PlayerController playerController;

    private NavMeshAgent agente;
    private Animator animator;

    private enum Estado { Patrullando, Persiguiendo, Atacando }
    private Estado estadoActual = Estado.Patrullando;
    private Vector3 puntoPatrulla;
    private float tiempoEspera = 0f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();

        // Buscar Animator en el objeto o en sus hijos
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (animator != null)
            Debug.Log("[Susurrador] Animator encontrado: " + animator.gameObject.name);
        else
            Debug.LogWarning("[Susurrador] Animator NO encontrado!");

        agente.speed = velocidadPatrulla;

        if (jugador == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                jugador = p.transform;
                playerController = p.GetComponent<PlayerController>();
            }
        }

        BuscarNuevoPuntoPatrulla();
    }

    void Update()
    {
        switch (estadoActual)
        {
            case Estado.Patrullando:
                Patrullar();
                DetectarJugador();
                ActualizarAnimacion(agente.velocity.magnitude, false);
                break;

            case Estado.Persiguiendo:
                Perseguir();
                ActualizarAnimacion(agente.velocity.magnitude, false);
                break;

            case Estado.Atacando:
                Atacar();
                ActualizarAnimacion(0, true);
                break;
        }
    }

    void ActualizarAnimacion(float velocidad, bool atacando)
    {
        if (animator == null) return;
        animator.SetFloat("Velocidad", velocidad);
        animator.SetBool("Atacando", atacando);
        Debug.Log("[Susurrador] Velocidad anim: " + velocidad);
    }

    void Patrullar()
    {
        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            tiempoEspera += Time.deltaTime;
            if (tiempoEspera > 2f)
            {
                BuscarNuevoPuntoPatrulla();
                tiempoEspera = 0f;
            }
        }
    }

    void DetectarJugador()
    {
        if (jugador == null || playerController == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        float nivelRuido = playerController.GetNivelRuido();

        if (distancia < distanciaDeteccion || nivelRuido > umbralRuido)
        {
            estadoActual = Estado.Persiguiendo;
            agente.speed = velocidadPersecucion;
            Debug.Log("[Susurrador] ¡Jugador detectado!");
        }
    }

    void Perseguir()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia < distanciaAtaque)
        {
            estadoActual = Estado.Atacando;
            agente.ResetPath();
        }
        else if (distancia > distanciaDeteccion * 1.5f)
        {
            estadoActual = Estado.Patrullando;
            agente.speed = velocidadPatrulla;
            BuscarNuevoPuntoPatrulla();
        }
        else
        {
            agente.SetDestination(jugador.position);
        }
    }

    void Atacar()
    {
        if (jugador == null) return;
        transform.LookAt(jugador);
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > distanciaAtaque)
            estadoActual = Estado.Persiguiendo;
    }

    void BuscarNuevoPuntoPatrulla()
    {
        Vector3 puntoAleatorio = transform.position + Random.insideUnitSphere * 15f;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(puntoAleatorio, out hit, 15f, NavMesh.AllAreas))
        {
            puntoPatrulla = hit.position;
            agente.SetDestination(puntoPatrulla);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}