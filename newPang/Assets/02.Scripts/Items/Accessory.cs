using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AccessoryType
{
    None = -1,
    Face,
    Hand,
    Bag,
    Head,
    Etc
}

public enum LocatorType
{
    Etc,
    Head,
    Weapon
}

public class Accessory : MonoBehaviour
{
    public string accessoryName;
    public AccessoryType type;
    public LocatorType locator;

    private void Awake()
    {
        accessoryName = gameObject.name;
    }
}
