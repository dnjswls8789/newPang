using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    EMPTY = 0,
    BASIC = 1,
    HORZ = 2,
    VERT = 3,
    CIRCLE = 4,
    LAZER = 5,
    SHUFFLE = 6,
}

public enum BlockBreed
{
    NA = -1,   //Not Assigned
    BREED_0 = 0,
    BREED_1 = 1,
    BREED_2 = 2,
    BREED_3 = 3,
    BREED_4 = 4,
    BREED_5 = 5,
}

public enum BlockStatus
{
    NORMAL,                 // 평상시
    MATCH,                  // 매칭 블럭 있는 상태
    CLEAR                   // 클리어 예정 상태
}

public enum MatchType
{
    NONE = 0,
    THREE = 3,    // 3 Match
    FOUR = 4,    // 4 Match     -> CLEAR_HORZ 또는 VERT 퀘스트
    FIVE = 5,    // 5 Match     -> CLEAR_LAZER 퀘스트
    THREE_THREE = 6,    // 3 + 3 Match -> CLEAR_CIRCLE 퀘스트 
    THREE_FOUR = 7,    // 3 + 4 Match -> CLEAR_CIRCLE 퀘스트
    THREE_FIVE = 8,    // 3 + 5 Match -> CLEAR_LAZER 퀘스트
    FOUR_FIVE = 9,    // 4 + 5 Match -> CLEAR_LAZER 퀘스트
    FOUR_FOUR = 10,   // 4 + 4 Match -> CLEAR_CIRCLE 퀘스트
}

public enum BlockQuestType  //블럭 클리어 발동 효과
{
    NONE = -1,
    CLEAR_SIMPLE = 0,       // 단일 블럭 제거
    CLEAR_HORZ = 1,         // 세로줄 블럭 제거 (내구도 -1)  ->  4 match 가로형
    CLEAR_VERT = 2,         // 가로줄 블럭 제거 -> 4 match 세로형
    CLEAR_CIRCLE = 3,       // 인접한 주변영역 블럭 제거 -> T L 매치 (3 x 3, 4 x 3)
    CLEAR_LAZER = 4,        // 지정된 블럭과 동일한 블럭 전체 제거 --> 5 match
    CLEAR_HORZ_BUFF = 5,    // HORZ + CIRCLE 조합
    CLEAR_VERT_BUFF = 6,    // VERT + CIRCLE 조합    
    CLEAR_CIRCLE_BUFF = 7,  // CIRCLE + CIRCLE 조합
    CLEAR_LAZER_BUFF = 8,    // LAZER + LAZER 조합
    SHUFFLE = 9    // LAZER + LAZER 조합
}

//static class BlockMethod
//{
//    public static bool IsSafeEqual(this Block block, Block targetBlock)
//    {
//        if (block == null || targetBlock == null)
//            return false;

//        if (block.IsEmpty() || targetBlock.IsEmpty())
//            return false;

//        return block.breed == targetBlock.breed;
//    }
//}
