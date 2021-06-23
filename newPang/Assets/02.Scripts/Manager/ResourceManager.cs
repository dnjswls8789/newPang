using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 나중에 로딩씬에서 만들자. 지금은 로비에서 만듬.
public class ResourceManager : SingletonClass<ResourceManager>
{
    public Dictionary<string, GameObject> gameObjectDic;
    public Dictionary<string, Material> matDic;

    protected override void Awake()
    {
        base.Awake();

        gameObjectDic = new Dictionary<string, GameObject>();
        matDic = new Dictionary<string, Material>();

        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Resource");

        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjectDic[gameObjects[i].name] = gameObjects[i];
        }

        Material[] mats = Resources.LoadAll<Material>("Resource");

        for (int i = 0; i < mats.Length; i++)
        {
            matDic[mats[i].name] = mats[i];
        }
    }


}
