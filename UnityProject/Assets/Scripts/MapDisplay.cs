using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Nav;
using RosMessageTypes.Std;

public class MapDisplay : MonoBehaviour
{
    [Header("ROS Settings")]
    public string mapTopic = "/map";

    [Header("Display Settings")]
    public RawImage mapImage;
    public GameObject mapPlane;
    public Material mapMaterial;

    private ROSConnection ros;
    private Texture2D mapTexture;
    private int mapWidth;
    private int mapHeight;
    private float mapResolution;
    private Vector3 mapOrigin;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<OccupancyGridMsg>(mapTopic, OnMapReceived);
    }

    void OnMapReceived(OccupancyGridMsg msg)
    {
        mapWidth = (int)msg.info.width;
        mapHeight = (int)msg.info.height;
        mapResolution = (float)msg.info.resolution;
        mapOrigin = new Vector3(
            (float)msg.info.origin.position.x,
            0,
            (float)msg.info.origin.position.y
        );

        Color[] colors = new Color[mapWidth * mapHeight];

        for (int i = 0; i < msg.data.Length; i++)
        {
            sbyte val = msg.data[i];
            if (val == -1)
                colors[i] = new Color(0.5f, 0.5f, 0.5f);
            else if (val == 0)
                colors[i] = Color.white;
            else
            {
                float t = Mathf.Clamp01(val / 100.0f);
                colors[i] = Color.Lerp(Color.white, Color.black, t);
            }
        }

        if (mapTexture == null || mapTexture.width != mapWidth || mapTexture.height != mapHeight)
        {
            mapTexture = new Texture2D(mapWidth, mapHeight, TextureFormat.RGBA32, false);
        }

        mapTexture.SetPixels(colors);
        mapTexture.Apply();

        if (mapImage != null)
            mapImage.texture = mapTexture;

        if (mapMaterial != null)
            mapMaterial.mainTexture = mapTexture;
    }
}
