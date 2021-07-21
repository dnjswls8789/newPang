using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GachaType
{
    Character,
    Accessory
}

public class GachaManager : SingletonClass<GachaManager>
{
    public GachaType gachaType;

    List<string> characterList;
    List<string> accessoryList;

    protected override void Awake()
    {
        characterList = new List<string>();
        accessoryList = new List<string>();

        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Resource/Characters");

        for (int i = 0; i < gameObjects.Length; i++)
        {
            characterList.Add(gameObjects[i].name);
        }

        GameObject[] items = Resources.LoadAll<GameObject>("Resource/Items");

        for (int i = 0; i < gameObjects.Length; i++)
        {
            accessoryList.Add(items[i].name);
        }
    }

    public void SetGachaType(GachaType type)
    {
        gachaType = type;
    }

    public string PlayGacha()
    {
        string result = "";

        if (gachaType == GachaType.Character)
        {
            int random = Random.Range(0, characterList.Count);

            result = characterList[random];
        }
        else if (gachaType == GachaType.Accessory)
        {
            int random = Random.Range(0, accessoryList.Count);

            result = accessoryList[random];
        }

        return result;
    }
}
