using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CustomType
{
    Character,
    Accessory
}

public class CustomTap : MonoBehaviour
{
    [SerializeField] CustomType type;
    [SerializeField] AccessoryType accessoryType;

    [SerializeField] GameObject cellPrefab;
    ToggleGroup toggleGroup;

    Dictionary<string, string> customDic;

    RectTransform rt;

    private void OnDisable()
    {
        rt.localPosition = new Vector2(0f, 0f);
    }

    private void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        rt = GetComponent<RectTransform>();

        if (type == CustomType.Character)
        {
            customDic = DataManager.GetInstance.userData.characters;
        }
        else if (type == CustomType.Accessory)
        {
            switch (accessoryType)
            {
                case AccessoryType.Face:
                    customDic = DataManager.GetInstance.userData.accessories["face"];
                    break;
                case AccessoryType.Hand:
                    customDic = DataManager.GetInstance.userData.accessories["hand"];
                    break;
                case AccessoryType.Bag:
                    customDic = DataManager.GetInstance.userData.accessories["bag"];
                    break;
                case AccessoryType.Head:
                    customDic = DataManager.GetInstance.userData.accessories["head"];
                    break;
                case AccessoryType.Etc:
                    customDic = DataManager.GetInstance.userData.accessories["etc"];
                    break;
                default:
                    break;
            }
        }
        foreach(KeyValuePair<string, string> kv in customDic)
        {
            GameObject cell = Instantiate(cellPrefab);

            cell.transform.SetParent(transform);
            cell.transform.localScale = Vector3.one;
            cell.transform.localPosition = Vector3.zero;

            cell.GetComponent<Toggle>().group = toggleGroup;

            if (type == CustomType.Character)
            {
                cell.transform.Find("head").GetComponent<RectTransform>().sizeDelta = new Vector2(45, 45);
                cell.transform.Find("name").GetComponent<TextMeshProUGUI>().fontSize = 10f;              
            }
            else
            {
                cell.transform.Find("head").GetComponent<RectTransform>().sizeDelta = new Vector2(35, 35);
                cell.transform.Find("name").GetComponent<TextMeshProUGUI>().fontSize = 9f;               
            }


            cell.GetComponent<CustomizeCell>().Init(type, accessoryType, kv.Key);
        }

        // db 에서 현재 장착 된 아이템 받아와서 걔 선택 해줘야된다.
        // 돌아가기 버튼 누를 때 db 에 갱신하자.
    }

}
