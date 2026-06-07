using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;

public class RosPublishCarPose : MonoBehaviour
{
    [Header("ROS Settings")]
    public string poseTopic = "/car_pose";
    public string frameId = "map";
    public float publishRate = 0.1f;

    private ROSConnection ros;
    private float timer;

    public static float LastPublishTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(poseTopic);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= publishRate)
        {
            timer = 0f;
            PublishPose();
        }
    }

    void PublishPose()
    {
        Vector3 rosPos = RosUtils.UnityToRosPosition(transform.position);
        Quaternion rosRot = RosUtils.UnityToRosRotation(transform.rotation);

        PoseMsg pose = new PoseMsg
        {
            position = new PointMsg(rosPos.x, rosPos.y, rosPos.z),
            orientation = new QuaternionMsg(rosRot.x, rosRot.y, rosRot.z, rosRot.w)
        };

        PoseStampedMsg msg = new PoseStampedMsg
        {
            header = new HeaderMsg
            {
                frame_id = frameId,
                stamp = RosUtils.GetRosTimeNow()
            },
            pose = pose
        };

        ros.Publish(poseTopic, msg);
        LastPublishTime = Time.time;
        Debug.Log($"[CarPose] published → ROS({rosPos.x:F2}, {rosPos.y:F2}, {rosPos.z:F2})");
    }
}
