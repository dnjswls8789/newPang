using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using System.Collections.Generic;

public static class ExtentionString
{
    public static byte[] ConvertStringToByteArray(this string str)
    {
        return Encoding.Unicode.GetBytes(str);
    }

    public static string ConvertByteArrayToString(this byte[] byteArray)
    {
        return Encoding.Unicode.GetString(byteArray);
    }
}

public static class ExtentionPhotonHashTable
{
    public static string GetByteToString(this ExitGames.Client.Photon.Hashtable photonHashTable, string _key)
    {
        return (photonHashTable[_key] as byte[]).ConvertByteArrayToString();
    }

    public static void SetStringToByte(this ExitGames.Client.Photon.Hashtable photonHashTable, string _key, string _value)
    {
        photonHashTable[_key] = _value.ConvertStringToByteArray();
    }
}

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private static LobbyManager m_Instance = null;
    public static LobbyManager GetInstance
    {
        get
        {
            if (!m_Instance)
            {
                GameObject obj;
                obj = GameObject.Find(typeof(LobbyManager).Name);
                if (!obj)
                {
                    obj = new GameObject(typeof(LobbyManager).Name);
                    m_Instance = obj.AddComponent<LobbyManager>();
                }
                else
                {
                    m_Instance = obj.GetComponent<LobbyManager>();
                }
            }

            return m_Instance;
        }
    }

    public bool serverConnect;

    private string gameVersion = "1";

    private RoomOptions roomOptions;    // 현재 룸 옵션
    private TypedLobby lobbyType;       // 현재 로비

    private long mmr;       // 내 mmr
    private long mmrWidth;  // 매치 mmr 최대범위

    public long enemyMmr;   // 매칭된 상대 mmr

    // 매칭 mmr 범위 분할 횟수 
    // 랭크게임 매치 mmr 범위 500 주면 Max 만큼 나눠서 100 차이 매칭 -> 없으면 200차이 매칭 이렇게 넓혀감
    private int rankMatchingCount = 0;
    private int maxRankMatchingCount = 5;

    private string uid;         // 내 uid. 안씀
    private string nickname;    // 내 닉네임

    public bool initAd;

    ExitGames.Client.Photon.Hashtable currentRoomProperty;

    private bool isGameStart;   // 게임 시작 여부
    public bool isMatching;    // 매칭 잡는 중인지. 게임시작하면 false.
    private bool isPause;      // 홈키 눌렀는지.

    private delegate void PunCallbackDelegate();
    private delegate void PunPlayerCallbackDelegate(Player otherPlayer);

    private PunCallbackDelegate onConnectedToMaster;           //포톤 서버 연결완료
    private PunCallbackDelegate onDisconnected;                //포톤 서버 연결해제
    private PunCallbackDelegate onJoinedLobby;                 //로비 입장완료
    private PunCallbackDelegate onJoinedRoom;                  //방 입장완료
    private PunCallbackDelegate onJoinRoomFailed;              //방 입장실패 (커스텀)
    private PunCallbackDelegate onJoinRandomFailed;            //방 입장실패 (랜덤매칭)
    private PunPlayerCallbackDelegate onPlayerEnteredRoom;     //방에 사람 들어옴
    private PunPlayerCallbackDelegate onOtherPlayerLeftRoom;   //상대방이 방에서 나감 (연결이 잠깐 끊겼다던지, 게임 끝난 후 나갔다던지)
    private PunCallbackDelegate onLeftRoom;                    //내가 방에서 나감

    public Action joinCustomRoomFailed;

    // 인터넷 연결상태 체크. MainLobbyScene 에서 3초 이상 연결이 끊겨있으면 팝업띄우자.
    float internetTerm = 0;
    float internetMaxTerm = 3;
    float internetEntireTerm = 0;    // 이거 게임끝날 때마다 0으로 초기화 시켜줘야 됨.
    float internetEntireMaxTerm = 5; // 총 누적 시간

    private string customRoomName;
    private bool isMakeCustomRoom;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 0.0005f;

        currentRoomProperty = new ExitGames.Client.Photon.Hashtable();

        StartCoroutine(CheckInternet());
    }

    private void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true;    // 방의 모든 플레이어의 씬을 마스터 클라이언트에게 맞추겠다.
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            serverConnect = true;
        }
        else
        {
            serverConnect = false;
        }
    }

    // 포톤서버 연결 완료.
    public override void OnConnectedToMaster()
    {
        Debug.LogWarning("OnConnectedToMaster");

        if (onConnectedToMaster != null)
        {
            onConnectedToMaster();
        }

        //ClearOnConnectedToMasterCallback();

        PhotonNetwork.LocalPlayer.CustomProperties.SetStringToByte("name", nickname);
        PhotonNetwork.LocalPlayer.CustomProperties["mmr"] = mmr;
        StopCoroutine("ServerReconnect");

        isPause = false;
    }

    public void AddOnConnectedToMasterCallback(params Action[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onConnectedToMaster += new PunCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnConnectedToMasterCallback()
    {
        if (onConnectedToMaster == null) return;
        foreach (Delegate d in onConnectedToMaster.GetInvocationList())
        {
            onConnectedToMaster -= (PunCallbackDelegate)d;
        }

        onConnectedToMaster = null;
    }

    // 포톤서버 연결해제.
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("OnDisconnected : " + cause.ToString());

        if (onDisconnected != null)
        {
            onDisconnected();
        }

        if (!PhotonNetwork.IsConnected)
        {
            // 서버 연결이 끊겼을 때, 재연결 시도를 해야하는 상황인지, 아닌지 구분
            // isGameStart == true 일 때, isMatching == true 일 때.
            if (isGameStart || isMatching)
            {
                StopCoroutine("ServerReconnect");
                StartCoroutine("ServerReconnect");
            }
        }
    }
    public void AddOnDisconnectedCallback(params Action[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onDisconnected += new PunCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnDisconnectedCallback()
    {
        if (onDisconnected == null) return;
        foreach (Delegate d in onDisconnected.GetInvocationList())
        {
            onDisconnected -= (PunCallbackDelegate)d;
        }

        onDisconnected = null;
    }

    // 로비 입장완료.
    public override void OnJoinedLobby()
    {
        Debug.LogWarning("OnJoinedLobby");

        if (onJoinedLobby != null)
        {
            onJoinedLobby();
        }

        //ClearOnJoinedLobbyCallback();
    }

    public void AddOnJoinedLobbyCallback(params Action[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onJoinedLobby += new PunCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnJoinedLobbyCallback()
    {
        if (onJoinedLobby == null) return;
        foreach (Delegate d in onJoinedLobby.GetInvocationList())
        {
            onJoinedLobby -= (PunCallbackDelegate)d;
        }

        onJoinedLobby = null;
    }

    // 방 입장완료.
    public override void OnJoinedRoom()
    {
        Debug.LogWarning("OnJoinedRoom. RoomName : " + PhotonNetwork.CurrentRoom.Name);

        if (onJoinedRoom != null)
        {
            onJoinedRoom();
        }

        //ClearOnJoinedRoomCallback();
    }

    public void AddOnJoinedRoomCallback(params Action[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onJoinedRoom += new PunCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnJoinedRoomCallback()
    {
        if (onJoinedRoom == null) return;
        foreach (Delegate d in onJoinedRoom.GetInvocationList())
        {
            onJoinedRoom -= (PunCallbackDelegate)d;
        }

        onJoinedRoom = null;
    }

    // 커스텀방 입장실패.
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("OnJoinRoomFailed. " + message + " returnCode : " + returnCode);

        if (onJoinRoomFailed != null)
        {
            onJoinRoomFailed();
        }

        //ClearOnJoinRoomFailedCallback();
    }

    public void AddOnJoinRoomFailedCallback(params Action[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onJoinRoomFailed += new PunCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnJoinRoomFailedCallback()
    {
        if (onJoinRoomFailed == null) return;
        foreach (Delegate d in onJoinRoomFailed.GetInvocationList())
        {
            onJoinRoomFailed -= (PunCallbackDelegate)d;
        }

        onJoinRoomFailed = null;
    }

    // 랜덤매칭 방 입장실패.
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("OnJoinRandomFailed. " + message + " returnCode : " + returnCode);

        if (onJoinRandomFailed != null)
        {
            onJoinRandomFailed();
        }

        //ClearOnJoinRandomFailedCallback();
    }

    public void AddOnJoinRandomFailedCallback(params Action[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onJoinRandomFailed += new PunCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnJoinRandomFailedCallback()
    {
        if (onJoinRandomFailed == null) return;
        foreach (Delegate d in onJoinRandomFailed.GetInvocationList())
        {
            onJoinRandomFailed -= (PunCallbackDelegate)d;
        }

        onJoinRandomFailed = null;
    }

    // 방에 상대 플레이어 들어옴.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogWarning("OnPlayerEnteredRoom");

        if (onPlayerEnteredRoom != null)
        {
            onPlayerEnteredRoom(newPlayer);
        }

        //ClearOnPlayerEnteredRoomCallback();
    }

    public void AddOnPlayerEnteredRoomCallback(params Action<Player>[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onPlayerEnteredRoom += new PunPlayerCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnPlayerEnteredRoomCallback()
    {
        if (onPlayerEnteredRoom == null) return;
        foreach (Delegate d in onPlayerEnteredRoom.GetInvocationList())
        {
            onPlayerEnteredRoom -= (PunPlayerCallbackDelegate)d;
        }

        onPlayerEnteredRoom = null;
    }

    // 방에서 상대 플레이어 나감.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning("OnPlayerLeftRoom");

        if (onOtherPlayerLeftRoom != null)
        {
            onOtherPlayerLeftRoom(otherPlayer);
        }

        //ClearOnOtherPlayerLeftRoomCallback();
    }

    public void AddOnOtherPlayerLeftRoomCallback(params Action<Player>[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onOtherPlayerLeftRoom += new PunPlayerCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnOtherPlayerLeftRoomCallback()
    {
        if (onOtherPlayerLeftRoom == null) return;
        foreach (Delegate d in onOtherPlayerLeftRoom.GetInvocationList())
        {
            onOtherPlayerLeftRoom -= (PunPlayerCallbackDelegate)d;
        }

        onOtherPlayerLeftRoom = null;
    }

    // 방에서 나옴.
    public override void OnLeftRoom()
    {
        Debug.LogWarning("OnLeftRoom");

        if (onLeftRoom != null)
        {
            onLeftRoom();
        }

        //ClearOnLeftRoomCallback();
    }

    public void AddOnLeftRoomCallback(params Action[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            onLeftRoom += new PunCallbackDelegate(actions[i]);
        }
    }

    public void ClearOnLeftRoomCallback()
    {
        if (onLeftRoom == null) return;
        foreach (Delegate d in onLeftRoom.GetInvocationList())
        {
            onLeftRoom -= (PunCallbackDelegate)d;
        }

        onLeftRoom = null;
    }

    public void AllClearCallbackDelegate()
    {
        ClearOnConnectedToMasterCallback();
        ClearOnDisconnectedCallback();
        ClearOnJoinedLobbyCallback();
        ClearOnJoinedRoomCallback();
        ClearOnJoinRandomFailedCallback();
        ClearOnJoinRoomFailedCallback();
        ClearOnLeftRoomCallback();
        ClearOnPlayerEnteredRoomCallback();
        ClearOnOtherPlayerLeftRoomCallback();
    }

    public override void OnCreatedRoom()
    {
        Debug.LogWarning("OnCreatedRoom");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("OnCreateRoomFailed : " + message + " returnCode : " + returnCode);
    }


    // 포톤 서버 연결
    private void JoinToMaster()
    {
        Debug.LogWarning("JoinToMaster");

        PhotonNetwork.ConnectUsingSettings();
    }

    //////////////////////////////// 노말 //////////////////////////////////////////////////////////////////////

    public void JoinNormalRoomWithCheckPhoton()
    {
        if (!isMatching) return;

        Debug.LogWarning("JoinNormalRoomWithCheckPhoton");
        //isMatching = true;
        AllClearCallbackDelegate();

        // 일반게임 매칭 돌리는데 방을 못찾았을 경우 콜백.
        AddOnJoinRandomFailedCallback(RandomJoinNormalRoomFailed);

        lobbyType = new TypedLobby("NormalLobby", LobbyType.SqlLobby);

        if (!PhotonNetwork.IsConnected)
        {
            // 서버 연결완료 콜백 // 에 방 찾는 함수 넣고 서버 연결 시도한다.
            AddOnConnectedToMasterCallback(RandomJoinNormalRoom);
            // 방 접속완료 콜백 // 게임 씬으로 넘어갔는지 체크하는 코루틴.
            AddOnJoinedRoomCallback(() => { StartCoroutine("WaitGameStart"); });
            JoinToMaster();
        }
        else
        {
            // 정상이면 서버에 연결 중인 경우는 없다. 안전하게 서버 연결 끊고 함수 다시 부른다.
            AddOnDisconnectedCallback(JoinNormalRoomWithCheckPhoton);
            PhotonNetwork.Disconnect();
        }
    }

    private void RandomJoinNormalRoom()
    {
        //C0 : 게임 시작여부 뭔지 잘 모르겠음..
        PhotonNetwork.JoinRandomRoom(null, 2, MatchmakingMode.RandomMatching, lobbyType, "C0 = 0");
    }

    private void MakeNormalRoom()
    {
        Debug.LogWarning("MakeNormalRoom");
        AllClearCallbackDelegate();

        // 방 만든 사람한테만 인원체크 후 게임시작 콜백 넣으면 ??
        AddOnPlayerEnteredRoomCallback(PlayerNumberCheck);

        roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        roomOptions.PlayerTtl = 5000;
        roomOptions.EmptyRoomTtl = 0;
        roomOptions.CleanupCacheOnLeave = false;

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "C0", 0 }};
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "C0"};

        currentRoomProperty = roomOptions.CustomRoomProperties;

        PhotonNetwork.CreateRoom(null, roomOptions, lobbyType);
    }

    private void RandomJoinNormalRoomFailed()
    {
        AllClearCallbackDelegate();

        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonNetwork.InRoom)
            {
                // 일반 게임 방 찾기 실패 했을 때, 서버에 정상적으로 연결 중이면 대기 중인 방이 없는 것. 내가 방 만든다.
                MakeNormalRoom();
            }
            else
            {
                // 방에 들어가있는 상태라면 뭔가 잘못된 것. 서버 연결 끊고 일반 매칭 처음부터 다시 시도.
                AddOnDisconnectedCallback(JoinNormalRoomWithCheckPhoton);
                PhotonNetwork.Disconnect();
            }
        }
        else
        {
            // 방 찾기 실패 했는데 서버 연결이 안돼있으면 // 서버 연결완료 콜백 // 에 일반게임 랜덤매칭 함수 넣고 서버 연결 시도. 연결 될 때까지 반복.
            // 네트워크 멀쩡한데 포톤이 문제라서 서버 연결이 안될 땐???
            AddOnConnectedToMasterCallback(RandomJoinNormalRoom);
            JoinToMaster();
        }
    }

    ///////////////////////////////////////////// AI ///////////////////////////////////////////////////////////////
    public void InvokeJoinAIRoom(float t)
    {
        if (!isMatching) return;
        Debug.LogWarning("InvokeJoinAIRoom");
        Invoke("JoinNormalAIRoomWithCheckPhoton", t);
    }

    private void JoinNormalAIRoomWithCheckPhoton()
    {
        if (!isMatching) return;

        Debug.LogWarning("JoinNormalAIRoomWithCheckPhoton");
        //isMatching = true;
        AllClearCallbackDelegate();

        lobbyType = new TypedLobby("AILobby", LobbyType.SqlLobby);

        if (!PhotonNetwork.IsConnected)
        {
            // 서버 연결완료 콜백 // 에 AI 방 입장 함수 넣고 서버 연결 시도한다.
            AddOnConnectedToMasterCallback(JoinAIRoom);
            // 방 접속완료 콜백 // 게임 씬으로 넘어갔는지 체크하는 코루틴.
            AddOnJoinedRoomCallback(() => { StartCoroutine("WaitGameStart"); },
                    () => { SceneManager.LoadScene("MainGameScene"); });
            JoinToMaster();
        }
        else
        {
            AddOnDisconnectedCallback(JoinNormalAIRoomWithCheckPhoton);
            PhotonNetwork.Disconnect();

            //if (PhotonNetwork.InRoom)
            //{
            //    // AI 매칭 전 이미 방에 들어가있는 상태라면?
            //    // 방에서 나왔을 때 콜백 // 방에서 나온 뒤에 AIRoom 입장.
            //    AddOnLeftRoomCallback(JoinAIRoom);
            //    // 방 접속완료 콜백 // 게임 씬으로 넘어갔는지 체크하는 코루틴.
            //    AddOnJoinedRoomCallback(
            //        () => { StartCoroutine("WaitGameStart"); },
            //        () => { SceneManager.LoadScene("MainGameScene"); });
            //    // 방에서 나온다.
            //    PhotonNetwork.LeaveRoom();
            //}
            //else
            //{
            //    JoinAIRoom();
            //    AddOnJoinedRoomCallback(
            //        () => { StartCoroutine("WaitGameStart"); },
            //        () => { SceneManager.LoadScene("MainGameScene"); });
            //}
        }
    }

    private void JoinAIRoom()
    {
        //PhotonNetwork.singleMode = true;
        //
        //roomOptions = new RoomOptions();
        //roomOptions.IsVisible = true;
        //roomOptions.MaxPlayers = 1;
        //roomOptions.PlayerTtl = 5000;
        //roomOptions.EmptyRoomTtl = 0;
        //roomOptions.CleanupCacheOnLeave = false;
        //
        //PhotonNetwork.EnterSingleRoom("AIRoom", roomOptions, true);
    }

    //////////////////////////////// 랭크 //////////////////////////////////////////////////////////////////////

    public void JoinRankRoomWithCheckPhoton(long _mmr, long _mmrWidth)
    {
        if (!isMatching) return;

        Debug.LogWarning("JoinRankRoomWithCheckPhoton");
        //isMatching = true;
        AllClearCallbackDelegate();

        // 일반게임 매칭 돌리는데 방을 못찾았을 경우 콜백.
        AddOnJoinRandomFailedCallback(RandomJoinRankRoomFailed);

        lobbyType = new TypedLobby("RankLobby", LobbyType.SqlLobby);

        if (!PhotonNetwork.IsConnected)
        {
            // 서버 연결완료 콜백 // 에 방 찾는 함수 넣고 서버 연결 시도한다.
            AddOnConnectedToMasterCallback(() => { RandomJoinRankRoom(_mmr, _mmrWidth); });
            // 방 접속완료 콜백 // 게임 씬으로 넘어갔는지 체크하는 코루틴.
            AddOnJoinedRoomCallback(() => { StartCoroutine("WaitGameStart"); });
            JoinToMaster();
        }
        else
        {
            // 정상이면 서버에 연결 중인 경우는 없다. 안전하게 서버 연결 끊고 함수 다시 부른다.
            AddOnDisconnectedCallback(() => { JoinRankRoomWithCheckPhoton(_mmr, _mmrWidth); });
            PhotonNetwork.Disconnect();
        }
    }

    private string MakeMmrSql(long _mmr, long _mmrWidth, long _maxRankMatchingCount, long _rankMatchingCount)
    {
        long lowMmr = _mmr - _mmrWidth / maxRankMatchingCount * rankMatchingCount;
        long highMmr = _mmr + _mmrWidth / maxRankMatchingCount * rankMatchingCount;
        string lowMmrStr = lowMmr.ToString();
        string highMmrStr = highMmr.ToString();

        return "C0 = 0 AND C1 < " + highMmrStr + " AND C1 > " + lowMmrStr;
    }

    private void RandomJoinRankRoom(long _mmr, long _mmrWidth)
    {
        mmr = _mmr;
        mmrWidth = _mmrWidth;

        RandomJoinRankRoom(MakeMmrSql(_mmr, _mmrWidth, maxRankMatchingCount, rankMatchingCount));
    }

    private void RandomJoinRankRoom(string _sqlLobbyFilter)
    {
        rankMatchingCount++;

        PhotonNetwork.JoinRandomRoom(null, 2, MatchmakingMode.RandomMatching, lobbyType, _sqlLobbyFilter);

    }

    private void MakeRankRoom()
    {
        Debug.LogWarning("MakeRankRoom");
        AllClearCallbackDelegate();

        // 방 만든 사람한테만 인원체크 후 게임시작 콜백 넣으면 ??
        AddOnPlayerEnteredRoomCallback(PlayerNumberCheck);

        roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        roomOptions.PlayerTtl = 5000;
        roomOptions.EmptyRoomTtl = 0;
        roomOptions.CleanupCacheOnLeave = false;

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "C0", 0 }};
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "C0", "C1", "C2" };

        currentRoomProperty = roomOptions.CustomRoomProperties;

        PhotonNetwork.CreateRoom(null, roomOptions, lobbyType);
    }

    private void RandomJoinRankRoomFailed()
    {
        AllClearCallbackDelegate();

        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonNetwork.InRoom)
            {
                // 일반 게임 방 찾기 실패 했을 때, 서버에 정상적으로 연결 중이면 대기 중인 방이 없는 것. 내가 방 만든다.
                MakeRankRoom();
            }
            else
            {
                // 방에 들어가있는 상태라면 뭔가 잘못된 것. 서버 연결 끊고 일반 매칭 처음부터 다시 시도.
                AddOnDisconnectedCallback(() => { JoinRankRoomWithCheckPhoton(mmr, mmrWidth); });
                PhotonNetwork.Disconnect();
            }
        }
        else
        {
            // 방 찾기 실패 했는데 서버 연결이 안돼있으면 // 서버 연결완료 콜백 // 에 일반게임 랜덤매칭 함수 넣고 서버 연결 시도. 연결 될 때까지 반복.
            // 네트워크 멀쩡한데 포톤이 문제라서 서버 연결이 안될 땐???
            AddOnConnectedToMasterCallback(() => { RandomJoinRankRoom(mmr, mmrWidth); });
            JoinToMaster();
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////// 커스텀 ///////////////////////////////////////////

    public void MakeCustomkRoomWithCheckPhoton()
    {
        //if (!isMatching) return;
        //
        //Debug.LogWarning("MakeCustomkRoomWithCheckPhoton");
        ////isMatching = true;
        //isMakeCustomRoom = true;
        //
        //int random = UnityEngine.Random.Range(10, 100);
        //customRoomName = (DBManager.GetInstance.userData.usercount + random * 10000).ToString();
        //
        //AllClearCallbackDelegate();
        //
        //lobbyType = new TypedLobby("CustomLobby", LobbyType.SqlLobby);
        //
        //if (!PhotonNetwork.IsConnected)
        //{
        //    // 서버 연결완료 콜백 // 에 방 만드는 함수 넣고 서버 연결 시도한다.
        //    AddOnConnectedToMasterCallback(MakeCustomRoom);
        //    // 방 접속완료 콜백 // 게임 씬으로 넘어갔는지 체크하는 코루틴.
        //    AddOnJoinedRoomCallback(() => { StartCoroutine("WaitGameStart"); });
        //    JoinToMaster();
        //}
        //else
        //{
        //    // 정상이면 서버에 연결 중인 경우는 없다. 안전하게 서버 연결 끊고 함수 다시 부른다.
        //    AddOnDisconnectedCallback(MakeCustomkRoomWithCheckPhoton);
        //    PhotonNetwork.Disconnect();
        //}
    }
    public void MakeCustomRoom()
    {
        Debug.LogWarning("MakeCustomRoom");
        AllClearCallbackDelegate();

        AddOnPlayerEnteredRoomCallback(PlayerNumberCheck);

        roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        roomOptions.PlayerTtl = 5000;
        roomOptions.EmptyRoomTtl = 0;
        roomOptions.CleanupCacheOnLeave = false;

        PhotonNetwork.CreateRoom(customRoomName, roomOptions, lobbyType);
    }

    public void JoinCustomRoomWithCheckPhoton(string roomName)
    {
        if (!isMatching) return;

        Debug.LogWarning("JoinCustomRoomWithCheckPhoton");
        //isMatching = true;
        isMakeCustomRoom = false;
        customRoomName = roomName;

        AllClearCallbackDelegate();

        AddOnJoinRoomFailedCallback(JoinCustomRoomFailed);

        lobbyType = new TypedLobby("CustomLobby", LobbyType.SqlLobby);

        if (!PhotonNetwork.IsConnected)
        {
            // 서버 연결완료 콜백 // 에 방 입장 함수 넣고 서버 연결 시도한다.
            AddOnConnectedToMasterCallback(() => { JoinCustomRoom(customRoomName); });
            // 방 접속완료 콜백 // 게임 씬으로 넘어갔는지 체크하는 코루틴.
            AddOnJoinedRoomCallback(() => { StartCoroutine("WaitGameStart"); });
            JoinToMaster();
        }
        else
        {
            // 정상이면 서버에 연결 중인 경우는 없다. 안전하게 서버 연결 끊고 함수 다시 부른다.
            AddOnDisconnectedCallback(() => { JoinCustomRoomWithCheckPhoton(customRoomName); });
            PhotonNetwork.Disconnect();
        }
    }

    private void JoinCustomRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    private void JoinCustomRoomFailed()
    {
        joinCustomRoomFailed?.Invoke();
        MatchingCancel();
    }

    ////////////////////////// 튜토리얼 ///////////////////////////////
    public void MakeTutorialRoomWithCheckPhoton()
    {
        AllClearCallbackDelegate();

        if (!PhotonNetwork.IsConnected)
        {
            AddOnConnectedToMasterCallback(JoinTutorialRoom);
            AddOnJoinedRoomCallback(
                    () => { StartCoroutine("WaitGameStart"); },
                    () => { SceneManager.LoadScene("TutorialScene"); });

            JoinToMaster();
}
        else
        {
            AddOnDisconnectedCallback(MakeTutorialRoomWithCheckPhoton);
            PhotonNetwork.Disconnect();
        }
    }

    private void JoinTutorialRoom()
    {
        //PhotonNetwork.singleMode = true;
        //
        //roomOptions = new RoomOptions();
        //roomOptions.IsVisible = true;
        //roomOptions.MaxPlayers = 1;
        //roomOptions.PlayerTtl = 5000;
        //roomOptions.EmptyRoomTtl = 0;
        //roomOptions.CleanupCacheOnLeave = false;
        //
        //PhotonNetwork.EnterSingleRoom("Tutorial", roomOptions, true);
    }


    /// ////////////////////////////////////////////////

    private void PlayerNumberCheck(Player newPlayer)
    {
        if (PhotonNetwork.PlayerList.Length == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (isGameStart)
            {
                return;
            }

            PhotonConnection.GetInstance.InitializeLoadSceneParam(PhotonNetwork.PlayerList.Length);

            CancelInvoke("JoinNormalAIRoomWithCheckPhoton");

            isGameStart = true;
            isMatching = false;

            currentRoomProperty["C0"] = true;

            PhotonNetwork.CurrentRoom.SetCustomProperties(currentRoomProperty);

            PhotonNetwork.LoadLevel("MainGameScene");

            if (newPlayer != PhotonNetwork.LocalPlayer)
            {
                enemyMmr = (long)newPlayer.CustomProperties["mmr"];
            }

            Debug.LogWarning("Matching Complete");
        }
    }

    IEnumerator WaitGameStart()
    {
        // 게임 씬으로 넘어가면 isGameStart = true;
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name != "MainLobbyScene");

        CancelInvoke("JoinNormalAIRoomWithCheckPhoton");

        isGameStart = true;
        isMatching = false;
    }

    public void LeaveMatch()
    {
        //if (PhotonNetwork.IsConnected)
        //{
        //    AddOnDisconnectedCallback(
        //        () =>
        //        {
        //            LoadingScene.SceneLoad("MainLobbyScene", false);
        //            isGameStart = false;
        //            internetEntireTerm = 0f;
        //        });
        //    PhotonNetwork.Disconnect();
        //}
        //else
        //{
        //    LoadingScene.SceneLoad("MainLobbyScene", false);
        //    isGameStart = false;
        //    internetEntireTerm = 0f;
        //}
    }

    public void MatchingCancel(bool pause = false)
    {
        Debug.LogWarning("MatchingCancel");

        AllClearCallbackDelegate();

        if (pause == false)
        {
            CancelInvoke("JoinNormalAIRoomWithCheckPhoton");

            isMatching = false;
        }

        StopCoroutine("WaitGameStart");
        StopCoroutine("ServerReconnect");

        if (PhotonNetwork.IsConnected)
        {
            // 방을 만들어놓은 상태라면 방을 닫아놓고 나가야 그 사이에 다른사람이 들어오는 걸 방지한다. ttl 0으로 해서 상관 없을 것 같긴함.
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.PlayerList.Length == 1)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
            }

            PhotonNetwork.Disconnect();
        }
    }


    public void LogInPhoton(string _name, string _userId, long _mmr)
    {
        nickname = _name;
        uid = _userId;
        mmr = _mmr;
    }


    IEnumerator CheckInternet()
    {
        while (true)
        {
            yield return null;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                internetTerm += Time.deltaTime;
            }
            else
            {
                internetTerm = 0f;
            }

            if (SceneManager.GetActiveScene().name == "MainLobbyScene")
            {
                if (internetMaxTerm < internetTerm)
                {
                    //UIManager.GetInstance.NetworkNotReachable();
                }
            }
            else if (SceneManager.GetActiveScene().name == "MainGameScene")
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    internetEntireTerm += Time.deltaTime;
                }

                if (internetMaxTerm < internetTerm || internetEntireMaxTerm < internetEntireTerm)
                {
                    //MainGameManager.GetInstance.NetworkNotReachable();
                }
            }
            else if (SceneManager.GetActiveScene().name == "TutorialScene")
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    internetEntireTerm += Time.deltaTime;
                }

                if (internetMaxTerm < internetTerm || internetEntireMaxTerm < internetEntireTerm)
                {
                    //TutorialManager.GetInstance.NetworkNotReachable();
                }
            }

            // 튜토리얼 씬 추가
        }
    }

    private IEnumerator ServerReconnect()
    {
        Debug.LogWarning("Server Reconnect");

        if (isGameStart)
        {
            if (PhotonNetwork.ReconnectAndRejoin())     // 포톤 서버 재연결 및 참여하고 있던 방으로 재접속.
            {
                // 연결 성공
                Debug.LogWarning("Successful reconnected and joined!");
                yield break;
            }
            else
            {
                // 실패
                //MainGameManager.GetInstance.NetworkNotReachable();
                yield break;
            }

        }
        else if (!isPause && isMatching)
        {
            // 일반, 랭크 구분한 뒤 처음부터 실행. makeCount 초기화 안함.
            if (lobbyType.Name == "NormalLobby")
            {
                JoinNormalRoomWithCheckPhoton();
            }
            else if (lobbyType.Name == "RankLobby")
            {
                JoinRankRoomWithCheckPhoton(mmr, mmrWidth);
            }
            else if (lobbyType.Name == "CustomLobby")
            {
                if (isMakeCustomRoom)
                {
                    MakeCustomkRoomWithCheckPhoton();
                }
                else
                {
                    JoinCustomRoomWithCheckPhoton(customRoomName);
                }
            }
            else if (lobbyType.Name == "AILobby")
            {
                JoinNormalAIRoomWithCheckPhoton();
            }
        }
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            isPause = true;

            // 앱 비활성화
            if (isMatching)
            {
                // 매칭 캔슬, 딜리게이트 클리어, 포톤 연결해제, makeCount, isMatching 초기화 안함.
                MatchingCancel(true);
            }
        }
        else
        {
            // 앱 활성화
            if (isMatching)
            {
                // 일반, 랭크 구분한 뒤 처음부터 실행. makeCount 초기화 안함.
                if (lobbyType.Name == "NormalLobby")
                {
                    JoinNormalRoomWithCheckPhoton();
                }
                else if (lobbyType.Name == "RankLobby")
                {
                    JoinRankRoomWithCheckPhoton(mmr, mmrWidth);
                }
                else if (lobbyType.Name == "CustomLobby")
                {
                    if (isMakeCustomRoom)
                    {
                        // 내가 만들어놓고 홈키 눌렀음.
                        MakeCustomkRoomWithCheckPhoton();
                    }
                    else
                    {
                        JoinCustomRoomWithCheckPhoton(customRoomName);
                    }
                }
                else if (lobbyType.Name == "AILobby")
                {
                    JoinNormalAIRoomWithCheckPhoton();
                }
            }
        }
    }
}
