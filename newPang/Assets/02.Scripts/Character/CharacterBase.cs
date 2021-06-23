using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
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
}
