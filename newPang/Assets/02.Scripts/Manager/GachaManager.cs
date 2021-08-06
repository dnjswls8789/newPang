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

    List<GachaWeight> characterList;
    List<string> accessoryList;

    public List<Dictionary<string, object>> weightData;


    protected override void Awake()
    {
        weightData = CSVReader.Read("CharacterGacha");
        int a = 0;
        for (int i = 0; i < weightData.Count; i++)
        {
            a += (int)weightData[i]["확률"];
        }

        characterList = new List<GachaWeight>();
        accessoryList = new List<string>();

        GachaWeight[] gameObjects = Resources.LoadAll<GachaWeight>("Resource/Characters");

        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].objectName = gameObjects[i].name;
            gameObjects[i].SetWeight();
            characterList.Add(gameObjects[i]);
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
            int random = Random.Range(0, 100000);
            int weight = 0;

            for (int i = 0; i < characterList.Count; i++)
            {
                weight += characterList[i].weight;

                if (random < weight)
                {
                    result = characterList[i].objectName;
                    break;
                }
            }
            
            if (result == "")
            {
                Debug.LogError("가챠 에러");
            }
        }
        else if (gachaType == GachaType.Accessory)
        {
            int random = Random.Range(0, accessoryList.Count);

            result = accessoryList[random];
        }

        return result;
    }
}
