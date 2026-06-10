using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CarBuilder : MonoBehaviour
{
    [Header("Colors")]
    public Color bodyColor = new Color(0f, 0.749f, 1f, 1f);
    public Color accentColor = new Color(1f, 0.42f, 0.21f, 1f);
    public Color wheelColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    public Color wheelRimColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [Header("Size")]
    public float bodyLength = 1.2f;
    public float bodyWidth = 0.6f;
    public float bodyHeight = 0.25f;
    public float wheelRadius = 0.1f;
    public float wheelThickness = 0.06f;

    private bool built = false;

    void Start()
    {
        if (!built) BuildCar();
    }

    public void BuildCar()
    {
        built = true;
        ClearChildren();

        float wheelY = wheelRadius;
        float halfBL = bodyLength * 0.5f;
        float halfBW = bodyWidth * 0.5f;
        float bodyY = wheelY + bodyHeight * 0.5f;

        // --- Body ---
        GameObject body = CreatePart(PrimitiveType.Cube, bodyColor);
        body.transform.localScale = new Vector3(bodyLength, bodyHeight, bodyWidth);
        body.transform.localPosition = new Vector3(0, bodyY, 0);

        // --- Cabin ---
        GameObject cabin = CreatePart(PrimitiveType.Cube, accentColor);
        cabin.transform.localScale = new Vector3(bodyLength * 0.5f, bodyHeight * 0.7f, bodyWidth * 0.85f);
        cabin.transform.localPosition = new Vector3(-bodyLength * 0.1f, bodyY + bodyHeight * 0.5f + bodyHeight * 0.35f, 0);

        // --- Windshield ---
        GameObject windshield = CreatePart(PrimitiveType.Cube, new Color(0.3f, 0.6f, 1f, 0.5f));
        windshield.transform.localScale = new Vector3(bodyLength * 0.35f, bodyHeight * 0.5f, bodyWidth * 0.75f);
        windshield.transform.localPosition = new Vector3(-bodyLength * 0.1f, bodyY + bodyHeight * 0.5f + bodyHeight * 0.15f, bodyWidth * 0.43f);

        // --- Wheels ---
        float wx = bodyLength * 0.32f;
        float wz = halfBW + wheelThickness * 0.5f;
        CreateWheel(new Vector3(wx, wheelY, wz));
        CreateWheel(new Vector3(wx, wheelY, -wz));
        CreateWheel(new Vector3(-wx, wheelY, wz));
        CreateWheel(new Vector3(-wx, wheelY, -wz));

        // --- Antenna pole ---
        GameObject antenna = CreatePart(PrimitiveType.Cylinder, accentColor);
        antenna.transform.localScale = new Vector3(0.025f, 0.3f, 0.025f);
        antenna.transform.localPosition = new Vector3(bodyLength * 0.22f, bodyY + bodyHeight * 0.5f + 0.15f, 0);

        // --- Antenna tip ---
        GameObject tip = CreatePart(PrimitiveType.Sphere, Color.red);
        tip.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        tip.transform.localPosition = new Vector3(bodyLength * 0.22f, bodyY + bodyHeight * 0.5f + 0.3f, 0);

        // --- Headlights (front) ---
        GameObject hlL = CreatePart(PrimitiveType.Sphere, Color.yellow);
        hlL.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        hlL.transform.localPosition = new Vector3(halfBL + 0.02f, bodyY - 0.03f, halfBW * 0.6f);

        GameObject hlR = CreatePart(PrimitiveType.Sphere, Color.yellow);
        hlR.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        hlR.transform.localPosition = new Vector3(halfBL + 0.02f, bodyY - 0.03f, -halfBW * 0.6f);
    }

    void CreateWheel(Vector3 pos)
    {
        GameObject wheel = CreatePart(PrimitiveType.Cylinder, wheelColor);
        wheel.transform.localScale = new Vector3(wheelThickness, wheelRadius, wheelThickness);
        wheel.transform.localPosition = pos;
        wheel.transform.rotation = Quaternion.Euler(0, 0, 90);

        GameObject rim = CreatePart(PrimitiveType.Cylinder, wheelRimColor);
        rim.transform.SetParent(wheel.transform);
        rim.transform.localScale = new Vector3(wheelThickness * 1.1f, wheelRadius * 0.5f, wheelThickness * 1.1f);
        rim.transform.localPosition = Vector3.zero;
        rim.transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    GameObject CreatePart(PrimitiveType type, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.transform.SetParent(transform);
        Renderer r = obj.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = color;
        r.material = mat;
        return obj;
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
        if (isActiveAndEnabled) BuildCar();
    }
}
