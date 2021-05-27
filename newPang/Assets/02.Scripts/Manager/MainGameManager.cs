using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum GameType
{
    Battle,
    CoOp
}
public class MainGameManager : SingletonClass<MainGameManager>
{
    public PhotonView pv;

    public GameType gameType;

    StageBuilder stageBuilder;
    public Stage stage;
    public Board board;

    protected override void Awake()
    {
        pv = GetComponent<PhotonView>();

        ObjectPoolManager.GetInstance.LoadAllGameObject();
        ObjectPoolManager.GetInstance.InitializeObjPool("Block", 1, 1, 200);
        ObjectPoolManager.GetInstance.InitializeObjPool("Cell", 1, 1, 100);

        BuildStage(1);

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
}
