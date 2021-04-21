using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    SpriteRenderer m_SpriteRenderer;

    protected CellType m_CellType;
    public CellType type
    {
        get { return m_CellType; }
        set { m_CellType = value; }
    }

    public void InitCell(CellType cellType)
    {
        m_CellType = cellType;
    }

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        //UpdateView();
    }

    public void UpdateView()
    {
        switch (type)
        {
            case CellType.EMPTY:
                m_SpriteRenderer.sprite = null;
                break;
            case CellType.BASIC:
                break;
            case CellType.FIXTURE:
                break;
            case CellType.JELLY:
                break;
            default:
                break;
        }
    }

    public void Move(float x, float y)
    {
        transform.position = new Vector3(x, y);
    }
}
