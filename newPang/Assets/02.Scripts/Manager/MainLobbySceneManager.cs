using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLobbySceneManager : SingletonClass<MainLobbySceneManager>
{
    [SerializeField] GameObject mainView;
    [SerializeField] GameObject characterView;

    [SerializeField] GameObject loadingPopip;

    Stack<GameObject> uiStack;


    protected override void Awake()
    {
        uiStack = new Stack<GameObject>();
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
        Popup(loadingPopip);
        LobbyManager.GetInstance.JoinNormalRoomWithCheckPhoton();
    }
    public void CoOpMatching()
    {
        ClosePopup();
        Popup(loadingPopip);
        LobbyManager.GetInstance.JoinCoOpRoomWithCheckPhoton();
    }

    //public void SingleGame()
    //{
    //    LobbyManager.GetInstance.JoinSingleRoomWithCheckPhoton();
    //}

    public void MatchingCancel()
    {
        LobbyManager.GetInstance.MatchingCancel();
    }

    public void OnCharacterView()
    {
        mainView.SetActive(false);
        characterView.SetActive(true);
    }

    public void ExitCharacterView()
    {
        characterView.SetActive(false);
        mainView.SetActive(true);
    }
}
