using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static int GetSizeOf<T>() where T : struct
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
    }
    
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
