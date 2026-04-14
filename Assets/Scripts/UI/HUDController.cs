using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Salud")]
    [SerializeField] private Image saludFill;
    [SerializeField] private Image saludBg;
    [SerializeField] private TextMeshProUGUI txtSalud;

    [Header("Cordura")]
    [SerializeField] private Image corduraFill;
    [SerializeField] private TextMeshProUGUI txtCordura;

    [Header("Linterna")]
    [SerializeField] private Image linternaFill;
    [SerializeField] private TextMeshProUGUI txtLinterna;

    [Header("Ruido")]
    [SerializeField] private TextMeshProUGUI txtRuido;
    [SerializeField] private Image ruidoIndicador;

    [Header("Crosshair")]
    [SerializeField] private Image crosshairCenter;
    [SerializeField] private Image[] crosshairLines; // 4 líneas

    [Header("Colores")]
    [SerializeField] private Color colorSaludAlta    = new Color(0.22f, 0.22f, 0.22f);
    [SerializeField] private Color colorSaludMedia   = new Color(0.45f, 0.08f, 0f);
    [SerializeField] private Color colorSaludBaja    = new Color(0.6f, 0f, 0f);
    [SerializeField] private Color colorCorduraAlta  = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] private Color colorCorduarBaja  = new Color(0.35f, 0f, 0.35f);

    [Header("Efecto cordura baja")]
    [SerializeField] private Image vignetteOverlay;   // overlay oscuro en los bordes
    [SerializeField] private float corduraBajaThreshold = 0.3f;

    // Referencias al GameManager o PlayerController
    // Asígnalas según tu arquitectura
    private float salud    = 1f;
    private float cordura  = 1f;
    private float linterna = 1f;
    private bool  ruidoActivo = false;

    private float vignetteAlpha = 0f;

    void Update()
    {
        ActualizarBarras();
        ActualizarRuido();
        ActualizarVignette();
    }

    // ── API pública — llama estos métodos desde PlayerController ──

    public void SetSalud(float valor01)
    {
        salud = Mathf.Clamp01(valor01);
    }

    public void SetCordura(float valor01)
    {
        cordura = Mathf.Clamp01(valor01);
    }

    public void SetLinterna(float valor01)
    {
        linterna = Mathf.Clamp01(valor01);
    }

    public void SetRuido(bool activo)
    {
        ruidoActivo = activo;
    }

    // ── Actualización visual ──────────────────────────────

    void ActualizarBarras()
    {
        // Salud
        if (saludFill != null)
        {
            saludFill.fillAmount = Mathf.Lerp(saludFill.fillAmount, salud, Time.deltaTime * 5f);
            if      (salud > 0.6f) saludFill.color = colorSaludAlta;
            else if (salud > 0.3f) saludFill.color = Color.Lerp(colorSaludMedia, colorSaludAlta, (salud - 0.3f) / 0.3f);
            else                   saludFill.color = Color.Lerp(colorSaludBaja, colorSaludMedia, salud / 0.3f);
        }
        if (txtSalud != null)
            txtSalud.text = Mathf.RoundToInt(salud * 100f).ToString();

        // Cordura
        if (corduraFill != null)
        {
            corduraFill.fillAmount = Mathf.Lerp(corduraFill.fillAmount, cordura, Time.deltaTime * 4f);
            corduraFill.color = Color.Lerp(colorCorduarBaja, colorCorduraAlta, cordura);
        }
        if (txtCordura != null)
            txtCordura.text = Mathf.RoundToInt(cordura * 100f).ToString();

        // Linterna
        if (linternaFill != null)
        {
            linternaFill.fillAmount = Mathf.Lerp(linternaFill.fillAmount, linterna, Time.deltaTime * 3f);
            // Parpadeo si batería baja
            if (linterna < 0.15f)
            {
                float blink = Mathf.PingPong(Time.time * 4f, 1f);
                linternaFill.color = Color.Lerp(new Color(0.15f, 0.1f, 0f), new Color(0.5f, 0.35f, 0f), blink);
            }
        }
        if (txtLinterna != null)
            txtLinterna.text = Mathf.RoundToInt(linterna * 100f).ToString() + "%";
    }

    void ActualizarRuido()
    {
        if (txtRuido == null) return;
        if (ruidoActivo)
        {
            txtRuido.text = "RUIDO DETECTADO";
            txtRuido.color = new Color(0.5f, 0.1f, 0.1f, Mathf.PingPong(Time.time * 2f, 1f) * 0.5f + 0.5f);
        }
        else
        {
            txtRuido.text = "";
        }

        if (ruidoIndicador != null)
            ruidoIndicador.enabled = ruidoActivo;
    }

    void ActualizarVignette()
    {
        if (vignetteOverlay == null) return;

        float targetAlpha = 0f;
        if (cordura < corduraBajaThreshold)
        {
            targetAlpha = (1f - cordura / corduraBajaThreshold) * 0.6f;
            // Pulso al ritmo del corazón si cordura muy baja
            if (cordura < 0.15f)
                targetAlpha *= 0.7f + Mathf.Sin(Time.time * 2.5f) * 0.3f;
        }

        vignetteAlpha = Mathf.Lerp(vignetteAlpha, targetAlpha, Time.deltaTime * 2f);
        vignetteOverlay.color = new Color(0.2f, 0f, 0f, vignetteAlpha);
    }

    // ── Efecto de daño (llama desde PlayerController) ────

    public void FlashDaño()
    {
        StartCoroutine(DamageFlash());
    }

    System.Collections.IEnumerator DamageFlash()
    {
        if (vignetteOverlay == null) yield break;
        Color c = vignetteOverlay.color;
        vignetteOverlay.color = new Color(0.5f, 0f, 0f, 0.7f);
        yield return new WaitForSeconds(0.1f);
        vignetteOverlay.color = c;
    }
}
