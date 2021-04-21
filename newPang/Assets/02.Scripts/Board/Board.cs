using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    int m_Row;
    int m_Col;

    public int maxRow { get { return m_Row; } }
    public int maxCol { get { return m_Col; } }

    Cell[,] m_Cells;
    public Cell[,] cells { get { return m_Cells; } }

    Block[,] m_Blocks;
    public Block[,] blocks { get { return m_Blocks; } }

    public void InitBoard(int row, int col)
    {
        m_Row = row;
        m_Col = col;

        m_Cells = new Cell[row, col];
        m_Blocks = new Block[row, col];
    }

    public void SetCellPosition()
    {
        float initX = CalcInitX(0.5f);
        float initY = CalcInitY(0.5f);
        for (int nRow = 0; nRow < m_Row; nRow++)
            for (int nCol = 0; nCol < m_Col; nCol++)
            {
                //3.1 Cell GameObject 생성을 요청한다.GameObject가 생성되지 않는 경우에 null을 리턴한다.
                Cell cell = m_Cells[nRow, nCol];
                cell?.Move(initX + nCol, initY + nRow);

                //3.2 Block GameObject 생성을 요청한다.
                //    GameObject가 생성되지 않는 경우에 null을 리턴한다. EMPTY 인 경우에 null
                Block block = m_Blocks[nRow, nCol];
                block?.SetCellPosition(nRow, nCol);
                block?.Move(initX + nCol, initY + nRow);
            }
    }

    public float CalcInitX(float offset = 0)
    {
        return -m_Col / 2.0f + offset;
    }

    //퍼즐의 시작 Y 위치, left - bottom 좌표
    //하단이 (0, 0) 이므로, 
    public float CalcInitY(float offset = 0)
    {
        return -m_Row / 2.0f + offset;
    }
}
