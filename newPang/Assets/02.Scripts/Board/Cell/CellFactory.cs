using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CellFactory
{
    public static Cell SpawnCell(StageInfo stageInfo, int nRow, int nCol)
    {
        Debug.Assert(stageInfo != null);
        Debug.Assert(nRow < stageInfo.row && nCol < stageInfo.col);

        return SpawnCell(stageInfo.GetCellType(nRow, nCol));
    }

    public static Cell SpawnCell(CellType cellType)
    {
        Cell cell = null;
        if (MainGameManager.GetInstance.gameType == GameType.Battle)
        {
            cell = MainGameManager.GetInstance.board.cellParent.gameObject.AddChildFromObjPool("Cell").GetComponent<Cell>();
        }
        else if (MainGameManager.GetInstance.gameType == GameType.CoOp)
        {
            cell = PhotonManager.GetInstance.InstantiateWithPhoton(MainGameManager.GetInstance.board.blockParent, "Cell", Vector3.zero).GetComponent<Cell>();
        }
        cell.InitCell(cellType);
        return cell;
    }
}
