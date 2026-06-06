using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;

public class RosOdomSubscriber : MonoBehaviour
{
    [Header("ROS Settings")]
    public string odomTopic = "/odom";

    private ROSConnection ros;
    private VirtualCarController carController;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<OdometryMsg>(odomTopic, OnOdomReceived);

        carController = GetComponent<VirtualCarController>();
    }

    void OnOdomReceived(OdometryMsg msg)
    {
        Vector3 rosPos = new Vector3(
            (float)msg.pose.pose.position.x,
            (float)msg.pose.pose.position.y,
            (float)msg.pose.pose.position.z
        );
        Quaternion rosRot = new Quaternion(
            (float)msg.pose.pose.orientation.x,
            (float)msg.pose.pose.orientation.y,
            (float)msg.pose.pose.orientation.z,
            (float)msg.pose.pose.orientation.w
        );

        Vector3 unityPos = RosUtils.RosToUnityPosition(rosPos);
        Quaternion unityRot = RosUtils.RosToUnityRotation(rosRot);

        transform.SetPositionAndRotation(unityPos, unityRot);
        Debug.Log($"[Odom] pos({msg.pose.pose.position.x:F2}, {msg.pose.pose.position.y:F2}) → Unity({unityPos.x:F2}, {unityPos.y:F2}, {unityPos.z:F2})");
    }
}
