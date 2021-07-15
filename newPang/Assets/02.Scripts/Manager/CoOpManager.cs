using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CoOpManager : SingletonClass<CoOpManager>
{
    public PhotonView pv;
    int targetScore;

    protected override void Awake()
    {
        pv = GetComponent<PhotonView>();

        MainGameManager.GetInstance.PlayerCharacterCreate();
    }

}
