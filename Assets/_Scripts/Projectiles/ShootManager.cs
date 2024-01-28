using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

using static Unity.Mathematics.math;

namespace Kaizerwald
{
    public partial class ShootManager : MonoBehaviour
    {
        public bool debug;
        
        //private ShootSystem shootSystem;
        private GameObject bulletPrefab;

        [field:SerializeField] private Regiment regiment;
        [field:SerializeField] public Regiment TargetRegiment { get; private set; }
        [field:SerializeField] public LayerMask HitMask { get; private set; }
        [field:SerializeField] public bool HasTarget { get; private set; }
        [field:SerializeField] public bool IsFiring { get; private set; }

        public int NumShooter;
        
        private JobHandle targetHandle;
        private NativeArray<RaycastHit> results;
        private NativeArray<SpherecastCommand> commands;
        
        [field:SerializeField] public AudioSource ShootSound{ get; private set; }
        private void Awake()
        {
            regiment = GetComponent<Regiment>();
            //shootSystem = FindObjectOfType<ShootSystem>();
            //bulletPrefab = shootSystem.bullet;

            ShootSound = gameObject.AddComponent<AudioSource>();
            ShootSound.spatialBlend = 1f;
        }

        private void Start()
        {
            //HitMask = regiment.IsPlayer ? LayerMask.GetMask("Enemy") : LayerMask.GetMask("Player");
        }

        private void Update()
        {
            /*
            if (regiment.IsMoving)
            {
                ShootTarget(false);
                HasTarget = false;
            }

            else if (!HasTarget)
            {
                if(IsFiring) ShootTarget(false);
                GetTarget();
            }
            else if(HasTarget)
            {
                if (IsFiring && NumShooter == regiment.CurrentLineFormation) return;
                ShootTarget(true);
            }
            */
        }
        
        private void LateUpdate()
        {
            if (!results.IsCreated && !commands.IsCreated) return;
            targetHandle.Complete();

            if (!HasTarget)
            {
                if (IsTargetAcquire())
                {
                    //AssignTargets();
                    ShootTarget(true);
                };
            }
            
            results.Dispose();
            commands.Dispose();
        }

//======================================================================================================================
// NEW SHOOT

        public void ShootBullet(Unit unit)
        {
            if (unit == null) return;
            //if (unit.Target == null || unit.Target.IsDead)
            //{
                
                Unit currentUnit = unit;
                Unit findTarget = GetTargetUnit(currentUnit.IndexInRegiment, TargetRegiment.CurrentFormation.Width);
                if(findTarget == null)
                {
                    SetNoTarget();
                    return;
                }
                //unit.Target = findTarget;
            //}
            
            Transform unitTransform = unit.transform;
            Vector3 unitPosition = unitTransform.position;

            Vector3 unitTargetPosition = /*TargetRegiment.Units[unit.Target.IndexInRegiment].transform.position +*/ new Vector3(0, 0.5f, 0);
            
            Vector3 dir = (unitTargetPosition - unitPosition).xOz().normalized;
            dir += Random.insideUnitSphere * 0.1f;
            
            Vector3 pos = unitPosition + Vector3.up + unitTransform.forward;
            
#if UNITY_EDITOR
            if (debug) Debug.DrawRay(pos, dir * 20f, Color.magenta, 3f);
#endif     
            
            GameObject bullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
            bullet.GetComponent<ProjectileComponent>().Fire(pos,dir);

            if (ShootSound.isPlaying) return;
            if(transform.position.DistanceTo(regiment.Units[regiment.Units.Count / 2].transform.position) > 5)
                transform.position = regiment.Units[regiment.Units.Count / 2].transform.position;
            //ShootSound.PlayOneShot(shootSystem.Audio);
        }
        
        private void AssignTargets()
        {
            for (int i = 0; i < regiment.Units.Count; i++)
            {
                Unit findTarget = GetTargetUnit(regiment.Units[i].IndexInRegiment, TargetRegiment.CurrentFormation.Width);
                if (findTarget == null)
                {
                    ShootTarget(false);
                    return;
                }
                
                //regiment.Units[i].Target = findTarget;
                break;
            }
            ShootTarget(true);
        }
        
        public void SetNoTarget()
        {
            ShootTarget(false);
            HasTarget = false;
            TargetRegiment = null;
        }

//======================================================================================================================
        private void ShootTarget(bool state)
        {
            //int numTarget = TargetRegiment.Units.Length;

            NumShooter = regiment.CurrentFormation.Width;
            for (int i = 0; i < regiment.CurrentFormation.Width; i++)
            {
                regiment.Units[i].Animation.SetFire(state);
            }
            IsFiring = state;
        }

        private bool IsTargetAcquire()
        {
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].transform == null) continue;
                if (!results[i].transform.TryGetComponent(out Unit unit)) continue;
                if (unit.IsDead) continue;
                HasTarget = true;
                TargetRegiment = unit.RegimentAttach;
                AssignTargets();
                return true;
            }
            HasTarget = false;
            TargetRegiment = null;
            return false;
        }

        private Unit GetTargetUnit(int indexInRegiment, int targetLineFormation)
        {
            //int currentIndex = indexInRegiment;
            int targetRowLength = targetLineFormation;
            (int x,_) = KzwMath.GetXY(indexInRegiment,targetLineFormation);

            //===============================================================================
            //CHECK IF LINE BIGGER THAN OPPOSITE
            int divisor = 1;
            if (regiment.CurrentFormation.Width > TargetRegiment.CurrentFormation.Width)
            {
                divisor = regiment.CurrentFormation.Width - TargetRegiment.CurrentFormation.Width;
                if(x > TargetRegiment.CurrentFormation.Width)
                    x /= divisor;
            }
            //===============================================================================
            int xCheck = x;
            int yCheck = 0;
            
            int maxX = targetRowLength;
            
            //CAREFULL: Need to adapt to CURRENT num Units!
            int maxY = Mathf.CeilToInt(TargetRegiment.CurrentFormation.BaseNumUnits / (float)targetRowLength) - 1;
/*      
#if UNITY_EDITOR
            Debug.Log($"max Y : {maxY} maxX : {maxX}");
#endif
*/           
            int indexLoop = 0;
            bool leftReach = xCheck <= 0;
            bool rightReach = xCheck >= maxX - 1;
            //bool2 leftRightReach = new bool2(false);
            
            //Internal Method
            int GetIndexTarget() => yCheck * targetRowLength + xCheck; //int2(xCheck, yCheck).GetIndex(maxX);
            bool CheckUnitNotNull()
            {
                //Debug.Log($"CHECK ZERO (x:{xCheck}y:{yCheck}){leftReach} {rightReach} maxIndex = {TargetRegiment.RegimentClass.BaseNumUnits}; currentIndex = {GetIndexTarget()};");
                return TargetRegiment.Units[GetIndexTarget()] != null;
            }
            

            bool exit = false;
            bool Exit() => leftReach == true && rightReach == true && yCheck == maxY;
            while (true)
            {
                exit = Exit();
                
                if (leftReach == true && rightReach == true)
                {
                    if (yCheck == maxY) return null;
                    if (maxY == 0) return null; //if there is only 1 line 
                    
                    yCheck += 1;
                    //Debug.Log($"Double egalité l:{leftReach} r:{rightReach} NEW Y: {yCheck}");
                    if (yCheck == maxY)
                    {
                        //left = false;
                        //right = false;
                        maxX = GetTargetLastRowSize();
                        
                        //ATTENTION
                        
                        //===============================================================================
                        //CHECK IF LINE BIGGER THAN OPPOSITE
                        //int divisor1 = 1;
                        if (regiment.CurrentFormation.Width > maxX)
                        {
                            //divisor1 = regiment.CurrentLineFormation - maxX;
                            (x,_) = KzwMath.GetXY(indexInRegiment,maxX);
                            //if(x > maxX) x = 0;
                        }
                        //===============================================================================
                        
                        //Debug.Log($"LastRow: {maxX}");
                    }
                    indexLoop = 0;
                }
                
                if (indexLoop == 0)
                {
                    xCheck = x;
                    leftReach = xCheck <= 0;
                    rightReach = xCheck >= maxX - 1;
                    //Debug.Log($"xCheck: {xCheck}; yCheck: {yCheck}");
                    if (CheckUnitNotNull() && !TargetRegiment.Units[GetIndexTarget()].IsDead)
                    {
                        //Debug.DrawLine(TargetRegiment.Units[GetIndexTarget()].transform.position,regiment.Units[indexInRegiment].transform.position,Color.yellow, 3f);
                        
                        //Debug.Log($"CHECK ZERO (x:{xCheck}y:{yCheck}){leftReach} {rightReach} maxIndex = {TargetRegiment.RegimentClass.BaseNumUnits}; currentIndex = {GetIndexTarget()};");
                        return TargetRegiment.Units[GetIndexTarget()];
                        //return true;
                    }

                    indexLoop += 1;
                }
                
                if (rightReach == false)
                {
                    //offset = 1;
                    xCheck = x + indexLoop * 1;
                    if (CheckUnitNotNull() && !TargetRegiment.Units[GetIndexTarget()].IsDead)
                    {
                        //Debug.Log($"CHECK RIGHT (x:{xCheck}y:{yCheck}){leftReach} {rightReach} maxIndex = {TargetRegiment.RegimentClass.BaseNumUnits}; currentIndex = {GetIndexTarget()};");
                        return TargetRegiment.Units[GetIndexTarget()];
                    }

                    if (xCheck >= maxX - 1) rightReach = true;
                }

                if (leftReach == false)
                {
                    //offset = -1;
                    xCheck = x + indexLoop * -1;
                    /*
                    if (GetIndexTarget() == -1)
                    {
                        Debug.Log($"x:{x}; left:{leftReach} right:{rightReach} xCheck: {xCheck}; yCheck: {yCheck}; indexLoop: {indexLoop}");
                    }
                    */
                    if (CheckUnitNotNull() && !TargetRegiment.Units[GetIndexTarget()].IsDead)
                    {
                        //Debug.Log($" CHECK LEFT (x:{xCheck}y:{yCheck}){leftReach} {rightReach} maxIndex = {TargetRegiment.RegimentClass.BaseNumUnits}; currentIndex = {GetIndexTarget()};");
                        return TargetRegiment.Units[GetIndexTarget()];
                    }

                    if (xCheck <= 0) leftReach = true;
                }

                indexLoop += 1;
                
            } 
            
            //Debug.Log("return null");
            //return null;
        }
        
        private int GetTargetLastRowSize()
        {
            float formationNumLine = TargetRegiment.CurrentFormation.BaseNumUnits / (float)TargetRegiment.CurrentFormation.Width;
            int numCompleteLine = Mathf.FloorToInt(formationNumLine);
            //======================================================================================
            int lastLineNumUnit = TargetRegiment.CurrentFormation.BaseNumUnits - (numCompleteLine * TargetRegiment.CurrentFormation.Width);
            return lastLineNumUnit;
        }
/*
        private void GetTarget()
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);

            List<int> searcher = new List<int>(regiment.CurrentFormation.Width);
            for (int i = 0; i < regiment.CurrentFormation.Width; i++)
            {
                if (regiment.Units[i].IsDead) continue;
                searcher.Add(i);
            }
            
            results = new (searcher.Count, Allocator.TempJob);
            commands = new (searcher.Count, Allocator.TempJob);

            for (int i = 0; i < searcher.Count; i++)
            {
                int unitIndex = searcher[i];
                Vector3 direction = regiment.UnitsTransform[unitIndex].forward;
                Vector3 origin = regiment.UnitsTransform[unitIndex].position + offset + direction;
#if UNITY_EDITOR
                if (debug) Debug.DrawRay(origin, direction * 20f);
#endif
                commands[i] = new SpherecastCommand(origin, 2f,direction, 20f,HitMask);
            }
            
            targetHandle = SpherecastCommand.ScheduleBatch(commands, results, regiment.CurrentFormation.Width);
        }
*/
        
    }
    
     
}
