using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Este script se agrega automáticamente por EchoesUIManager
// NO lo agregues manualmente al Inspector
[RequireComponent(typeof(RawImage))]
public class BloodSystem : MonoBehaviour
{
    private Texture2D tex;
    private RawImage ri;
    private Color32[] pixels;
    private int W, H;

    private class Stream
    {
        public float x, y, speed, width;
        public byte r;
        public byte alpha;
        public List<Vector2> tail = new List<Vector2>();
        public int maxTail;
        public bool done;
    }

    private List<Stream> streams = new List<Stream>();
    private List<SplatData> splats = new List<SplatData>();
    private List<PuddleData> puddles = new List<PuddleData>();

    private struct SplatData { public float x, y, radius; public byte r, a; }
    private struct PuddleData { public float x, w, maxW; public byte r, a; }

    private float spawnTimer;
    private int spawnCol;

    void Start()
    {
        ri = GetComponent<RawImage>();
        ri.raycastTarget = false;
        ri.color = Color.white;

        W = Screen.width;
        H = Screen.height;
        tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        ri.texture = tex;
        pixels = new Color32[W * H];

        // Manchas estáticas de fondo
        for (int i = 0; i < 28; i++)
            AddSplat(Random.Range(0, W), Random.Range(0, H),
                     Random.Range(3f, 16f),
                     (byte)Random.Range(90, 140),
                     (byte)Random.Range(8, 35));

        // Streams iniciales cubriendo todo el ancho
        int n = 34;
        for (int i = 0; i < n; i++)
        {
            float x = (float)i / n * W + Random.Range(-W / (float)n * 0.3f, W / (float)n * 0.3f);
            SpawnStream(x, -Random.Range(0f, H * 0.7f));
        }
    }

    void Update()
    {
        System.Array.Clear(pixels, 0, pixels.Length);

        // Manchas estáticas
        foreach (var s in splats)
            DrawSplat(Mathf.RoundToInt(s.x), Mathf.RoundToInt(H - s.y), s.radius, s.r, s.a);

        // Charcos
        for (int i = 0; i < puddles.Count; i++)
        {
            var p = puddles[i];
            float newW = Mathf.Min(p.w + 0.04f, p.maxW);
            DrawPuddle(Mathf.RoundToInt(p.x), newW, p.r, p.a);
            puddles[i] = new PuddleData { x = p.x, w = newW, maxW = p.maxW, r = p.r, a = p.a };
        }

        // Spawn nuevos streams
        spawnTimer += Time.deltaTime;
        if (spawnTimer > 0.45f)
        {
            spawnTimer = 0f;
            float x = (float)(spawnCol % 34) / 34f * W + Random.Range(-8f, 8f);
            SpawnStream(x, -20f);
            spawnCol++;
            if (Random.value < 0.35f)
                SpawnStream(Random.Range(0f, W), -20f);
        }

        // Actualizar y dibujar streams
        for (int i = streams.Count - 1; i >= 0; i--)
        {
            Stream s = streams[i];
            if (s.done) { streams.RemoveAt(i); continue; }

            s.y += s.speed * Time.deltaTime;
            s.tail.Add(new Vector2(s.x, s.y)); // X fija, solo baja en Y
            if (s.tail.Count > s.maxTail) s.tail.RemoveAt(0);

            DrawStream(s);

            if (s.y > H + 30f)
            {
                AddSplat(s.x, 6f, s.width * 2.5f + Random.Range(0f, 7f), s.r, (byte)(s.alpha * 0.4f));
                puddles.Add(new PuddleData { x = s.x, w = s.width * 2f, maxW = s.width * 10f, r = s.r, a = (byte)(s.alpha * 0.55f) });
                s.done = true;
            }
        }

        // Charco base degradado en el fondo
        for (int x = 0; x < W; x++)
            for (int y = 0; y < 20; y++)
                SetPixel(x, y, new Color32(50, 0, 0, (byte)(y * 3)));

        tex.SetPixels32(pixels);
        tex.Apply();
    }

    void SpawnStream(float x, float startY)
    {
        if (streams.Count >= 50) return;
        Stream s = new Stream();
        s.x = Mathf.Clamp(x, 0, W);
        s.y = startY;
        s.speed = Random.Range(25f, 80f);
        s.width = Random.Range(1.2f, 5f);
        s.r = (byte)Random.Range(90, 145);
        s.alpha = (byte)Random.Range(100, 230);
        s.maxTail = Random.Range(80, 130);
        streams.Add(s);
    }

    void DrawStream(Stream s)
    {
        if (s.tail.Count < 2) return;
        for (int i = 1; i < s.tail.Count; i++)
        {
            float prog = (float)i / s.tail.Count;
            float lw = s.width * (0.3f + prog * 0.7f);
            byte a = (byte)(s.alpha * prog * 0.88f);
            Color32 c = new Color32(s.r, 0, 0, a);
            int x0 = Mathf.RoundToInt(s.tail[i - 1].x);
            int y0 = Mathf.RoundToInt(H - s.tail[i - 1].y);
            int x1 = Mathf.RoundToInt(s.tail[i].x);
            int y1 = Mathf.RoundToInt(H - s.tail[i].y);
            DrawThickLine(x0, y0, x1, y1, Mathf.RoundToInt(lw * 0.5f), c);
        }
        if (s.tail.Count > 0)
        {
            Vector2 tip = s.tail[s.tail.Count - 1];
            DrawTear(Mathf.RoundToInt(tip.x), Mathf.RoundToInt(H - tip.y), s.width, s.r, s.alpha);
        }
    }

    void DrawTear(int cx, int cy, float w, byte r, byte alpha)
    {
        int hw = Mathf.Max(1, Mathf.RoundToInt(w * 0.6f));
        int len = Mathf.RoundToInt(w * 3.5f);
        for (int dy = -len; dy <= len / 2; dy++)
        {
            float t = Mathf.Clamp01((float)(dy + len) / (len * 1.5f));
            float norm = (float)dy / (len * 0.6f);
            float rx = hw * Mathf.Sqrt(Mathf.Max(0f, 1f - norm * norm));
            byte a = (byte)(alpha * Mathf.Lerp(0.35f, 1f, t));
            for (int dx = -(int)rx; dx <= (int)rx; dx++)
                SetPixel(cx + dx, cy + dy, new Color32(r, 0, 0, a));
        }
    }

    void DrawSplat(int cx, int cy, float maxR, byte r, byte alpha)
    {
        int ir = Mathf.RoundToInt(maxR);
        for (int dy = -ir; dy <= Mathf.RoundToInt(ir * 0.5f); dy++)
        for (int dx = -ir; dx <= ir; dx++)
        {
            float d = Mathf.Sqrt(dx * dx + (dy * 2f) * (dy * 2f));
            if (d <= ir)
            {
                byte a = (byte)(alpha * (1f - d / ir) * 0.75f);
                SetPixel(cx + dx, cy + dy, new Color32(r, 0, 0, a));
            }
        }
        for (int j = 0; j < 5; j++)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float dist = maxR * Random.Range(0.9f, 1.9f);
            int sx = cx + Mathf.RoundToInt(Mathf.Cos(ang) * dist);
            int sy = cy + Mathf.RoundToInt(Mathf.Sin(ang) * dist * 0.4f);
            int sr = Mathf.RoundToInt(Random.Range(1f, 4f));
            byte a = (byte)(alpha * 0.45f);
            for (int dy = -sr; dy <= sr; dy++)
            for (int dx = -sr; dx <= sr; dx++)
                if (dx * dx + dy * dy <= sr * sr)
                    SetPixel(sx + dx, sy + dy, new Color32(r, 0, 0, a));
        }
    }

    void DrawPuddle(int cx, float pw, byte r, byte alpha)
    {
        int ow = Mathf.RoundToInt(pw);
        int oh = Mathf.Max(1, Mathf.RoundToInt(pw * 0.18f));
        for (int dy = -oh; dy <= oh; dy++)
        for (int dx = -ow; dx <= ow; dx++)
        {
            float ex = (float)dx / ow;
            float ey = (float)dy / oh;
            float d = ex * ex + ey * ey;
            if (d <= 1f)
            {
                byte a = (byte)(alpha * (1f - d) * 0.7f);
                SetPixel(cx + dx, oh + dy, new Color32(r, 0, 0, a));
            }
        }
    }

    void DrawThickLine(int x0, int y0, int x1, int y1, int hw, Color32 c)
    {
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy, steps = 0;
        while (steps++ < 600)
        {
            for (int ox = -hw; ox <= hw; ox++)
            for (int oy = -hw; oy <= hw; oy++)
                if (ox * ox + oy * oy <= hw * hw)
                    SetPixel(x0 + ox, y0 + oy, c);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx)  { err += dx; y0 += sy; }
        }
    }

    void AddSplat(float x, float y, float r, byte rc, byte a)
        => splats.Add(new SplatData { x = x, y = y, radius = r, r = rc, a = a });

    void SetPixel(int x, int y, Color32 c)
    {
        if (x < 0 || x >= W || y < 0 || y >= H) return;
        int idx = y * W + x;
        Color32 e = pixels[idx];
        float ea = e.a / 255f, na = c.a / 255f;
        float fa = ea + na * (1f - ea);
        if (fa <= 0f) return;
        pixels[idx] = new Color32(
            (byte)((e.r * ea + c.r * na * (1f - ea)) / fa),
            (byte)((e.g * ea + c.g * na * (1f - ea)) / fa),
            (byte)((e.b * ea + c.b * na * (1f - ea)) / fa),
            (byte)(fa * 255));
    }

    void OnDestroy() { if (tex) Destroy(tex); }
}
