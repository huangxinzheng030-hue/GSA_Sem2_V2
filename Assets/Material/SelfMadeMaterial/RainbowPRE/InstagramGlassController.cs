using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class InstagramGlassController : MonoBehaviour
{
    [Header("References")]
    public Renderer targetRenderer;

    [Header("Texture Animation")]
    [Range(32, 512)] public int textureSize = 128;
    [Range(0.01f, 0.2f)] public float updateInterval = 0.03f;
    public float colorSpeed = 0.2f;
    public float hueScaleX = 0.65f;
    public float hueScaleY = 0.15f;
    public float waveStrength = 0.08f;
    public float waveFrequency = 6f;

    [Header("Material Look")]
    [Range(0f, 1f)] public float alpha = 0.32f;
    [Range(0f, 1f)] public float metallic = 1f;
    [Range(0f, 1f)] public float smoothness = 0f; // Roughness = 1 -> Smoothness = 0

    [Header("Emission")]
    [ColorUsage(true, true)]
    public Color emissionTint = Color.white;
    [Range(0f, 5f)] public float emissionIntensity = 1.2f;

    private Material runtimeMat;
    private Texture2D gradientTex;
    private Color[] pixels;
    private float timer;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        runtimeMat = targetRenderer.material;

        gradientTex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        gradientTex.wrapMode = TextureWrapMode.Clamp;
        gradientTex.filterMode = FilterMode.Bilinear;

        pixels = new Color[textureSize * textureSize];

        runtimeMat.SetTexture("_BaseMap", gradientTex);
        runtimeMat.SetTexture("_EmissionMap", gradientTex);
        runtimeMat.EnableKeyword("_EMISSION");

        ApplyMaterialSettings();
        UpdateGradientTexture(0f);
    }

    void Update()
    {
        if (runtimeMat == null || gradientTex == null)
            return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateGradientTexture(Time.time);
        }

        ApplyMaterialSettings();
    }

    void ApplyMaterialSettings()
    {
        // URP/Lit 常用属性
        runtimeMat.SetFloat("_Metallic", metallic);
        runtimeMat.SetFloat("_Smoothness", smoothness);

        // 纹理提供颜色，BaseColor 主要控制整体透明度
        runtimeMat.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));

        // 发光
        runtimeMat.SetColor("_EmissionColor", emissionTint * emissionIntensity);
    }

    void UpdateGradientTexture(float timeValue)
    {
        int index = 0;

        for (int y = 0; y < textureSize; y++)
        {
            float v = (float)y / (textureSize - 1);

            for (int x = 0; x < textureSize; x++)
            {
                float u = (float)x / (textureSize - 1);

                float wave = Mathf.Sin((u + v + timeValue * colorSpeed) * waveFrequency * Mathf.PI * 2f) * waveStrength;

                float hue = Mathf.Repeat(
                    u * hueScaleX +
                    v * hueScaleY +
                    timeValue * colorSpeed +
                    wave,
                    1f
                );

                Color c = Color.HSVToRGB(hue, 0.9f, 1f);
                pixels[index++] = c;
            }
        }

        gradientTex.SetPixels(pixels);
        gradientTex.Apply(false, false);
    }

    void OnDestroy()
    {
        if (gradientTex != null)
            Destroy(gradientTex);
    }
}