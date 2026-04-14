using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Ciclo día/noche")]
    [SerializeField] private float duracionDia = 1800f;   // 30 min
    [SerializeField] private float duracionNoche = 1800f; // 30 min
    [SerializeField] private Light luzDireccional;
    [SerializeField] private Gradient colorLuzDia;
    [SerializeField] private AnimationCurve intensidadLuz;

    [Header("Secciones del laberinto")]
    [SerializeField] private SeccionLaberinto[] secciones; // 10 secciones

    [Header("Puertas")]
    [SerializeField] private Puerta puertaNorte;
    [SerializeField] private Puerta puertaSur;
    [SerializeField] private Puerta puertaEste;
    [SerializeField] private Puerta puertaOeste;

    [Header("Eventos de criaturas nocturnas")]
    [SerializeField] private float probabilidadEventoNoche = 0.20f; // 20%
    [SerializeField] private float intervaloChequeoEvento = 120f;   // cada 2 min
    [SerializeField] private GameObject[] prefabsCriaturas;
    [SerializeField] private Transform[] puntosSpawnCentro;

    [Header("Eventos públicos")]
    public UnityEvent onAmanecer;
    public UnityEvent onAnochecer;
    public UnityEvent<int> onNuevoDia;

    public bool EsDia { get; private set; } = true;
    public int DiaActual { get; private set; } = 1;
    public float TiempoRestante { get; private set; }

    private float timerCiclo;
    private float timerEvento;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        timerCiclo = duracionDia;
        TiempoRestante = duracionDia;
        AbrirSeccion(DiaActual - 1);
        AbrirTodasLasPuertas();
    }

    void Update()
    {
        timerCiclo -= Time.deltaTime;
        TiempoRestante = timerCiclo;

        ActualizarLuz();

        if (timerCiclo <= 0f)
        {
            CambiarCiclo();
        }

        if (!EsDia)
        {
            timerEvento -= Time.deltaTime;
            if (timerEvento <= 0f)
            {
                ChequearEventoNocturno();
                timerEvento = intervaloChequeoEvento;
            }
        }
    }

    void CambiarCiclo()
    {
        EsDia = !EsDia;

        if (EsDia)
        {
            timerCiclo = duracionDia;
            DiaActual++;
            onNuevoDia?.Invoke(DiaActual);
            onAmanecer?.Invoke();
            AbrirSeccion(DiaActual - 1);
            Debug.Log($"[NoExit] Amanecer — Día {DiaActual} · Sección {DiaActual} abierta");
        }
        else
        {
            timerCiclo = duracionNoche;
            timerEvento = intervaloChequeoEvento;
            onAnochecer?.Invoke();
            Debug.Log($"[NoExit] Anochecer — Día {DiaActual}");
        }
    }

    void ActualizarLuz()
    {
        if (luzDireccional == null) return;

        float progreso = 1f - (timerCiclo / (EsDia ? duracionDia : duracionNoche));
        luzDireccional.color = colorLuzDia.Evaluate(EsDia ? progreso : 1f);
        luzDireccional.intensity = intensidadLuz.Evaluate(EsDia ? progreso : 0f);

        // Rotar el sol/luna
        float anguloBase = EsDia ? 0f : 180f;
        luzDireccional.transform.rotation = Quaternion.Euler(
            Mathf.Lerp(anguloBase, anguloBase + 180f, progreso), -30f, 0f
        );
    }

    void AbrirSeccion(int indice)
    {
        if (secciones == null || indice < 0 || indice >= secciones.Length) return;
        secciones[indice].Abrir();
        Debug.Log($"[NoExit] Sección {indice + 1} desbloqueada");
    }

    void AbrirTodasLasPuertas()
    {
        puertaNorte?.Abrir();
        puertaSur?.Abrir();
        puertaEste?.Abrir();
        puertaOeste?.Abrir();
    }

    void ChequearEventoNocturno()
    {
        if (Random.value > probabilidadEventoNoche) return;
        if (prefabsCriaturas == null || prefabsCriaturas.Length == 0) return;
        if (puntosSpawnCentro == null || puntosSpawnCentro.Length == 0) return;

        int cantidadSpawn = Random.Range(1, 3);
        for (int i = 0; i < cantidadSpawn; i++)
        {
            GameObject prefab = prefabsCriaturas[Random.Range(0, prefabsCriaturas.Length)];
            Transform punto = puntosSpawnCentro[Random.Range(0, puntosSpawnCentro.Length)];
            Instantiate(prefab, punto.position, Quaternion.identity);
        }

        Debug.Log($"[NoExit] Evento nocturno — {cantidadSpawn} criatura(s) en el centro");
    }

    public string GetTiempoFormateado()
    {
        int minutos = Mathf.FloorToInt(TiempoRestante / 60f);
        int segundos = Mathf.FloorToInt(TiempoRestante % 60f);
        return $"{(EsDia ? "DÍA" : "NOCHE")} {DiaActual}  —  {minutos:00}:{segundos:00}";
    }
}
