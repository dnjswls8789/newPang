using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainLobbySceneManager : SingletonClass<MainLobbySceneManager>
{
    public Transform mainCharacterLocator;
    public Transform customCharacterLocator;
    [SerializeField] GameObject mainView;
    [SerializeField] GameObject characterView;

    [SerializeField] GameObject loadingPopup;

    [SerializeField] Toggle characterToggle;
    [SerializeField] Toggle accessoryToggle;
    [SerializeField] GameObject characterInner;
    [SerializeField] GameObject ItemInner;

    Stack<GameObject> uiStack;


    protected override void Awake()
    {
        uiStack = new Stack<GameObject>();
    }

    private void Start()
    {
        if (DataManager.GetInstance.userData.customs.ContainsKey("character"))
        {
            if (DataManager.GetInstance.userData.customs["character"] == "empty")
            {
                DataManager.GetInstance.ChangeCharacter("Cat00", mainCharacterLocator);
            }
            else
            {
                DataManager.GetInstance.ChangeCharacter(DataManager.GetInstance.userData.customs["character"], mainCharacterLocator);
            }
        }
    }

    public void Popup(GameObject popup)
    {
        popup.SetActive(true);
        uiStack.Push(popup);
    }

    public void ClosePopup()
    {
        if (uiStack.Count > 0)
        {
            uiStack.Pop().SetActive(false);
        }
    }

    public void PVPMatching()
    {
        ClosePopup();
        Popup(loadingPopup);
        LobbyManager.GetInstance.JoinNormalRoomWithCheckPhoton();
    }
    public void CoOpMatching()
    {
        ClosePopup();
        Popup(loadingPopup);
        LobbyManager.GetInstance.JoinCoOpRoomWithCheckPhoton();
    }
    public void MatchingCancel()
    {
        LobbyManager.GetInstance.MatchingCancel();
    }

    public void OnCharacterView()
    {
        customCharacterLocator.gameObject.SetActive(true);
        mainCharacterLocator.gameObject.SetActive(false);
        mainView.SetActive(false);
        characterView.SetActive(true);

        DataManager.GetInstance.custom.ChangeView(customCharacterLocator);
    }

    public void ExitCharacterView()
    {
        accessoryToggle.isOn = true;
        characterToggle.isOn = true;
        customCharacterLocator.gameObject.SetActive(false);
        mainCharacterLocator.gameObject.SetActive(true);

        characterView.SetActive(false);
        mainView.SetActive(true);

        DataManager.GetInstance.custom.ChangeView(mainCharacterLocator);
    }

    public void ToggleCharacterInner(bool isOn)
    {
        if (isOn)
        {
            ItemInner.SetActive(false);
            characterInner.SetActive(true);
        }
    }

    public void ToggleItemInner(bool isOn)
    {
        if (isOn)
        {
            characterInner.SetActive(false);
            ItemInner.SetActive(true);
        }
    }
}
