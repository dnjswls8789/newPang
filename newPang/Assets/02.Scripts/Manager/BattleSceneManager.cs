using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class BattleSceneManager : SingletonClass<BattleSceneManager>
{
    public PhotonView pv;

    StageBuilder stageBuilder;
    public Stage stage;
    public Board board;

    public const float winPoint = 30000f;
    int playerScore;
    int enemyScore;

    [SerializeField] Slider battleSlider;
    [SerializeField] Text scoreText;

    //
    [SerializeField] GameObject resultTap;
    [SerializeField] Text winText;
    //

    protected override void Awake()
    {
        pv = GetComponent<PhotonView>();

        ObjectPoolManager.GetInstance.LoadAllGameObject();
        ObjectPoolManager.GetInstance.InitializeObjPool("Block", 1, 1, 200);
        ObjectPoolManager.GetInstance.InitializeObjPool("Cell", 1, 1, 100);

        BuildStage(1);

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
    }

    void BuildStage(int stageNumber)
    {
        stageBuilder = new StageBuilder(stageNumber); // 스테이지 번호
        stageBuilder.ComposeStage(stage);

        board.SetCellPosition();
        board.MatchingCheckShuffle();

        while (!board.PangCheck())
        {
            board.AllShuffle();
            board.MatchingCheckShuffle();
        }
    }

    private void Update()
    {
        scoreText.text = playerScore.ToString();

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
