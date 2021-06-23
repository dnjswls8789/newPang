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
            else if (child.name == "Face01")
            {
                faceRenderer = child.GetComponent<SkinnedMeshRenderer>();
            }

        }
    }
}
