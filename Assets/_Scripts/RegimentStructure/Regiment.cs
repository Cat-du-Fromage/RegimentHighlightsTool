using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace KaizerWald
{
    public class TransformAccessWrapper<T> : IDisposable
    where T : MonoBehaviour
    {
        public List<T> Datas{ get; private set; }
        public List<Transform> Transforms{ get; private set; }
        public Dictionary<Transform, int> DictionaryTransformIndex{ get; private set; }
        public TransformAccessArray UnitsTransformAccessArray { get; private set; }

        public TransformAccessWrapper(List<T> datas)
        {
            Transforms = new List<Transform>(datas.Count);
            DictionaryTransformIndex = new Dictionary<Transform, int>(datas.Count);
            for (int i = 0; i < datas.Count; i++)
            {
                Transforms.Add(datas[i].transform);
                DictionaryTransformIndex.Add(datas[i].transform, i);
            }
            UnitsTransformAccessArray = new TransformAccessArray(Transforms.ToArray());
        }
        
        public int Count => Transforms.Count;

        public T this[int index] => Datas[index];

        public void Add(T data)
        {
            Datas.Add(data);
            Transforms.Add(data.transform);
            UnitsTransformAccessArray.Add(data.transform);
            DictionaryTransformIndex.Add(data.transform, Transforms.Count-1);
        }

        public void Remove(T data)
        {
            if (!UnitsTransformAccessArray.isCreated) return;
            int lastIndex = Transforms.Count - 1;
            int indexToRemove = DictionaryTransformIndex[data.transform];
            
            UnitsTransformAccessArray.RemoveAtSwapBack(indexToRemove);
            DictionaryTransformIndex[Transforms[lastIndex]] = indexToRemove;
            DictionaryTransformIndex.Remove(Transforms[indexToRemove]);
            
            Transforms.RemoveAtSwapBack(indexToRemove);
        }

        public void Dispose()
        {
            if (!UnitsTransformAccessArray.isCreated) return;
            UnitsTransformAccessArray.Dispose();
        }
    }
    
    
    public class Regiment : MonoBehaviour, ISelectable
    {
        [field:SerializeField] public bool IsPreselected { get; set; }
        [field:SerializeField] public bool IsSelected { get; set; }

        [field:SerializeField] public RegimentType RegimentType { get; private set; }
        [field:SerializeField] public ulong OwnerID { get; private set; }
        [field:SerializeField] public int RegimentID { get; private set; }
        
        public FormationData Formation { get; private set; }

        public TransformAccessWrapper<Unit> UnitsListWrapper { get; private set; }
        /*
        [field:SerializeField] public List<Unit> Units { get; private set; }
        [field:SerializeField] public List<Transform> UnitsTransform { get; private set; }
        private Dictionary<Transform, int> UnitsDictionaryTransformIndex;
        public TransformAccessArray UnitsTransformAccessArray { get; private set; }
        */
        public List<Unit> Units => UnitsListWrapper.Datas;
        public List<Transform> UnitsTransform => UnitsListWrapper.Transforms;
        public TransformAccessArray UnitsTransformAccessArray => UnitsListWrapper.UnitsTransformAccessArray;
        
        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Unity Events ◇◇◇◇◇                                                                                   ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void OnDestroy()
        {
            UnitsListWrapper.Dispose();
            //if (!UnitsTransformAccessArray.isCreated) return;
            //UnitsTransformAccessArray.Dispose();
        }

        //╔════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
        //║ ◇◇◇◇◇ Initialization Methods ◇◇◇◇◇                                                                         ║
        //╚════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public void Initialize(ulong ownerID, UnitFactory unitFactory, RegimentSpawner currentSpawner, string regimentName = default)
        {
            name = regimentName ?? $"Player{ownerID}_Regiment{RegimentID}";
            OwnerID = ownerID;
            RegimentID = transform.GetInstanceID();
            RegimentType = currentSpawner.RegimentType;
            Formation = new FormationData(currentSpawner.RegimentType);
            CreateAndRegisterUnits(unitFactory);
        }

        private void CreateAndRegisterUnits(UnitFactory unitFactory)
        {
            List<Unit> units = unitFactory.CreateRegimentsUnit(this, RegimentType.RegimentClass.BaseNumberUnit, RegimentType.UnitPrefab);
            UnitsListWrapper = new TransformAccessWrapper<Unit>(units);
            /*
            UnitsTransform = new List<Transform>(Units.Count);
            UnitsDictionaryTransformIndex = new Dictionary<Transform, int>(Units.Count);
            
            for (int i = 0; i < Units.Count; i++)
            {
                UnitsTransform.Add(Units[i].transform);
                UnitsDictionaryTransformIndex.Add(Units[i].transform, i);
            }
            
            UnitsTransformAccessArray = new TransformAccessArray(UnitsTransform.ToArray());
            */
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◆◆◆◆ Regiment Update Event ◆◆◆◆                                                                           │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘

        private void OnLateUpdate()
        {
            
        }
        
        public void OnDeadUnit(Unit unit)
        {
            UnitsListWrapper.Remove(unit);
            /*
            if (!UnitsTransformAccessArray.isCreated) return;
            int lastIndex = UnitsTransform.Count - 1;
            int indexToRemove = UnitsDictionaryTransformIndex[unit.transform];
            
            UnitsTransformAccessArray.RemoveAtSwapBack(indexToRemove);
            UnitsDictionaryTransformIndex[UnitsTransform[lastIndex]] = indexToRemove;
            
            UnitsDictionaryTransformIndex.Remove(UnitsTransform[indexToRemove]);
            UnitsTransform.RemoveAtSwapBack(indexToRemove);
            Units.RemoveAtSwapBack(indexToRemove);
            */
        }
    }
    
}
