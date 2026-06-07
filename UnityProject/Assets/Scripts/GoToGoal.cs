using UnityEngine;

public class GoToGoal : MonoBehaviour
{
    [Header("Navigation")]
    public float arrivalDistance = 0.3f;
    public float maxSpeed = 1.5f;
    public float turnSpeed = 2.0f;
    public float linearSpeed = 1.0f;

    private VirtualCarController carController;
    private PathPreview pathPreview;
    private Vector3? currentGoal;
    private bool isNavigating;

    void Start()
    {
        carController = GetComponent<VirtualCarController>();
        pathPreview = GetComponent<PathPreview>();
        if (pathPreview == null)
            pathPreview = gameObject.AddComponent<PathPreview>();
    }

    void Update()
    {
        if (NavGoalMarker.LastGoal != null)
        {
            currentGoal = NavGoalMarker.LastGoal.Value;
            NavGoalMarker.LastGoal = null;
            isNavigating = true;
            pathPreview.ShowPath(transform.position, currentGoal.Value);
            Debug.Log($"[GoToGoal] Target set: ({currentGoal.Value.x:F2}, {currentGoal.Value.z:F2})");
        }

        if (!isNavigating || currentGoal == null)
        {
            carController.SetVelocity(0, 0);
            return;
        }

        Vector3 goal = currentGoal.Value;
        Vector3 dir = goal - transform.position;
        dir.y = 0;
        float distance = dir.magnitude;

        if (distance < arrivalDistance)
        {
            Debug.Log($"[GoToGoal] Arrived! distance={distance:F2}");
            pathPreview.ClearPath();
            isNavigating = false;
            currentGoal = null;
            carController.SetVelocity(0, 0);
            return;
        }

        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.y;
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

        float angularZ = 0;
        if (Mathf.Abs(angleDiff) > 5f)
            angularZ = Mathf.Sign(angleDiff) * turnSpeed;
        else
            angularZ = 0;

        float linearX = linearSpeed;

        Debug.Log($"[GoToGoal] dist={distance:F2} angle={angleDiff:F1} → vel=({linearX:F2}, {angularZ:F2})");
        carController.SetVelocity(linearX, angularZ);
    }
}
