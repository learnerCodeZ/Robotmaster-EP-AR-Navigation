using UnityEngine;
using RosMessageTypes.BuiltinInterfaces;

public static class RosUtils
{
    public static Vector3 UnityToRosPosition(Vector3 unityPos)
    {
        return new Vector3(unityPos.z, -unityPos.x, unityPos.y);
    }

    public static Vector3 RosToUnityPosition(Vector3 rosPos)
    {
        return new Vector3(-rosPos.y, rosPos.z, rosPos.x);
    }

    public static Quaternion UnityToRosRotation(Quaternion unityRot)
    {
        Vector3 euler = unityRot.eulerAngles;
        return Quaternion.Euler(euler.z, -euler.x, euler.y);
    }

    public static Quaternion RosToUnityRotation(Quaternion rosRot)
    {
        Vector3 euler = rosRot.eulerAngles;
        return Quaternion.Euler(-euler.y, euler.z, euler.x);
    }

    public static TimeMsg GetRosTimeNow()
    {
        System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        System.TimeSpan span = System.DateTime.UtcNow - epoch;
        return new TimeMsg
        {
            sec = (uint)span.TotalSeconds,
            nanosec = (uint)(span.Ticks % System.TimeSpan.TicksPerSecond) * 100
        };
    }
}
