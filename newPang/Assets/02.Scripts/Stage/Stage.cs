using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public Board board;
    public int maxRow { get { return board.maxRow; } }
    public int maxCol { get { return board.maxCol; } }

    int m_BlockCount;
    public int blockCount { get { return m_BlockCount; } }
    public Block[,] blocks { get { return board.blocks; } }

    InputManager m_InputManager;

    //Event Members
    bool m_bTouchDown;          //입력상태 처리 플래그, 유효한 블럭을 클릭한 경우 true
    BlockPos m_BlockDownPos;    //블럭 인덱스 (보드에 저장된 위치)
    Vector3 m_ClickPos;

    public void InitStage(StageInfo _stageInfo)
    {
        board.InitBoard(_stageInfo.row, _stageInfo.col);
        m_BlockCount = _stageInfo.blockCount;

        m_InputManager = new InputManager(transform);
    }

    private void Update()
    {
        OnInputHandler();
    }

    void OnInputHandler()
    {
        //1. Touch Down 
        if (!m_bTouchDown && m_InputManager.isTouchDown)
        {
            //1.1 보드 기준 Local 좌표를 구한다.
            Vector2 point = m_InputManager.touch2BoardPosition;

            //1.2 Play 영역(보드)에서 클릭하지 않는 경우는 무시
            if (!IsInsideBoard(point))
                return;

            //1.3 클릭한 위치의 블럭이 스왑 가능한지.
            BlockPos blockPos;
            if (IsOnValideBlock(point, out blockPos))
            {
                //1.3.1 유효한(스와이프 가능한) 블럭에서 클릭한 경우
                m_bTouchDown = true;        //클릭 상태 플래그 ON
                m_BlockDownPos = blockPos;  //클릭한 블럭의 위치(row, col) 저장
                m_ClickPos = point;         //클릭한 Local 좌표 저장
                                            //Debug.Log($"Mouse Down In Board : (blockPos})");

            }
        }
        //2. Touch UP : 유효한 블럭 위에서 Down 후에만 UP 이벤트 처리
        else if (m_bTouchDown)// && m_InputManager.isTouchUp)
        {
            //2.1 보드 기준 Local 좌표를 구한다.
            Vector2 point = m_InputManager.touch2BoardPosition;

            //2.2 스와이프 방향을 구한다.
            Swipe swipeDir = m_InputManager.EvalSwipeDir(m_ClickPos, point);

            //Debug.Log($"Swipe : {swipeDir} , Block = {m_BlockDownPos}");

            if (swipeDir != Swipe.NA)
            {
                board.DoSwipeAction(m_BlockDownPos.row, m_BlockDownPos.col, swipeDir);
                m_bTouchDown = false;   //클릭 상태 플래그 OFF
            }

        }

        if (m_bTouchDown && m_InputManager.isTouchUp)
        {
            m_bTouchDown = false;   //클릭 상태 플래그 OFF
        }
    }

    /*
         * 보드안에서 발생한 이벤트인지 체크한다       
         */
    public bool IsInsideBoard(Vector2 ptOrg)
    {
        // 계산의 편의를 위해서 (0, 0)을 기준으로 좌표를 이동한다. 
        // 8 x 8 보드인 경우: x(-4 ~ +4), y(-4 ~ +4) -> x(0 ~ +8), y(0 ~ +8) 
        Vector2 point = new Vector2(ptOrg.x + (maxCol / 2.0f), ptOrg.y + (maxRow / 2.0f));

        if (point.y < 0 || point.x < 0 || point.y > maxRow || point.x > maxCol)
            return false;

        return true;
    }

    public bool IsOnValideBlock(Vector2 point, out BlockPos blockPos)
    {
        //1. World 좌표 -> 보드의 블럭 인덱스로 변환한다.
        Vector2 pos = new Vector2(point.x + (maxCol / 2.0f), point.y + (maxRow / 2.0f));
        int nRow = (int)pos.x;
        int nCol = (int)pos.y;

        //리턴할 블럭 인덱스 생성
        blockPos = new BlockPos(nRow, nCol);

        //2. 스와이프 가능한 블럭인지 체크한다.
        if (blocks[nRow, nCol] == null) 
            return false;

        return blocks[nRow, nCol].IsSwipeable();
    }
}
