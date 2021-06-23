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

        userData = new FBUserData();

        userData.characters = new Dictionary<string, string>();
        userData.accessories = new Dictionary<string, Dictionary<string, string>>();
        userData.customs = new Dictionary<string, string>();

        userData.nickname = "차녕";
        userData.gold = 100;
        //userData.characters.Add("")
        userData.customs.Add("character", "empty");
        userData.customs.Add("face", "empty");
        userData.customs.Add("hand", "empty");
        userData.customs.Add("bag", "empty");
        userData.customs.Add("head", "empty");
        userData.customs.Add("etc", "empty");

        GameObject[] characters = Resources.LoadAll<GameObject>("Resource/Characters");

        for (int i = 0; i < characters.Length; i++)
        {
            userData.characters[characters[i].name] = characters[i].name;
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

    public void ChangeCharacter()
    {

    }

    public void EpuipAccessorie(Accessory item)
    {
        switch (item.type)
        {
            case AccessoryType.Face:
                userData.customs["face"] = item.name;
                break;
            case AccessoryType.Hand:
                userData.customs["hand"] = item.name;
                break;
            case AccessoryType.Bag:
                userData.customs["bag"] = item.name;
                break;
            case AccessoryType.Head:
                userData.customs["head"] = item.name;
                break;
            case AccessoryType.Etc:
                userData.customs["etc"] = item.name;
                break;
            default:
                break;
        }

        custom.EpuipAccessorie(item);
    }
}
