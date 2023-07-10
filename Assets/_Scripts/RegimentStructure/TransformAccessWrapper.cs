using System;
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
            Datas = new List<T>(datas.Count);
            Transforms = new List<Transform>(datas.Count);
            DictionaryTransformIndex = new Dictionary<Transform, int>(datas.Count);
            for (int i = 0; i < datas.Count; i++)
            {
                Datas.Add(datas[i]);
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
            Remove(data.transform);
        }
        
        public void Remove(Transform data)
        {
            if (!UnitsTransformAccessArray.isCreated) return;
            int lastIndex = Transforms.Count - 1;
            int indexToRemove = DictionaryTransformIndex[data];
            
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
}