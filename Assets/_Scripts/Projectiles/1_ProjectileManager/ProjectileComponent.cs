using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

namespace KaizerWald
{
    public partial class ProjectileComponent : MonoBehaviour, IPoolable<ProjectileComponent>
    {
        private const float MaxDistance = 1024f;
        private const float Velocity = 5f;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field:SerializeField] public EBulletType BulletType { get; private set; }
        
        [SerializeField] private float MuzzleVelocity = 10f;
        [SerializeField] private LayerMask UnitLayerMask;
        
        [SerializeField] private Rigidbody BulletRigidbody;
        [SerializeField] private TrailRenderer Trail;
        
        private float3 startPosition;
        
        private Unit enemyHit;
        private Transform bulletTransform;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void Awake()
        {
            Initialize();
        }
        
        public void OnUpdate()
        {
            if (BulletRigidbody.velocity != Vector3.zero && !CheckFadeDistance()) return;
            ReturnToPool();
        }

        private void OnCollisionEnter(Collision other)
        {
            bool hit = other.gameObject.layer == UnitLayerMask.GetLayerIndex();
            if (!hit || !CheckHasUnitComponent(other.gameObject, out Unit unit) || unit.IsDead) return;
            RegisterHitUnitToRegiment(unit);
            //unit.OnDeath();
            ReturnToPool();
        }

        private void RegisterHitUnitToRegiment(Unit unit)
        {
            unit.RegimentAttach.OnDeadUnit(unit);
        }

        private bool CheckHasUnitComponent(GameObject hitGameObject, out Unit unit)
        {
            if (hitGameObject.TryGetComponent(out unit)) return true;
#if UNITY_EDITOR
            Debug.LogError($"Hit GameObject: {hitGameObject.name} with unitLayer: {UnitLayerMask.GetLayerIndex()} but dont have Unit Component");
#endif
            return false;
        }
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ IPoolable Interface ◈◈◈◈◈◈                                                                     ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private Action<ProjectileComponent> returnToPool;
        public void Initialize(Action<ProjectileComponent> returnAction) => returnToPool = returnAction;
        public void ReturnToPool() => returnToPool.Invoke(this);
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void Initialize()
        {
            bulletTransform = transform;
            BulletRigidbody = GetComponent<Rigidbody>();
            Trail = GetComponent<TrailRenderer>();
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Firing Behaviour ◈◈◈◈◈◈                                                                        ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private bool CheckFadeDistance() => distancesq(bulletTransform.position, startPosition) > MaxDistance;
        
        public void MakeReady(Vector3 positionInRifle)
        {
            bulletTransform.position = positionInRifle;
        }

        public void Fire(Vector3 positionInRifle, Vector3 direction)
        {
            startPosition = positionInRifle;
            BulletRigidbody.velocity = direction * Velocity;
            BulletRigidbody.useGravity = true;
            Trail.emitting = true;
            BulletRigidbody.AddForce(BulletRigidbody.velocity * MuzzleVelocity, ForceMode.Impulse);
        }
        
    }
}
