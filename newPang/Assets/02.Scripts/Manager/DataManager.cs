using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBUserData
{
    public string nickname;
    public long gold;

    public Dictionary<string, string> characters;
    public Dictionary<string, Dictionary<string, string>> accessories;
    public Dictionary<string, string> customs;   // 현재 장착목록 face, hand, bag, head, etc 다섯개 string 값으로 찾아서 empty 면 x
}

public class DataManager : SingletonClass<DataManager>
{
    public FBUserData userData;
    public CharacterCustom custom;

    protected override void Awake()
    {
        base.Awake();

        custom = gameObject.AddComponent<CharacterCustom>();

        userData = new FBUserData();

        userData.characters = new Dictionary<string, string>();
        userData.accessories = new Dictionary<string, Dictionary<string, string>>();
        userData.customs = new Dictionary<string, string>();

        // 이 밑의 userData 값들은 모두 db 에서 받는 값들.

        if (PlayerPrefs.HasKey("character"))
        {
            userData.customs.Add("character", PlayerPrefs.GetString("character"));
        }
        else
        {
            userData.customs.Add("character", "Bear00");
        }

        if (PlayerPrefs.HasKey("face"))
        {
            userData.customs.Add("face", PlayerPrefs.GetString("face"));
        }
        else
        {
            userData.customs.Add("face", "Face01");
        }

        if (PlayerPrefs.HasKey("hand"))
        {
            userData.customs.Add("hand", PlayerPrefs.GetString("hand"));
        }
        else
        {
            userData.customs.Add("hand", "empty");

        }

        if (PlayerPrefs.HasKey("bag"))
        {
            userData.customs.Add("bag", PlayerPrefs.GetString("bag"));
        }
        else
        {
            userData.customs.Add("bag", "empty");

        }

        if (PlayerPrefs.HasKey("head"))
        {
            userData.customs.Add("head", PlayerPrefs.GetString("head"));
        }
        else
        {
            userData.customs.Add("head", "empty");

        }

        if (PlayerPrefs.HasKey("etc"))
        {
            userData.customs.Add("etc", PlayerPrefs.GetString("etc"));
        }
        else
        {
            userData.customs.Add("etc", "empty");

        }

        userData.nickname = "차녕";
        userData.gold = 50000;


        object[] characters = Resources.LoadAll("Resource/Characters");

        for (int i = 0; i < characters.Length; i++)
        {
            GameObject go = characters[i] as GameObject;

            if (go != null)
            {
                userData.characters[go.name] = go.name;
            }
        }

        Material[] face = Resources.LoadAll<Material>("Resource/Items/Face");
        userData.accessories["face"] = new Dictionary<string, string>();

        for (int i = 0; i < face.Length; i++)
        {
            userData.accessories["face"][face[i].name] = face[i].name;
        }

        GameObject[] hand = Resources.LoadAll<GameObject>("Resource/Items/Hand");
        userData.accessories["hand"] = new Dictionary<string, string>();

        for (int i = 0; i < hand.Length; i++)
        {
            userData.accessories["hand"][hand[i].name] = hand[i].name;
        }

        GameObject[] head = Resources.LoadAll<GameObject>("Resource/Items/Head");
        userData.accessories["head"] = new Dictionary<string, string>();

        for (int i = 0; i < head.Length; i++)
        {
            userData.accessories["head"][head[i].name] = head[i].name;
        }

        GameObject[] bag = Resources.LoadAll<GameObject>("Resource/Items/Bag");
        userData.accessories["bag"] = new Dictionary<string, string>();

        for (int i = 0; i < bag.Length; i++)
        {
            userData.accessories["bag"][bag[i].name] = bag[i].name;
        }

        GameObject[] etc = Resources.LoadAll<GameObject>("Resource/Items/Etc");
        userData.accessories["etc"] = new Dictionary<string, string>();

        for (int i = 0; i < etc.Length; i++)
        {
            userData.accessories["etc"][etc[i].name] = etc[i].name;
        }


    }

    public void ChangeCharacter(string characterName, Transform locator)
    {
        if (userData.characters.ContainsKey(characterName))
        {
            userData.customs["character"] = characterName;
            PlayerPrefs.SetString("character", characterName);
            custom.ChangeCharacter(userData.customs["character"], locator);
        }
    }

    public void EpuipAccessorie(string itemName, AccessoryType accessoryType)
    {
        switch (accessoryType)
        {
            case AccessoryType.Face:
                    userData.customs["face"] = itemName;
                    PlayerPrefs.SetString("face", itemName);
                break;
            case AccessoryType.Hand:
                    userData.customs["hand"] = itemName;
                    PlayerPrefs.SetString("hand", itemName);
                break;
            case AccessoryType.Bag:
                    userData.customs["bag"] = itemName;
                    PlayerPrefs.SetString("bag", itemName);
                break;
            case AccessoryType.Head:
                    userData.customs["head"] = itemName;
                    PlayerPrefs.SetString("head", itemName);
                break;
            case AccessoryType.Etc:
                    userData.customs["etc"] = itemName;
                    PlayerPrefs.SetString("etc", itemName);
                break;
            default:
                break;
        }

        custom.EpuipAccessorie(itemName, accessoryType);
    }
}
