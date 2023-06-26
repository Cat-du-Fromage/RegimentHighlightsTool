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
                if (instance) return instance;
                instance = new GameObject().AddComponent<T>();
                instance.name = instance.GetType().ToString();
                DontDestroyOnLoad(instance.gameObject);
                return instance;
            }
        }

        protected virtual void Awake()
        {
            instance = FindAnyObjectByType<T>();
        }
        // implement your Awake, Start, Update, or other methods here...
    }
}
