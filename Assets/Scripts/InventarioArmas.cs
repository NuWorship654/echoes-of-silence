// Scripts/InventarioArmas.cs
using UnityEngine;
using System.Collections.Generic;

public class InventarioArmas : MonoBehaviour
{
    private Dictionary<TipoArma, int> armas = new Dictionary<TipoArma, int>();
    public TipoArma? armaEquipada = null;

    void Start()
    {
        foreach (TipoArma tipo in System.Enum.GetValues(typeof(TipoArma)))
            armas[tipo] = 0;
    }

    public void AgregarArma(TipoArma tipo, int cantidad)
    {
        armas[tipo] += cantidad;

        // Equipar automáticamente si no hay arma equipada
        if (armaEquipada == null)
            armaEquipada = tipo;

        Debug.Log($"[Armas] {tipo}: {armas[tipo]}");
    }

    public bool TieneArma(TipoArma tipo) => armas.ContainsKey(tipo) && armas[tipo] > 0;

    public void Equipar(TipoArma tipo)
    {
        if (TieneArma(tipo))
        {
            armaEquipada = tipo;
            Debug.Log($"[Armas] Equipado: {tipo}");
        }
    }

    public int GetCantidad(TipoArma tipo) => armas.ContainsKey(tipo) ? armas[tipo] : 0;

    public TipoArma? GetArmaEquipada() => armaEquipada;

    public Dictionary<TipoArma, int> GetTodasLasArmas() => armas;
}
