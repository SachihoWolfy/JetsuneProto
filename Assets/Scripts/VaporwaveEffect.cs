using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class VaporwaveEffect : MonoBehaviour
{
    public Shader shader;
    [Range(0f, 1f)] public float edgeThreshold = 0.2f;
    private Material mat;

    void Start()
    {
        if (shader == null)
            shader = Shader.Find("Hidden/VaporwaveEdge");

        if (shader && !mat)
            mat = new Material(shader);
    }

    public Color color1 = new Color(1f, 0.4f, 0.7f);
    public Color color2 = new Color(0.4f, 1f, 1f);
    public Color color3 = new Color(0.6f, 0.3f, 1f);
    public float saturation = 1.0f;
    public Color edgeColor = Color.white;
    [Range(0f, 1f)] public float edgeStrength = 0.7f;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!mat)
        {
            Graphics.Blit(src, dest);
            return;
        }

        mat.SetFloat("_EdgeThreshold", edgeThreshold);
        mat.SetColor("_Color1", color1);
        mat.SetColor("_Color2", color2);
        mat.SetColor("_Color3", color3);
        mat.SetColor("_EdgeColor", edgeColor);
        mat.SetFloat("_EdgeStrength", edgeStrength);

        Graphics.Blit(src, dest, mat);
    }

}
