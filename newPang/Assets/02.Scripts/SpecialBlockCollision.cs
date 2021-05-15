using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialBlockCollision : MonoBehaviour
{
    public Vector3 baseScale;

    public void InitScale()
    {
        //transform.localScale = baseScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Block"))
        {
            Block block = collision.gameObject.GetComponent<Block>();

            // 이 블럭이 보드 안에 있는지 체크해야한다.
            if (block != null && block.IsClearable())
            {
                block.status = BlockStatus.MATCH;
                block.DoActionClear(true);
            }

        }
    }
}
