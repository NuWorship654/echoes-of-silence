using UnityEngine;

public class SeccionLaberinto : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int numeroSeccion;
    [SerializeField] private GameObject puertaBloqueo; // pared/puerta que bloquea la entrada
    [SerializeField] private GameObject[] spawnersCreaturas;

    public bool EstaAbierta { get; private set; } = false;

    void Start()
    {
        Bloquear();
    }

    public void Abrir()
    {
        EstaAbierta = true;
        if (puertaBloqueo != null)
            puertaBloqueo.SetActive(false);

        foreach (var spawner in spawnersCreaturas)
            spawner?.SetActive(true);

        Debug.Log($"[Sección {numeroSeccion}] Abierta");
    }

    public void Bloquear()
    {
        EstaAbierta = false;
        if (puertaBloqueo != null)
            puertaBloqueo.SetActive(true);

        foreach (var spawner in spawnersCreaturas)
            spawner?.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!EstaAbierta && other.CompareTag("Player"))
        {
            // Empujar al jugador de vuelta — sección bloqueada
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direccionAfuera = (other.transform.position - transform.position).normalized;
                rb.AddForce(direccionAfuera * 8f, ForceMode.Impulse);
            }
        }
    }
}
