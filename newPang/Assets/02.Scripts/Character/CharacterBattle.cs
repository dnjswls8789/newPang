using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterBattle : MonoBehaviour
{
    public PhotonView pv;
    Animator anim;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();

        anim.runtimeAnimatorController = Resources.Load("Animation/CharacterInGameAnim") as RuntimeAnimatorController;
    }

    [PunRPC]
    public void SetAnimInt(string paramName, int value)
    {
        if (IsIdle())
        {
            anim.SetInteger(paramName, value);
        }
    }

    public bool IsIdle()
    {
        return anim.GetBool("Idle");
    }

    public void PlayDashAnim()
    {

    }
}
