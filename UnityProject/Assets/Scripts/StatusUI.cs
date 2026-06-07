using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Nav;

public class StatusUI : MonoBehaviour
{
    [Header("ROS Topics")]
    public string cmdVelTopic = "/cmd_vel";
    public string odomTopic = "/odom";

    [Header("Settings")]
    public float timeout = 30f;

    private ROSConnection ros;
    private float lastMsgTime;
    private string velInfo = "Waiting...";
    private string posInfo = "Waiting...";

    private Text connectionText;
    private Text velocityText;
    private Text positionText;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TwistMsg>(cmdVelTopic, OnCmdVel);
        ros.Subscribe<OdometryMsg>(odomTopic, OnOdom);

        CreateUI();
    }

    void CreateUI()
    {
        var canvasObj = new GameObject("StatusCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        connectionText = MakeText("StatusText", canvasObj.transform, 10, -10);
        velocityText = MakeText("VelocityText", canvasObj.transform, 10, -40);
        positionText = MakeText("PositionText", canvasObj.transform, 10, -70);
    }

    Text MakeText(string name, Transform parent, float x, float y)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(400, 30);

        var text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.text = "";
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(1, -1);

        return text;
    }

    void Update()
    {
        bool connected = (Time.time - RosPublishCarPose.LastPublishTime) < timeout;

        if (connectionText != null)
        {
            connectionText.text = connected ? "ROS Connected" : "ROS Disconnected";
            connectionText.color = connected ? Color.green : Color.red;
        }
        if (velocityText != null)
            velocityText.text = velInfo;
        if (positionText != null)
            positionText.text = posInfo;
    }

    void OnCmdVel(TwistMsg msg)
    {
        lastMsgTime = Time.time;
        velInfo = string.Format("Vel: lin({0:F2}) ang({1:F2})", msg.linear.x, msg.angular.z);
    }

    void OnOdom(OdometryMsg msg)
    {
        lastMsgTime = Time.time;
        posInfo = string.Format("Pos: ({0:F2}, {1:F2}, {2:F2})",
            msg.pose.pose.position.x, msg.pose.pose.position.y, msg.pose.pose.position.z);
    }
}
