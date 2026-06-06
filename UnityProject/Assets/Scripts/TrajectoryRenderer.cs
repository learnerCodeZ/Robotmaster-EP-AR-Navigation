using System.Collections.Generic;
using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour
{
    [Header("Trajectory Settings")]
    public LineRenderer lineRenderer;
    public float sampleInterval = 0.2f;
    public int maxPoints = 5000;
    public int trimAmount = 1000;

    private List<Vector3> trajectoryPoints = new List<Vector3>();
    private float timer;

    void Start()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.02f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= sampleInterval)
        {
            timer = 0f;
            AddPoint(transform.position);
        }
    }

    void AddPoint(Vector3 point)
    {
        trajectoryPoints.Add(point);

        if (trajectoryPoints.Count > maxPoints)
        {
            trajectoryPoints.RemoveRange(0, trimAmount);
        }

        lineRenderer.positionCount = trajectoryPoints.Count;
        lineRenderer.SetPositions(trajectoryPoints.ToArray());
    }

    public void ClearTrajectory()
    {
        trajectoryPoints.Clear();
        lineRenderer.positionCount = 0;
    }
}
