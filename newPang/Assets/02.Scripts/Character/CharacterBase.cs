using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterBase : MonoBehaviour
{
    public PhotonView pv;

    public Transform headLocator;
    public Transform weaponLocator;
    public Transform etcLocator;

    public SkinnedMeshRenderer faceRenderer;

    private void Awake()
    {
        if (headLocator == null || weaponLocator == null || etcLocator == null || faceRenderer == null)
        {
            InitLocator();
        }

        pv = GetComponent<PhotonView>();
        
    }

    public void InitLocator()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name == "Accessories_locator")
            {
                etcLocator = child;
            }
            else if (child.name == "Head_Accessories_locator")
            {
                headLocator = child;
            }
            else if (child.name == "WeaponR_locator")
            {
                weaponLocator = child;
            }
            else if (child.name.Length >= 4)
            {
                if (child.name.Substring(0, 4) == "Face")
                {
                    if (faceRenderer == null)
                    {
                        faceRenderer = child.GetComponent<SkinnedMeshRenderer>();
                    }
                }
            }
        }
    }

    public void InitTransform()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    [PunRPC]
    public void SetOtherPlayer()
    {
        MainGameManager.GetInstance.otherPlayerCb = gameObject.AddComponent<CharacterBattle>();
        transform.SetParent(MainGameManager.GetInstance.otherPlayerLocator);

        InitTransform();
    }

    [PunRPC]
    public void EquipAccessory(string itemName)
    {
        if (itemName != "empty")
        {
            GameObject go = Instantiate(ResourceManager.GetInstance.gameObjectDic[itemName]);
            Accessory ac = go.GetComponent<Accessory>();

            if (ac.type != AccessoryType.Face)
            {
                switch (ac.locator)
                {
                    case LocatorType.Etc:
                        go.transform.SetParent(etcLocator);
                        break;
                    case LocatorType.Head:
                        go.transform.SetParent(headLocator);
                        break;
                    case LocatorType.Weapon:
                        go.transform.SetParent(weaponLocator);
                        break;
                    default:
                        break;
                }

                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            else
            {
                faceRenderer.material = ResourceManager.GetInstance.matDic[itemName];
            }
        }
    }
}
