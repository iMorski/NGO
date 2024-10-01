using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        Instance = this;

        SetFPS();
    }
    
    public Vector3 CalculatePosition(Vector3 Position, Unit Unit, Vector2 Input)
    {
        Vector3 Point = Position + Unit.Speed * GetTimeSinceUpdate() * new Vector3(
            Input.x, 0.0f, Input.y);

        if (Collision(Position, Point, Unit.CollisionDistance))
        {
            if (Input.x == 0 || Input.y == 0) return Position;
            
            // Create Point Group

            List<Vector3> CollisionCheckPointGroup = new List<Vector3>();

            for (int i = 0; i < Unit.CollisionCheckCount; i++)
            {
                Vector3 CollisionCheckPoint = new Vector3(
                    Position.x + Mathf.Cos(Mathf.PI * i * 2 / Unit.CollisionCheckCount), 0.0f,
                    Position.z + Mathf.Sin(Mathf.PI * i * 2 / Unit.CollisionCheckCount));

                CollisionCheckPointGroup.Add(CollisionCheckPoint);
            }

            // Check Point Group

            List<Vector3> CollisionCheckPointGroupAvailable = new List<Vector3>();

            foreach (Vector3 CollisionCheckPoint in CollisionCheckPointGroup)
            {
                if (!Collision(Position, CollisionCheckPoint, Unit.CollisionDistance))
                    CollisionCheckPointGroupAvailable.Add(CollisionCheckPoint);
            }

            // Get Point

            Vector3 CheckPoint = CollisionCheckPointGroupAvailable[0];

            for (int i = 1; i < CollisionCheckPointGroupAvailable.Count; i++)
            {
                if (Vector3.Distance(CollisionCheckPointGroupAvailable[i], Point) <
                    Vector3.Distance(CheckPoint, Point)) CheckPoint = CollisionCheckPointGroupAvailable[i];
            }

            CheckPoint = CheckPoint - Position;
            CheckPoint = Position + Unit.Speed * GetTimeSinceUpdate() * CheckPoint;

            return CheckPoint;
        }
        
        return Point;
    }

    bool Collision(Vector3 Position, Vector3 Point, float CollisionDistance)
    {
        return Physics.Raycast(Position, Vector3.Normalize(
            Point - Position), CollisionDistance);
    }
    
    public GameObject GetUnitGO(Unit Unit)
    {
        return Unit.gameObject;
    }

    public bool Zero(Vector3 Value)
    {
        return !(Value != new Vector3());
    }

    public float GetTimeSinceUpdate()
    {
        return Time.fixedDeltaTime;
    }

    public long GetTime()
    {
        return DateTimeOffset.Now.ToUniversalTime().ToUnixTimeMilliseconds();
    }

    private void SetFPS()
    {
        Application.targetFrameRate =
            (int) Screen.currentResolution.refreshRateRatio.value;
    }
}
