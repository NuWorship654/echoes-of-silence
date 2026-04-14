// Scripts/Inventario.cs
using UnityEngine;
using System.Collections.Generic;

public class Inventario : MonoBehaviour
{
    private Dictionary<TipoRecurso, int> recursos = new Dictionary<TipoRecurso, int>();

    void Start()
    {
        foreach (TipoRecurso tipo in System.Enum.GetValues(typeof(TipoRecurso)))
            recursos[tipo] = 0;
    }

    public void AgregarRecurso(TipoRecurso tipo, int cantidad)
    {
        recursos[tipo] += cantidad;
        Debug.Log($"[Inventario] {tipo}: {recursos[tipo]}");
    }

    public bool TieneRecursos(TipoRecurso tipo, int cantidad)
    {
        return recursos.ContainsKey(tipo) && recursos[tipo] >= cantidad;
    }

    public bool ConsumirRecursos(TipoRecurso tipo, int cantidad)
    {
        if (!TieneRecursos(tipo, cantidad)) return false;
        recursos[tipo] -= cantidad;
        return true;
    }

    public int GetCantidad(TipoRecurso tipo)
    {
        return recursos.ContainsKey(tipo) ? recursos[tipo] : 0;
    }

    public Dictionary<TipoRecurso, int> GetTodosLosRecursos() => recursos;
}
