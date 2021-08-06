using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator : MonoBehaviour
{
    private static Dictionary<string, string> objectName;

    private static void CharacterNameInit()
    {
        objectName = new Dictionary<string, string>
        {
            // Character
            {"Bear00", "곰도리" }, {"Bear01", "점도리" }, { "Bear02", "샤벳도리"}, { "Bear03", "곤도리"}, { "Bear04", "누도리"},
            {"Bear05", "판다" }, {"Bear06", "코도리" }, { "Bear07", "흑돌"}, { "Bear08", "군도리"}, { "Bear09", "홍도리"},
            {"Bear10", "샤도리" }, {"Bear11", "민초도리" }, { "Bear12", "하토리"}, { "Bear13", "예도리"}, { "Bear14", "싼타도리"},
            //
            {"Cat00", "나비" }, {"Cat01", "등이" }, { "Cat02", "줄냥이"}, { "Cat03", "흑냥"}, { "Cat04", "핑크냥이"},
            {"Cat05", "태비냥" }, {"Cat06", "샴냥이" }, { "Cat07", "치즈냥"}, { "Cat08", "샤벳냥"}, { "Cat09", "군냥이"},
            {"Cat10", "꾸냥" }, {"Cat11", "깜냥" }, { "Cat12", "귀염냥"}, { "Cat13", "달빛냥이"}, { "Cat14", "싼타냥"},
            //
            {"Frog00", "흰굴이" }, {"Frog01", "개굴이" }, { "Frog02", "두껍이"}, { "Frog03", "청개굴이"}, { "Frog04", "독굴이"},
            {"Frog05", "범생굴이" }, {"Frog06", "핑굴이" }, { "Frog07", "똑굴이"}, { "Frog08", "흑굴"}, { "Frog09", "그굴이"},
            {"Frog10", "퍼플굴" }, {"Frog11", "눈굴이" }, { "Frog12", "샤벳굴이"}, { "Frog13", "흰점굴이"}, { "Frog14", "싼타굴"},
            //
            {"Bunny00", "바니" }, {"Bunny01", "샤벳바니" }, { "Bunny02", "핑바니"}, { "Bunny03", "점바니"}, { "Bunny04", "흑토"},
            {"Bunny05", "반바니" }, {"Bunny06", "초바니" }, { "Bunny07", "민초바니"}, { "Bunny08", "곤바니"}, { "Bunny09", "군바니"},
            {"Bunny10", "홍니" }, {"Bunny11", "퐁바니" }, { "Bunny12", "보라니"}, { "Bunny13", "버버니"}, { "Bunny14", "싼타바니"},
            //
            {"Dog00", "백구" }, {"Dog01", "점박이" }, { "Dog02", "점순이"}, { "Dog03", "점돌이"}, { "Dog04", "멍구"},
            {"Dog05", "치즈돌이" }, {"Dog06", "초코" }, { "Dog07", "핑구"}, { "Dog08", "똥개"}, { "Dog09", "달마시안"},
            {"Dog10", "간지멍" }, {"Dog11", "황구" }, { "Dog12", "맹구"}, { "Dog13", "치즈"}, { "Dog14", "싼타멍"},
            //
            {"Monkey00", "흰몽이" }, {"Monkey01", "오랑이" }, { "Monkey02", "반몽이"}, { "Monkey03", "곤몽이"}, { "Monkey04", "오공"},
            {"Monkey05", "날몽이" }, {"Monkey06", "하트몽" }, { "Monkey07", "앵그리몽"}, { "Monkey08", "환타몽"}, { "Monkey09", "초록몽"},
            {"Monkey10", "흑몽" }, {"Monkey11", "민초몽" }, { "Monkey12", "체스몽"}, { "Monkey13", "노랭이몽"}, { "Monkey14", "싼타몽"},

            // Face
            {"Face01", "보통" }, { "Face02", "오잉"}, { "Face03", "스마일"}, { "Face04", "윙크A"}, {"Face05", "훗" }, 
            {"Face06", "꺄아" }, { "Face07", "정색"}, { "Face08", "화남A"}, { "Face09", "놀람"},
            {"Face10", "슬픔" }, {"Face10b", "웃픔" }, { "Face11", "그래"}, { "Face12", "윙크B"}, { "Face13", "억울"},
            {"Face14", "화남B" }, {"Face15", "츄" }, { "Face16", "민망"}, { "Face17", "뀨"}, { "Face18", "츄릅"},
            {"Face19", "헤롱A" }, {"Face19b", "헤롱B" }, { "Face20", "헤롱C"}, { "Face21", "메롱"}, { "Face22", "알쏭"},
            {"Face23", "앙마" }, {"Face24", "반짝" }, { "Face25", "주금"}, { "Face26", "하트"}, { "Face26b", "발그레"},
            {"Face27", "머니" }, {"Face28", "쥬금" }, { "Face28b", "쥬금2"},

            // Hand
            {"BoneA", "뼉다구" }, { "BoneB", "막대사탕"}, { "BoneC", "정강이뼈"}, { "CleaverA", "식칼"}, {"FishmaceA", "광어" },
            {"FishmaceA2", "생선" }, { "FishmaceA3", "고등어"}, { "FishmaceA4", "연어"}, { "HookA", "후크팔"},
            {"HookB", "꼬챙이" }, {"HookC", "갈고리" }, { "MonkeymaceA", "여의봉"}, { "MonkeymaceB", "정의봉"}, { "MonkeymaceC", "흑봉"},
            {"SwordToyA", "단검" }, {"SwordToyB", "얼음검" }, { "SwordToyC", "불검"}, { "WalkingstickA", "신사봉"}, { "WalkingstickA2", "지팡이"},
            {"WalkingstickA3", "우산" }, {"WalkingstickA4", "싼타팡" },

            //Bag
            {"BackpackA", "책가방A" }, { "BackpackA2", "책가방B"}, { "BackpackA3", "책가방C"}, { "BackpackA4", "분홍팩"}, {"BagA", "전대A" },
            {"BagA2", "전대B" }, { "BagA3", "전대C"}, { "BagA4", "일수가방"}, { "SantabagA", "보따리"},
            {"SantabagA2", "더블백" }, {"WrapA", "망토" }, { "WrapB", "누더기"}, { "WrapC", "망토B"},

            // Head
            {"Cap_turboA", "프로팰러A" }, { "Cap_turboB", "프로팰러B"}, { "Cap_turboC", "프로팰러C"}, { "CrownA", "왕관"}, {"CrownB", "은관" },
            {"CrownC", "핑크관" }, { "HatA", "누구게"}, { "HeadringA", "금고아"}, { "HeadringB", "홍고아"},
            {"HeadringC", "흑고아" }, {"MinihatA", "신사" }, { "MinihatA2", "엉클샘"}, { "MinihatA3", "정모"}, { "MinihatA4", "파티모"},
            {"Pirate_HatA", "해적모" }, {"Pirate_HatB", "선장모" }, { "Pirate_HatC", "하트모"}, { "SantahatA", "산타모"}, { "SantahatA2", "초록모"},

            // Etc
            {"CollarA", "목걸이A" }, { "CollarB", "목걸이B"}, { "CollarC", "목걸이C"}, { "GlassesA", "뿔테A"}, {"GlassesA2", "뿔테B" },
            {"GlassesA3", "뿔테C" }, { "GlassesA4", "핑크테"}, {"MustacheA", "산타수염" }, {"MustacheA2", "수염" }, 
            { "ScarfA", "스카프A"}, { "ScarfA2", "스카프B"}, { "ScarfA3", "스카프C"},
        };
    }


    public static string TranslationName(string _text)
    {
        if (objectName == null)
        {
            CharacterNameInit();
        }

        if (objectName.ContainsKey(_text))
        {
            return objectName[_text];
        }
        else
        {
            return "";
        }
    }

}
