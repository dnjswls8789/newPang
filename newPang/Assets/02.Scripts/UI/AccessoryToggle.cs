using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessoryToggle : MonoBehaviour
{
    [SerializeField] GameObject view;

    public void OnChangedToggle(bool isOn)
    {
        view.SetActive(isOn);
    }
}
