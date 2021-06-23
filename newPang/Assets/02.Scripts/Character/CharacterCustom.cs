using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustom : MonoBehaviour
{
    CharacterBase characterBase;

    Accessory face;
    Accessory hand;
    Accessory bag;
    Accessory head;
    Accessory etc;

    public void ChangeCharacter(string characterName)
    {
        if (!ResourceManager.GetInstance.gameObjectDic.ContainsKey(characterName))
        {
            return;
        }

        if (characterBase != null)
        {
            Destroy(characterBase.gameObject);
        }

        GameObject c = Instantiate(ResourceManager.GetInstance.gameObjectDic[characterName]);

        //// 이건 로비 씬이니깐 로비매니저 자식으로 보낸 것.
        //// 인게임은 따로 SetParent 설정해주자.
        //c.transform.SetParent(MainLobbySceneManager.GetInstance.mainCharacterLocator);
        ////

        //c.transform.localScale = Vector3.one;
        //c.transform.localRotation = Quaternion.identity;
        //c.transform.localPosition = Vector3.zero;

        characterBase = c.AddComponent<CharacterBase>();
        characterBase.InitLocator();

        if (MainLobbySceneManager.GetInstance.mainCharacterLocator.gameObject.activeSelf)
        {
            ChangeView(MainLobbySceneManager.GetInstance.mainCharacterLocator);
        }
        else
        {
            ChangeView(MainLobbySceneManager.GetInstance.customCharacterLocator);
        }

        if (DataManager.GetInstance.userData.customs["face"] != "empty")
        {
            EpuipAccessorie(DataManager.GetInstance.userData.customs["face"], AccessoryType.Face);
        }

        if (DataManager.GetInstance.userData.customs["hand"] != "empty")
        {
            EpuipAccessorie(DataManager.GetInstance.userData.customs["hand"], AccessoryType.Hand);
        }

        if (DataManager.GetInstance.userData.customs["bag"] != "empty")
        {
            EpuipAccessorie(DataManager.GetInstance.userData.customs["bag"], AccessoryType.Bag);
        }

        if (DataManager.GetInstance.userData.customs["head"] != "empty")
        {
            EpuipAccessorie(DataManager.GetInstance.userData.customs["head"], AccessoryType.Head);
        }

        if (DataManager.GetInstance.userData.customs["etc"] != "empty")
        {
            EpuipAccessorie(DataManager.GetInstance.userData.customs["etc"], AccessoryType.Etc);
        }
    }

    public void EpuipAccessorie(string itemName, AccessoryType accessoryType)
    {
        GameObject go = null;
        Accessory ac = null;

        if (itemName != "empty")
        {
            go = Instantiate(ResourceManager.GetInstance.gameObjectDic[itemName]);
            ac = go.GetComponent<Accessory>();

            if (ac.type != AccessoryType.Face)
            {
                switch (ac.locator)
                {
                    case LocatorType.Etc:
                        go.transform.SetParent(characterBase.etcLocator);
                        break;
                    case LocatorType.Head:
                        go.transform.SetParent(characterBase.headLocator);
                        break;
                    case LocatorType.Weapon:
                        go.transform.SetParent(characterBase.weaponLocator);
                        break;
                    default:
                        break;
                }

                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
        }


        switch (accessoryType)
        {
            case AccessoryType.Face:
                if (face != null)
                {
                    Destroy(face.gameObject);
                }
                face = ac;
                characterBase.faceRenderer.material = ResourceManager.GetInstance.matDic[itemName];
                break;
            case AccessoryType.Hand:
                if (hand != null)
                {
                    Destroy(hand.gameObject);
                }
                hand = ac;
                break;
            case AccessoryType.Bag:
                if (bag != null)
                {
                    Destroy(bag.gameObject);
                }
                bag = ac;
                break;
            case AccessoryType.Head:
                if (head != null)
                {
                    Destroy(head.gameObject);
                }
                head = ac;
                break;
            case AccessoryType.Etc:
                if (etc != null)
                {
                    Destroy(etc.gameObject);
                }
                etc = ac;
                break;
            default:
                break;
        }

    }

    public void ChangeView(Transform parent)
    {
        characterBase.transform.SetParent(parent);

        characterBase.transform.localScale = Vector3.one;
        characterBase.transform.localRotation = Quaternion.identity;
        characterBase.transform.localPosition = Vector3.zero;
    }
}
