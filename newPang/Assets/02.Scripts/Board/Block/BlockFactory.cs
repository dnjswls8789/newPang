using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockFactory
{
    public static Block SpawnBlock(BlockType blockType, int blockCount, BlockBreed breed = BlockBreed.NA)
    {
        Block block = MainGameManager.GetInstance.board.blockParent.gameObject.AddChildFromObjPool("Block").GetComponent<Block>();
        block.board = MainGameManager.GetInstance.board;

        //Set Breed
        if (blockType == BlockType.BASIC)
        {
            // 다른거 매치될 거 있을 때,
            if (breed == BlockBreed.NA)
            {
                block.InitBlock((BlockBreed)UnityEngine.Random.Range(0, blockCount), blockType, new Vector2Int(0, 0));
                //block.breed = (BlockBreed)UnityEngine.Random.Range(0, blockCount);
            }
            // 매치될 거 없어서 만들어줘야 할 때,
            else
            {
                block.InitBlock(breed, blockType, new Vector2Int(0, 0));
                //block.breed = breed;
            }
        }
        else if (blockType == BlockType.EMPTY)
        {
            block.InitBlock(BlockBreed.NA, blockType, new Vector2Int(0, 0));
            //block.breed = BlockBreed.NA;
        }
        return block;
    }
}
