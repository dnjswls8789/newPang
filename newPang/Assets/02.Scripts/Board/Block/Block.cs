using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BlockPos
{
    public int row { get; set; }
    public int col { get; set; }

    public BlockPos(int nRow = 0, int nCol = 0)
    {
        row = nRow;
        col = nCol;
    }

    //----------------------------------------------------------------------
    // Struct 필수 override function
    //----------------------------------------------------------------------
    public override bool Equals(object obj)
    {
        return obj is BlockPos pos && row == pos.row && col == pos.row;
    }

    public override int GetHashCode()
    {
        var hashCode = -928284752;
        hashCode = hashCode * -1521134295 + row.GetHashCode();
        hashCode = hashCode * -1521134295 + col.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return $"(row = {row}, col = {col})";
    }
}
public class Block : MonoBehaviour
{
    public Board board;
    [SerializeField] BlockConfig m_BlockConfig;
    SpriteRenderer m_SpriteRenderer;

    public BlockStatus status;
    public BlockType m_BlockType;

    public BlockBreed m_Breed;   //렌더링되는 블럭 캐린터(즉, 이미지 종류)

    public MatchType m_MatchType;

    public BlockQuestType m_QuestType;

    public Vector2Int m_cellPosition;
    public int m_Durability;                 //내구도
    public bool swiping;
    public bool droping;

    public BlockType type
    {
        get { return m_BlockType; }
        set { m_BlockType = value; }
    }
    public BlockBreed breed
    {
        get { return m_Breed; }
        set
        {
            m_Breed = value;
            UpdateView();
        }
    }
    public MatchType matchType
    {
        get { return m_MatchType; }
        set { m_MatchType = value; }
    }
    public BlockQuestType questType
    {
        get { return m_QuestType; }
        set { m_QuestType = value; }
    }
    public Vector2Int cellPosition
    {
        get { return m_cellPosition; }
        set { m_cellPosition = value; }
    }
    public void SetCellPosition(int x, int y)
    {
        cellPosition = new Vector2Int(x, y);
    }

    public virtual int durability
    {
        get { return m_Durability; }
        set { m_Durability = value; }
    }

    public bool isSwiping
    {
        get { return swiping; }
        set { swiping = value; }
    }

    public bool isDroping
    {
        get { return droping; }
        set { droping = value; }
    }

    public void InitBlock(BlockBreed _breed, BlockType _type, Vector2Int position)
    {
        status = BlockStatus.NORMAL;

        matchType = MatchType.NONE;
        type = _type;
        questType = BlockQuestType.CLEAR_SIMPLE;

        breed = _breed;

        cellPosition = position;

        m_Durability = 1;

        isSwiping = false;
        isDroping = false;

        transform.localScale = Vector3.one;
    }

    public void ChangeBlock()
    {
        status = BlockStatus.NORMAL;

        matchType = MatchType.NONE;
        type = BlockType.BASIC;
        questType = BlockQuestType.CLEAR_SIMPLE;

        m_Durability = 1;

        transform.localScale = Vector3.one;
    }

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateView()
    {
        switch (type)
        {
            case BlockType.EMPTY:
                m_SpriteRenderer.sprite = null;
                break;
            case BlockType.BASIC:
                m_SpriteRenderer.sprite = m_BlockConfig.basicBlockSprites[(int)breed];
                break;
            case BlockType.HORZ:
                m_SpriteRenderer.sprite = m_BlockConfig.horzBlockSprites[(int)breed];
                break;
            case BlockType.VERT:
                m_SpriteRenderer.sprite = m_BlockConfig.vertBlockSprites[(int)breed];
                break;
            case BlockType.CIRCLE:
                m_SpriteRenderer.sprite = m_BlockConfig.circleBlockSprites[(int)breed];
                break;
            case BlockType.LAZER:
                m_SpriteRenderer.sprite = m_BlockConfig.lazerSprite;
                break;
            default:
                break;
        }
    }

    public void Move(float x, float y)
    {
        transform.position = new Vector3(x, y);
    }


    // 블럭을 제거한다.  

    public void DoActionClear(bool bDestroy, System.Action callBack = null)
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(CoStartSimpleExplosion(bDestroy, callBack));
        }
    }

    /*
     * 블럭이 폭발한 후, GameObject를 삭제한다.
     */
    IEnumerator CoStartSimpleExplosion(bool bDestroy, System.Action callBack)
    {
        //1. 크기가 줄어드는 액션 실행한다 : 폭파되면서 자연스럽게 소멸되는 모양 연출, 1 -> 0.3으로 줄어든다.
        yield return Action2D.Scale(transform, Constants.BLOCK_DESTROY_SCALE, 0.2f);

        //2. 폭파시키는 효과 연출 : 블럭 자체의 Clear 효과를 연출한다.
        GameObject explosionObj = BattleSceneManager.GetInstance.gameObject.AddChildFromObjPool(m_BlockConfig.GetExplosionObject(questType).name, 2f);
        ParticleSystem.MainModule newModule = explosionObj.GetComponent<ParticleSystem>().main;
        newModule.startColor = m_BlockConfig.GetBlockColor(breed);

        explosionObj.SetActive(true);
        explosionObj.transform.position = transform.position;

        Quest();

        yield return new WaitForSeconds(0.15f);

        if (bDestroy)
        {
            //3. 블럭 GameObject 객체 삭제 or make size zero
            status = BlockStatus.CLEAR;
            gameObject.InstantEnqueue();
            //InitBlock()

            BattleSceneManager.GetInstance.AddScore(100);
        }
        else
        {
            ChangeBlock();
        }

        callBack?.Invoke();
    }

    public void BlockDropCheck()
    {
        if (IsDropable())
        {
            int dropIndex = 0;

            for (int i = 1; i < board.maxCol; i++)
            {
                if (cellPosition.y - i < 0) break;

                if (board.blocks[cellPosition.x, cellPosition.y - i].status == BlockStatus.CLEAR)
                {
                    dropIndex++;
                }
                else if (!board.blocks[cellPosition.x, cellPosition.y - i].IsDropable())
                {
                    break;
                }
            }

            if (dropIndex > 0)
            {
                StartCoroutine(CoDoDropAction(dropIndex));
            }
        }
    }

    public void SpawnBlockDrop(int dropCount)
    {
        if (IsDropable())
        {
            StartCoroutine(CoDoSpawnBlockDropAction(dropCount));
        }
    }

    IEnumerator CoDoDropAction(int dropIndex)
    {
        isDroping = true;

        Block baseBlock = this;
        Block targetBlock = board.blocks[cellPosition.x, cellPosition.y - dropIndex];

        float duration = m_BlockConfig.dropSpeed[dropIndex - 1];

        float initX = board.CalcInitX(0.5f);
        float initY = board.CalcInitY(0.5f);

        targetBlock.SetCellPosition(cellPosition.x, cellPosition.y);
        baseBlock.SetCellPosition(cellPosition.x, cellPosition.y - dropIndex);
        
        board.blocks[baseBlock.cellPosition.x, baseBlock.cellPosition.y] = baseBlock;
        board.blocks[targetBlock.cellPosition.x, targetBlock.cellPosition.y] = targetBlock;

        StartCoroutine(Action2D.MoveTo(baseBlock.transform, new Vector3(initX + baseBlock.cellPosition.x, initY + baseBlock.cellPosition.y), duration));
        targetBlock?.Move(initX + targetBlock.cellPosition.x, initY + targetBlock.cellPosition.y);

        yield return new WaitForSeconds(duration);

        isDroping = false;

        yield return null;

        bool found;
        board.BlockPangCheck(cellPosition.x, cellPosition.y, out found, false);
    }

    IEnumerator CoDoSpawnBlockDropAction(int dropCount)
    {
        isDroping = true;

        float duration = m_BlockConfig.dropSpeed[dropCount];

        float initX = board.CalcInitX(0.5f);
        float initY = board.CalcInitX(0.5f);

        board.blocks[cellPosition.x, cellPosition.y] = this;

        StartCoroutine(Action2D.MoveTo(transform, new Vector3(initX + cellPosition.x, initY + cellPosition.y), duration));

        yield return new WaitForSeconds(duration);

        isDroping = false;

        yield return null;

        bool found;
        board.BlockPangCheck(cellPosition.x, cellPosition.y, out found, false);
    }

    // 블럭의 매칭 결과를 조합한다
    public void MatchTypeAdd(MatchType matchTypeTarget)
    {
        if (matchType == MatchType.FOUR && matchTypeTarget == MatchType.FOUR)
            matchType = MatchType.FOUR_FOUR;

        matchType = (MatchType)((int)matchType + (int)matchTypeTarget);
    }

    public void MatchTypeUpdate(MatchType match, bool horzMatch)
    {
        switch (match)
        {
            case MatchType.NONE:
                questType = BlockQuestType.CLEAR_SIMPLE;
                type = BlockType.BASIC;
                break;
            case MatchType.THREE:
                questType = BlockQuestType.CLEAR_SIMPLE;
                type = BlockType.BASIC;
                break;
            case MatchType.FOUR:
                // 세로로 매치 됐으면 가로 특수블럭.
                if (horzMatch)
                {
                    questType = BlockQuestType.CLEAR_VERT;
                    type = BlockType.VERT;
                }
                else
                {
                    questType = BlockQuestType.CLEAR_HORZ;
                    type = BlockType.HORZ;
                }
                break;
            case MatchType.FIVE:
                questType = BlockQuestType.CLEAR_LAZER;
                type = BlockType.LAZER;
                breed = BlockBreed.NA;
                break;
            case MatchType.THREE_THREE:
                questType = BlockQuestType.CLEAR_CIRCLE;
                type = BlockType.CIRCLE;
                break;
            case MatchType.THREE_FOUR:
                questType = BlockQuestType.CLEAR_CIRCLE;
                type = BlockType.CIRCLE;
                break;
            case MatchType.THREE_FIVE:
                questType = BlockQuestType.CLEAR_LAZER;
                type = BlockType.LAZER;
                breed = BlockBreed.NA;
                break;
            case MatchType.FOUR_FIVE:
                questType = BlockQuestType.CLEAR_LAZER;
                type = BlockType.LAZER;
                breed = BlockBreed.NA;
                break;
            case MatchType.FOUR_FOUR:
                questType = BlockQuestType.CLEAR_CIRCLE;
                type = BlockType.CIRCLE;
                break;
            default:
                break;
        }

        UpdateView();

        bool found;
        board.BlockPangCheck(cellPosition.x, cellPosition.y, out found, false);
    }

    public void SetQuestType(BlockQuestType quest)
    {
        questType = quest;

        switch (questType)
        {
            case BlockQuestType.NONE:
                break;
            case BlockQuestType.CLEAR_SIMPLE:
                break;
            case BlockQuestType.CLEAR_HORZ:
                type = BlockType.HORZ;
                break;
            case BlockQuestType.CLEAR_VERT:
                type = BlockType.VERT;
                break;
            case BlockQuestType.CLEAR_CIRCLE:
                type = BlockType.CIRCLE;
                break;
            case BlockQuestType.CLEAR_LAZER:
                break;
            case BlockQuestType.CLEAR_HORZ_BUFF:
                break;
            case BlockQuestType.CLEAR_VERT_BUFF:
                break;
            case BlockQuestType.CLEAR_CIRCLE_BUFF:
                break;
            case BlockQuestType.CLEAR_LAZER_BUFF:
                break;
            default:
                break;
        }


        UpdateView();
    }
    public void Quest()
    {
        switch (questType)
        {
            case BlockQuestType.CLEAR_HORZ:
                board.ClearHorz(cellPosition.x, cellPosition.y);
                break;
            case BlockQuestType.CLEAR_VERT:
                board.ClearVert(cellPosition.x, cellPosition.y);
                break;
            case BlockQuestType.CLEAR_CIRCLE:
                board.ClearCircle(cellPosition.x, cellPosition.y);
                break;
            case BlockQuestType.CLEAR_LAZER:
                board.ClearLazer((BlockBreed)UnityEngine.Random.Range(0, BattleSceneManager.GetInstance.stage.blockCount));
                break;
            case BlockQuestType.CLEAR_HORZ_BUFF:
                break;
            case BlockQuestType.CLEAR_VERT_BUFF:
                break;
            case BlockQuestType.CLEAR_CIRCLE_BUFF:
                break;
            case BlockQuestType.CLEAR_LAZER_BUFF:
                break;
            default:
                break;
        }
    }

    // 같은 브리드인지.
    public bool IsSafeEqual(Block targetBlock)
    {
        if (targetBlock == null)
            return false;

        if (IsEmpty() || targetBlock.IsEmpty())
            return false;

        return breed == targetBlock.breed;
    }

    // 매치 체크.
    public bool MatchCheck(Block targetBlock)
    {
        //return IsSafeEqual(targetBlock) && status == BlockStatus.NORMAL && targetBlock.status == BlockStatus.NORMAL && !isSwiping && !isDroping;
        return IsSafeEqual(targetBlock) && IsMatchable(true) && targetBlock.IsMatchable(true);
    }

    // 떨어질 수 있는 상태인지.
    public bool IsDropable()
    {
        return status == BlockStatus.NORMAL && !IsEmpty() && !isSwiping && !isDroping;
    }

    // 빈 블럭인지.
    public bool IsEmpty()
    {
        return type == BlockType.EMPTY;
    }

    // 스왑 가능한 상태인지.
    public bool IsSwipeable()
    {
        return status == BlockStatus.NORMAL && !IsEmpty() && !isSwiping && !isDroping;
    }

    // 매칭 가능한 블럭인지.
    public bool IsMatchable(bool movingCheck = false)
    {
        if (!movingCheck)
            return status == BlockStatus.NORMAL && !IsEmpty() && breed != BlockBreed.NA;
        else
            return status == BlockStatus.NORMAL && !IsEmpty() && breed != BlockBreed.NA && !isSwiping && !isDroping;
    }

    public bool IsClearable()
    {
        return status == BlockStatus.NORMAL && !IsEmpty() && !isSwiping;
    }
}
