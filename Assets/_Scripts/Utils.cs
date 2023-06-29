using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension
{
    public static List<I> FindObjectsOfInterface<I>() 
    where I : class
    {
        MonoBehaviour[] monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
        List<I> list = new List<I>();
        foreach(MonoBehaviour behaviour in monoBehaviours)
        {
            I[] components = behaviour.GetComponents<I>();
            if (components == null || components.Length == 0) continue;
            list.AddRange(components);
            /*
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null) continue;
                list.Add(components[i]);
            }
            */
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
