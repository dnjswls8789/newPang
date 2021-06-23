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

            }
            else
            {

            }
        }
        else
        {
            bgImage.sprite = bgBasicImage;
            bgImage.color = bgBasicColor;
            text.color = textBasicColor;
        }
    }
}
