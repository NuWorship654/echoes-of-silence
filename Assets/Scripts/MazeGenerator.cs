// Scripts/MazeGenerator.cs
using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class MazeGenerator : MonoBehaviour
{
    [Header("Tamaño del Laberinto")]
    public int ancho = 15;
    public int alto = 15;
    public float tamañoCelda = 4f;
    public float alturaParedes = 3f;

    [Header("Materiales")]
    public Material materialPared;
    public Material materialPiso;
    public Material materialTecho;

    private bool[,] visitado;
    private bool[,] paredDerecha;
    private bool[,] paredAbajo;
    private GameObject contenedorLaberinto;
    private NavMeshSurface navMeshSurface;

    public Vector3 posicionInicio { get; private set; }
    public Vector3 posicionSalida { get; private set; }

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        GenerarLaberinto();
    }

    public void GenerarLaberinto()
    {
        if (contenedorLaberinto != null)
            Destroy(contenedorLaberinto);

        contenedorLaberinto = new GameObject("Laberinto");

        visitado = new bool[ancho, alto];
        paredDerecha = new bool[ancho, alto];
        paredAbajo = new bool[ancho, alto];

        for (int x = 0; x < ancho; x++)
            for (int z = 0; z < alto; z++)
            {
                paredDerecha[x, z] = true;
                paredAbajo[x, z] = true;
            }

        GenerarConBacktracker();
        ConstruirLaberinto();

        posicionInicio = new Vector3(tamañoCelda / 2, 1f, tamañoCelda / 2);
        posicionSalida = new Vector3((ancho - 1) * tamañoCelda, 1f, (alto - 1) * tamañoCelda);

        // Rehornear NavMesh después de generar el laberinto
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("[MazeGenerator] NavMesh rehornado correctamente.");
        }

        Debug.Log("[MazeGenerator] Laberinto generado correctamente.");
    }

    void GenerarConBacktracker()
    {
        Stack<Vector2Int> pila = new Stack<Vector2Int>();
        Vector2Int actual = new Vector2Int(0, 0);
        visitado[0, 0] = true;
        pila.Push(actual);

        while (pila.Count > 0)
        {
            List<Vector2Int> vecinos = ObtenerVecinosNoVisitados(actual);

            if (vecinos.Count > 0)
            {
                Vector2Int siguiente = vecinos[Random.Range(0, vecinos.Count)];
                EliminarPared(actual, siguiente);
                visitado[siguiente.x, siguiente.y] = true;
                pila.Push(actual);
                actual = siguiente;
            }
            else
            {
                actual = pila.Pop();
            }
        }
    }

    List<Vector2Int> ObtenerVecinosNoVisitados(Vector2Int celda)
    {
        List<Vector2Int> vecinos = new List<Vector2Int>();
        Vector2Int[] direcciones = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var dir in direcciones)
        {
            Vector2Int vecino = celda + dir;
            if (vecino.x >= 0 && vecino.x < ancho &&
                vecino.y >= 0 && vecino.y < alto &&
                !visitado[vecino.x, vecino.y])
            {
                vecinos.Add(vecino);
            }
        }
        return vecinos;
    }

    void EliminarPared(Vector2Int a, Vector2Int b)
    {
        Vector2Int diff = b - a;
        if (diff.x == 1) paredDerecha[a.x, a.y] = false;
        else if (diff.x == -1) paredDerecha[b.x, b.y] = false;
        else if (diff.y == 1) paredAbajo[a.x, a.y] = false;
        else if (diff.y == -1) paredAbajo[b.x, b.y] = false;
    }

    void ConstruirLaberinto()
    {
        for (int x = 0; x < ancho; x++)
        {
            for (int z = 0; z < alto; z++)
            {
                Vector3 origen = new Vector3(x * tamañoCelda, 0, z * tamañoCelda);

                CrearPlano(origen + new Vector3(tamañoCelda / 2, 0, tamañoCelda / 2),
                    new Vector3(tamañoCelda, 1, tamañoCelda), materialPiso, "Piso");

                CrearPlano(origen + new Vector3(tamañoCelda / 2, alturaParedes, tamañoCelda / 2),
                    new Vector3(tamañoCelda, 1, tamañoCelda), materialTecho, "Techo", true);

                if (x == 0)
                    CrearPared(origen + new Vector3(0, alturaParedes / 2, tamañoCelda / 2),
                        new Vector3(0.2f, alturaParedes, tamañoCelda), materialPared);

                if (z == 0)
                    CrearPared(origen + new Vector3(tamañoCelda / 2, alturaParedes / 2, 0),
                        new Vector3(tamañoCelda, alturaParedes, 0.2f), materialPared);

                if (paredDerecha[x, z])
                    CrearPared(origen + new Vector3(tamañoCelda, alturaParedes / 2, tamañoCelda / 2),
                        new Vector3(0.2f, alturaParedes, tamañoCelda), materialPared);

                if (paredAbajo[x, z])
                    CrearPared(origen + new Vector3(tamañoCelda / 2, alturaParedes / 2, tamañoCelda),
                        new Vector3(tamañoCelda, alturaParedes, 0.2f), materialPared);
            }
        }
    }

    void CrearPared(Vector3 posicion, Vector3 escala, Material material)
    {
        GameObject pared = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pared.transform.position = posicion;
        pared.transform.localScale = escala;
        pared.transform.parent = contenedorLaberinto.transform;
        if (material != null)
            pared.GetComponent<Renderer>().material = material;
        pared.isStatic = true;
    }

    void CrearPlano(Vector3 posicion, Vector3 escala, Material material, string nombre, bool voltear = false)
    {
        GameObject plano = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plano.name = nombre;
        plano.transform.position = posicion;
        plano.transform.localScale = new Vector3(escala.x, 0.1f, escala.z);
        plano.transform.parent = contenedorLaberinto.transform;
        if (material != null)
            plano.GetComponent<Renderer>().material = material;
        plano.isStatic = true;
    }

    public Vector3 GetPosicionInicio() => posicionInicio;
    public Vector3 GetPosicionSalida() => posicionSalida;
}