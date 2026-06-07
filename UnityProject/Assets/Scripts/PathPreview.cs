using UnityEngine;

public class PathPreview : MonoBehaviour
{
    [Header("Line Settings")]
    public Color previewColor = new Color(0.3f, 0.6f, 1.0f, 0.5f);
    public float lineHeight = 0.51f;
    public int resolution = 20;
    public float lineWidth = 0.03f;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = previewColor;
        lineRenderer.endColor = previewColor;
        lineRenderer.useWorldSpace = true;
    }

    public void ShowPath(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = resolution;
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            point.y = lineHeight;
            lineRenderer.SetPosition(i, point);
        }
    }

    public void ClearPath()
    {
        lineRenderer.positionCount = 0;
    }
}
