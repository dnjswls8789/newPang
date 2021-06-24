using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class BattleSceneManager : SingletonClass<BattleSceneManager>
{
    public PhotonView pv;

    public const float winPoint = 3000000f;
    int playerScore;
    int enemyScore;

    [SerializeField] Transform playerLocator;
    [SerializeField] Transform enemyLocator;

    [SerializeField] Slider battleSlider;
    [SerializeField] Text playerScoreText;
    [SerializeField] Text EnemyScoreText;

    //
    [SerializeField] GameObject resultTap;
    [SerializeField] Text winText;
    //

    protected override void Awake()
    {
        pv = GetComponent<PhotonView>();

        if (LobbyManager.GetInstance.lobbyName != "SingleLobby")
        {
            battleSlider.minValue = 0;
            battleSlider.maxValue = winPoint;
            battleSlider.value = battleSlider.maxValue * 0.5f;
        }
        else
        {
            battleSlider.gameObject.SetActive(false);
        }

        DataManager.GetInstance.ChangeCharacter(DataManager.GetInstance.userData.customs["character"], playerLocator);
    }

    private void Update()
    {
        playerScoreText.text = playerScore.ToString();
        EnemyScoreText.text = enemyScore.ToString();

        if (LobbyManager.GetInstance.lobbyName != "SingleLobby")
        {
            battleSlider.value = playerScore - enemyScore + battleSlider.maxValue * 0.5f;

            if (battleSlider.value >= winPoint)
            {
                resultTap.SetActive(true);
                winText.text = "승리";
            }
            else if (battleSlider.value <= 0)
            {
                resultTap.SetActive(true);
                winText.text = "패배";
            }
        }
    }

    [PunRPC]
    public void SetEnemyScore(int score)
    {
        enemyScore = score;
    }

    public void AddScore(int score)
    {
        playerScore += score;
        //playerScore += (int)(Mathf.Pow(MainGameManager.GetInstance.combo, 2f) * score);

        if (LobbyManager.GetInstance.lobbyName != "SingleLobby")
        {
            pv.RPC("SetEnemyScore", RpcTarget.Others, playerScore);
        }
    }

    public void QuitGame()
    {
        LobbyManager.GetInstance.AllClearCallbackDelegate();
        LobbyManager.GetInstance.LeaveMatch();
    }

}
