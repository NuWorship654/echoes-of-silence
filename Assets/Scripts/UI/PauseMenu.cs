using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("Panel de pausa")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private CanvasGroup pauseGroup;

    [Header("Botones")]
    [SerializeField] private Button btnReanudar;
    [SerializeField] private Button btnConfiguracion;
    [SerializeField] private Button btnMenuPrincipal;

    [Header("Efectos")]
    [SerializeField] private Image flashImage;

    [Header("Escenas")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);

        btnReanudar.onClick.AddListener(Reanudar);
        btnMenuPrincipal.onClick.AddListener(IrAlMenu);
        if (btnConfiguracion != null)
            btnConfiguracion.onClick.AddListener(AbrirConfig);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    public void Reanudar()
    {
        StartCoroutine(FadeOutAndResume());
    }

    void IrAlMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadScene(mainMenuScene));
    }

    void AbrirConfig()
    {
        Debug.Log("Abrir configuración");
    }

    IEnumerator FadeIn()
    {
        if (pauseGroup == null) yield break;
        pauseGroup.alpha = 0f;
        while (pauseGroup.alpha < 1f)
        {
            pauseGroup.alpha += Time.unscaledDeltaTime * 4f;
            yield return null;
        }
        pauseGroup.alpha = 1f;
    }

    IEnumerator FadeOutAndResume()
    {
        if (pauseGroup != null)
        {
            while (pauseGroup.alpha > 0f)
            {
                pauseGroup.alpha -= Time.unscaledDeltaTime * 6f;
                yield return null;
            }
        }
        pausePanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
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
