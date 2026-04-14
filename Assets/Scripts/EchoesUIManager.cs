using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EchoesUIManager : MonoBehaviour
{
    [Header("Escenas")]
    public string gameSceneName     = "Game";
    public string mainMenuSceneName = "MainMenu";

    [Header("Fuentes (opcional)")]
    public TMP_FontAsset titleFont;
    public TMP_FontAsset uiFont;

    private GameObject mainMenuPanel, pausePanel, gameOverPanel, hudPanel;
    private TextMeshProUGUI txtEchoes, txtSilence, txtRuido, txtMoriste, txtSubtitulo;
    private Image flashImage, bloodOverlay, saludFill, corduraFill, linternaFill;
    private GameObject eyesObj;
    private CanvasGroup pauseGroup;
    private bool isPaused = false;
    private float _salud = 1f, _cordura = 1f, _linterna = 1f;
    private bool _ruidoActivo = false;

    private readonly string[] glitchEchoes  = {"ECH0ES","ECHOES","ΞCHOES","ECH▓ES","ECHOES"};
    private readonly string[] glitchSilence = {"SILENC€","SЇLENCE","SIL▒NCE","SILΞNCE","SILENCE"};
    private readonly string[] subtitulos = {
        "El Susurrador te encontro.",
        "Nadie escucha tus gritos.",
        "El silencio te consumio.",
        "No habia escapatoria.",
        "Debiste correr mas rapido."
    };

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Canvas canvas = MakeCanvas();

        flashImage   = MakeFullImg(canvas.transform, new Color(0,0,0,0), "Flash");
        bloodOverlay = MakeFullImg(canvas.transform, new Color(0.35f,0,0,0), "BloodOverlay");

        MakeBloodLayer(canvas.transform);
        MakeMainMenuPanel(canvas.transform);
        MakeHUDPanel(canvas.transform);
        MakePausePanel(canvas.transform);
        MakeGameOverPanel(canvas.transform);

        flashImage.transform.SetAsLastSibling();

        bool enMenu = SceneManager.GetActiveScene().name != gameSceneName;
        mainMenuPanel.SetActive(enMenu);
        hudPanel.SetActive(!enMenu);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        if (enMenu)
        {
            StartCoroutine(EntryFade());
            StartCoroutine(GlitchLoop());
            StartCoroutine(EyesLoop());
            StartCoroutine(FlashLoop());
        }
    }

    Canvas MakeCanvas()
    {
        GameObject go = new GameObject("EchoesCanvas");
        go.transform.SetParent(transform);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0f;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    void MakeBloodLayer(Transform parent)
    {
        GameObject go = new GameObject("BloodLayer");
        go.transform.SetParent(parent, false);
        Stretch(go.AddComponent<RectTransform>());
        RawImage ri = go.AddComponent<RawImage>();
        ri.color = Color.white;
        ri.raycastTarget = false;
        go.AddComponent<BloodSystem>();
    }

    void MakeMainMenuPanel(Transform parent)
    {
        mainMenuPanel = NewPanel(parent, new Color(0,0,0,0), "MainMenu");

        // ECHOES — ocupa ancho completo, tercio superior
        txtEchoes = MakeTMP(mainMenuPanel.transform, "ECHOES", 140, new Color(0.87f,0,0), FontStyles.Bold);
        var rtE = txtEchoes.rectTransform;
        rtE.anchorMin = new Vector2(0.05f, 0.62f);
        rtE.anchorMax = new Vector2(0.95f, 0.82f);
        rtE.offsetMin = rtE.offsetMax = Vector2.zero;
        txtEchoes.alignment = TextAlignmentOptions.Center;
        txtEchoes.characterSpacing = 12;
        txtEchoes.enableAutoSizing = false;
        txtEchoes.fontSize = 130;
        if (titleFont) txtEchoes.font = titleFont;

        // OF
        var txtOf = MakeTMP(mainMenuPanel.transform, "OF", 32, new Color(0.35f,0,0), FontStyles.Bold);
        var rtOf = txtOf.rectTransform;
        rtOf.anchorMin = new Vector2(0.3f, 0.585f);
        rtOf.anchorMax = new Vector2(0.7f, 0.625f);
        rtOf.offsetMin = rtOf.offsetMax = Vector2.zero;
        txtOf.alignment = TextAlignmentOptions.Center;
        txtOf.characterSpacing = 22;
        txtOf.enableAutoSizing = false;
        txtOf.fontSize = 30;
        if (titleFont) txtOf.font = titleFont;

        // SILENCE
        txtSilence = MakeTMP(mainMenuPanel.transform, "SILENCE", 140, new Color(0.87f,0,0), FontStyles.Bold);
        var rtS = txtSilence.rectTransform;
        rtS.anchorMin = new Vector2(0.02f, 0.44f);
        rtS.anchorMax = new Vector2(0.98f, 0.60f);
        rtS.offsetMin = rtS.offsetMax = Vector2.zero;
        txtSilence.alignment = TextAlignmentOptions.Center;
        txtSilence.characterSpacing = 6;
        txtSilence.enableAutoSizing = false;
        txtSilence.fontSize = 130;
        if (titleFont) txtSilence.font = titleFont;

        // Linea separadora
        var lin = MakeImg(mainMenuPanel.transform, new Color(0.6f,0,0,0.7f), "Line");
        var rtL = lin.rectTransform;
        rtL.anchorMin = new Vector2(0.2f, 0.415f);
        rtL.anchorMax = new Vector2(0.8f, 0.418f);
        rtL.offsetMin = rtL.offsetMax = Vector2.zero;

        // Botones proporcionales
        float[] anchorsY = { 0.36f, 0.29f, 0.22f, 0.15f };
        string[] labels = {"▸  NUEVA PARTIDA","CONTINUAR","CONFIGURACION","SALIR"};
        for (int i = 0; i < labels.Length; i++)
            MakeMenuBtn(mainMenuPanel.transform, labels[i], anchorsY[i], i);

        // Ojos
        eyesObj = new GameObject("Eyes");
        eyesObj.transform.SetParent(mainMenuPanel.transform, false);
        var rtEyes = eyesObj.AddComponent<RectTransform>();
        rtEyes.anchorMin = new Vector2(0.38f, 0.88f);
        rtEyes.anchorMax = new Vector2(0.62f, 0.93f);
        rtEyes.offsetMin = rtEyes.offsetMax = Vector2.zero;
        for (int i = 0; i < 2; i++)
        {
            var eye = MakeImg(eyesObj.transform, new Color(0.8f,0,0,1f), "Eye"+i);
            eye.rectTransform.anchorMin = new Vector2(i==0?0.15f:0.65f, 0.1f);
            eye.rectTransform.anchorMax = new Vector2(i==0?0.35f:0.85f, 0.9f);
            eye.rectTransform.offsetMin = eye.rectTransform.offsetMax = Vector2.zero;
        }
        eyesObj.SetActive(false);

        // Footer
        var foot = MakeTMP(mainMenuPanel.transform,"ECHOES OF SILENCE  v0.1.0",11,new Color(0.1f,0.01f,0.01f),FontStyles.Normal);
        var rtF = foot.rectTransform;
        rtF.anchorMin = new Vector2(0f,0f);
        rtF.anchorMax = new Vector2(1f,0.05f);
        rtF.offsetMin = rtF.offsetMax = Vector2.zero;
        foot.alignment = TextAlignmentOptions.Center;
        if (uiFont) foot.font = uiFont;
    }

    void MakeMenuBtn(Transform parent, string label, float anchorYCenter, int index)
    {
        float half = 0.038f;
        var go = new GameObject("MBtn"+index);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.25f, anchorYCenter - half);
        rt.anchorMax = new Vector2(0.75f, anchorYCenter + half);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.06f,0,0,0.92f);

        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = new Color(0.06f,0,0,0.92f);
        cb.highlightedColor = new Color(0.18f,0.02f,0.02f,0.98f);
        cb.pressedColor     = new Color(0.28f,0.03f,0.03f,1f);
        cb.colorMultiplier  = 1f;
        btn.colors = cb;

        var border = MakeImg(go.transform, new Color(0.8f,0,0, index==0?1f:0.5f), "Border");
        border.rectTransform.anchorMin = Vector2.zero;
        border.rectTransform.anchorMax = new Vector2(0f,1f);
        border.rectTransform.offsetMin = Vector2.zero;
        border.rectTransform.offsetMax = new Vector2(4f,0f);

        var txt = MakeTMP(go.transform, label, 22,
            index==0 ? new Color(1f,0.15f,0.15f) : new Color(0.75f,0.18f,0.18f),
            index==0 ? FontStyles.Bold : FontStyles.Normal);
        Stretch(txt.rectTransform);
        txt.alignment = TextAlignmentOptions.Center;
        txt.characterSpacing = 6;
        txt.enableAutoSizing = false;
        txt.fontSize = 22;
        if (uiFont) txt.font = uiFont;

        int idx = index;
        btn.onClick.AddListener(() => OnMenuBtn(idx));
    }

    void OnMenuBtn(int idx)
    {
        switch (idx)
        {
            case 0: StartCoroutine(LoadScene(gameSceneName)); break;
            case 1: if (PlayerPrefs.HasKey("SaveExists")) StartCoroutine(LoadScene(gameSceneName)); break;
            case 2: Debug.Log("Config"); break;
            case 3: StartCoroutine(QuitSeq()); break;
        }
    }

    void MakeHUDPanel(Transform parent)
    {
        hudPanel = NewPanel(parent, new Color(0,0,0,0), "HUD");

        saludFill   = MakeBarLeft(hudPanel.transform, "SALUD",   new Vector2(20,70));
        corduraFill = MakeBarLeft(hudPanel.transform, "CORDURA", new Vector2(20,44));
        linternaFill = MakeBarRight(hudPanel.transform, "LINTERNA", new Vector2(-20,70));

        txtRuido = MakeTMP(hudPanel.transform,"",10,new Color(0.5f,0.1f,0.1f),FontStyles.Normal);
        txtRuido.rectTransform.anchorMin = new Vector2(1,0);
        txtRuido.rectTransform.anchorMax = new Vector2(1,0);
        txtRuido.rectTransform.pivot     = new Vector2(1,0);
        txtRuido.rectTransform.anchoredPosition = new Vector2(-20,44);
        txtRuido.rectTransform.sizeDelta = new Vector2(200,20);
        txtRuido.alignment = TextAlignmentOptions.Right;
        if (uiFont) txtRuido.font = uiFont;

        var cross = new GameObject("Cross");
        cross.transform.SetParent(hudPanel.transform, false);
        var rtC = cross.AddComponent<RectTransform>();
        rtC.anchorMin = rtC.anchorMax = new Vector2(0.5f,0.5f);
        rtC.sizeDelta = new Vector2(20,20);

        var h = MakeImg(cross.transform, new Color(1,1,1,0.22f), "H");
        PlaceRect(h.rectTransform, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(14,1));
        var v = MakeImg(cross.transform, new Color(1,1,1,0.22f), "V");
        PlaceRect(v.rectTransform, new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1,14));
    }

    Image MakeBarLeft(Transform parent, string label, Vector2 pos)
    {
        var c = new GameObject("Bar_"+label);
        c.transform.SetParent(parent, false);
        var rt = c.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(140,14);

        var lbl = MakeTMP(c.transform, label, 8, new Color(0.2f,0.05f,0.05f), FontStyles.Normal);
        lbl.rectTransform.anchorMin = new Vector2(0,1); lbl.rectTransform.anchorMax = new Vector2(1,1);
        lbl.rectTransform.anchoredPosition = new Vector2(0,2); lbl.rectTransform.sizeDelta = new Vector2(0,10);
        if (uiFont) lbl.font = uiFont;

        var bg = MakeImg(c.transform, new Color(0.05f,0,0,0.8f), "Bg");
        Stretch(bg.rectTransform);

        var fill = MakeImg(c.transform, new Color(0.22f,0.22f,0.22f), "Fill");
        fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;
        return fill;
    }

    Image MakeBarRight(Transform parent, string label, Vector2 pos)
    {
        var c = new GameObject("Bar_"+label);
        c.transform.SetParent(parent, false);
        var rt = c.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1,0);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(100,14);

        var lbl = MakeTMP(c.transform, label, 8, new Color(0.2f,0.05f,0.05f), FontStyles.Normal);
        lbl.rectTransform.anchorMin = new Vector2(0,1); lbl.rectTransform.anchorMax = new Vector2(1,1);
        lbl.rectTransform.anchoredPosition = new Vector2(0,2); lbl.rectTransform.sizeDelta = new Vector2(0,10);
        if (uiFont) lbl.font = uiFont;

        var bg = MakeImg(c.transform, new Color(0.05f,0,0,0.8f), "Bg");
        Stretch(bg.rectTransform);

        var fill = MakeImg(c.transform, new Color(0.2f,0.15f,0f), "Fill");
        fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;
        return fill;
    }

    void MakePausePanel(Transform parent)
    {
        pausePanel = NewPanel(parent, new Color(0,0,0,0.92f), "Pause");
        pauseGroup = pausePanel.AddComponent<CanvasGroup>();

        var tP = MakeTMP(pausePanel.transform,"PAUSA",48,new Color(0.33f,0.03f,0.03f),FontStyles.Bold);
        PlaceRect(tP.rectTransform, new Vector2(0.5f,0.5f), new Vector2(0,90), new Vector2(400,70));
        tP.alignment = TextAlignmentOptions.Center; tP.characterSpacing = 18;
        if (titleFont) tP.font = titleFont;

        var sep = MakeImg(pausePanel.transform, new Color(0.3f,0,0,0.5f), "Sep");
        PlaceRect(sep.rectTransform, new Vector2(0.5f,0.5f), new Vector2(0,50), new Vector2(180,1));

        string[] pl = {"REANUDAR","CONFIGURACION","MENU PRINCIPAL"};
        for (int i = 0; i < pl.Length; i++) MakePauseBtn(pausePanel.transform, pl[i], new Vector2(0,10f-i*52f), i);
    }

    void MakePauseBtn(Transform parent, string label, Vector2 pos, int index)
    {
        var go = new GameObject("PBtn"+index);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(260,40);

        var bg = go.AddComponent<Image>(); bg.color = new Color(0.02f,0,0,0.7f);
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor = new Color(0.02f,0,0,0.7f); cb.highlightedColor = new Color(0.1f,0,0,0.9f);
        btn.colors = cb;

        var txt = MakeTMP(go.transform, label, 12,
            index==0?new Color(0.6f,0.1f,0.1f):new Color(0.25f,0.05f,0.05f), FontStyles.Normal);
        Stretch(txt.rectTransform); txt.alignment = TextAlignmentOptions.Center;
        if (uiFont) txt.font = uiFont;

        int idx = index;
        btn.onClick.AddListener(() => {
            if (idx==0) Reanudar();
            else if (idx==2) { Time.timeScale=1f; StartCoroutine(LoadScene(mainMenuSceneName)); }
        });
    }

    void MakeGameOverPanel(Transform parent)
    {
        gameOverPanel = NewPanel(parent, new Color(0.01f,0,0,0.95f), "GameOver");
        gameOverPanel.AddComponent<CanvasGroup>();

        txtMoriste = MakeTMP(gameOverPanel.transform,"",80,new Color(0.42f,0,0),FontStyles.Bold);
        PlaceRect(txtMoriste.rectTransform, new Vector2(0.5f,0.5f), new Vector2(0,60), new Vector2(700,100));
        txtMoriste.alignment = TextAlignmentOptions.Center; txtMoriste.characterSpacing = 12;
        if (titleFont) txtMoriste.font = titleFont;

        txtSubtitulo = MakeTMP(gameOverPanel.transform,"",13,new Color(0.17f,0.03f,0.03f),FontStyles.Normal);
        PlaceRect(txtSubtitulo.rectTransform, new Vector2(0.5f,0.5f), new Vector2(0,18), new Vector2(500,24));
        txtSubtitulo.alignment = TextAlignmentOptions.Center;
        if (uiFont) txtSubtitulo.font = uiFont;

        var lin = MakeImg(gameOverPanel.transform, new Color(0.3f,0,0,0.5f), "Lin");
        PlaceRect(lin.rectTransform, new Vector2(0.5f,0.5f), new Vector2(0,-10), new Vector2(200,1));

        string[] gl = {"INTENTAR DE NUEVO","MENU PRINCIPAL"};
        for (int i = 0; i < gl.Length; i++) MakeGOBtn(gameOverPanel.transform, gl[i], new Vector2(0,-40f-i*52f), i);
    }

    void MakeGOBtn(Transform parent, string label, Vector2 pos, int index)
    {
        var go = new GameObject("GOBtn"+index);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(280,42);

        var bg = go.AddComponent<Image>(); bg.color = new Color(0.03f,0,0,0.8f);
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor = new Color(0.03f,0,0,0.8f); cb.highlightedColor = new Color(0.1f,0,0,0.95f);
        btn.colors = cb;

        var txt = MakeTMP(go.transform, label, 12,
            index==0?new Color(0.53f,0.07f,0.07f):new Color(0.27f,0.05f,0.05f), FontStyles.Normal);
        Stretch(txt.rectTransform); txt.alignment = TextAlignmentOptions.Center;
        if (uiFont) txt.font = uiFont;

        int idx = index;
        btn.onClick.AddListener(() => { Time.timeScale=1f; StartCoroutine(LoadScene(idx==0?gameSceneName:mainMenuSceneName)); });
    }

    // ── API publica ──────────────────────────────────────
    public void ShowGameOver()
    {
        if (txtSubtitulo) txtSubtitulo.text = subtitulos[Random.Range(0,subtitulos.Length)];
        gameOverPanel.SetActive(true);
        hudPanel.SetActive(false);
        StartCoroutine(GameOverSeq());
    }

    public void TogglePause()   { if (isPaused) Reanudar(); else Pausar(); }
    public void SetSalud(float v)    { _salud    = Mathf.Clamp01(v); }
    public void SetCordura(float v)  { _cordura  = Mathf.Clamp01(v); }
    public void SetLinterna(float v) { _linterna = Mathf.Clamp01(v); }
    public void SetRuido(bool b)     { _ruidoActivo = b; }

    // ── Update ───────────────────────────────────────────
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool esMenu = scene.name == mainMenuSceneName;
        if (mainMenuPanel) mainMenuPanel.SetActive(esMenu);
        if (hudPanel)      hudPanel.SetActive(!esMenu);
        if (pausePanel)    pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (flashImage)    flashImage.color = new Color(0,0,0,0);
        isPaused = false;
        Time.timeScale = 1f;

        // Mostrar/ocultar capa de sangre segun la escena
        var bloodLayer = transform.Find("EchoesCanvas/BloodLayer");
        if (bloodLayer) bloodLayer.gameObject.SetActive(esMenu);

        if (esMenu)
        {
            StartCoroutine(EntryFade());
            StartCoroutine(GlitchLoop());
            StartCoroutine(EyesLoop());
            StartCoroutine(FlashLoop());
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && hudPanel != null && hudPanel.activeSelf)
            TogglePause();
        if (hudPanel != null && hudPanel.activeSelf) UpdateHUD();
    }

    void UpdateHUD()
    {
        if (saludFill)
        {
            saludFill.fillAmount = Mathf.Lerp(saludFill.fillAmount, _salud, Time.deltaTime*5f);
            saludFill.color = Color.Lerp(new Color(0.6f,0,0), new Color(0.22f,0.22f,0.22f), _salud);
        }
        if (corduraFill)
        {
            corduraFill.fillAmount = Mathf.Lerp(corduraFill.fillAmount, _cordura, Time.deltaTime*4f);
            corduraFill.color = Color.Lerp(new Color(0.35f,0,0.35f), new Color(0.15f,0.15f,0.15f), _cordura);
        }
        if (linternaFill)
            linternaFill.fillAmount = Mathf.Lerp(linternaFill.fillAmount, _linterna, Time.deltaTime*3f);

        if (txtRuido)
        {
            txtRuido.text = _ruidoActivo ? "RUIDO DETECTADO" : "";
            if (_ruidoActivo)
                txtRuido.color = new Color(0.5f,0.1f,0.1f, Mathf.PingPong(Time.time*2f,1f)*0.5f+0.5f);
        }
    }

    // ── Pausa ────────────────────────────────────────────
    void Pausar()
    {
        isPaused = true; Time.timeScale = 0f;
        pausePanel.SetActive(true);
        StartCoroutine(FadeGroup(pauseGroup, 0f, 1f, 0.25f));
    }

    void Reanudar()
    {
        StartCoroutine(FadeGroup(pauseGroup, 1f, 0f, 0.2f, () => {
            pausePanel.SetActive(false); isPaused = false; Time.timeScale = 1f;
        }));
    }

    // ── Corrutinas ───────────────────────────────────────
    IEnumerator GameOverSeq()
    {
        Time.timeScale = 0f;
        if (bloodOverlay)
        {
            float t = 0f;
            while (t < 1f) { t += Time.unscaledDeltaTime*1.2f; bloodOverlay.color = new Color(0.35f,0,0,Mathf.Lerp(0f,0.65f,t)); yield return null; }
        }
        yield return new WaitForSecondsRealtime(0.35f);
        if (flashImage) { flashImage.color=new Color(1,1,1,0.8f); yield return new WaitForSecondsRealtime(0.07f); flashImage.color=new Color(1,1,1,0f); }
        var cg = gameOverPanel.GetComponent<CanvasGroup>();
        if (cg) yield return StartCoroutine(FadeGroup(cg, 0f, 1f, 0.6f));
        if (txtMoriste) yield return StartCoroutine(TypeText(txtMoriste, "MORISTE", 0.08f));
    }

    IEnumerator GlitchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(6f,13f));
            for (int i = 0; i < glitchEchoes.Length; i++)
            {
                if (txtEchoes)  txtEchoes.text  = glitchEchoes[i];
                if (txtSilence) txtSilence.text = glitchSilence[i];
                yield return new WaitForSeconds(0.065f);
            }
            if (txtEchoes)  txtEchoes.text  = "ECHOES";
            if (txtSilence) txtSilence.text = "SILENCE";
        }
    }

    IEnumerator EyesLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(8f,18f));
            if (eyesObj) eyesObj.SetActive(true);
            yield return new WaitForSeconds(Random.Range(0.6f,1.2f));
            if (eyesObj) eyesObj.SetActive(false);
        }
    }

    IEnumerator FlashLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f,6f));
            if (flashImage)
            {
                flashImage.color=new Color(1,1,1,0.05f); yield return new WaitForSeconds(0.05f);
                flashImage.color=new Color(1,1,1,0f);    yield return new WaitForSeconds(0.12f);
                flashImage.color=new Color(1,1,1,0.03f); yield return new WaitForSeconds(0.04f);
                flashImage.color=new Color(1,1,1,0f);
            }
        }
    }

    IEnumerator EntryFade()
    {
        if (!flashImage) yield break;
        flashImage.color = new Color(0,0,0,1f);
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime*1.5f; flashImage.color = new Color(0,0,0,1f-t); yield return null; }
        flashImage.color = new Color(0,0,0,0f);
    }

    IEnumerator LoadScene(string name)
    {
        float t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime*2f; if(flashImage) flashImage.color=new Color(0,0,0,t); yield return null; }
        // Ocultar todos los paneles antes de cargar
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (hudPanel)      hudPanel.SetActive(false);
        if (pausePanel)    pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        SceneManager.LoadScene(name);
        // Cuando cargue la nueva escena, mostrar el panel correcto
        bool esMenu = name == mainMenuSceneName;
        if (mainMenuPanel) mainMenuPanel.SetActive(esMenu);
        if (hudPanel)      hudPanel.SetActive(!esMenu);
    }

    IEnumerator QuitSeq()
    {
        float t = 0f;
        while (t < 1f) { t += Time.unscaledDeltaTime*2f; if(flashImage) flashImage.color=new Color(0,0,0,t); yield return null; }
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur, System.Action done=null)
    {
        if (!cg) yield break;
        float t = 0f; cg.alpha = from;
        while (t < 1f) { t += Time.unscaledDeltaTime/dur; cg.alpha = Mathf.Lerp(from,to,t); yield return null; }
        cg.alpha = to; done?.Invoke();
    }

    IEnumerator TypeText(TextMeshProUGUI tmp, string text, float delay)
    {
        tmp.text = "";
        foreach (char ch in text) { tmp.text += ch; yield return new WaitForSecondsRealtime(delay); }
    }

    // ── Helpers UI ───────────────────────────────────────
    GameObject NewPanel(Transform parent, Color col, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        Stretch(go.AddComponent<RectTransform>());
        var img = go.AddComponent<Image>(); img.color = col; img.raycastTarget = col.a > 0.01f;
        return go;
    }

    Image MakeFullImg(Transform parent, Color col, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        Stretch(go.AddComponent<RectTransform>());
        var img = go.AddComponent<Image>(); img.color = col; img.raycastTarget = false;
        return img;
    }

    Image MakeImg(Transform parent, Color col, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>(); img.color = col; img.raycastTarget = false;
        return img;
    }

    TextMeshProUGUI MakeTMP(Transform parent, string text, float size, Color col, FontStyles style)
    {
        string sn = text.Length > 8 ? text.Substring(0,8) : text;
        var go = new GameObject("T_"+sn); go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text=text; tmp.fontSize=size; tmp.color=col; tmp.fontStyle=style; tmp.raycastTarget=false;
        return tmp;
    }

    void Stretch(RectTransform rt)
    { rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero; }

    void PlaceRect(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    { rt.anchorMin=rt.anchorMax=anchor; rt.anchoredPosition=pos; rt.sizeDelta=size; }
}
