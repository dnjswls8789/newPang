using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonClass<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_Instance = null;

    public static T GetInstance
    {
        get
        {
            if (!m_Instance)
            {
                GameObject obj;
                obj = GameObject.Find(typeof(T).Name);
                if (!obj)
                {
                    obj = new GameObject(typeof(T).Name);
                    m_Instance = obj.AddComponent<T>();
                }
                else
                {
                    m_Instance = obj.GetComponent<T>();
                }
            }

            return m_Instance;
        }
    }

    protected virtual void Awake()
    {
        //m_Instance = typeof(T) as T;
        var obj = FindObjectsOfType(typeof(T));
        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    protected virtual void OnDestroy()
    {
        m_Instance = null;
    }
}
