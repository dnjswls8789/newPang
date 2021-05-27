using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLobbyScene : SingletonClass<MainLobbyScene>
{
    protected override void Awake()
    {
        
    }

    public void Matching()
    {
        LobbyManager.GetInstance.JoinNormalRoomWithCheckPhoton();
    }

    public void SingleGame()
    {
        LobbyManager.GetInstance.JoinSingleRoomWithCheckPhoton();
    }

    public void CoOpMatching()
    {
        LobbyManager.GetInstance.JoinCoOpRoomWithCheckPhoton();
    }
}
