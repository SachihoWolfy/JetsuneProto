using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(LineRenderer), typeof(SplineContainer))]
public class SplineRenderer : MonoBehaviour
{
    public Material arrowMaterial;  // Assign a material with your arrow texture
    public Material arrowGlowMaterial;
    public float textureTiling = 1f; // Adjust this for spacing between arrows
    public int resolution = 100;    // Number of points on the spline
    public bool isShowing;

    private LineRenderer lineRenderer;
    private SplineContainer splineContainer;

    void Start()
    {
        // Get components
        lineRenderer = GetComponent<LineRenderer>();
        splineContainer = GetComponent<SplineContainer>();

        // Render the spline
        RenderSpline();
        SetLineVisibility(false);
        isShowing = false;
    }

    void RenderSpline()
    {
        if (splineContainer == null || splineContainer.Spline == null || lineRenderer == null)
        {
            Debug.LogWarning("Spline or LineRenderer is missing!");
            return;
        }

        Spline spline = splineContainer.Spline;
        Vector3[] positions = new Vector3[resolution + 1];

        for (int i = 0; i <= resolution; i++)
        {
            float t = Mathf.Clamp((float)i / resolution, 0f, 1f);
            positions[i] = transform.TransformPoint(SplineUtility.EvaluatePosition(spline, t));
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);

        // Increase thickness
        lineRenderer.widthMultiplier = 5f;

        // Optional: Add dynamic thickness
        /*lineRenderer.widthCurve = new AnimationCurve(
            new Keyframe(0, 0.2f),
            new Keyframe(0.5f, 0.5f),
            new Keyframe(1, 0.2f)
        );*/

        // Set up material and texture
        lineRenderer.material = arrowMaterial;
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.sharedMaterial.mainTextureScale = new Vector2(1/(textureTiling * resolution), 1f);
    }

    public IEnumerator FadeLine(bool fadeIn, float duration)
    {
        if (lineRenderer == null || lineRenderer.material == null)
        {
            Debug.LogWarning("LineRenderer or material is not assigned!");
            yield break;
        }

        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;
        lineRenderer.material = arrowMaterial;
        Color lineColor = lineRenderer.material.color;

        // Ensure LineRenderer is enabled when fading in
        if (fadeIn) lineRenderer.enabled = true;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            lineRenderer.material.color = new Color(lineColor.r, lineColor.g, lineColor.b, alpha);
            yield return null;
        }
        lineRenderer.material = arrowGlowMaterial;
        // Ensure LineRenderer is disabled when fading out
        if (!fadeIn) lineRenderer.enabled = false;
    }
    public void SetLineVisibility(bool isVisible)
    {
        if (lineRenderer == null)
        {
            Debug.LogWarning("LineRenderer is not assigned!");
            return;
        }

        lineRenderer.enabled = isVisible;
    }
    public void ShowLine()
    {
        RenderSpline(); // Render spline data
        StartCoroutine(FadeLine(true, 0.5f));
    }

    public void HideLine()
    {
        StartCoroutine(FadeLine(false, 0.5f));
    }

    private void Update()
    {
        isShowing = lineRenderer.enabled;
    }
}