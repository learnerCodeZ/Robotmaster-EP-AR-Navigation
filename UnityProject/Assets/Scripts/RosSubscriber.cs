using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class RosSubscriber : MonoBehaviour
{
    [Header("ROS Settings")]
    public string cmdVelTopic = "/cmd_vel";

    private ROSConnection ros;
    private VirtualCarController carController;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TwistMsg>(cmdVelTopic, OnCmdVelReceived);

        carController = GetComponent<VirtualCarController>();
    }

    void OnCmdVelReceived(TwistMsg msg)
    {
        float linearX = (float)msg.linear.x;
        float angularZ = (float)msg.angular.z;
        carController.SetVelocity(linearX, angularZ);
        Debug.Log($"[CmdVel] linear({linearX:F2}) angular({angularZ:F2})");
    }
}
