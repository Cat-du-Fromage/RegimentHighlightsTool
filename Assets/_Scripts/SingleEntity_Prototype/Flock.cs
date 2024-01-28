using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kaizerwald
{
    public class Flock : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private FlockUnit flockUnitPrefab;
        [SerializeField] private int flockSize;
        [SerializeField] private Vector3 spawnBounds;
        
        [field:Header("Speed Setup")]
        [field:SerializeField, Range(0, 10)] public float MinSpeed { get; private set; }
        [field:SerializeField, Range(0, 10)] public float MaxSpeed { get; private set; }
        
        [field:Header("Detection Distances")]
        [field:SerializeField, Range(0, 10)] public float CohesionDistance { get; private set; }
        [field:SerializeField, Range(0, 10)] public float AvoidanceDistance { get; private set; }
        [field:SerializeField, Range(0, 10)] public float AligementDistance { get; private set; }
        [field:SerializeField, Range(0, 10)] public float ObstacleDistance { get; private set; }
        [field:SerializeField, Range(0, 100)] public float BoundsDistance { get; private set; }

        [field:Header("Behaviour Weights")]
        [field:SerializeField, Range(0, 10)] public float CohesionWeight { get; private set; }
        [field:SerializeField, Range(0, 10)] public float AvoidanceWeight { get; private set; }
        [field:SerializeField, Range(0, 10)] public float AlignmentWeight { get; private set; }
        [field:SerializeField, Range(0, 10)] public float BoundsWeight { get; private set; }
        [field:SerializeField, Range(0, 100)] public float ObstacleWeight { get; private set; }

        public FlockUnit[] allUnits;
        
        public Transform FlockManagerTransform { get; private set; }
        public Vector3 Position => FlockManagerTransform.position;
        public Vector3 Forward => FlockManagerTransform.forward;
        public Vector2 Position2D => new (Position.x,Position.z);
        public Vector2 Forward2D => new (Forward.x,Forward.z);
        
        private void Awake()
        {
            FlockManagerTransform = transform;
        }

        private void Start()
        {
            GenerateUnits();
        }

        private void Update()
        {
            for (int i = 0; i < allUnits.Length; i++)
            {
                allUnits[i].MoveUnit();
            }
        }

        private void GenerateUnits()
        {
            allUnits = new FlockUnit[flockSize];
            for (int i = 0; i < flockSize; i++)
            {
                Vector3 spawnPosition = Position + Vector3.Scale(Random.insideUnitSphere, spawnBounds);
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                allUnits[i] = Instantiate(flockUnitPrefab, spawnPosition, rotation);
                allUnits[i].name = $"Agent_{i}";
                allUnits[i].AssignFlock(this);
                allUnits[i].InitializeSpeed(Random.Range(MinSpeed, MaxSpeed));
            }
        }
    }
}
