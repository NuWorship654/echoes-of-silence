// Scripts/CraftingSystem.cs
using UnityEngine;
using System.Collections.Generic;

public enum TipoArma { Lanza, HachaImprovisada, CuchilloPiedra, Trampa, Arco, Flechas }

[System.Serializable]
public class RecetaCrafting
{
    public TipoArma resultado;
    public int cantidadResultado = 1;
    public List<Ingrediente> ingredientes = new List<Ingrediente>();

    public string GetDescripcion()
    {
        string desc = "";
        foreach (var ing in ingredientes)
            desc += $"{ing.cantidad} {ing.tipo}  ";
        return desc.Trim();
    }
}

[System.Serializable]
public class Ingrediente
{
    public TipoRecurso tipo;
    public int cantidad;
}

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    public List<RecetaCrafting> recetas = new List<RecetaCrafting>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        InicializarRecetas();
    }

    void InicializarRecetas()
    {
        recetas.Add(new RecetaCrafting
        {
            resultado = TipoArma.Lanza,
            cantidadResultado = 1,
            ingredientes = new List<Ingrediente>
            {
                new Ingrediente { tipo = TipoRecurso.Madera, cantidad = 3 },
                new Ingrediente { tipo = TipoRecurso.Piedra, cantidad = 1 }
            }
        });

        recetas.Add(new RecetaCrafting
        {
            resultado = TipoArma.HachaImprovisada,
            cantidadResultado = 1,
            ingredientes = new List<Ingrediente>
            {
                new Ingrediente { tipo = TipoRecurso.Madera, cantidad = 2 },
                new Ingrediente { tipo = TipoRecurso.Piedra, cantidad = 2 }
            }
        });

        recetas.Add(new RecetaCrafting
        {
            resultado = TipoArma.CuchilloPiedra,
            cantidadResultado = 1,
            ingredientes = new List<Ingrediente>
            {
                new Ingrediente { tipo = TipoRecurso.Madera, cantidad = 1 },
                new Ingrediente { tipo = TipoRecurso.Piedra, cantidad = 2 }
            }
        });

        recetas.Add(new RecetaCrafting
        {
            resultado = TipoArma.Trampa,
            cantidadResultado = 1,
            ingredientes = new List<Ingrediente>
            {
                new Ingrediente { tipo = TipoRecurso.Madera, cantidad = 4 },
                new Ingrediente { tipo = TipoRecurso.Fibra, cantidad = 1 }
            }
        });

        recetas.Add(new RecetaCrafting
        {
            resultado = TipoArma.Arco,
            cantidadResultado = 1,
            ingredientes = new List<Ingrediente>
            {
                new Ingrediente { tipo = TipoRecurso.Madera, cantidad = 3 },
                new Ingrediente { tipo = TipoRecurso.Fibra, cantidad = 2 }
            }
        });

        recetas.Add(new RecetaCrafting
        {
            resultado = TipoArma.Flechas,
            cantidadResultado = 5,
            ingredientes = new List<Ingrediente>
            {
                new Ingrediente { tipo = TipoRecurso.Madera, cantidad = 1 },
                new Ingrediente { tipo = TipoRecurso.Piedra, cantidad = 1 }
            }
        });
    }

    public bool PuedeCraftear(RecetaCrafting receta, Inventario inventario)
    {
        foreach (var ing in receta.ingredientes)
        {
            if (!inventario.TieneRecursos(ing.tipo, ing.cantidad))
                return false;
        }
        return true;
    }

    public bool Craftear(RecetaCrafting receta, Inventario inventario, InventarioArmas inventarioArmas)
    {
        if (!PuedeCraftear(receta, inventario)) return false;

        foreach (var ing in receta.ingredientes)
            inventario.ConsumirRecursos(ing.tipo, ing.cantidad);

        inventarioArmas.AgregarArma(receta.resultado, receta.cantidadResultado);
        Debug.Log($"[Crafting] Crafteado: {receta.resultado} x{receta.cantidadResultado}");
        return true;
    }

    public List<RecetaCrafting> GetRecetasDisponibles(Inventario inventario)
    {
        List<RecetaCrafting> disponibles = new List<RecetaCrafting>();
        foreach (var receta in recetas)
        {
            if (PuedeCraftear(receta, inventario))
                disponibles.Add(receta);
        }
        return disponibles;
    }
}
