using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizeCell : MonoBehaviour
{
    public CustomType type;
    public AccessoryType accessoryType;
    public string objectName;

    [SerializeField] Image itemImage;

    Image bgImage;
    [SerializeField] Sprite bgBasicImage;
    [SerializeField] Sprite bgSelectImage;
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] Color bgBasicColor;
    [SerializeField] Color bgSelectColor;

    [SerializeField] Color textBasicColor;
    [SerializeField] Color textSelectColor;

    Toggle toggle;

    private void Awake()
    {
        bgImage = GetComponent<Image>();
        toggle = GetComponent<Toggle>();

        OnChangedToggle(toggle.isOn);
    }

    public void Init(CustomType _type, AccessoryType _accessoryType, string _objectName)
    {
        type = _type;
        accessoryType = _accessoryType;
        objectName = _objectName;

        text.text = objectName;

        if (type == CustomType.Character)
        {
            if (DataManager.GetInstance.userData.customs["character"] == objectName)
            {
                toggle.isOn = true;
            }
        }
        else if (type == CustomType.Accessory)
        {
            switch (accessoryType)
            {
                case AccessoryType.None:
                    break;
                case AccessoryType.Face:
                    if (DataManager.GetInstance.userData.customs["face"] == objectName)
                    {
                       toggle.isOn = true;
                    }
                    break;
                case AccessoryType.Hand:
                    if (DataManager.GetInstance.userData.customs["hand"] == objectName)
                    {
                        toggle.isOn = true;
                    }
                    break;
                case AccessoryType.Bag:
                    if (DataManager.GetInstance.userData.customs["bag"] == objectName)
                    {
                        toggle.isOn = true;
                    }
                    break;
                case AccessoryType.Head:
                    if (DataManager.GetInstance.userData.customs["head"] == objectName)
                    {
                        toggle.isOn = true;
                    }
                    break;
                case AccessoryType.Etc:
                    if (DataManager.GetInstance.userData.customs["etc"] == objectName)
                    {
                        toggle.isOn = true;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void OnChangedToggle(bool isOn)
    {
        if (isOn)
        {
            bgImage.sprite = bgSelectImage;
            bgImage.color = bgSelectColor;
            text.color = textSelectColor;

            if (type == CustomType.Character)
            {
                DataManager.GetInstance.ChangeCharacter(objectName, MainLobbySceneManager.GetInstance.customCharacterLocator);
            }
            else
            {
                DataManager.GetInstance.EpuipAccessorie(objectName, accessoryType);
            }

        }
        else
        {
            bgImage.sprite = bgBasicImage;
            bgImage.color = bgBasicColor;
            text.color = textBasicColor;

            if (toggle.group != null && !toggle.group.AnyTogglesOn())
            {
                DataManager.GetInstance.EpuipAccessorie("empty", accessoryType);
            }

        }
    }
}
