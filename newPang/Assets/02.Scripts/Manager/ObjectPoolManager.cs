using System.Collections.Generic;
using UnityEngine;

public static class ExtentionGameObject_ObjPool
{
    public static GameObject AddChildFromObjPool(this GameObject obj, string objPoolName, float invokeTime = -1.0f)
    {
        return obj.AddChildFromObjPool(objPoolName, 1, -1, 1, invokeTime);
    }

    public static GameObject AddChildFromObjPool(this GameObject obj, string objPoolName, int addPool, int minPool, int startPoolSize, float invokeTime = -1.0f)
    {
        GameObject tmpGameObject;
        ObjectPool tmpObjectPool;
        if (!ObjectPoolManager.GetInstance.poolDic.TryGetValue(objPoolName, out tmpObjectPool))
        {
            ObjectPoolManager.GetInstance.InitializeObjPool(ObjectPoolManager.GetInstance.GetPrefab(objPoolName), addPool, minPool, startPoolSize);
        }

        tmpGameObject = ObjectPoolManager.GetInstance.poolDic[objPoolName].DequeueObj(invokeTime);

        if (tmpGameObject == null)
        {
            //가져올수없을경우 default object반환 필요
            tmpGameObject = new GameObject();
        }

        tmpGameObject.transform.SetParent(obj.transform);

        return tmpGameObject;
    }

    public static GameObject AddChildFromObjPool(this GameObject obj, string objPoolName, Vector3 pos, Quaternion rot, Vector3 scale, float invokeTime = -1.0f)
    {
        GameObject tmpGameObject = obj.AddChildFromObjPool(objPoolName, 1, -1, 1, invokeTime);

        tmpGameObject.transform.position = pos;
        tmpGameObject.transform.rotation = rot;
        tmpGameObject.transform.localScale = scale;

        return tmpGameObject;
    }

    public static GameObject AddChildFromObjPool(this GameObject obj, string objPoolName, Vector3 pos, float invokeTime = -1.0f)
    {
        GameObject tmpGameObject = obj.AddChildFromObjPool(objPoolName, 1, -1, 1, invokeTime);

        tmpGameObject.transform.position = pos;

        return tmpGameObject;
    }

    public static ObjectPoolTimer AddPoolingTimer(this GameObject gameObj, ObjectPool objPool, PoolItem poolItem)
    {
        ObjectPoolTimer objPoolTimer = gameObj.AddComponent<ObjectPoolTimer>();
        objPoolTimer.linkedGameObject = gameObj;
        objPoolTimer.linkedObjectPool = objPool;
        objPoolTimer.linkedPoolItem = poolItem;

        return objPoolTimer;
    }
    
    public static void PoolingSwitch(this GameObject gameObj) // 토글식
    {
        gameObj.SetActive(!gameObj.activeSelf);
    }
    
    public static void PoolingSwitch(this GameObject gameObj, bool objPoolSwitch)
    {
        gameObj.SetActive(objPoolSwitch);
    }

    public static void InstantEnqueue(this GameObject gameObj)
    {
        ObjectPoolTimer objPoolTimer = gameObj.GetComponent<ObjectPoolTimer>();

        if (objPoolTimer != null)
        {
            objPoolTimer.DeActivateObjectPool();
        }
    }
}

[System.Serializable]
public class PoolItem
{
    [HideInInspector] public GameObject obj;
    [HideInInspector] public ObjectPoolTimer objPoolTimer;

    public PoolItem(GameObject _obj, ObjectPool _objPool)
    {
        obj = _obj;
        objPoolTimer = obj.AddPoolingTimer(_objPool, this);
    }

    public void DestroyPoolItem()
    {
        objPoolTimer.DestroyObjectPoolTimer();

        obj = null;
        objPoolTimer = null;
    }
}

[System.Serializable]
public class ObjectPool
{
    [HideInInspector] public GameObject originalPrefab;
    [HideInInspector] public GameObject parentObject;
    [HideInInspector] public Queue<PoolItem> pool;

    [HideInInspector] public int minPool;
    [HideInInspector] public int addPool;

    public ObjectPool(GameObject _originalPrefab, int _minPool, int _addPool)
    {
        pool = new Queue<PoolItem>();
        originalPrefab = _originalPrefab;
        
        parentObject = GameObject.Find(originalPrefab.name);
        if (!parentObject)
        {
            parentObject = new GameObject(originalPrefab.name);
        }

        minPool = _minPool;
        addPool = _addPool;
    }

    public GameObject DequeueObj(float _invokeTime = -1.0f)
    {
        if (pool.Count < minPool)
        {
            AddPoolImmediately(addPool);
        }

        PoolItem poolItem;

        do
        {
            if (pool.Count <= 0)
            {
                AddPoolImmediately(addPool);
            }

            poolItem = null;

            poolItem = pool.Dequeue();

        } while (poolItem.obj == null);


        if (_invokeTime > 0)
        {
            poolItem.objPoolTimer.ActivateObjectPool(_invokeTime);
        }
        else
        {
            poolItem.obj.PoolingSwitch(true);
        }

        poolItem.objPoolTimer.isEnqueue = false;

#if UNITY_EDITOR
        Debug.Log("[" + originalPrefab.name + "] Dequeue MaxSize [" + pool.Count + "]");
#endif

        return poolItem.obj;
    }

    public GameObject InstantiateWithCustom(GameObject _prefab, bool isActive = false)
    {
        if (!parentObject)
        {
            parentObject = new GameObject(_prefab.name);
        }

        GameObject instantiatedObj;

        instantiatedObj = GameObject.Instantiate(_prefab, parentObject.transform);
        instantiatedObj.PoolingSwitch(isActive);

        return instantiatedObj;
    }

    public void AddPoolImmediately(int poolCount)
    {
        for (int i = 0; i < poolCount; i++)
        {
            pool.Enqueue(new PoolItem(InstantiateWithCustom(originalPrefab), this));
        }

        if (pool.Count < minPool - 1)
        {
            AddPoolImmediately(poolCount);
        }

#if UNITY_EDITOR
        Debug.Log("[" + originalPrefab.name + "] Add Queue Size [" + poolCount + "]\nMaxSize [" + pool.Count + "]");
#endif
    }

    public void DestroyObjectPool()
    {
        originalPrefab = null;
        parentObject = null ;
        pool.Clear();
        pool = null;
    }
}

public class ObjectPoolTimer : MonoBehaviour
{
    [HideInInspector] public GameObject linkedGameObject;
    [HideInInspector] public PoolItem linkedPoolItem;
    [HideInInspector] public ObjectPool linkedObjectPool;
    [HideInInspector] public bool isEnqueue;

    public void ActivateObjectPool(float _invokeTimer)
    {
        if (!linkedGameObject) return;

        linkedGameObject.PoolingSwitch(true);
        Invoke("DeActivateObjectPool", _invokeTimer);
    }

    public void DeActivateObjectPool()
    {
        if (!linkedGameObject) return;
        if (linkedObjectPool == null) return;
        if (isEnqueue) return;
        CancelInvoke("DeActivateObjectPool");

        linkedGameObject.PoolingSwitch(false);

        //풀 부모 오브젝트로 반환
        if (linkedGameObject != null && linkedObjectPool != null && linkedObjectPool.parentObject != null)
        {
            linkedGameObject.transform.SetParent(linkedObjectPool.parentObject.transform);

            linkedObjectPool.pool.Enqueue(linkedPoolItem);
            
            isEnqueue = true;

#if UNITY_EDITOR
            Debug.Log("[" + linkedGameObject.name + "]" + " is Enqueue!\nQueueCount is [" + linkedObjectPool.pool.Count + "]!");
#endif
        }
    }

    public void DestroyObjectPoolTimer()
    {
        CancelInvoke();

        linkedGameObject = null;
        linkedPoolItem = null;
        linkedObjectPool = null;
        if (gameObject)
            Destroy(gameObject);
    }
}

public class ObjectPoolManager : SingletonClass<ObjectPoolManager>
{
    public Dictionary<string, ObjectPool> poolDic;
    public Dictionary<string, GameObject> resourceDic;

    protected override void Awake()
    {
        if (poolDic == null)
        {
            poolDic = new Dictionary<string, ObjectPool>();
        }

        if (resourceDic == null)
        {
            resourceDic = new Dictionary<string, GameObject>();
        }
    }
    
    public bool InitializeObjPool(GameObject _prefab, int addPool, int minPool, int startPoolSize)
    {

        bool result = false;

        if (_prefab == null)
        {
#if UNITY_EDITOR
            Debug.Log("Prefab is Null!");
#endif
            return result;
        }

        if (poolDic.ContainsKey(_prefab.name) && poolDic[_prefab.name] == null)
        {
            poolDic.Remove(_prefab.name);
        }

        if (poolDic.ContainsKey(_prefab.name) == false)
        {
            ObjectPool objPool = new ObjectPool(_prefab, minPool, addPool);
            
            objPool.AddPoolImmediately(startPoolSize);
            
            poolDic.Add(_prefab.name, objPool);

            result = true;
        }

        return result;
    }

    public bool InitializeObjPool(string prefabName, int addPool, int minPool, int startPoolSize)
    {
        bool result = false;

        if (poolDic.ContainsKey(prefabName) && poolDic[prefabName] == null)
        {
            poolDic.Remove(prefabName);
        }

        if (poolDic.ContainsKey(prefabName) == false)
        {
            GameObject go = GetPrefab(prefabName);
            ObjectPool objPool = new ObjectPool(go, minPool, addPool);

            objPool.AddPoolImmediately(startPoolSize);

            poolDic.Add(prefabName, objPool);

            result = true;
        }

        return result;
    }

    public void CleanUpPoolDictionary()
    {
        foreach(ObjectPool _objPools in poolDic.Values)
        {
            if (_objPools != null)
            {
                _objPools.DestroyObjectPool();
            }
        }

        poolDic.Clear();

#if UNITY_EDITOR
        Debug.Log("CleanUp Pool Dictionary!");
#endif
    }

    public GameObject GetPrefab(string name)
    {
        //if (resourceDic.TryGetValue(name, out _prefab))
        if (resourceDic.ContainsKey(name))
        {
            return resourceDic[name];
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("프리팹 이름 잘못됐나..?");
#endif
            return null;
        }
    }

    public void LoadAllGameObject()
    {
        GameObject[] objects = Resources.LoadAll<GameObject>("");

        for (int i = 0; i < objects.Length; i++)
        {
            resourceDic[objects[i].name] = objects[i];
        }
    }

    public void CleanUpResourceDictionary()
    {
        //foreach (GameObject go in resourceDic.Values)
        //{
        //    if (go != null)
        //    {
        //        
        //    }
        //}

        resourceDic.Clear();

        Resources.UnloadUnusedAssets();
    }
}
