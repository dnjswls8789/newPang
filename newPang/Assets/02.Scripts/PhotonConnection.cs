using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonConnection : SingletonClass<PhotonConnection>
{
    PhotonView pv;

    bool[] loadSceneParam;

    protected override void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public void InitializeLoadSceneParam(int _size)
    {
        loadSceneParam = new bool[_size];
    }

    public void LoadSceneComplete() //로딩 완료되면 호출
    {
        pv.RPC("SetLoadSceneParam", RpcTarget.All, true, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    public void SetLoadSceneParam(bool _isComplete, int _actorNumber)
    {
        loadSceneParam[_actorNumber] = _isComplete;
    }

    public bool IsAllCompleteLoadScene()
    {
        bool _check = true;

        foreach(bool _b in loadSceneParam)
        {
            if (_b == false)
            {
                _check = false;
            }
        }

        return _check;
    }
}
