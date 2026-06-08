using UnityEngine;
using UnityEngine.XR;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using MixedReality.Toolkit;

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
        if (IsAirTapPressed())
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                LastGoal = hit.point;
                SetGoal(hit.point);
            }
        }
    }

    /// <summary>
    /// Detects air tap via MRTK3 Hands Aggregator pinch.
    /// Editor: Left Ctrl + LMB (right hand). HoloLens 2: actual air tap.
    /// Falls back to mouse click if MRTK3 subsystem is unavailable.
    /// </summary>
    private bool IsAirTapPressed()
    {
        if (XRSubsystemHelpers.HandsAggregator != null)
        {
            if (XRSubsystemHelpers.HandsAggregator.TryGetPinchProgress(
                XRNode.RightHand, out bool ready, out bool pinching, out float amount))
            {
                return pinching;
            }
        }
        return Input.GetMouseButtonDown(0);
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
