using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTrajectory : MonoBehaviour
{
    public static DrawTrajectory Instance { get; private set; }
    
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] [Range(1, 100)] private int _lineSegmentCount = 100;

    private List<Vector3> _linePoints = new List<Vector3>();

    
    
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateTrajectory(Vector3 forceVector, Rigidbody rigidBody, Vector3 startingPoint)
    {
        Vector3 velocity = (forceVector / rigidBody.mass) * Time.fixedDeltaTime;
        float FlightDuration = (2 * velocity.y) / Physics.gravity.y;
        float stepTime = FlightDuration / _lineSegmentCount;

        _linePoints.Clear();

        for (int i = 0; i < _lineSegmentCount; i++)
        {
            float stepTimePassed = stepTime * i;

            Vector3 MovementVector = new Vector3
            (
                velocity.x * stepTimePassed,
                velocity.y * stepTimePassed - 0.5f * Physics.gravity.y * stepTimePassed * stepTimePassed,
                velocity.z * stepTimePassed
            );
            RaycastHit hit;
            if (Physics.Raycast(startingPoint, -MovementVector, out hit, MovementVector.magnitude))
                break;

            _linePoints.Add(-MovementVector + startingPoint);
        }

        _lineRenderer.positionCount = _linePoints.Count;
        _lineRenderer.SetPositions(_linePoints.ToArray());
    }

    public void HideLine()
    {
        _lineRenderer.positionCount = 0;
    }
    
}
