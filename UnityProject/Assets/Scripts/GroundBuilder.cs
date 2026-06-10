using UnityEngine;

public class GroundBuilder : MonoBehaviour
{
    [Header("Ground")]
    public float radius = 5f;
    public Color groundColor = new Color(1f, 1f, 1f, 0.06f);
    public Color gridColor = new Color(1f, 1f, 1f, 0.18f);
    public Color axisColor = new Color(0f, 0.8f, 1f, 0.4f);
    public int gridLines = 16;
    public float lineWidth = 0.015f;

    private bool built = false;

    void Start()
    {
        if (!built) BuildGround();
    }

    public void BuildGround()
    {
        built = true;
        ClearChildren();

        // Ground disc
        GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.transform.SetParent(transform);
        disc.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        disc.transform.localPosition = new Vector3(0, 0, 0);
        disc.name = "GroundDisc";
        Renderer discRenderer = disc.GetComponent<Renderer>();
        Material discMat = new Material(Shader.Find("Unlit/Color"));
        discMat.color = groundColor;
        SetMaterialTransparent(discMat);
        discRenderer.material = discMat;

        // Grid lines
        float extent = radius * 0.95f;
        float step = extent * 2f / gridLines;

        for (int i = 0; i <= gridLines; i++)
        {
            float pos = -extent + i * step;
            Color lineColor = (i == gridLines / 2) ? axisColor : gridColor;
            float width = (i == gridLines / 2) ? lineWidth * 2f : lineWidth;

            // X direction
            CreateGridLine(new Vector3(-extent, 0.005f, pos), new Vector3(extent, 0.005f, pos), lineColor, width);

            // Z direction
            CreateGridLine(new Vector3(pos, 0.005f, -extent), new Vector3(pos, 0.005f, extent), lineColor, width);
        }

        // Concentric circles
        int rings = 3;
        float ringStep = extent / rings;
        for (int r = 1; r <= rings; r++)
        {
            CreateCircle(new Vector3(0, 0.005f, 0), r * ringStep, gridColor, lineWidth * 0.7f);
        }
    }

    void CreateGridLine(Vector3 start, Vector3 end, Color color, float width)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = width;
        lr.endWidth = width;
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = color;
        lr.material = mat;
    }

    void CreateCircle(Vector3 center, float radius, Color color, float width)
    {
        int segments = 32;
        GameObject circleObj = new GameObject("Circle");
        circleObj.transform.SetParent(transform);
        LineRenderer lr = circleObj.AddComponent<LineRenderer>();
        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = center.x + Mathf.Cos(angle) * radius;
            float z = center.z + Mathf.Sin(angle) * radius;
            lr.SetPosition(i, new Vector3(x, center.y, z));
        }
        lr.startWidth = width;
        lr.endWidth = width;
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = color;
        lr.material = mat;
    }

    void SetMaterialTransparent(Material mat)
    {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_ALPHABLEND_ON");
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    public void Rebuild()
    {
        built = false;
        if (isActiveAndEnabled) BuildGround();
    }
}
