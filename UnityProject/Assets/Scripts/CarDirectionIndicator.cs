using UnityEngine;

public class CarDirectionIndicator : MonoBehaviour
{
    [Header("Arrow Settings")]
    public Color arrowColor = Color.yellow;
    public Vector3 localOffset = new Vector3(0, 0, 0.55f);
    public Vector3 localScale = new Vector3(0.15f, 0.12f, 0.2f);

    void Start()
    {
        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrow.transform.SetParent(transform);
        arrow.transform.localPosition = localOffset;
        arrow.transform.localRotation = Quaternion.identity;
        arrow.transform.localScale = localScale;

        Renderer r = arrow.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Standard"));
            r.material.color = arrowColor;
        }
    }
}
