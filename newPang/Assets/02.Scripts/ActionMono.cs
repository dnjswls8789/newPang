using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ActionMono : MonoBehaviour
{
    public PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    [PunRPC]
    void Scale(float toScale, float duration)
    {
        StartCoroutine(Action2D.Scale(transform, toScale, duration));
    }

    [PunRPC]
    void MoveTo(Vector3 to, float duration)
    {
        StartCoroutine(Action2D.MoveTo(transform, to, duration));
    }
}
