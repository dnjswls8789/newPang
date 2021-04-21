﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] BlockConfig m_BlockConfig;
    SpriteRenderer m_SpriteRenderer;

    public BlockStatus status;

    public BlockBreed m_Breed;   //렌더링되는 블럭 캐린터(즉, 이미지 종류)
    public BlockBreed breed
    {
        get { return m_Breed; }
        set
        {
            m_Breed = value;
            UpdateView();
        }
    }

    protected BlockType m_BlockType;
    public BlockType type
    {
        get { return m_BlockType; }
        set { m_BlockType = value; }
    }

    protected BlockQuestType m_QuestType;
    public BlockQuestType questType
    {
        get { return m_QuestType; }
        set { m_QuestType = value; }
    }

    Vector2Int m_cellPosition;
    public Vector2Int cellPosition
    {
        get { return m_cellPosition; }
        set { m_cellPosition = value; }
    }
    public void SetCellPosition(int x, int y)
    {
        cellPosition = new Vector2Int(x, y);
    }

    int m_Durability;                 //내구도
    public virtual int durability
    {
        get { return m_Durability; }
        set { m_Durability = value; }
    }

    private bool swiping;
    public bool isSwiping
    {
        get { return swiping; }
        set { swiping = value; }
    }

    private bool droping;
    public bool isDroping
    {
        get { return droping; }
        set { droping = value; }
    }

    public void InitBlock(BlockBreed _breed, BlockType _type, Vector2Int position)
    {
        status = BlockStatus.NORMAL;

        breed = _breed;
        type = _type;
        cellPosition = position;

        m_Durability = 1;
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

    public void DoActionClear()
    {
        StartCoroutine(CoStartSimpleExplosion(true));
    }

    /*
     * 블럭이 폭발한 후, GameObject를 삭제한다.
     */
    IEnumerator CoStartSimpleExplosion(bool bDestroy = true)
    {
        //1. 크기가 줄어드는 액션 실행한다 : 폭파되면서 자연스럽게 소멸되는 모양 연출, 1 -> 0.3으로 줄어든다.
        yield return Action2D.Scale(transform, Constants.BLOCK_DESTROY_SCALE, 4f);

        //2. 폭파시키는 효과 연출 : 블럭 자체의 Clear 효과를 연출한다.
        GameObject explosionObj = BattleSceneManager.GetInstance.gameObject.AddChildFromObjPool(m_BlockConfig.GetExplosionObject(questType).name, 2f);
        ParticleSystem.MainModule newModule = explosionObj.GetComponent<ParticleSystem>().main;
        newModule.startColor = m_BlockConfig.GetBlockColor(breed);

        explosionObj.SetActive(true);
        explosionObj.transform.position = this.transform.position;

        yield return new WaitForSeconds(0.1f);

        //3. 블럭 GameObject 객체 삭제 or make size zero
        gameObject.InstantEnqueue();
    }

    public bool IsEmpty()
    {
        return type == BlockType.EMPTY;
    }

    public bool IsSwipale()
    {
        return !isDroping && isSwiping && status == BlockStatus.NORMAL;
    }
}