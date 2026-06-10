using UnityEngine;

public class SceneVisuals : MonoBehaviour
{
    [Header("Scene Objects")]
    public string carObjectName = "RobotCar";
    public string groundObjectName = "Ground";

    [Header("Car Colors")]
    public Color carBody = new Color(0f, 0.749f, 1f, 1f);
    public Color carAccent = new Color(1f, 0.42f, 0.21f, 1f);
    public Color carWheel = new Color(0.15f, 0.15f, 0.15f, 1f);

    [Header("Ground Colors")]
    public Color groundFill = new Color(1f, 1f, 1f, 0.06f);
    public Color groundGrid = new Color(1f, 1f, 1f, 0.18f);
    public Color groundAxis = new Color(0f, 0.8f, 1f, 0.4f);

    void Start()
    {
        SetupCamera();
        EnsureCar();
        EnsureGround();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        }
    }

    void EnsureCar()
    {
        GameObject carObj = GameObject.Find(carObjectName);
        if (carObj == null)
        {
            carObj = new GameObject(carObjectName);
        }

        CarBuilder builder = carObj.GetComponent<CarBuilder>();
        if (builder == null)
        {
            builder = carObj.AddComponent<CarBuilder>();
        }

        builder.bodyColor = carBody;
        builder.accentColor = carAccent;
        builder.wheelColor = carWheel;
        builder.Rebuild();
    }

    void EnsureGround()
    {
        GameObject groundObj = GameObject.Find(groundObjectName);
        if (groundObj == null)
        {
            groundObj = new GameObject(groundObjectName);
        }

        GroundBuilder builder = groundObj.GetComponent<GroundBuilder>();
        if (builder == null)
        {
            builder = groundObj.AddComponent<GroundBuilder>();
        }

        builder.groundColor = groundFill;
        builder.gridColor = groundGrid;
        builder.axisColor = groundAxis;
        builder.Rebuild();
    }
}
