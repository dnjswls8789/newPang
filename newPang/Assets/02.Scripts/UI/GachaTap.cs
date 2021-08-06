using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GachaTap : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Sprite boxSprite;
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI text;
    Animator anim;

    string result;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayGacha()
    {
        DataManager.GetInstance.userData.gold -= 100;
        MainLobbySceneManager.GetInstance.userInfo.RefreshGold();

        anim.SetTrigger("Play");
        result = GachaManager.GetInstance.PlayGacha();
    }

    public void ResultAnimation()
    {
        text.text = Translator.TranslationName(result);
        image.sprite = ResourceManager.GetInstance.spriteDic[result];
    }

    public void init()
    {
        text.text = "";
        result = "";
        image.sprite = boxSprite;

        anim.SetTrigger("Init");
    }

    private void OnDisable()
    {
        init();
    }
}
