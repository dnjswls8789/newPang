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
        for (int nCol = 0; nCol < m_Col; nCol++)
        {
            for (int nRow = 0; nRow < m_Row; nRow++)
            {
                //3.1 Cell GameObject 생성을 요청한다.GameObject가 생성되지 않는 경우에 null을 리턴한다.
                Cell cell = m_Cells[nRow, nCol];
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

    public void DoSwipeAction(int nRow, int nCol, Swipe swipeDir)
    {
        StartCoroutine(CoDoSwipeAction(nRow, nCol, swipeDir));
    }

    IEnumerator CoDoSwipeAction(int nRow, int nCol, Swipe swipeDir)
    {
        int nSwipeRow = nRow, nSwipeCol = nCol;
        nSwipeRow += swipeDir.GetTargetRow(); //Right : +1, LEFT : -1
        nSwipeCol += swipeDir.GetTargetCol(); //UP : +1, DOWN : -1

        float initX = CalcInitX(0.5f);
        float initY = CalcInitY(0.5f);

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
            blocks[nRow, nCol] = targetBlock;
            blocks[nSwipeRow, nSwipeCol] = baseBlock;
            yield return new WaitForSeconds(Constants.SWIPE_DURATION);


            blocks[nRow, nCol]?.Move(initX + nRow, initY + nCol);
            blocks[nSwipeRow, nSwipeCol]?.Move(initX + nSwipeRow, initY + nSwipeCol);

            baseBlock.isSwiping = false;
            targetBlock.isSwiping = false;

            targetBlock.SetCellPosition(nRow, nCol);
            baseBlock.SetCellPosition(nSwipeRow, nSwipeCol);

            bool baseCheck = false;
            bool targetCheck = false;
            SwipedBlockPangCheck(nRow, nCol, out baseCheck);
            SwipedBlockPangCheck(nSwipeRow, nSwipeCol, out targetCheck);
            //////////// 움직인 두개 매치 체크 하고 둘다 아니면 돌려보내기 ////////////////////
            if (!baseCheck && !targetCheck)
            {
                baseBlock = blocks[nRow, nCol];
                targetBlock = blocks[nSwipeRow, nSwipeCol];

                basePos = baseBlock.transform.position;
                targetPos = targetBlock.transform.position;

                StartCoroutine(Action2D.MoveTo(baseBlock.transform, targetPos, Constants.SWIPE_DURATION));
                StartCoroutine(Action2D.MoveTo(targetBlock.transform, basePos, Constants.SWIPE_DURATION));

                baseBlock.isSwiping = true;
                targetBlock.isSwiping = true;

                //2.2.2 Board에 저장된 블럭의 위치를 교환한다
                blocks[nRow, nCol] = targetBlock;
                blocks[nSwipeRow, nSwipeCol] = baseBlock;
                yield return new WaitForSeconds(Constants.SWIPE_DURATION);


                blocks[nRow, nCol]?.Move(initX + nRow, initY + nCol);
                blocks[nSwipeRow, nSwipeCol]?.Move(initX + nSwipeRow, initY + nSwipeCol);

                baseBlock.isSwiping = false;
                targetBlock.isSwiping = false;

                targetBlock.SetCellPosition(nRow, nCol);
                baseBlock.SetCellPosition(nSwipeRow, nSwipeCol);

                SwipedBlockPangCheck(nRow, nCol, out baseCheck);
                SwipedBlockPangCheck(nSwipeRow, nSwipeCol, out targetCheck);
            }
        }

    }

    public void SwipedBlockPangCheck(int nRow, int nCol, out bool found)
    {
        found = false;
        List<Block> matchedBlockList = new List<Block>();
        List<Block> clearList = new List<Block>();

        Block baseBlock = blocks[nRow, nCol];
        if (baseBlock == null) return;

        matchedBlockList.Add(baseBlock);

        //1. 가로 블럭 검색
        Block block;

        //1.1 오른쪽 방향
        for (int i = nRow + 1; i < maxRow; i++)
        {
            block = m_Blocks[i, nCol];
            if (block == null) break;
            if (!block.IsSafeEqual(baseBlock) || block.status != BlockStatus.NORMAL || block.isSwiping)
                break;

            matchedBlockList.Add(block);
        }

        //1.2 왼쪽 방향
        for (int i = nRow - 1; i >= 0; i--)
        {
            block = m_Blocks[i, nCol];
            if (block == null) break;
            if (!block.IsSafeEqual(baseBlock) || block.status != BlockStatus.NORMAL || block.isSwiping)
                break;

            matchedBlockList.Add(block);
        }

        //1.3 매치된 상태인지 판단한다
        //    기준 블럭(baseBlock)을 제외하고 좌우에 2개이상이면 기준블럭 포함해서 3개이상 매치되는 경우로 판단할 수 있다
        if (matchedBlockList.Count >= 3)
        {
            for (int i = 0; i < matchedBlockList.Count; i++)
            {
                clearList.Add(matchedBlockList[i]);
            }

            //SetBlockStatusMatched(matchedBlockList, true);
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
            if (!block.IsSafeEqual(baseBlock) || block.status != BlockStatus.NORMAL || block.isSwiping)
                break;

            matchedBlockList.Add(block);
        }

        //2.2 아래쪽 검색
        for (int i = nCol - 1; i >= 0; i--)
        {
            block = m_Blocks[nRow, i];
            if (block == null) break;
            if (!block.IsSafeEqual(baseBlock) || block.status != BlockStatus.NORMAL || block.isSwiping)
                break;

            matchedBlockList.Add(block);
        }

        //2.3 매치된 상태인지 판단한다
        //    기준 블럭(baseBlock)을 제외하고 상하에 2개이상이면 기준블럭 포함해서 3개이상 매치되는 경우로 판단할 수 있다
        if (matchedBlockList.Count >= 3)
        {
            for (int i = 0; i < matchedBlockList.Count; i++)
            {
                // 가로에서 이미 찾아서 clearList 에 baseBlock 들어있을 때는 baseBlock 안넣음. (matchedList[0] == baseBlock)
                if (found && i == 0) continue;
                clearList.Add(matchedBlockList[i]);
            }

            //SetBlockStatusMatched(matchedBlockList, false);
            found = true;
        }

        if (found)
        {
            for (int i = 0; i < clearList.Count; i++)
            {
                clearList[i].status = BlockStatus.CLEAR;
                clearList[i].DoActionClear();
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
                blocks[row, col].breed = (BlockBreed)UnityEngine.Random.Range(0, BattleSceneManager.GetInstance.stage.blockCount);
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

                        BlockBreed newBreed = (BlockBreed)UnityEngine.Random.Range(0, BattleSceneManager.GetInstance.stage.blockCount);
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
                                    newBreed = (BlockBreed)UnityEngine.Random.Range(0, BattleSceneManager.GetInstance.stage.blockCount);
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
                                    newBreed = (BlockBreed)UnityEngine.Random.Range(0, BattleSceneManager.GetInstance.stage.blockCount);
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

                        BlockBreed newBreed = (BlockBreed)UnityEngine.Random.Range(0, BattleSceneManager.GetInstance.stage.blockCount);
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
                                    newBreed = (BlockBreed)UnityEngine.Random.Range(0, BattleSceneManager.GetInstance.stage.blockCount);
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
                                    newBreed = (BlockBreed)UnityEngine.Random.Range(0, BattleSceneManager.GetInstance.stage.blockCount);
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
                if (baseBlock == null || baseBlock.IsEmpty()) continue;

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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                        if (block != null)
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
                                    if (block != null)
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
                                    if (block != null)
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
                                    if (block != null)
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
                                    if (block != null)
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
}
