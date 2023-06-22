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
}
