using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kaizerwald
{
    public interface IPool<T>
    {
        public T Pull();
        public void Push(T item);
    }

    public interface IPoolable<out T>
    {
        public void Initialize(Action<T> returnAction);
        public void ReturnToPool();
        
        //Basic implementation ADD this:
        //private Action<ProjectileComponent> returnToPool;
        //public void Initialize(Action<ProjectileComponent> returnAction) => returnToPool = returnAction;
        //public void ReturnToPool() => returnToPool?.Invoke(this);
    }
    
    public class ObjectPool<T> : IPool<T> 
    where T : MonoBehaviour, IPoolable<T>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private GameObject prefab;
        private Stack<T> pooledObjects = new Stack<T>();
        
        private Action<T> pullObject;
        private Action<T> pushObject;
        
        public int PooledCount => pooledObjects.Count;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTORS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public ObjectPool(GameObject pooledObject, int numToSpawn = 0)
        {
            prefab = pooledObject;
            Spawn(numToSpawn);
        }

        public ObjectPool(GameObject pooledObject, Action<T> pullObject, int numToSpawn = 0) : this(pooledObject, numToSpawn)
        {
            this.pullObject = pullObject;
        }
        
        public ObjectPool(GameObject pooledObject, Action<T> pullObject, Action<T> pushObject, int numToSpawn = 0) : this(pooledObject, pullObject, numToSpawn)
        {
            this.pushObject = pushObject;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                  ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void Spawn(int number)
        {
            for (int i = 0; i < number; i++)
            {
                T item = Object.Instantiate(prefab).GetComponent<T>();
                pooledObjects.Push(item);
                item.gameObject.SetActive(false);
            }
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ PULL ◈◈◈◈◈◈                                                                                    ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Pull By Component ◇◇◇◇◇◇                                                                           │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public T Pull()
        {
            T item = PooledCount > 0 ? pooledObjects.Pop() : Object.Instantiate(prefab).GetComponent<T>();
            item.gameObject.SetActive(true); //ensure the object is on
            item.Initialize(Push);
            pullObject?.Invoke(item); //allow default behavior and turning object back on
            return item;
        }

        public T Pull(Vector3 position)
        {
            T item = Pull();
            item.transform.position = position;
            return item;
        }

        public T Pull(Vector3 position, Quaternion rotation)
        {
            T item = Pull();
            item.transform.SetPositionAndRotation(position, rotation);
            return item;
        }

        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Pull GameObject ◇◇◇◇◇◇                                                                             │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public GameObject PullGameObject()
        {
            return Pull().gameObject;
        }

        public GameObject PullGameObject(Vector3 position)
        {
            GameObject item = Pull().gameObject;
            item.transform.position = position;
            return item;
        }

        public GameObject PullGameObject(Vector3 position, Quaternion rotation)
        {
            GameObject item = Pull().gameObject;
            item.transform.SetPositionAndRotation(position, rotation);
            return item;
        }

        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ PUSH ◈◈◈◈◈◈                                                                                    ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void Push(T item)
        {
            pooledObjects.Push(item);
            pushObject?.Invoke(item); //create default behavior to turn off objects
            item.gameObject.SetActive(false);
        }
    }
}
