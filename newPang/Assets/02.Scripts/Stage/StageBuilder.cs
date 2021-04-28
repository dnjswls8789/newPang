using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageBuilder
{
    StageInfo m_StageInfo;
    int m_nStage;

    public StageBuilder(int nStage)
    {
        m_nStage = nStage;
    }

    /// <summary>
    /// 주어진 크기의 Stage를 생성하고,  Stage를 구성하는 보드의 Cell과 Block을 구성한다
    /// </summary>
    /// <returns>생성된 Stage 객체</returns>
    public void ComposeStage(Stage stage)
    {
        Debug.Assert(m_nStage > 0, $"Invalide Stage : {m_nStage}");

        //0. 스테이지 정보를 로드한다.(보드 크기, Cell/블럭 정보 등)
        m_StageInfo = LoadStage(m_nStage);

        stage.InitStage(m_StageInfo);

        //2. Cell,Block 초기 값을 생성한다.
        for (int nCol = 0; nCol < m_StageInfo.col; nCol++)
        {
            for (int nRow = 0; nRow < m_StageInfo.row; nRow++)
            {
                stage.board.blocks[nRow, nCol] = SpawnBlockForStage(nRow, nCol);
                stage.board.cells[nRow, nCol] = SpawnCellForStage(nRow, nCol);

                stage.board.blocks[nRow, nCol].UpdateView();
                stage.board.cells[nRow, nCol].UpdateView();
            }
        }
    }

    /// <summary>
    /// 스테이지 구성을 위해서 구성정보를 로드한다. 
    /// </summary>
    /// <param name="nStage">스테이지 번</param>
    public StageInfo LoadStage(int nStage)
    {
        StageInfo stageInfo = StageReader.LoadStage(nStage);
        if (stageInfo != null)
        {
            Debug.Log(stageInfo.ToString());
        }

        return stageInfo;
    }

    /// <summary>
    /// 지정된 위치에 적합한 Block 객체를 생성한다. 
    /// </summary>
    /// <param name="nRow">행</param>
    /// <param name="nCol">열</param>
    /// <returns></returns>
    Block SpawnBlockForStage(int nRow, int nCol)
    {
        if (m_StageInfo.GetCellType(nRow, nCol) == CellType.EMPTY)
            return SpawnEmptyBlock();

        return SpawnBlock();
    }

    /// <summary>
    /// 지정된 위치에 적합한 Cell 객체를 생성한다.
    /// </summary>
    /// <param name="nRow"></param>
    /// <param name="nCol"></param>
    /// <returns></returns>
    Cell SpawnCellForStage(int nRow, int nCol)
    {
        Debug.Assert(m_StageInfo != null);
        Debug.Assert(nRow < m_StageInfo.row && nCol < m_StageInfo.col);

        return CellFactory.SpawnCell(m_StageInfo, nRow, nCol);
    }

    /// <summary>
    /// 기본형 블럭을 요청한다.
    /// </summary>
    /// <returns>생성된 Block 객체</returns>
    public Block SpawnBlock(BlockBreed breed = BlockBreed.NA)
    {
        return BlockFactory.SpawnBlock(BlockType.BASIC, blockCount, breed);
    }

    /// <summary>
    /// BlockType.EMPTY인 블럭을 요청한다
    /// </summary>
    /// <returns>생성된 Block 객체</returns>
    public Block SpawnEmptyBlock()
    {
        Block newBlock = BlockFactory.SpawnBlock(BlockType.EMPTY, blockCount);

        return newBlock;
    }

    public int blockCount
    {
        get { return m_StageInfo.blockCount; }
    }
}
