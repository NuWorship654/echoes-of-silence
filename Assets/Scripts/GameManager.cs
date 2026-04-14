// Scripts/GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    public MazeGenerator mazeGenerator;
    public PlayerController player;

    void Start()
    {
        // Esperar a que el laberinto se genere y posicionar al jugador
        Invoke("PosicionarJugador", 0.1f);
    }

    void PosicionarJugador()
    {
        if (mazeGenerator != null && player != null)
        {
            Vector3 inicio = mazeGenerator.GetPosicionInicio();
            player.transform.position = inicio;
            Debug.Log("[GameManager] Jugador posicionado en: " + inicio);
        }
    }
}