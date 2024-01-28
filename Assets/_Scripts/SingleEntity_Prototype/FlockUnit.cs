using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.Vector3;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;

namespace Kaizerwald
{
    public class FlockUnit : MonoBehaviour
    {
        [SerializeField] private float FOVAngle;
        [SerializeField] private float smoothDamp;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private Vector3[] directionsToCheckWhenAvoidingObstacles;

        private List<FlockUnit> cohesionNeighbours = new (10);
        private List<FlockUnit> avoidanceNeighbours = new (10);
        private List<FlockUnit> alignmentNeighbours = new (10);
        
        private Flock assignedFlock;
        private float speed;
        private Vector2 currentVelocity;
        private Vector2 currentObstacleAvoidanceVector;

        public Transform myTransform { get; private set; }
        public Vector3 Position => myTransform.position;
        public Vector3 Forward => myTransform.forward;
        public Vector2 Position2D => new (Position.x,Position.z);
        public Vector2 Forward2D => new (Forward.x,Forward.z);
        
        private void Awake()
        {
            myTransform = transform;
        }

        public void AssignFlock(Flock flock) => assignedFlock = flock;
        public void InitializeSpeed(float value) => speed = value;
        private bool IsInFOV(Vector3 position) => Angle(Forward, position - Position) <= FOVAngle;
        
        public void MoveUnit()
        {
            FindNeighbours();
            CalculateSpeed();

            Vector2 cohesionVector  = CalculateCohesionVector() * assignedFlock.CohesionWeight;
            Vector2 avoidanceVector = CalculateAvoidanceVector() * assignedFlock.AvoidanceWeight;
            Vector2 alignmentVector = CalculateAlignmentVector() * assignedFlock.AlignmentWeight;
            Vector2 boundsVector    = CalculateBoundsVector() * assignedFlock.BoundsWeight;
            Vector2 obstacleVector  = CalculateObstacleVector() * assignedFlock.ObstacleWeight;
            
            //Debug.Log($"{name} cohesionVector = {cohesionVector} | alignmentVector = {alignmentVector} | boundsVector = {boundsVector}");
            //Debug.Log($"{name} obstacleVector = {obstacleVector}");
            
            Vector2 moveVector = cohesionVector + avoidanceVector + alignmentVector + boundsVector + obstacleVector;
            moveVector = Vector2.SmoothDamp(Forward2D, moveVector, ref currentVelocity, smoothDamp).normalized * speed;
            //moveVector = moveVector == Vector2.zero ? Forward2D : moveVector;
            Vector3 moveDirection = moveVector == Vector2.zero ? Forward : new Vector3(moveVector.x, 0, moveVector.y);
            myTransform.forward = moveDirection;
            myTransform.position += moveDirection * Time.deltaTime;
        }
        
        private void FindNeighbours()
        {
            cohesionNeighbours.Clear();
            avoidanceNeighbours.Clear();
            alignmentNeighbours.Clear();
            FlockUnit[] allUnits = assignedFlock.allUnits;
            foreach (FlockUnit currentUnit in allUnits)
            {
                if (currentUnit == this) continue;
                float currentNeighbourDistanceSqr = SqrMagnitude(currentUnit.myTransform.position - myTransform.position);
                if(currentNeighbourDistanceSqr <= assignedFlock.CohesionDistance * assignedFlock.CohesionDistance)
                {
                    cohesionNeighbours.Add(currentUnit);
                }
                if (currentNeighbourDistanceSqr <= assignedFlock.AvoidanceDistance * assignedFlock.AvoidanceDistance)
                {
                    avoidanceNeighbours.Add(currentUnit);
                }
                if (currentNeighbourDistanceSqr <= assignedFlock.AligementDistance * assignedFlock.AligementDistance)
                {
                    alignmentNeighbours.Add(currentUnit);
                }
            }
        }
        
        private void CalculateSpeed()
        {
            if (cohesionNeighbours.Count == 0) return;
            speed = 0;
            foreach (FlockUnit flockUnit in cohesionNeighbours)
            {
                speed += flockUnit.speed;
            }
            speed = Clamp(speed / cohesionNeighbours.Count, assignedFlock.MinSpeed, assignedFlock.MaxSpeed);
        }
        
        private Vector2 CalculateCohesionVector()
        {
            if (cohesionNeighbours.Count == 0) return Vector2.zero;
            Vector2 cohesionVector = Vector2.zero;
            int neighboursInFOV = 0;
            foreach (FlockUnit flockUnit in cohesionNeighbours)
            {
                if (!IsInFOV(flockUnit.Position)) continue;
                cohesionVector += flockUnit.Position2D;
                neighboursInFOV++;
            }
            return neighboursInFOV == 0 ? Vector2.zero : (cohesionVector / neighboursInFOV - Position2D).normalized;
        }
        
        private Vector2 CalculateAlignmentVector()
        {
            if (alignmentNeighbours.Count == 0) return Forward2D;
            Vector2 alignmentVector = Forward2D;
            int neighboursInFOV = 0;
            foreach (FlockUnit flockUnit in alignmentNeighbours)
            {
                if (!IsInFOV(flockUnit.Position)) continue;
                alignmentVector += flockUnit.Forward2D;
                neighboursInFOV++;
            }
            return neighboursInFOV == 0 ? Vector2.zero : (alignmentVector / neighboursInFOV).normalized;
        }
        
        private Vector2 CalculateAvoidanceVector()
        {
            if (alignmentNeighbours.Count == 0) return Vector2.zero;
            Vector2 avoidanceVector = Vector2.zero;
            int neighboursInFOV = 0;
            foreach (FlockUnit flockUnit in avoidanceNeighbours)
            {
                if (!IsInFOV(flockUnit.Position)) continue;
                avoidanceVector += Position2D - flockUnit.Position2D;
                neighboursInFOV++;
            }
            return neighboursInFOV == 0 ? Vector2.zero : (avoidanceVector / neighboursInFOV).normalized;
        }
        
        private Vector2 CalculateBoundsVector()
        {
            Vector2 offsetToCenter = assignedFlock.Position2D - Position2D;
            bool isNearCenter = (offsetToCenter.magnitude >= assignedFlock.BoundsDistance * 0.9f);
            return isNearCenter ? offsetToCenter.normalized : Vector2.zero;
        }
        
        private Vector2 CalculateObstacleVector()
        {
            if(Approximately(assignedFlock.ObstacleWeight, 0)) return Vector2.zero;
            Vector2 obstacleVector = Vector2.zero;
            if (Raycast(Position, Forward, assignedFlock.ObstacleDistance, obstacleMask))
            {
                obstacleVector = FindBestDirectionToAvoidObstacle();
            }
            else
            {
                currentObstacleAvoidanceVector = Vector2.zero;
            }
            return obstacleVector;
        }
        
        private Vector2 FindBestDirectionToAvoidObstacle()
        {
            if(currentObstacleAvoidanceVector != Vector2.zero /*&& !Raycast(Position, Forward, assignedFlock.ObstacleDistance, obstacleMask)*/)
            {
                return currentObstacleAvoidanceVector;
            }
            
            float maxDistance = int.MinValue;
            Vector2 selectedDirection = Vector2.zero;
            for (int i = 0; i < directionsToCheckWhenAvoidingObstacles.Length; i++)
            {
                Vector3 currentDirection = myTransform.TransformDirection(directionsToCheckWhenAvoidingObstacles[i].normalized);
                Vector2 currentDirection2D = new (currentDirection.x, currentDirection.z);
                if(Raycast(Position, currentDirection, out RaycastHit hit, assignedFlock.ObstacleDistance, obstacleMask))
                {
                    float currentDistance = (hit.point - Position).sqrMagnitude;
                    if (!(currentDistance > maxDistance)) continue;
                    maxDistance = currentDistance;
                    selectedDirection = currentDirection2D;
                }
                else
                {
                    selectedDirection = currentDirection2D;
                    currentObstacleAvoidanceVector = currentDirection2D.normalized;
                    return selectedDirection.normalized;
                }
            }
            return selectedDirection.normalized;
        }
    }
}
