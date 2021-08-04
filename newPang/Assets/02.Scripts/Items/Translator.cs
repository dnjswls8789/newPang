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
            {"Bunny10", "홍니" }, {"Bunny11", "풍바니" }, { "Bunny12", "보라니"}, { "Bunny13", "버버니"}, { "Bunny14", "싼타바니"},
            //
            {"Dog00", "백구" }, {"Dog01", "점박이" }, { "Dog02", "점순이"}, { "Dog03", "점돌이"}, { "Dog04", "멍구"},
            {"Dog05", "치즈돌이" }, {"Dog06", "초코" }, { "Dog07", "핑구"}, { "Dog08", "똥개"}, { "Dog09", "달마시안"},
            {"Dog10", "간지멍" }, {"Dog11", "황구" }, { "Dog12", "맹구"}, { "Dog13", "치즈"}, { "Dog14", "싼타멍"},
            //
            {"Monkey00", "흰몽이" }, {"Monkey01", "오랑이" }, { "Monkey02", "반몽이"}, { "Monkey03", "곤몽이"}, { "Monkey04", "오공"},
            {"Monkey05", "날몽이" }, {"Monkey06", "하트몽" }, { "Monkey07", "앵그리몽"}, { "Monkey08", "환타몽"}, { "Monkey09", "초록몽"},
            {"Monkey10", "흥몽" }, {"Monkey11", "민초몽" }, { "Monkey12", "체스몽"}, { "Monkey13", "노랭이몽"}, { "Monkey14", "싼타몽"},

            // Face
            //{"Face00", "곰도리" }, {"Face01", "점도리" }, { "Face02", "샤벳도리"}, { "Face03", "곤도리"}, { "Face04", "누도리"},
            //{"Face05", "판다" }, {"Face06", "코도리" }, { "Face07", "흑돌"}, { "Face08", "군도리"}, { "Face09", "홍도리"},
            //{"Face10", "샤도리" }, {"Face10b", "민초도리" }, { "Face11", "하토리"}, { "Face12", "예도리"}, { "Face13", "싼타도리"},
            //{"Face14", "곰도리" }, {"Face15", "점도리" }, { "Face16", "샤벳도리"}, { "Face17", "곤도리"}, { "Face18", "누도리"},
            //{"Face19", "판다" }, {"Face19b", "코도리" }, { "Face20", "흑돌"}, { "Face21", "군도리"}, { "Face22", "홍도리"},
            //{"Face23", "샤도리" }, {"Face24", "민초도리" }, { "Face25", "하토리"}, { "Face26", "예도리"}, { "Face26b", "싼타도리"},
            //{"Face27", "샤도리" }, {"Face28", "민초도리" }, { "Face28b", "하토리"},

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
