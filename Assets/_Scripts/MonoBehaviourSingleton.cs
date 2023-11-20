using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KaizerWald
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour
    where T : MonoBehaviour
    {
        // This is really the only blurb of code you need to implement a Unity singleton
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindFirstObjectByType<T>();
                return instance;
            }
        }

        protected virtual void Awake()
        {
            instance = GetComponent<T>();
            if (instance == null)
            {
                Debug.LogError($"NO SINGLETON FOUND");
                return;
            }
            DontDestroyOnLoad(instance.gameObject);
        }
    }
}
