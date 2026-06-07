using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

public class NavGoalMarker : MonoBehaviour
{
    [Header("ROS Settings")]
    public string goalTopic = "/goal_pose";

    [Header("Marker Settings")]
    public GameObject markerPrefab;
    public Color markerColor = Color.green;
    public float markerSize = 0.8f;

    public static Vector3? LastGoal;

    private ROSConnection ros;
    private GameObject currentMarker;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(goalTopic);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                LastGoal = hit.point;
                SetGoal(hit.point);
            }
        }
    }

    void SetGoal(Vector3 position)
    {
        if (currentMarker != null)
            Destroy(currentMarker);

        if (markerPrefab != null)
        {
            currentMarker = Instantiate(markerPrefab, position, Quaternion.identity);
        }
        else
        {
            currentMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentMarker.transform.position = position;
            currentMarker.transform.localScale = Vector3.one * markerSize;

            var renderer = currentMarker.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = markerColor;
        }

        Vector3 rosPos = RosUtils.UnityToRosPosition(position);

        PoseMsg pose = new PoseMsg
        {
            position = new PointMsg(rosPos.x, rosPos.y, rosPos.z),
            orientation = new QuaternionMsg(0, 0, 0, 1)
        };

        PoseStampedMsg msg = new PoseStampedMsg
        {
            header = new HeaderMsg
            {
                frame_id = "map",
                stamp = RosUtils.GetRosTimeNow()
            },
            pose = pose
        };

        ros.Publish(goalTopic, msg);
        Debug.Log($"[Goal] clicked → ROS({rosPos.x:F2}, {rosPos.y:F2}, {rosPos.z:F2})");
    }
}
