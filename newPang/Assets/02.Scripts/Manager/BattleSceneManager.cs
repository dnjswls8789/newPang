using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneManager : SingletonClass<BattleSceneManager>
{
    StageBuilder stageBuilder;
    public Stage stage;
    public Board board;

    protected override void Awake()
    {
        ObjectPoolManager.GetInstance.LoadAllGameObject();
        BuildStage(1);
    }

    void BuildStage(int stageNumber)
    {
        stageBuilder = new StageBuilder(stageNumber); // 스테이지 번호
        stageBuilder.ComposeStage(stage);

        board.SetCellPosition();
        board.MatchingCheckShuffle();

        int a = 0;
        while (!board.PangCheck())
        {
            a++;
            board.AllShuffle();
            board.MatchingCheckShuffle();
        }
        Debug.LogError(a);

        Debug.LogError(board.PangCheck());
    }
}
