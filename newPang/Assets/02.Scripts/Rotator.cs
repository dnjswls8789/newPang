using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Rotator : MonoBehaviour
{
    public Transform parent;
    PhotonView pv;

    public void Update()
    {
        if (parent == null) return;
        if (pv == null)
        {
            pv = GetComponent<PhotonView>();
        }

        if (pv != null)
        {
            if (pv.Owner != PhotonNetwork.LocalPlayer) return;
        }

        transform.position = parent.position;
    }

    private void OnDisable()
    {
        parent = null;
    }

}
