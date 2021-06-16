using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class FirebaseGoogleAuth : MonoBehaviour
{
    public Text email;

    void Start()
    {
        PlayGamesPlatform.InitializeInstance(
 new PlayGamesClientConfiguration.Builder().Build());
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        // Google Play Games 활성화
    }


    public void TryGoogleLogin()
    {
        if (!Social.localUser.authenticated) // 로그인 되어 있지 않다면
        {
            Social.localUser.Authenticate(success => // 로그인 시도
            {
                if (success) // 성공하면
                {
                    email.text = Social.localUser.userName;
                }
                else // 실패하면
                {
                    email.text = "Fail...";
                }
            });
        }
    }


    public void TryGoogleLogout()
    {
        if (Social.localUser.authenticated) // 로그인 되어 있다면
        {
            PlayGamesPlatform.Instance.SignOut(); // 로그아웃
            email.text = "Logout";
        }
    }
}