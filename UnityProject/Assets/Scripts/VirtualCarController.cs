using UnityEngine;

public class VirtualCarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxLinearSpeed = 2.0f;
    public float maxAngularSpeed = 1.5f;

    private float currentLinearX;
    private float currentAngularZ;

    void FixedUpdate()
    {
        Vector3 move = transform.forward * currentLinearX * Time.fixedDeltaTime;
        transform.Translate(move, Space.World);

        Quaternion rot = Quaternion.Euler(0, currentAngularZ * Mathf.Rad2Deg * Time.fixedDeltaTime, 0);
        transform.rotation *= rot;
    }

    public void SetVelocity(float linearX, float angularZ)
    {
        currentLinearX = Mathf.Clamp(linearX, -maxLinearSpeed, maxLinearSpeed);
        currentAngularZ = Mathf.Clamp(angularZ, -maxAngularSpeed, maxAngularSpeed);
    }
}
