using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GachaWeight : MonoBehaviour
{
    public string objectName;
    public int weight;

    public void SetWeight()
    {
        bool a = false;
        string itemName = Translator.TranslationName(objectName);
        for (int i = 0; i < GachaManager.GetInstance.weightData.Count; i++)
        {
            if ((string)GachaManager.GetInstance.weightData[i]["이름"] == itemName)
            {
                weight = (int)GachaManager.GetInstance.weightData[i]["확률"];
                a = true;
            }
        }
    }
}
