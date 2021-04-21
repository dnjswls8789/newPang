using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public Board m_Board;
    public Board board { get { return m_Board; } }
    public int maxRow { get { return m_Board.maxRow; } }
    public int maxCol { get { return m_Board.maxCol; } }

    int m_BlockCount;
    public int blockCount { get { return m_BlockCount; } }

    public void InitStage(StageInfo _stageInfo)
    {
        board.InitBoard(_stageInfo.row, _stageInfo.col);
        m_BlockCount = _stageInfo.blockCount;
    }
}
