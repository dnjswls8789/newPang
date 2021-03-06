using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public static class Action2D
{
    /*
     * 지정된 시간동안 지정된 위치로 이동한다.
     * 
     * @param target 애니메이션을 적용할 타겟 GameObject
     * @param to 이동할 목표 위치
     * @param duration 이동 시간
     * @param bSelfRemove 애니메이션 종류 후 타겟 GameObject 삭제 여부 플래그
     */
    //static bool[] c = new bool[10000];

    public static IEnumerator MoveTo(Transform target, Vector3 to, float duration, bool bSelfRemove = false)
    {
        //bool s = false;
        //PhotonView pv = target.GetComponent<PhotonView>();
        //if (pv != null)
        //{
        //    if (c[pv.ViewID])
        //    {
        //        Debug.LogError(pv.ViewID + " / 진행 중");
        //        s = true;
        //    }
        //    c[pv.ViewID] = true;
        //}

        Vector2 startPos = target.transform.position;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.smoothDeltaTime;
            target.transform.position = Vector2.Lerp(startPos, to, elapsed / duration);

            yield return null;
        }

        //if (pv != null)
        //{
        //    if (s && c[pv.ViewID])
        //    {
        //        Debug.LogError(pv.ViewID + " / 뒤에 놈이 먼저 끝남");
        //    }
        //
        //    c[pv.ViewID] = false;
        //}

        target.transform.position = to;

        if (bSelfRemove)
            target.gameObject.InstantEnqueue();

        yield break;

    }

    /*
     * param toScale 커지는(줄어지는) 크기, 예를 들어, 0.5인 경우 현재 크기에서 절반으로 줄어든다.
     * param speed 초당 커지는 속도. 예를 들어, 2인 경우 초당 2배 만큼 커지거나 줄어든다. 
     */
    public static IEnumerator Scale(Transform target, float toScale, float duration, bool bSelfRemove = false)
    {
        ////1. 방향 결정 : 커지는 방향이면 +, 줄어드는 방향이면 -
        //bool bInc = target.localScale.x < toScale;
        //float fDir = bInc ? 1 : -1;

        //float factor;
        //while (true)
        //{
        //    factor = Time.deltaTime * speed * fDir;
        //    target.localScale = new Vector3(target.localScale.x + factor, target.localScale.y + factor, target.localScale.z);

        //    if ((!bInc && target.localScale.x <= toScale) || (bInc && target.localScale.x >= toScale))
        //        break;

        //    yield return null;
        //}

        //yield break;

        Vector3 startScale = target.localScale;
        Vector3 endScale = new Vector3(toScale, toScale, toScale);

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);

            yield return null;
        }

        target.localScale = endScale;

        //if (bSelfRemove)
            //target.gameObject.InstantEnqueue();

        yield break;
    }

}
