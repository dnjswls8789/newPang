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
        if (characterBase != null)
        {
            Destroy(characterBase.gameObject);
        }

        //characterBase = cb;
    }

    public void EpuipAccessorie(Accessory item)
    {
        switch (item.type)
        {
            case AccessoryType.Face:
                face = item;
                characterBase.faceRenderer.material = ResourceManager.GetInstance.matDic[item.name];
                break;
            case AccessoryType.Hand:
                hand = item;
                break;
            case AccessoryType.Bag:
                bag = item;
                break;
            case AccessoryType.Head:
                head = item;
                break;
            case AccessoryType.Etc:
                etc = item;
                break;
            default:
                break;
        }

        if (item.type != AccessoryType.Face)
        {
            GameObject go = Instantiate(ResourceManager.GetInstance.gameObjectDic[item.name]);

            switch (item.locator)
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
        }
    }
}
