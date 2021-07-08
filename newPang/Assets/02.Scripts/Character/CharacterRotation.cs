using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRotation : MonoBehaviour
{
    bool down;
    Vector3 point;
    private void OnMouseDown()
    {
        point = Input.mousePosition;
        down = true;
    }

    private void OnMouseUp()
    {
        point = Vector3.zero;
        down = false;
    }

    private void OnMouseDrag()
    {
        if (down)
        {
            Vector3 curPoint = Input.mousePosition;
            float dist = Vector3.Distance(point, curPoint);
            int left;

            if (curPoint.x - point.x < 0)
            {
                left = 1;
            }
            else
            {
                left = -1;
            }

            point = curPoint;

            transform.Rotate(0, dist * left * 0.35f, 0);
        }
    }
}
