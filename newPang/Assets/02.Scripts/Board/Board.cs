using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public class TestBlocks
{
    public Block[] block;
}

public class Board : MonoBehaviour
{
    public PhotonView pv;
    public TestBlocks[] testBlocks;

    public GameObject cellParent;
    public GameObject blockParent;

    int m_Row;
    int m_Col;

    public int maxRow { get { return m_Row; } }
    public int maxCol { get { return m_Col; } }

    Cell[,] m_Cells;
    public Cell[,] cells { get { return m_Cells; } }

    Block[,] m_Blocks;
    public Block[,] blocks { get { return m_Blocks; } }
    

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public void InitBoard(int row, int col)
    {
        m_Row = row;
        m_Col = col;

        m_Cells = new Cell[row, col];
        m_Blocks = new Block[row, col];

        testBlocks = new TestBlocks[row];
        for (int i = 0; i < row; i++)
        {
            testBlocks[i] = new TestBlocks();
            testBlocks[i].block = new Block[col];
        }
    }

    private void Update()
    {
        if (MainGameManager.GetInstance.IsCoOpRemote()) return;

        BoardUpdate();

        ////////////////// 인스펙터에서 보기 위함 ///////////////////////
        for (int i = 0; i < maxCol; i++)
        {
            for (int j = 0; j < maxRow; j++)
            {
                testBlocks[j].block[i] = blocks[i, j];
            }
        }
        ////////////////// 나중에 지우기 ////////////////////////////////
        if (Input.GetKeyDown(KeyCode.A))
        {
            BoardShuffle();
        }
    }


    public void BoardUpdate()
    {
        for (int row = 0; row < maxRow; row++)
        {
            for (int col = 0; col < maxCol; col++)
            {
                blocks[row, col].BlockDropCheck();
            }
        }

        BlockSpawnAndDrop(PangCheck());
    }

    public void BlockSpawnAndDrop(bool pangCheck)
    {
        // 팡 가능 있으면 그냥 랜덤생성
        if (pangCheck)
        {
            for (int row = 0; row < maxRow; row++)
            {
                int clearCount = GetClearCount(row);

                float initX = CalcInitX(0.5f);
                float initY = CalcInitX(0.5f);

                for (int i = 0; i < clearCount; i++)
                {
                    Block spawnBlock = BlockFactory.SpawnBlock(BlockType.BASIC, MainGameManager.GetInstance.stage.blockCount);

                    spawnBlock.SetCellPosition(row, maxCol - 1 - i);
                    //blocks[spawnBlock.cellPosition.x, spawnBlock.cellPosition.y] = spawnBlock;
                    spawnBlock.Move(initX + row, maxCol - 1 + initY + clearCount - i);

                    spawnBlock.SpawnBlockDrop(clearCount - 1);
                }
            }
        }
        // 팡 가능 없으면 직접 브리드 값 넣기
        else
        {
            int spawnCount = 0;
            BlockBreed baseBreed = BlockBreed.NA;
            for (int row = 0; row < maxRow; row++)
            {
                int clearCount = GetClearCount(row);

                float initX = CalcInitX(0.5f);
                float initY = CalcInitX(0.5f);

                for (int i = 0; i < clearCount; i++)
                {
                    //int col = maxCol - 1 - i;
                    int col = maxCol - clearCount + i;
                    Block spawnBlock = null;
                    // 1 번째 생성
                    if (spawnCount == 0)
                    {
                        Block targetBlock = null;
                        // 왼쪽 비교.
                        if (row - 1 >= 0 && blocks[row - 1, col].IsMatchable())
                        {
                            targetBlock = blocks[row - 1, col];
                        }
                        // 오른쪽 비교
                        else if (row + 1 < maxRow && blocks[row + 1, col].IsMatchable())
                        {
                            targetBlock = blocks[row + 1, col];
                        }
                        // 아래쪽 비교
                        else if (col - 1 >= 0 && blocks[row, col - 1].IsMatchable())
                        {
                            targetBlock = blocks[row, col - 1];
                        }
                        // 블럭 떨어진 후 위쪽은 블럭이 없으니 안해도 됨.

                        if (targetBlock != null)
                        {
                            baseBreed = targetBlock.breed;

                            BlockBreed newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);

                            // 대상 블럭이랑 다른 블럭 생성.
                            while (baseBreed == newBreed)
                            {
                                newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                            }

                            spawnBlock = BlockFactory.SpawnBlock(BlockType.BASIC, MainGameManager.GetInstance.stage.blockCount, newBreed);
                            spawnCount++;

                        }
                        else
                        {
                            // 랜덤생성 하고 넘어가기.
                            spawnBlock = BlockFactory.SpawnBlock(BlockType.BASIC, MainGameManager.GetInstance.stage.blockCount);
                        }
                    }
                    // 2, 3 번째 생성
                    else if (spawnCount > 0 && spawnCount < 3)
                    {
                        // 2, 3 번째 블럭은 baseBreed 랑 같은 블럭 생성.
                        spawnBlock = BlockFactory.SpawnBlock(BlockType.BASIC, MainGameManager.GetInstance.stage.blockCount, baseBreed);
                        spawnCount++;

                    }
                    // 그 뒤는 랜덤
                    else
                    {
                        spawnBlock = BlockFactory.SpawnBlock(BlockType.BASIC, MainGameManager.GetInstance.stage.blockCount);
                        spawnCount++;

                    }


                    spawnBlock.SetCellPosition(row, col);
                    //blocks[spawnBlock.cellPosition.x, spawnBlock.cellPosition.y] = spawnBlock;
                    spawnBlock.Move(initX + row, initY + col + clearCount);

                    spawnBlock.SpawnBlockDrop(clearCount - 1);
                }
            }

        }

    }

    public int GetClearCount(int row)
    {
        int count = 0;

        for (int col = maxCol - 1; col >= 0; col--)
        {
            if (blocks[row, col].status == BlockStatus.CLEAR)
            {
                count++;
            }
            else if (!blocks[row, col].IsDropable())
            {
                break;
            }
        }
        return count;
    }

    public void SetCellPosition()
    {
        float initX = CalcInitX(0.5f);
        float initY = CalcInitY(0.5f);
        for (int nCol = 0; nCol < m_Col; nCol++)
        {
            for (int nRow = 0; nRow < m_Row; nRow++)
            {
                //3.1 Cell GameObject 생성을 요청한다.GameObject가 생성되지 않는 경우에 null을 리턴한다.
                Cell cell = m_Cells[nRow, nCol];
                cell?.SetCellPosition(nRow, nCol);
                cell?.Move(initX + nRow, initY + nCol);

                //3.2 Block GameObject 생성을 요청한다.
                //    GameObject가 생성되지 않는 경우에 null을 리턴한다. EMPTY 인 경우에 null
                Block block = m_Blocks[nRow, nCol];
                block?.SetCellPosition(nRow, nCol);
                block?.Move(initX + nRow, initY + nCol);
            }
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

    [PunRPC]
    public void DoSwipeActionToRemote(int nRow, int nCol, Swipe swipeDir)
    {
        StartCoroutine(CoDoSwipeActionToRemote(nRow, nCol, swipeDir));
    }

    [PunRPC]
    public void DoSwipeActionFromRemote(int nRow, int nCol, Swipe swipeDir)
    {
        StartCoroutine(CoDoSwipeActionFromRemote(nRow, nCol, swipeDir));
    }

    [PunRPC]
    public void DoSwipeActionToHost(int nRow, int nCol, Swipe swipeDir)
    {
        StartCoroutine(CoDoSwipeActionToHost(nRow, nCol, swipeDir));
    }

    IEnumerator CoDoSwipeActionToRemote(int nRow, int nCol, Swipe swipeDir)
    {
        int nSwipeRow = nRow, nSwipeCol = nCol;
        nSwipeRow += swipeDir.GetTargetRow(); //Right : +1, LEFT : -1
        nSwipeCol += swipeDir.GetTargetCol(); //UP : +1, DOWN : -1

        if (nSwipeRow < 0 || nSwipeRow >= maxRow || nSwipeCol < 0 || nSwipeCol >= maxCol)
        {
            yield break;
        }

        if (blocks[nRow, nCol] == null || blocks[nSwipeRow, nSwipeCol] == null)
        {
            yield break;
        }

        // 두 블럭 다 스왑 가능한지.
        if (blocks[nRow, nCol].IsSwipeable() && blocks[nSwipeRow, nSwipeCol].IsSwipeable())
        {
            Block baseBlock = blocks[nRow, nCol];
            Block targetBlock = blocks[nSwipeRow, nSwipeCol];

            Vector3 basePos = baseBlock.transform.position;
            Vector3 targetPos = targetBlock.transform.position;

            StartCoroutine(Action2D.MoveTo(baseBlock.transform, targetPos, Constants.SWIPE_DURATION));
            StartCoroutine(Action2D.MoveTo(targetBlock.transform, basePos, Constants.SWIPE_DURATION));

            if (MainGameManager.GetInstance.IsCoOpHost())
            {
                baseBlock.pv.RPC("RPCMoveToAction", RpcTarget.Others, targetPos, Constants.SWIPE_DURATION);
                targetBlock.pv.RPC("RPCMoveToAction", RpcTarget.Others, basePos, Constants.SWIPE_DURATION);
            }

            baseBlock.isSwiping = true;
            targetBlock.isSwiping = true;

            //2.2.2 Board에 저장된 블럭의 위치를 교환한다
            //blocks[nRow, nCol] = targetBlock;
            //blocks[nSwipeRow, nSwipeCol] = baseBlock;

            baseBlock.SetBoardBlock(nSwipeRow, nSwipeCol);
            targetBlock.SetBoardBlock(nRow, nCol);

            if (MainGameManager.GetInstance.IsCoOpHost() || MainGameManager.GetInstance.IsCoOpRemote())
            {
                baseBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nSwipeRow, nSwipeCol);
                targetBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nRow, nCol);
            }

            baseBlock.SetCellPosition(nSwipeRow, nSwipeCol);
            targetBlock.SetCellPosition(nRow, nCol);

            yield return new WaitForSeconds(Constants.SWIPE_DURATION);

            //blocks[nRow, nCol].Move(initX + nRow, initY + nCol);
            //blocks[nSwipeRow, nSwipeCol].Move(initX + nSwipeRow, initY + nSwipeCol);

            baseBlock.isSwiping = false;
            targetBlock.isSwiping = false;

            bool baseCheck = false;
            bool targetCheck = false;
            BlockPangCheck(nRow, nCol, out baseCheck, true);
            BlockPangCheck(nSwipeRow, nSwipeCol, out targetCheck, true);

            if (SwipQuest(baseBlock, targetBlock))
            {
                MainGameManager.GetInstance.combo++;
                yield break;
            }

            //////////// 움직인 두개 매치 체크 하고 둘다 아니면 돌려보내기 ////////////////////
            if (!baseCheck && !targetCheck)
            {
                MainGameManager.GetInstance.combo = 0;

                baseBlock = blocks[nRow, nCol];
                targetBlock = blocks[nSwipeRow, nSwipeCol];

                basePos = baseBlock.transform.position;
                targetPos = targetBlock.transform.position;

                StartCoroutine(Action2D.MoveTo(baseBlock.transform, targetPos, Constants.SWIPE_DURATION));
                StartCoroutine(Action2D.MoveTo(targetBlock.transform, basePos, Constants.SWIPE_DURATION));

                if (MainGameManager.GetInstance.IsCoOpHost())
                {
                    baseBlock.pv.RPC("RPCMoveToAction", RpcTarget.Others, targetPos, Constants.SWIPE_DURATION);
                    targetBlock.pv.RPC("RPCMoveToAction", RpcTarget.Others, basePos, Constants.SWIPE_DURATION);
                }

                baseBlock.isSwiping = true;
                targetBlock.isSwiping = true;

                //2.2.2 Board에 저장된 블럭의 위치를 교환한다
                //blocks[nRow, nCol] = targetBlock;
                //blocks[nSwipeRow, nSwipeCol] = baseBlock;

                baseBlock.SetBoardBlock(nSwipeRow, nSwipeCol);
                targetBlock.SetBoardBlock(nRow, nCol);

                if (MainGameManager.GetInstance.IsCoOpHost() || MainGameManager.GetInstance.IsCoOpRemote())
                {
                    baseBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nSwipeRow, nSwipeCol);
                    targetBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nRow, nCol);
                }

                targetBlock.SetCellPosition(nRow, nCol);
                baseBlock.SetCellPosition(nSwipeRow, nSwipeCol);

                yield return new WaitForSeconds(Constants.SWIPE_DURATION);

                //blocks[nRow, nCol]?.Move(initX + nRow, initY + nCol);
                //blocks[nSwipeRow, nSwipeCol]?.Move(initX + nSwipeRow, initY + nSwipeCol);

                baseBlock.isSwiping = false;
                targetBlock.isSwiping = false;

                BlockPangCheck(nRow, nCol, out baseCheck, true);
                BlockPangCheck(nSwipeRow, nSwipeCol, out targetCheck, true);
            }
        }

    }

    IEnumerator CoDoSwipeActionFromRemote(int nRow, int nCol, Swipe swipeDir)
    {
        int nSwipeRow = nRow, nSwipeCol = nCol;
        nSwipeRow += swipeDir.GetTargetRow(); //Right : +1, LEFT : -1
        nSwipeCol += swipeDir.GetTargetCol(); //UP : +1, DOWN : -1

        if (nSwipeRow < 0 || nSwipeRow >= maxRow || nSwipeCol < 0 || nSwipeCol >= maxCol)
        {
            yield break;
        }

        if (blocks[nRow, nCol] == null || blocks[nSwipeRow, nSwipeCol] == null)
        {
            yield break;
        }

        // 두 블럭 다 스왑 가능한지.
        if (blocks[nRow, nCol].IsSwipeable() && blocks[nSwipeRow, nSwipeCol].IsSwipeable())
        {
            Block baseBlock = blocks[nRow, nCol];
            Block targetBlock = blocks[nSwipeRow, nSwipeCol];

            Vector3 basePos = baseBlock.transform.position;
            Vector3 targetPos = targetBlock.transform.position;

            StartCoroutine(Action2D.MoveTo(baseBlock.transform, targetPos, Constants.SWIPE_DURATION));
            StartCoroutine(Action2D.MoveTo(targetBlock.transform, basePos, Constants.SWIPE_DURATION));

            baseBlock.isSwiping = true;
            targetBlock.isSwiping = true;

            //2.2.2 Board에 저장된 블럭의 위치를 교환한다
            //blocks[nRow, nCol] = targetBlock;
            //blocks[nSwipeRow, nSwipeCol] = baseBlock;

            baseBlock.SetBoardBlock(nSwipeRow, nSwipeCol);
            targetBlock.SetBoardBlock(nRow, nCol);

            if (MainGameManager.GetInstance.IsCoOpHost() || MainGameManager.GetInstance.IsCoOpRemote())
            {
                baseBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nSwipeRow, nSwipeCol);
                targetBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nRow, nCol);
            }

            targetBlock.SetCellPosition(nRow, nCol);
            baseBlock.SetCellPosition(nSwipeRow, nSwipeCol);

            yield return new WaitForSeconds(Constants.SWIPE_DURATION);

            //blocks[nRow, nCol].Move(initX + nRow, initY + nCol);
            //blocks[nSwipeRow, nSwipeCol].Move(initX + nSwipeRow, initY + nSwipeCol);

            baseBlock.isSwiping = false;
            targetBlock.isSwiping = false;

            bool baseCheck = false;
            bool targetCheck = false;
            BlockPangCheck(nRow, nCol, out baseCheck, true);
            BlockPangCheck(nSwipeRow, nSwipeCol, out targetCheck, true);

            if (SwipQuest(baseBlock, targetBlock))
            {
                MainGameManager.GetInstance.combo++;
                yield break;
            }

            //////////// 움직인 두개 매치 체크 하고 둘다 아니면 돌려보내기 ////////////////////
            if (!baseCheck && !targetCheck)
            {
                baseBlock = blocks[nRow, nCol];
                targetBlock = blocks[nSwipeRow, nSwipeCol];

                basePos = baseBlock.transform.position;
                targetPos = targetBlock.transform.position;

                StartCoroutine(Action2D.MoveTo(baseBlock.transform, targetPos, Constants.SWIPE_DURATION));
                StartCoroutine(Action2D.MoveTo(targetBlock.transform, basePos, Constants.SWIPE_DURATION));

                if (MainGameManager.GetInstance.IsCoOpHost())
                {
                    baseBlock.pv.RPC("RPCMoveToAction", RpcTarget.Others, targetPos, Constants.SWIPE_DURATION);
                    targetBlock.pv.RPC("RPCMoveToAction", RpcTarget.Others, basePos, Constants.SWIPE_DURATION);
                }

                baseBlock.isSwiping = true;
                targetBlock.isSwiping = true;

                //2.2.2 Board에 저장된 블럭의 위치를 교환한다
                //blocks[nRow, nCol] = targetBlock;
                //blocks[nSwipeRow, nSwipeCol] = baseBlock;

                baseBlock.SetBoardBlock(nSwipeRow, nSwipeCol);
                targetBlock.SetBoardBlock(nRow, nCol);

                if (MainGameManager.GetInstance.IsCoOpHost() || MainGameManager.GetInstance.IsCoOpRemote())
                {
                    baseBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nSwipeRow, nSwipeCol);
                    targetBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nRow, nCol);
                }

                targetBlock.SetCellPosition(nRow, nCol);
                baseBlock.SetCellPosition(nSwipeRow, nSwipeCol);

                yield return new WaitForSeconds(Constants.SWIPE_DURATION);

                //blocks[nRow, nCol]?.Move(initX + nRow, initY + nCol);
                //blocks[nSwipeRow, nSwipeCol]?.Move(initX + nSwipeRow, initY + nSwipeCol);

                baseBlock.isSwiping = false;
                targetBlock.isSwiping = false;

                BlockPangCheck(nRow, nCol, out baseCheck, true);
                BlockPangCheck(nSwipeRow, nSwipeCol, out targetCheck, true);
            }
        }

    }

    IEnumerator CoDoSwipeActionToHost(int nRow, int nCol, Swipe swipeDir)
    {
        int nSwipeRow = nRow, nSwipeCol = nCol;
        nSwipeRow += swipeDir.GetTargetRow(); //Right : +1, LEFT : -1
        nSwipeCol += swipeDir.GetTargetCol(); //UP : +1, DOWN : -1

        if (nSwipeRow < 0 || nSwipeRow >= maxRow || nSwipeCol < 0 || nSwipeCol >= maxCol)
        {
            yield break;
        }

        if (blocks[nRow, nCol] == null || blocks[nSwipeRow, nSwipeCol] == null)
        {
            yield break;
        }

        // 두 블럭 다 스왑 가능한지.
        if (blocks[nRow, nCol].IsSwipeable() && blocks[nSwipeRow, nSwipeCol].IsSwipeable())
        {
            Block baseBlock = blocks[nRow, nCol];
            Block targetBlock = blocks[nSwipeRow, nSwipeCol];

            Vector3 basePos = baseBlock.transform.position;
            Vector3 targetPos = targetBlock.transform.position;

            StartCoroutine(Action2D.MoveTo(baseBlock.transform, targetPos, Constants.SWIPE_DURATION));
            StartCoroutine(Action2D.MoveTo(targetBlock.transform, basePos, Constants.SWIPE_DURATION));

            if (MainGameManager.GetInstance.IsCoOpRemote())
            {
                pv.RPC("DoSwipeActionFromRemote", RpcTarget.Others, nRow, nCol, swipeDir);
            }

            baseBlock.isSwiping = true;
            targetBlock.isSwiping = true;

            //2.2.2 Board에 저장된 블럭의 위치를 교환한다
            //blocks[nRow, nCol] = targetBlock;
            //blocks[nSwipeRow, nSwipeCol] = baseBlock;

            baseBlock.SetBoardBlock(nSwipeRow, nSwipeCol);
            targetBlock.SetBoardBlock(nRow, nCol);

            if (MainGameManager.GetInstance.IsCoOpHost() || MainGameManager.GetInstance.IsCoOpRemote())
            {
                baseBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nSwipeRow, nSwipeCol);
                targetBlock.pv.RPC("SetBoardBlock", RpcTarget.Others, nRow, nCol);
            }

            targetBlock.SetCellPosition(nRow, nCol);
            baseBlock.SetCellPosition(nSwipeRow, nSwipeCol);

            yield return new WaitForSeconds(Constants.SWIPE_DURATION);

            //blocks[nRow, nCol].Move(initX + nRow, initY + nCol);
            //blocks[nSwipeRow, nSwipeCol].Move(initX + nSwipeRow, initY + nSwipeCol);

            baseBlock.isSwiping = false;
            targetBlock.isSwiping = false;

        }         
    }

    public bool BlockPangCheck(int nRow, int nCol)
    {
        bool found = false;

        Block baseBlock = blocks[nRow, nCol];
        if (baseBlock == null || baseBlock.status != BlockStatus.NORMAL || baseBlock.type != BlockType.BASIC) return found;

        List<Block> matchedBlockList = new List<Block>();

        matchedBlockList.Add(baseBlock);

        //1. 가로 블럭 검색
        Block block;

        //1.1 오른쪽 방향
        for (int i = nRow + 1; i < maxRow; i++)
        {
            block = m_Blocks[i, nCol];
            if (block == null) break;
            if (!block.MatchCheck(baseBlock))
                break;

            matchedBlockList.Add(block);
        }

        //1.2 왼쪽 방향
        for (int i = nRow - 1; i >= 0; i--)
        {
            block = m_Blocks[i, nCol];
            if (block == null) break;
            if (!block.MatchCheck(baseBlock))
                break;

            matchedBlockList.Add(block);
        }

        //1.3 매치된 상태인지 판단한다
        //    기준 블럭(baseBlock)을 제외하고 좌우에 2개이상이면 기준블럭 포함해서 3개이상 매치되는 경우로 판단할 수 있다
        if (matchedBlockList.Count >= 3)
        {
            found = true;
            return found;
        }


        matchedBlockList.Clear();

        //2. 세로 블럭 검색
        matchedBlockList.Add(baseBlock);

        //2.1 위쪽 검색
        for (int i = nCol + 1; i < maxCol; i++)
        {
            block = m_Blocks[nRow, i];
            if (block == null) break;
            if (!block.MatchCheck(baseBlock))
                break;

            matchedBlockList.Add(block);
        }

        //2.2 아래쪽 검색
        for (int i = nCol - 1; i >= 0; i--)
        {
            block = m_Blocks[nRow, i];
            if (block == null) break;
            if (!block.MatchCheck(baseBlock))
                break;

            matchedBlockList.Add(block);
        }

        //2.3 매치된 상태인지 판단한다
        //    기준 블럭(baseBlock)을 제외하고 상하에 2개이상이면 기준블럭 포함해서 3개이상 매치되는 경우로 판단할 수 있다
        if (matchedBlockList.Count >= 3)
        {
            found = true;
        }

        return found;
    }


    // 특수블럭 생성 시 스와이프 매치체크랑 일반 매치체크랑 나눠야한다.
    // 스와이프 시엔 스와이프 블럭이 특수블럭으로 변경.
    // 일반 매치땐 왼쪽 아래 블럭이 특수블럭으로 변경.
    public void BlockPangCheck(int nRow, int nCol, out bool found, bool swiped)
    {
        found = false;

        Block baseBlock = blocks[nRow, nCol];
        if (baseBlock == null) return;

        List<Block> matchedBlockList = new List<Block>();
        List<Block> clearList = new List<Block>();

        bool horzMatch = false;

        matchedBlockList.Add(baseBlock);

        //1. 가로 블럭 검색
        Block block;

        //1.1 오른쪽 방향
        for (int i = nRow + 1; i < maxRow; i++)
        {
            block = m_Blocks[i, nCol];
            if (block == null) break;
            if (!block.MatchCheck(baseBlock))
                break;

            matchedBlockList.Add(block);
        }

        //1.2 왼쪽 방향
        for (int i = nRow - 1; i >= 0; i--)
        {
            block = m_Blocks[i, nCol];
            if (block == null) break;
            if (!block.MatchCheck(baseBlock))
                break;

            matchedBlockList.Add(block);
        }

        //1.3 매치된 상태인지 판단한다
        //    기준 블럭(baseBlock)을 제외하고 좌우에 2개이상이면 기준블럭 포함해서 3개이상 매치되는 경우로 판단할 수 있다
        if (matchedBlockList.Count >= 3)
        {
            for (int i = 0; i < matchedBlockList.Count; i++)
            {
                matchedBlockList[i].MatchTypeAdd((MatchType)Mathf.Min(matchedBlockList.Count, 5));
                clearList.Add(matchedBlockList[i]);
            }

            found = true;
        }


        matchedBlockList.Clear();

        //2. 세로 블럭 검색
        matchedBlockList.Add(baseBlock);

        //2.1 위쪽 검색
        for (int i = nCol + 1; i < maxCol; i++)
        {
            block = m_Blocks[nRow, i];
            if (block == null) break;
            if (!block.MatchCheck(baseBlock))
                break;

            matchedBlockList.Add(block);
        }

        //2.2 아래쪽 검색
        for (int i = nCol - 1; i >= 0; i--)
        {
            block = m_Blocks[nRow, i];
            if (block == null) break;
            if (!block.MatchCheck(baseBlock))
                break;

            matchedBlockList.Add(block);
        }

        //2.3 매치된 상태인지 판단한다
        //    기준 블럭(baseBlock)을 제외하고 상하에 2개이상이면 기준블럭 포함해서 3개이상 매치되는 경우로 판단할 수 있다
        if (matchedBlockList.Count >= 3)
        {
            for (int i = 0; i < matchedBlockList.Count; i++)
            {
                matchedBlockList[i].MatchTypeAdd((MatchType)Mathf.Min(matchedBlockList.Count, 5));
                // 가로에서 이미 찾아서 clearList 에 baseBlock 들어있을 때는 baseBlock 안넣음. (matchedList[0] == baseBlock)
                if (found && i == 0) continue;
                clearList.Add(matchedBlockList[i]);
            }

            horzMatch = true;
            found = true;
        }

        if (found)
        {
            MainGameManager.GetInstance.combo++;

            Block changedBlock = clearList[0];
            for (int i = 1; i < clearList.Count; i++)
            {
                if (changedBlock.matchType < clearList[i].matchType)
                {
                    changedBlock = clearList[i];
                }
            }

            MatchType matchType = changedBlock.matchType;

            for (int i = 0; i < clearList.Count; i++)
            {
                clearList[i].score = (int)(Mathf.Pow(MainGameManager.GetInstance.combo, 2f) * 10);

                if ((int)changedBlock.matchType >= (int)MatchType.FOUR && changedBlock == clearList[i])
                {
                    clearList[i].status = BlockStatus.MATCH;
                    clearList[i].DoActionClear(false, () => { changedBlock.MatchTypeUpdate(matchType, horzMatch); });
                    //clearList[i].DoActionClear(false, () => { changedBlock.MatchTypeUpdate(matchType, swiped ? horzMatch : !horzMatch); });
                }
                else
                {
                    clearList[i].status = BlockStatus.MATCH;
                    clearList[i].DoActionClear(true);
                }
            }
        }

        return;
    }
    public void AllShuffle()
    {
        for (int col = 0; col < m_Col; col++)
        {
            for (int row = 0; row < m_Row; row++)
            {
                blocks[row, col].breed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
            }
        }
    }

    [PunRPC]
    public void BoardShuffle()
    {
        for (int col = 0; col < m_Col; col++)
        {
            for (int row = 0; row < m_Row; row++)
            {
                if (m_Blocks[row, col] != null && m_Blocks[row, col].IsMatchable(true) && m_Blocks[row, col].type == BlockType.BASIC)
                {
                    BlockBreed newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                    m_Blocks[row, col].SetBlockBreed(newBreed);
                }
            }
        }

        bool check = true;
        while (check)
        {
            check = false;
            //{
            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    if (!m_Blocks[row, col].IsMatchable(true)) continue;

                    if (BlockPangCheck(row, col))
                    {
                        BlockBreed newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                        m_Blocks[row, col].SetBlockBreed(newBreed);

                        check = true;
                    }
                }
            }
        }

        for (int row = 0; row < maxRow; row++)
        {
            for (int col = 0; col < maxCol; col++)
            {
                if (!m_Blocks[row, col].IsMatchable(true)) continue;
                if (BlockPangCheck(row, col))
                {
                    BoardShuffle();
                }
            }
        }
    }

    public void MatchingCheckShuffle()
    {
        for (int col = 0; col < m_Col; col++)
        {
            for (int row = 0; row < m_Row; row++)
            {
                // 왼쪽 아래부터 체크.
                // 왼쪽 아래로 두칸씩 검사.
                if (row > 1)
                {
                    // 왼쪽 두개랑 같으면 매치. 블럭 변경해줘야 한다.
                    if (m_Blocks[row, col].IsSafeEqual(m_Blocks[row - 1, col]) && m_Blocks[row, col].IsSafeEqual(m_Blocks[row - 2, col]))
                    {
                        bool colCheck = false;
                        // 바꿀 때 아래도 확인 할까?
                        if (col > 1)
                        {
                            // 아래는 현재 블럭이랑 상관없이 아래 두개가 같은지만 체크하자.
                            if (m_Blocks[row, col - 1].IsSafeEqual(m_Blocks[row, col - 2]))
                            {
                                // 같으면 블럭 변경할 때 참고하자.
                                colCheck = true;
                            }
                        }

                        BlockBreed newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                        // 혹시 모르니 왼쪽과 아래의 블럭과 다른 블럭으로 변경.
                        while (true)
                        {
                            // 아래 두개도 같으면?
                            if (colCheck)
                            {
                                // 왼쪽, 아래 블럭이랑 다른 브리드로 변경.
                                // 왼쪽, 아래 블럭과 같으면 다시 랜덤
                                if (newBreed == m_Blocks[row - 1, col].breed || newBreed == m_Blocks[row, col - 1].breed)
                                {
                                    newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                                }
                                // 왼쪽 아래랑 다르면 브리드 바꾸고 나가자
                                else
                                {
                                    m_Blocks[row, col].breed = newBreed;
                                    break;
                                }
                            }
                            else
                            {
                                // 왼쪽 블럭이랑 다른 브리드로 변경.
                                // 왼쪽과 같으면 다시 랜덤
                                if (newBreed == m_Blocks[row - 1, col].breed)
                                {
                                    newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                                }
                                // 왼쪽이랑 다르면 브리드 바꾸고 나가자
                                else
                                {
                                    m_Blocks[row, col].breed = newBreed;
                                    break;
                                }
                            }

                        }

                        // 여기서 아래랑 다르게 바꿨으니 아래는 체크 안하고 나가도 된다.
                        continue;
                    }
                }

                if (col > 1)
                {
                    // 아래쪽 두개랑 같으면 매치. 블럭 변경해줘야 한다.
                    if (m_Blocks[row, col].IsSafeEqual(m_Blocks[row, col - 1]) && m_Blocks[row, col].IsSafeEqual(m_Blocks[row, col - 2]))
                    {
                        bool rowCheck = false;
                        // 바꿀 때 왼쪽도 확인.
                        if (row > 1)
                        {
                            // 왼쪽은 현재 블럭이랑 상관없이 왼쪽 두개가 같은지만 체크하자.
                            if (m_Blocks[row - 1, col].IsSafeEqual(m_Blocks[row - 2, col]))
                            {
                                // 같으면 블럭 변경할 때 참고하자.
                                rowCheck = true;
                            }
                        }

                        BlockBreed newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                        // 혹시 모르니 왼쪽과 아래의 블럭과 다른 블럭으로 변경.
                        while (true)
                        {
                            // 아래 두개도 같으면?
                            if (rowCheck)
                            {
                                // 왼쪽, 아래 블럭이랑 다른 브리드로 변경.
                                // 왼쪽, 아래 블럭과 같으면 다시 랜덤
                                if (newBreed == m_Blocks[row - 1, col].breed || newBreed == m_Blocks[row, col - 1].breed)
                                {
                                    newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                                }
                                // 왼쪽 아래랑 다르면 브리드 바꾸고 나가자
                                else
                                {
                                    m_Blocks[row, col].breed = newBreed;
                                    break;
                                }
                            }
                            else
                            {
                                // 아래 블럭이랑 다른 브리드로 변경.
                                // 아래랑 같으면 다시 랜덤
                                if (newBreed == m_Blocks[row, col - 1].breed)
                                {
                                    newBreed = (BlockBreed)UnityEngine.Random.Range(0, MainGameManager.GetInstance.stage.blockCount);
                                }
                                // 아래랑 다르면 브리드 바꾸고 나가자
                                else
                                {
                                    m_Blocks[row, col].breed = newBreed;
                                    break;
                                }
                            }

                        }
                    }
                }
            }
        }
    }

    public bool PangCheck()
    {
        for (int nCol = 0; nCol < maxCol; nCol++)
        {
            for (int nRow = 0; nRow < maxRow; nRow++)
            {
                // 왼쪽 아래부터 오른쪽 위로 체크하니깐
                // 왼쪽이랑 아래는 체크 안해도 이미 지나온 길이겠지.
                Block baseBlock = blocks[nRow, nCol];
                if (baseBlock == null || !baseBlock.IsSwipeable()) continue;

                // 가로 2개 * 3, 세로 2개 * 3, 가로 띈 2개 * 2, 세로 띈 2개 * 2 체크해야함.

                // 가로 2개
                //  * *      * *   *
                //      *            이 모양 있는지 체크.
                if (nRow + 1 < maxRow)
                {
                    Block block = blocks[nRow + 1, nCol];
                    if (block != null)
                    {
                        if (baseBlock.IsSafeEqual(block))
                        {
                            // 양쪽 6칸 체크해야함. null 체크, 보드 밖 체크 꼼꼼히

                            // 왼쪽 한칸
                            int leftRow = nRow - 1;
                            if (leftRow >= 0)
                            {
                                Block leftBlock = blocks[leftRow, nCol];
                                if (leftBlock != null && leftBlock.IsSwipeable())
                                {
                                    // 왼쪽 위 체크
                                    if (nCol + 1 < maxCol)
                                    {
                                        block = blocks[leftRow, nCol + 1];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    // 왼쪽 아래
                                    if (nCol - 1 >= 0)
                                    {
                                        block = blocks[leftRow, nCol - 1];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    // 띈 왼쪽
                                    if (leftRow - 1 >= 0)
                                    {
                                        block = blocks[leftRow - 1, nCol];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }

                            int rightRow = nRow + 2;
                            if (rightRow < maxRow)
                            {
                                Block rightBlock = blocks[rightRow, nCol];
                                if (rightBlock != null && rightBlock.IsSwipeable())
                                {
                                    // 오른쪽 위 체크
                                    if (nCol + 1 < maxCol)
                                    {
                                        block = blocks[rightRow, nCol + 1];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    // 오른쪽 아래
                                    if (nCol - 1 >= 0)
                                    {
                                        block = blocks[rightRow, nCol - 1];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    // 띈 오른쪽
                                    if (rightRow + 1 < maxRow)
                                    {
                                        block = blocks[rightRow + 1, nCol];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // 세로 2개
                //         *   
                //    *   
                //  *      *
                //  *      *   이런 모양 있는지 체크.
                if (nCol + 1 < maxCol)
                {
                    Block block = blocks[nRow, nCol + 1];
                    if (block != null)
                    {
                        if (baseBlock.IsSafeEqual(block))
                        {
                            // 양쪽 6칸 체크해야함. null 체크, 보드 밖 체크 꼼꼼히

                            // 아래 한칸
                            int downCol = nCol - 1;
                            if (downCol >= 0)
                            {
                                Block bottomBlock = blocks[nRow, downCol];
                                if (bottomBlock != null && bottomBlock.IsSwipeable())
                                {
                                    // 아래 오른쪽 체크
                                    if (nRow + 1 < maxRow)
                                    {
                                        block = blocks[nRow + 1, downCol];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    // 아래 왼쪽
                                    if (nRow - 1 >= 0)
                                    {
                                        block = blocks[nRow - 1, downCol];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    // 띈 아래쪽
                                    if (downCol - 1 >= 0)
                                    {
                                        block = blocks[nRow, downCol - 1];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }

                            int upCol = nCol + 2;
                            if (upCol < maxRow)
                            {
                                Block topBlock = blocks[nRow, upCol];
                                if (topBlock != null && topBlock.IsSwipeable())
                                {
                                    // 위 오른쪽 체크
                                    if (nRow + 1 < maxRow)
                                    {
                                        block = blocks[nRow + 1, upCol];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    // 위 왼쪽
                                    if (nRow - 1 >= 0)
                                    {
                                        block = blocks[nRow - 1, upCol];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    // 띈 위
                                    if (upCol + 1 < maxCol)
                                    {
                                        block = blocks[nRow, upCol + 1];
                                        if (block != null && block.IsSwipeable())
                                        {
                                            if (baseBlock.IsSafeEqual(block))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // 가로 띈 2개
                //  *   *
                //    *       이 모양 있는지 체크.
                if (nRow + 2 < maxRow)
                {
                    Block block = blocks[nRow + 2, nCol];
                    if (block != null)
                    {
                        if (baseBlock.IsSafeEqual(block))
                        {
                            // 가운데
                            int middleRow = nRow + 1;
                            Block middleBlock = blocks[middleRow, nCol];
                            if (middleBlock != null && middleBlock.IsSwipeable())
                            {
                                // 가운데 위 체크
                                if (nCol + 1 < maxCol)
                                {
                                    block = blocks[middleRow, nCol + 1];
                                    if (block != null && block.IsSwipeable())
                                    {
                                        if (baseBlock.IsSafeEqual(block))
                                        {
                                            return true;
                                        }
                                    }
                                }
                                // 가운데 아래
                                if (nCol - 1 >= 0)
                                {
                                    block = blocks[middleRow, nCol - 1];
                                    if (block != null && block.IsSwipeable())
                                    {
                                        if (baseBlock.IsSafeEqual(block))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // 세로 띈 2개
                //  *
                //    *
                //  *       이 모양 있는지 체크.
                if (nCol + 2 < maxCol)
                {
                    Block block = blocks[nRow, nCol + 2];
                    if (block != null)
                    {
                        if (baseBlock.IsSafeEqual(block))
                        {
                            // 가운데
                            int middleCol = nCol + 1;
                            Block middleBlock = blocks[nRow, middleCol];
                            if (middleBlock != null && middleBlock.IsSwipeable())
                            {
                                // 가운데 오른쪽 체크
                                if (nRow + 1 < maxRow)
                                {
                                    block = blocks[nRow + 1, middleCol];
                                    if (block != null && block.IsSwipeable())
                                    {
                                        if (baseBlock.IsSafeEqual(block))
                                        {
                                            return true;
                                        }
                                    }
                                }
                                // 가운데 왼쪽
                                if (nRow - 1 >= 0)
                                {
                                    block = blocks[nRow - 1, middleCol];
                                    if (block != null && block.IsSwipeable())
                                    {
                                        if (baseBlock.IsSafeEqual(block))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }


    private List<Block> GetRowBlock(int col)
    {
        List<Block> blockRow = new List<Block>();

        if (col < 0 || col >= maxCol) 
            return blockRow;

        for (int i = 0; i < maxRow; i++)
        {
            Block block = blocks[i, col];
            if (block == null || !block.IsClearable()) 
                continue;

            blockRow.Add(block);
        }

        return blockRow;
    }

    private List<Block> GetColBlock(int row)
    {
        List<Block> blockCol = new List<Block>();

        if (row < 0 || row >= maxRow) return blockCol;

        for (int i = 0; i < maxCol; i++)
        {
            Block block = blocks[row, i];
            if (block == null || !block.IsClearable())
                continue;

            blockCol.Add(block);
        }

        return blockCol;
    }

    private List<List<Block>> GetCircleBlock(int row, int col)
    {
        List<List<Block>> blockCircle = new List<List<Block>>();

        for (int i = -1; i < 2; i++)
        {
            int cRow = row + i;
            if (cRow < 0 || cRow >= maxRow) continue;

            List<Block> colBlock = new List<Block>();
            for (int j = -1; j < 2; j++)
            {
                int cCol = col + j;
                if (cCol < 0 || cCol >= maxCol) continue;

                Block block = blocks[cRow, cCol];

                if (block == null || !block.IsClearable())
                    continue;

                colBlock.Add(block);
            }
            blockCircle.Add(colBlock);
        }

        return blockCircle;
    }

    private List<List<Block>> GetCircleDoubleBlock(int row, int col)
    {
        List<List<Block>> blockCircle = new List<List<Block>>();

        for (int i = -2; i < 3; i++)
        {
            int cRow = row + i;
            if (cRow < 0 || cRow >= maxRow) continue;

            List<Block> colBlock = new List<Block>();
            for (int j = -2; j < 3; j++)
            {
                int cCol = col + j;
                if (cCol < 0 || cCol >= maxCol) continue;

                Block block = blocks[cRow, cCol];

                if (block == null || !block.IsClearable())
                    continue;

                colBlock.Add(block);
            }
            blockCircle.Add(colBlock);
        }

        return blockCircle;
    }

    private List<Block> GetEqualBreed(BlockBreed targetBreed)
    {
        List<Block> targetList = new List<Block>();
        for (int i = 0; i < maxRow; i++)
        {
            for (int j = 0; j < maxCol; j++)
            {
                Block block = blocks[i, j];
                if (block == null || !block.IsClearable())
                    continue;

                if (blocks[i, j].breed == targetBreed)
                {
                    targetList.Add(blocks[i, j]);
                }
            }
        }

        return targetList;
    }

    //public void ClearVert(int col)
    //{
    //    List<Block> blockRow = GetRowBlock(col);
    //    
    //    for (int i = 0; i < blockRow.Count; i++)
    //    {
    //        blockRow[i].status = BlockStatus.MATCH;
    //        blockRow[i].DoActionClear(true);
    //    }
    //}

    public void ClearVert(int row, int col)
    {
        if (row < 0 || row >= maxRow || col < 0 || col >= maxCol) return;

        GameObject cl = PhotonManager.GetInstance.InstantiateWithPhoton(gameObject,
            "VerticalCollision",
            cells[row, col].transform.position,
            0.4f);
        GameObject cr = PhotonManager.GetInstance.InstantiateWithPhoton(gameObject,
            "VerticalCollision",
            cells[row, col].transform.position,
            0.4f);

        //GameObject cl = gameObject.AddChildFromObjPool("VerticalCollision", cells[row, col].transform.position, 0.4f);
        //GameObject cr = gameObject.AddChildFromObjPool("VerticalCollision", cells[row, col].transform.position, 0.4f);

        //cl.GetComponent<SpecialBlockCollision>().InitScale();
        //cr.GetComponent<SpecialBlockCollision>().InitScale();

        StartCoroutine(Action2D.MoveTo(cl.transform, cl.transform.position + Vector3.left * 10, 0.4f));
        StartCoroutine(Action2D.MoveTo(cr.transform, cr.transform.position + Vector3.right * 10, 0.4f));

        if (MainGameManager.GetInstance.IsCoOpHost())
        {
            ActionMono cla = cl.GetComponent<ActionMono>();
            ActionMono cra = cr.GetComponent<ActionMono>();

            if (cla != null)
            {
                cla.pv.RPC("MoveTo", RpcTarget.Others, cl.transform.position + Vector3.left * 10, 0.4f);
            }

            if (cra != null)
            {
                cra.pv.RPC("MoveTo", RpcTarget.Others, cr.transform.position + Vector3.right * 10, 0.4f);
            }
        }
    }

    //public void ClearHorz(int row)
    //{
    //    List<Block> blockCol = GetColBlock(row);
    //
    //    for (int i = 0; i < blockCol.Count; i++)
    //    {
    //        blockCol[i].status = BlockStatus.MATCH;
    //        blockCol[i].DoActionClear(true);
    //    }
    //}

    public void ClearHorz(int row, int col)
    {
        if (row < 0 || row >= maxRow || col < 0 || col >= maxCol) return;

        GameObject ct;
        GameObject cb;
        if (MainGameManager.GetInstance.gameType == GameType.CoOp)
        {
            ct = PhotonManager.GetInstance.InstantiateWithPhoton(gameObject,
                "HorizonCollision",
                cells[row, col].transform.position,
                0.4f);
            cb = PhotonManager.GetInstance.InstantiateWithPhoton(gameObject,
                "HorizonCollision",
                cells[row, col].transform.position,
                0.4f);
        }
        else
        {
            ct = MainGameManager.GetInstance.gameObject.AddChildFromObjPool(
                "HorizonCollision",
                cells[row, col].transform.position,
                0.4f);
            cb = MainGameManager.GetInstance.gameObject.AddChildFromObjPool(
                "HorizonCollision",
                cells[row, col].transform.position,
                0.4f);
        }

        //GameObject ct = gameObject.AddChildFromObjPool("HorizonCollision", cells[row, col].transform.position, 0.4f);
        //GameObject cb = gameObject.AddChildFromObjPool("HorizonCollision", cells[row, col].transform.position, 0.4f);

        //ct.GetComponent<SpecialBlockCollision>().InitScale();
        //cb.GetComponent<SpecialBlockCollision>().InitScale();

        StartCoroutine(Action2D.MoveTo(ct.transform, ct.transform.position + Vector3.up * 10, 0.4f));
        StartCoroutine(Action2D.MoveTo(cb.transform, cb.transform.position + Vector3.down * 10, 0.4f));

        if (MainGameManager.GetInstance.IsCoOpHost())
        {
            ActionMono cta = ct.GetComponent<ActionMono>();
            ActionMono cba = cb.GetComponent<ActionMono>();

            if (cta != null)
            {
                cta.pv.RPC("MoveTo", RpcTarget.Others, ct.transform.position + Vector3.up * 10, 0.4f);
            }

            if (cba != null)
            {
                cba.pv.RPC("MoveTo", RpcTarget.Others, cb.transform.position + Vector3.down * 10, 0.4f);
            }
        }
    }

    //public void ClearCircle(int row, int col)
    //{
    //    List<List<Block>> blockCircle = GetCircleBlock(row, col);

    //    for (int i = 0; i < blockCircle.Count; i++)
    //    {
    //        for (int j = 0; j < blockCircle[i].Count; j++)
    //        {
    //            blockCircle[i][j].status = BlockStatus.MATCH;
    //            blockCircle[i][j].DoActionClear(true);
    //        }
    //    }
    //}

    public void ClearCircle(int row, int col)
    {
        if (row < 0 || row >= maxRow || col < 0 || col >= maxCol) return;

        GameObject cc;
        if (MainGameManager.GetInstance.gameType == GameType.CoOp)
        {
            cc = PhotonManager.GetInstance.InstantiateWithPhoton(gameObject,
            "CircleEffect",
            cells[row, col].transform.position,
            1f);
        }
        else
        {
            cc = MainGameManager.GetInstance.gameObject.AddChildFromObjPool(
                "CircleEffect",
                cells[row, col].transform.position,
                1f);
        }
        //GameObject cc = gameObject.AddChildFromObjPool("CircleEffect", cells[row, col].transform.position, 1f);

        StartCoroutine(CreateCircleCollision(row, col, 0.2f));

        //cc.GetComponent<SpecialBlockCollision>().InitScale();

        //StartCoroutine(Action2D.Scale(cc.transform, 2.5f, 0.02f));
    }

    IEnumerator CreateCircleCollision(int row, int col, float delay)
    {
        yield return new WaitForSeconds(delay);

        gameObject.AddChildFromObjPool("CircleCollision", cells[row, col].transform.position, 0.05f);
    }

    public void ClearCircleDouble(int row, int col)
    {
        if (row < 0 || row >= maxRow || col < 0 || col >= maxCol) return;

        GameObject cc;
        if (MainGameManager.GetInstance.gameType == GameType.CoOp)
        {
            cc = PhotonManager.GetInstance.InstantiateWithPhoton(gameObject,
                "CircleDoubleEffect",
                cells[row, col].transform.position,
                1f);
        }
        else
        {
            cc = MainGameManager.GetInstance.gameObject.AddChildFromObjPool(
                "CircleDoubleEffect",
                cells[row, col].transform.position,
                1f);
        }
        //gameObject.AddChildFromObjPool("CircleDoubleEffect", cells[row, col].transform.position, 1f);

        StartCoroutine(CreateCircleDoubleCollision(row, col, 0.25f));
    }

    IEnumerator CreateCircleDoubleCollision(int row, int col, float delay)
    {
        yield return new WaitForSeconds(delay);

        gameObject.AddChildFromObjPool("CircleDoubleCollision", cells[row, col].transform.position, 0.05f);
    }

    public void ClearLazer(BlockBreed targetBreed)
    {
        List<Block> clearList = GetEqualBreed(targetBreed);
        for (int i = 0; i < clearList.Count; i++)
        {
            clearList[i].status = BlockStatus.MATCH;
            clearList[i].score = MainGameManager.GetInstance.combo * 100;
            clearList[i].DoActionClear(true);
        }
    }

    public IEnumerator ClearAll()
    {
        for (int i = maxCol - 1; i >= 0; i--)
        {
            for (int j = 0; j < maxRow; j++)
            {
                Block block = blocks[j, i];
                if (block == null || !block.IsClearable())
                    continue;

                blocks[j, i].status = BlockStatus.MATCH;
                blocks[j, i].score = MainGameManager.GetInstance.combo * 100;
                blocks[j, i].DoActionClear(true);

                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    public IEnumerator ChangeQuestWithBreed(BlockBreed breed, BlockQuestType quest)
    {
        List<Block> clearList = GetEqualBreed(breed);

        for (int i = 0; i < clearList.Count; i++)
        {
            if (clearList[i].questType != BlockQuestType.CLEAR_SIMPLE) continue;
            if (quest == BlockQuestType.CLEAR_HORZ || quest == BlockQuestType.CLEAR_VERT)
            {
                int random = UnityEngine.Random.Range(0, 2);
                if (random == 0)
                {
                    clearList[i].SetQuestType(BlockQuestType.CLEAR_HORZ);
                }
                else
                {
                    clearList[i].SetQuestType(BlockQuestType.CLEAR_VERT);
                }
            }
            else
            {
                clearList[i].SetQuestType(quest);
            }
            clearList[i].status = BlockStatus.MATCH;

            yield return new WaitForSeconds(0.05f);
        }

        for (int i = 0; i < clearList.Count; i++)
        {
            clearList[i].DoActionClear(true);

            yield return new WaitForSeconds(0.05f);
        }
    }

    public bool SwipQuest(Block baseBlock, Block targetBlock)
    {
        // 일자 + 일자      타겟블럭 기준 십자 폭발.
        // 일자 + 써클      타겟블럭 기준 3줄 십자 폭발.
        // 일자 + 레이저    해당 브리드 블럭 일자로 변경 후 폭발.

        // 써클 + 써클      타겟블럭 기준 5 * 5 폭발.
        // 써클 + 레이저    해당 브리드 블럭 써클로 변경 후 폭발.

        // 레이저 + 심플    해당 브리드 블럭 폭발.
        // 레이저 + 레이저  모든 블럭 폭발.
        //=============================================================

        // 일자 + 일자
        if ((baseBlock.questType == BlockQuestType.CLEAR_HORZ || baseBlock.questType == BlockQuestType.CLEAR_VERT) &&
            (targetBlock.questType == BlockQuestType.CLEAR_HORZ || targetBlock.questType == BlockQuestType.CLEAR_VERT))
        {
            baseBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            targetBlock.questType = BlockQuestType.CLEAR_SIMPLE;

            //baseBlock.status = BlockStatus.MATCH;
            //targetBlock.status = BlockStatus.MATCH;
            //
            //baseBlock.DoActionClear(true);
            //targetBlock.DoActionClear(true);

            ClearHorz(baseBlock.cellPosition.x, baseBlock.cellPosition.y);
            ClearVert(baseBlock.cellPosition.x, baseBlock.cellPosition.y);

            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 3);

            return true;
        }
        // 일자 + 써클
        else if (((baseBlock.questType == BlockQuestType.CLEAR_HORZ || baseBlock.questType == BlockQuestType.CLEAR_VERT) &&
            targetBlock.questType == BlockQuestType.CLEAR_CIRCLE) ||
            (baseBlock.questType == BlockQuestType.CLEAR_CIRCLE &&
            (targetBlock.questType == BlockQuestType.CLEAR_HORZ || targetBlock.questType == BlockQuestType.CLEAR_VERT)))
        {
            baseBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            targetBlock.questType = BlockQuestType.CLEAR_SIMPLE;

            //baseBlock.status = BlockStatus.MATCH;
            //targetBlock.status = BlockStatus.MATCH;
            //
            //baseBlock.DoActionClear(true);
            //targetBlock.DoActionClear(true);

            gameObject.AddChildFromObjPool("CircleEffect", cells[baseBlock.cellPosition.x, baseBlock.cellPosition.y].transform.position, 1f);

            ClearHorz(baseBlock.cellPosition.x - 1, baseBlock.cellPosition.y);
            ClearHorz(baseBlock.cellPosition.x, baseBlock.cellPosition.y);
            ClearHorz(baseBlock.cellPosition.x + 1, baseBlock.cellPosition.y);
            ClearVert(baseBlock.cellPosition.x, baseBlock.cellPosition.y - 1);
            ClearVert(baseBlock.cellPosition.x, baseBlock.cellPosition.y);
            ClearVert(baseBlock.cellPosition.x, baseBlock.cellPosition.y + 1);

            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 3);

            return true;
        }
        //일자 + 레이저    해당 브리드 블럭 일자로 변경 후 폭발.
        else if ((baseBlock.questType == BlockQuestType.CLEAR_HORZ || baseBlock.questType == BlockQuestType.CLEAR_VERT) &&
            targetBlock.questType == BlockQuestType.CLEAR_LAZER)
        {
            int random = UnityEngine.Random.Range(0, 2);
            if (random == 0)
            {
                StartCoroutine(ChangeQuestWithBreed(baseBlock.breed, BlockQuestType.CLEAR_HORZ));
            }
            else
            {
                StartCoroutine(ChangeQuestWithBreed(baseBlock.breed, BlockQuestType.CLEAR_VERT));
            }

            targetBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            //baseBlock.status = BlockStatus.MATCH;
            targetBlock.status = BlockStatus.MATCH;

            //baseBlock.DoActionClear(true);
            targetBlock.DoActionClear(true);

            // ClearLazer(baseBlock.breed);
            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 3);

            return true;
        }
        else if ((targetBlock.questType == BlockQuestType.CLEAR_HORZ || targetBlock.questType == BlockQuestType.CLEAR_VERT) &&
            baseBlock.questType == BlockQuestType.CLEAR_LAZER)
        {
            int random = UnityEngine.Random.Range(0, 2);
            if (random == 0)
            {
                StartCoroutine(ChangeQuestWithBreed(targetBlock.breed, BlockQuestType.CLEAR_HORZ));
            }
            else
            {
                StartCoroutine(ChangeQuestWithBreed(targetBlock.breed, BlockQuestType.CLEAR_VERT));
            }

            baseBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            baseBlock.status = BlockStatus.MATCH;
            //targetBlock.status = BlockStatus.MATCH;

            baseBlock.DoActionClear(true);
            //targetBlock.DoActionClear(true);

            //ClearLazer(targetBlock.breed);
            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 3);

            return true;
        }
         //써클 + 써클      타겟블럭 기준 5 * 5 폭발.
        else if (baseBlock.questType == BlockQuestType.CLEAR_CIRCLE && targetBlock.questType == BlockQuestType.CLEAR_CIRCLE)
        {
            baseBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            targetBlock.questType = BlockQuestType.CLEAR_SIMPLE;

           //baseBlock.status = BlockStatus.MATCH;
           //targetBlock.status = BlockStatus.MATCH;
           //
           //baseBlock.DoActionClear(true);
           //targetBlock.DoActionClear(true);

            ClearCircleDouble(baseBlock.cellPosition.x, baseBlock.cellPosition.y);
            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 3);

            return true;
        }
        // 써클 + 레이저    해당 브리드 블럭 써클로 변경 후 폭발.
        else if (baseBlock.questType == BlockQuestType.CLEAR_CIRCLE && targetBlock.questType == BlockQuestType.CLEAR_LAZER)
        {
            StartCoroutine(ChangeQuestWithBreed(baseBlock.breed, BlockQuestType.CLEAR_CIRCLE));

            targetBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            //baseBlock.status = BlockStatus.MATCH;
            targetBlock.status = BlockStatus.MATCH;

            //baseBlock.DoActionClear(true);
            targetBlock.DoActionClear(true);

            //ClearLazer(baseBlock.breed);
            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 3);

            return true;
        }
        else if (targetBlock.questType == BlockQuestType.CLEAR_CIRCLE && baseBlock.questType == BlockQuestType.CLEAR_LAZER)
        {
            StartCoroutine(ChangeQuestWithBreed(targetBlock.breed, BlockQuestType.CLEAR_CIRCLE));

            baseBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            baseBlock.status = BlockStatus.MATCH;
            //targetBlock.status = BlockStatus.MATCH;

            baseBlock.DoActionClear(true);
            //targetBlock.DoActionClear(true);

            //ClearLazer(targetBlock.breed);
            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 3);

            return true;
        }
        // 레이저 + 심플    해당 브리드 블럭 폭발.
        else if (baseBlock.questType == BlockQuestType.CLEAR_LAZER && (targetBlock.questType == BlockQuestType.CLEAR_SIMPLE || targetBlock.questType == BlockQuestType.SHUFFLE))
        {
            baseBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            baseBlock.status = BlockStatus.MATCH;
            baseBlock.DoActionClear(true);

            ClearLazer(targetBlock.breed);

            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 4);

            return true;
        }
        else if (targetBlock.questType == BlockQuestType.CLEAR_LAZER && (baseBlock.questType == BlockQuestType.CLEAR_SIMPLE || baseBlock.questType == BlockQuestType.SHUFFLE))
        {
            targetBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            targetBlock.status = BlockStatus.MATCH;
            targetBlock.DoActionClear(true);

            ClearLazer(baseBlock.breed);

            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 4);

            return true;
        }
        // 레이저 + 레이저  모든 블럭 폭발.
        else if (baseBlock.questType == BlockQuestType.CLEAR_LAZER && targetBlock.questType == BlockQuestType.CLEAR_LAZER)
        {
            baseBlock.questType = BlockQuestType.CLEAR_SIMPLE;
            targetBlock.questType = BlockQuestType.CLEAR_SIMPLE;

            baseBlock.status = BlockStatus.MATCH;
            targetBlock.status = BlockStatus.MATCH;

            baseBlock.DoActionClear(true);
            targetBlock.DoActionClear(true);

            StartCoroutine(ClearAll());

            MainGameManager.GetInstance.playerCb.pv.RPC("SetAnimInt", RpcTarget.All, "Attack", 3);

            return true;
        }

        return false;
    }
}





















