using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup panelGroup;

    [Header("Texto")]
    [SerializeField] private TextMeshProUGUI txtMoriste;
    [SerializeField] private TextMeshProUGUI txtSubtitulo;

    [Header("Botones")]
    [SerializeField] private Button btnReintentar;
    [SerializeField] private Button btnMenuPrincipal;

    [Header("Efectos")]
    [SerializeField] private Image flashImage;
    [SerializeField] private Image bloodOverlay;  // imagen roja semitransparente que cubre la pantalla

    [Header("Escenas")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string mainMenuScene  = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioClip gameOverStinger;

    private string[] subtitulos = {
        "El Susurrador te encontró.",
        "Nadie escucha tus gritos.",
        "El silencio te consumió.",
        "No había escapatoria.",
        "Debiste correr más rápido."
    };

    void Start()
    {
        gameOverPanel.SetActive(false);
        if (bloodOverlay != null)
            bloodOverlay.color = new Color(0.4f, 0f, 0f, 0f);

        btnReintentar.onClick.AddListener(Reintentar);
        btnMenuPrincipal.onClick.AddListener(IrAlMenu);
    }

    // Llama esto cuando el jugador muere
    public void ShowGameOver()
    {
        if (txtSubtitulo != null)
            txtSubtitulo.text = subtitulos[Random.Range(0, subtitulos.Length)];

        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        // Parar el tiempo
        Time.timeScale = 0f;

        // Overlay de sangre roja
        if (bloodOverlay != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 1.2f;
                bloodOverlay.color = new Color(0.35f, 0f, 0f, Mathf.Lerp(0f, 0.65f, t));
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(0.4f);

        // Reproducir stinger de audio
        if (gameOverStinger != null)
            AudioSource.PlayClipAtPoint(gameOverStinger, Vector3.zero, 1f);

        // Flash blanco rápido
        if (flashImage != null)
        {
            flashImage.color = new Color(1f, 1f, 1f, 0.8f);
            yield return new WaitForSecondsRealtime(0.08f);
            flashImage.color = new Color(1f, 1f, 1f, 0f);
        }

        // Mostrar panel con fade
        gameOverPanel.SetActive(true);
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            while (panelGroup.alpha < 1f)
            {
                panelGroup.alpha += Time.unscaledDeltaTime * 1.8f;
                yield return null;
            }
        }

        // Efecto de typing en el título
        if (txtMoriste != null)
            yield return StartCoroutine(TypeText(txtMoriste, "MORISTE", 0.08f));
    }

    IEnumerator TypeText(TextMeshProUGUI tmp, string fullText, float delay)
    {
        tmp.text = "";
        foreach (char c in fullText)
        {
            tmp.text += c;
            yield return new WaitForSecondsRealtime(delay);
        }
    }

    void Reintentar()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadScene(gameSceneName));
    }

    void IrAlMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadScene(mainMenuScene));
    }

    IEnumerator LoadScene(string sceneName)
    {
        if (flashImage != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 2f;
                flashImage.color = new Color(0f, 0f, 0f, t);
                yield return null;
            }
        }
        SceneManager.LoadScene(sceneName);
    }
}
