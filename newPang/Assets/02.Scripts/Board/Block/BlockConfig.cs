using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bingle/Block Config", fileName = "BlockConfig.asset")]
public class BlockConfig : ScriptableObject
{
    public float[] dropSpeed;
    public Sprite lazerSprite;
    public Sprite[] basicBlockSprites;
    public Sprite[] vertBlockSprites;
    public Sprite[] horzBlockSprites;
    public Sprite[] circleBlockSprites;
    public Sprite[] shuffleBlockSprites;
    public Color[] blockColors;
    public GameObject[] explosion;
    public GameObject[] simpleExplosionColor;
    public GameObject[] blockChangeEffects;

    public GameObject GetExplosionObject(BlockQuestType questType)
    {
        if (questType != BlockQuestType.NONE || questType != BlockQuestType.CLEAR_SIMPLE)
        {
            return explosion[(int)questType];
        }
        else
        {
            return explosion[0];
        }

    }

    public GameObject GetSimpleExplosionEffect(BlockBreed breed)
    {
        if ((int)breed < 0)
        {
            return explosion[0];
        }
        else
        {
            return simpleExplosionColor[(int)breed];
        }
    }

    public GameObject GetChangeBlockEffect(BlockBreed breed)
    {
        return blockChangeEffects[(int)breed];
    }
}
