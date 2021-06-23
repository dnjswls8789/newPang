using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UserInfoUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nickNameText;
    [SerializeField] TextMeshProUGUI goldText;

    private void Start()
    {
        nickNameText.text = DataManager.GetInstance.userData.nickname;
        goldText.text = string.Format("{0:#,###}", DataManager.GetInstance.userData.gold);
    }
}
