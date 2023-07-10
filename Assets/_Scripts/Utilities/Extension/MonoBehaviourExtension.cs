using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public static class MonoBehaviourExtension
    {
            public static void RemoveComponent<TComponent>(this GameObject obj, bool immediate = false)
            where TComponent : Component
            {
                if (!obj.TryGetComponent(out TComponent component)) return;
                if (immediate)
                {
                    Object.DestroyImmediate((Object)component, true);
                }
                else
                {
                    Object.Destroy((Object)component);
                }
            }
            
            public static List<T> FindObjectsWithInterface<T>() 
            where T : class
            {
                MonoBehaviour[] monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
                List<T> list = new List<T>();
                foreach(MonoBehaviour behaviour in monoBehaviours)
                {
                    T[] components = behaviour.GetComponents<T>();
                    if (components == null || components.Length == 0) continue;
                    list.AddRange(components);
                }
                return list;
            }
            
            public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
            {
                return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
            }
            
            public static T GetOrAddComponent<T>(this Transform transform)
            where T : Component
            {
                return transform.gameObject.GetOrAddComponent<T>();
            }
            
            public static T GetOrAddComponent<T>(this MonoBehaviour script)
            where T : Component
            {
                return script.gameObject.GetOrAddComponent<T>();
            }
    }
}
