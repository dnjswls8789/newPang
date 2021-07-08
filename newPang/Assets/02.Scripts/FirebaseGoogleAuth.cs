using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class FirebaseGoogleAuth : MonoBehaviour
{
    public Text text;
    void Start()
    {
        PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .RequestEmail()
            .Build());
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        // 구글 플레이 게임 활성화

    }


    public void TryGoogleLogin()
    {
        Social.localUser.Authenticate((bool _bSuccess) =>
        {
            if (true == _bSuccess)
            {
                text.text = Social.localUser.id + "\n" + Social.localUser.userName;
            }
            else
            {
                text.text = "Google Login Fail";
            }
        });
    }


    public void TryGoogleLogout()
    {
        if (Social.localUser.authenticated) // 로그인 되어 있다면
        {
            PlayGamesPlatform.Instance.SignOut(); // Google 로그아웃
            text.text = "Google Logout";
        }
    }


}