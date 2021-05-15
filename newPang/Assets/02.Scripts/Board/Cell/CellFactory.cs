﻿using System.Collections;
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
        Cell cell = BattleSceneManager.GetInstance.board.cellParent.gameObject.AddChildFromObjPool("Cell").GetComponent<Cell>();
        cell.InitCell(cellType);
        return cell;
    }
}
