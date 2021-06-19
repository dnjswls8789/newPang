using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizeCell : MonoBehaviour
{
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

    public void OnChangedToggle(bool isOn)
    {
        if (isOn)
        {
            bgImage.sprite = bgSelectImage;
            bgImage.color = bgSelectColor;
            text.color = textSelectColor;
        }
        else
        {
            bgImage.sprite = bgBasicImage;
            bgImage.color = bgBasicColor;
            text.color = textBasicColor;
        }
    }
}
