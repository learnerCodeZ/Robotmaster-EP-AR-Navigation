using UnityEngine;

public class SceneVisuals : MonoBehaviour
{
    [Header("Robot")]
    public string robotName = "RobotCar";
    public Color robotColor = new Color(0.2f, 0.5f, 0.9f, 1f);

    [Header("Ground")]
    public string groundName = "Ground";
    public Color groundColor = new Color(0.35f, 0.4f, 0.35f, 1f);

    [Header("Camera")]
    public Color skyColor = new Color(0.4f, 0.6f, 0.9f, 1f);

    void Start()
    {
        ApplyColor(robotName, robotColor);
        ApplyColor(groundName, groundColor);

        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = skyColor;
    }

    void ApplyColor(string objName, Color color)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj == null) return;

        Renderer r = obj.GetComponent<Renderer>();
        if (r == null) return;

        r.material = new Material(Shader.Find("Standard"));
        r.material.color = color;
    }
}
