using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBUserData
{
    public string nickname;
    public long gold;

    public IDictionary characters;
    public IDictionary Items;   // face, hand, bag, head, etc 다섯개 string 값으로 찾아서 empty 면 x
}

public class DataManager : SingletonClass<DataManager>
{
    FBUserData userData;

    protected override void Awake()
    {
        base.Awake();

        userData = new FBUserData();
    }
}
