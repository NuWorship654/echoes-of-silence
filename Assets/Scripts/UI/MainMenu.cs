using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup menuGroup;
    [SerializeField] private Button btnNuevaPartida;
    [SerializeField] private Button btnContinuar;
    [SerializeField] private Button btnConfiguracion;
    [SerializeField] private Button btnSalir;

    [Header("Título - efectos glitch")]
    [SerializeField] private TextMeshProUGUI txtEchoes;
    [SerializeField] private TextMeshProUGUI txtSilence;

    [Header("Efectos")]
    [SerializeField] private Image flashImage;           // Image negro/blanco que cubre todo
    [SerializeField] private Image scanlineImage;       // Imagen con textura de scanlines (opcional)
    [SerializeField] private GameObject eyesObject;     // Dos imágenes de ojos rojos

    [Header("Escenas")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string configSceneName = "Config";

    [Header("Audio")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private string[] glitchEchoes  = { "ECH0ES", "€CHOES", "ΞCHOES", "ECH▓ES", "ECHOES" };
    private string[] glitchSilence = { "$ILENCE", "SЇLENCE", "SIL▒NCE", "SILΞNCE", "SILENCE" };

    private bool hasSave = false;

    void Start()
    {
        // Comprobar si hay partida guardada
        hasSave = PlayerPrefs.HasKey("SaveExists");
        btnContinuar.interactable = hasSave;

        // Asignar listeners
        btnNuevaPartida.onClick.AddListener(OnNuevaPartida);
        btnContinuar.onClick.AddListener(OnContinuar);
        btnConfiguracion.onClick.AddListener(OnConfiguracion);
        btnSalir.onClick.AddListener(OnSalir);

        // Flash inicial de entrada
        if (flashImage != null)
            StartCoroutine(EntryFlash());

        // Ocultar ojos
        if (eyesObject != null)
            eyesObject.SetActive(false);

        // Iniciar corrutinas de efectos
        StartCoroutine(GlitchLoop());
        StartCoroutine(EyesLoop());
        StartCoroutine(FlashLoop());

        // Ambience
        if (ambienceSource != null && ambienceClip != null)
        {
            ambienceSource.clip = ambienceClip;
            ambienceSource.loop = true;
            ambienceSource.volume = 0.4f;
            ambienceSource.Play();
        }
    }

    // ── Botones ──────────────────────────────────────────

    void OnNuevaPartida()
    {
        PlayClick();
        StartCoroutine(LoadScene(gameSceneName));
    }

    void OnContinuar()
    {
        if (!hasSave) return;
        PlayClick();
        StartCoroutine(LoadScene(gameSceneName));
    }

    void OnConfiguracion()
    {
        PlayClick();
        // Abre panel de config (implementa tú mismo) o carga escena
        // SceneManager.LoadScene(configSceneName);
        Debug.Log("Abrir configuración");
    }

    void OnSalir()
    {
        PlayClick();
        StartCoroutine(QuitGame());
    }

    // ── Efectos de horror ────────────────────────────────

    IEnumerator GlitchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(6f, 12f));
            yield return StartCoroutine(DoGlitch());
        }
    }

    IEnumerator DoGlitch()
    {
        for (int i = 0; i < glitchEchoes.Length; i++)
        {
            if (txtEchoes  != null) txtEchoes.text  = glitchEchoes[i];
            if (txtSilence != null) txtSilence.text = glitchSilence[i];
            yield return new WaitForSeconds(0.065f);
        }
        if (txtEchoes  != null) txtEchoes.text  = "ECHOES";
        if (txtSilence != null) txtSilence.text = "SILENCE";
    }

    IEnumerator EyesLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(8f, 18f));
            if (eyesObject != null)
            {
                eyesObject.SetActive(true);
                yield return new WaitForSeconds(Random.Range(0.6f, 1.2f));
                eyesObject.SetActive(false);
            }
        }
    }

    IEnumerator FlashLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f));
            yield return StartCoroutine(DoFlash(0.05f, 0.05f));
            yield return new WaitForSeconds(0.12f);
            yield return StartCoroutine(DoFlash(0.03f, 0.04f));
        }
    }

    IEnumerator DoFlash(float targetAlpha, float duration)
    {
        if (flashImage == null) yield break;
        flashImage.color = new Color(1f, 1f, 1f, targetAlpha);
        yield return new WaitForSeconds(duration);
        flashImage.color = new Color(1f, 1f, 1f, 0f);
    }

    IEnumerator EntryFlash()
    {
        if (flashImage == null) yield break;
        flashImage.color = new Color(0f, 0f, 0f, 1f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            flashImage.color = new Color(0f, 0f, 0f, 1f - t);
            yield return null;
        }
        flashImage.color = new Color(0f, 0f, 0f, 0f);
    }

    // ── Transiciones ─────────────────────────────────────

    IEnumerator LoadScene(string sceneName)
    {
        if (flashImage != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                flashImage.color = new Color(0f, 0f, 0f, t);
                yield return null;
            }
        }
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator QuitGame()
    {
        if (flashImage != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                flashImage.color = new Color(0f, 0f, 0f, t);
                yield return null;
            }
        }
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── Audio helpers ────────────────────────────────────

    void PlayClick()
    {
        if (clickClip != null)
            AudioSource.PlayClipAtPoint(clickClip, Vector3.zero, 0.8f);
    }

    // Llama esto desde cada botón en su evento OnPointerEnter (EventTrigger)
    public void PlayHover()
    {
        if (hoverClip != null)
            AudioSource.PlayClipAtPoint(hoverClip, Vector3.zero, 0.4f);
    }
}
