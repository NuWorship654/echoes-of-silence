using UnityEngine;
using System.Collections;

public class Puerta : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string nombrePuerta = "Norte";
    [SerializeField] private Transform pivote;
    [SerializeField] private float anguloAbierto = 90f;
    [SerializeField] private float velocidadAnimacion = 2f;

    private Quaternion rotacionCerrada;
    private Quaternion rotacionAbierta;
    private bool estaAbierta = false;
    private Coroutine animacionActual;

    void Start()
    {
        rotacionCerrada = pivote.localRotation;
        rotacionAbierta = Quaternion.Euler(0f, anguloAbierto, 0f) * rotacionCerrada;
    }

    public void Abrir()
    {
        if (estaAbierta) return;
        estaAbierta = true;
        if (animacionActual != null) StopCoroutine(animacionActual);
        animacionActual = StartCoroutine(AnimarPuerta(rotacionAbierta));
        Debug.Log($"[Puerta {nombrePuerta}] Abierta");
    }

    public void Cerrar()
    {
        if (!estaAbierta) return;
        estaAbierta = false;
        if (animacionActual != null) StopCoroutine(animacionActual);
        animacionActual = StartCoroutine(AnimarPuerta(rotacionCerrada));
        Debug.Log($"[Puerta {nombrePuerta}] Cerrada");
    }

    IEnumerator AnimarPuerta(Quaternion destino)
    {
        while (Quaternion.Angle(pivote.localRotation, destino) > 0.1f)
        {
            pivote.localRotation = Quaternion.Slerp(
                pivote.localRotation, destino, Time.deltaTime * velocidadAnimacion
            );
            yield return null;
        }
        pivote.localRotation = destino;
    }

    public bool EstaAbierta => estaAbierta;
}
