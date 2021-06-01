using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Cell : MonoBehaviour
{
    public PhotonView pv;
    SpriteRenderer m_SpriteRenderer;

    public Vector2Int m_cellPosition;
    public Vector2Int cellPosition
    {
        get { return m_cellPosition; }
        set { m_cellPosition = value; }
    }

    [PunRPC]
    public void SetCellPosition(int x, int y)
    {
        if (MainGameManager.GetInstance.IsCoOpHost())
        {
            pv.RPC("SetCellPosition", RpcTarget.Others, x, y);
        }
        cellPosition = new Vector2Int(x, y);
    }

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
        pv = GetComponent<PhotonView>();
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
