using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class Stage : MonoBehaviour
{
    public PhotonView pv;

    public Board board;
    public int maxRow { get { return board.maxRow; } }
    public int maxCol { get { return board.maxCol; } }

    int m_BlockCount;
    public int blockCount { get { return m_BlockCount; } }
    public Block[,] blocks { get { return board.blocks; } }

    InputManager m_InputManager;

    bool swipable;

    ////Event Members
    bool m_bTouchDown;          //입력상태 처리 플래그, 유효한 블럭을 클릭한 경우 true
    BlockPos m_BlockDownPos;    //블럭 인덱스 (보드에 저장된 위치)
    Vector3 m_ClickPos;

    // 터치
    bool m_TouchDown;
    //Vector3 m_ClickPos;
    Vector3 m_CurPos;
    Cell m_ClickCell;

    int pointerId;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

#if UNITY_EDITOR
        pointerId = -1;
#elif UNITY_IOS || UNITY_ANDROID
        pointerId = 0;
#endif
    }

    [PunRPC]
    public void InitStage(int row, int col, int blockCount)
    {
        if (MainGameManager.GetInstance.IsCoOpHost())
        {
            pv.RPC("InitStage", RpcTarget.Others, row, col, blockCount);
        }

        board.InitBoard(row, col);
        m_BlockCount = blockCount;

        m_InputManager = new InputManager(transform);
    }

    private void Update()
    {
        //OnInputHandler();

        InputUpdate();
    }

    void InputUpdate()
    {
        if (!m_TouchDown)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject(pointerId)) return;

                Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit hit;
                int rayerMask = 1 << LayerMask.NameToLayer("Cell");

                if (Physics.Raycast(clickPos, Vector3.forward, out hit, 100f, rayerMask))
                {
                    //해당 터치가 시작됐다면.
                    m_ClickPos = clickPos;
                    m_CurPos = m_ClickPos;
                    m_TouchDown = true;

                    m_ClickCell = hit.transform.GetComponent<Cell>();
                }
            }
        }
        else
        {
            m_CurPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Swipe swipeDir = EvalSwipeDir(m_ClickPos, m_CurPos);

            if (swipeDir != Swipe.NA)
            {
                if (m_ClickCell != null)
                {
                    if (MainGameManager.GetInstance.IsCoOpRemote())
                    {
                        //board.DoSwipeActionToHost(m_ClickCell.cellPosition.x, m_ClickCell.cellPosition.y, swipeDir);
                        board.pv.RPC("DoSwipeActionToRemote", RpcTarget.Others, m_ClickCell.cellPosition.x, m_ClickCell.cellPosition.y, swipeDir);
                    }
                    else
                    {
                        board.DoSwipeActionToRemote(m_ClickCell.cellPosition.x, m_ClickCell.cellPosition.y, swipeDir);
                    }
                }
                m_TouchDown = false;   //클릭 상태 플래그 OFF
            }

        }

        if (Input.GetMouseButtonUp(0))
        {
            m_ClickPos = Vector3.zero;
            m_CurPos = Vector3.zero;
            m_TouchDown = false;
            m_ClickCell = null;
        }

    }

    //void OnInputHandler()
    //{
    //    //1. Touch Down 
    //    if (!m_bTouchDown && m_InputManager.isTouchDown)
    //    {
    //        //1.1 보드 기준 Local 좌표를 구한다.
    //        Vector2 point = m_InputManager.touch2BoardPosition;
    //
    //        //1.2 Play 영역(보드)에서 클릭하지 않는 경우는 무시
    //        if (!IsInsideBoard(point))
    //            return;
    //
    //        //1.3 클릭한 위치의 블럭이 스왑 가능한지.
    //        BlockPos blockPos;
    //        if (IsOnValideBlock(point, out blockPos))
    //        {
    //            //1.3.1 유효한(스와이프 가능한) 블럭에서 클릭한 경우
    //            m_bTouchDown = true;        //클릭 상태 플래그 ON
    //            m_BlockDownPos = blockPos;  //클릭한 블럭의 위치(row, col) 저장
    //            m_ClickPos = point;         //클릭한 Local 좌표 저장
    //                                        //Debug.Log($"Mouse Down In Board : (blockPos})");
    //
    //        }
    //    }
    //    //2. Touch UP : 유효한 블럭 위에서 Down 후에만 UP 이벤트 처리
    //    else if (m_bTouchDown)// && m_InputManager.isTouchUp)
    //    {
    //        //2.1 보드 기준 Local 좌표를 구한다.
    //        Vector2 point = m_InputManager.touch2BoardPosition;
    //
    //        //2.2 스와이프 방향을 구한다.
    //        Swipe swipeDir = m_InputManager.EvalSwipeDir(m_ClickPos, point);
    //
    //        //Debug.Log($"Swipe : {swipeDir} , Block = {m_BlockDownPos}");
    //
    //        if (swipeDir != Swipe.NA)
    //        {
    //            board.DoSwipeAction(m_BlockDownPos.row, m_BlockDownPos.col, swipeDir);
    //            m_bTouchDown = false;   //클릭 상태 플래그 OFF
    //        }
    //
    //    }
    //
    //    if (m_bTouchDown && m_InputManager.isTouchUp)
    //    {
    //        m_bTouchDown = false;   //클릭 상태 플래그 OFF
    //    }
    //}

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

    public Swipe EvalSwipeDir(Vector2 vtStart, Vector2 vtEnd)
    {
        float angle = EvalDragAngle(vtStart, vtEnd);
        if (angle < 0)
            return Swipe.NA;

        int swipe = ((int)(angle + 45)) % 360 / 90;

        switch (swipe)
        {
            case 0: return Swipe.RIGHT;
            case 1: return Swipe.UP;
            case 2: return Swipe.LEFT;
            case 3: return Swipe.DOWN;
        }

        return Swipe.NA;
    }

    /*
     * 두 포인트 사이의 각도를 구한다.
     * Input(마우스, 터치) 장치 드래그시에 드래그한 각도를 구하는데 활용한다.
     */
    float EvalDragAngle(Vector2 vtStart, Vector2 vtEnd)
    {
        Vector2 dragDirection = vtEnd - vtStart;
        if (dragDirection.magnitude <= 0.3f)    // 스왑인식 거리
            return -1f;

        //dragDirection.Normalize();
        //Debug.Log($"eval angle : {vtStart} , {vtEnd}, magnitude = {dragDirection.magnitude}");

        float aimAngle = Mathf.Atan2(dragDirection.y, dragDirection.x);

        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        return aimAngle * Mathf.Rad2Deg;
    }
}
