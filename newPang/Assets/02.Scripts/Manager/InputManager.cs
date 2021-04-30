using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Swipe
{
    NA = -1,
    RIGHT = 0,
    UP = 1,
    LEFT = 2,
    DOWN = 3
}

public static class SwipeDirMethod
{
    public static int GetTargetRow(this Swipe swipeDir)
    {
        switch (swipeDir)
        {
            case Swipe.DOWN: return -1; ;
            case Swipe.UP: return 1;
            default:
                return 0;
        }
    }

    public static int GetTargetCol(this Swipe swipeDir)
    {
        switch (swipeDir)
        {
            case Swipe.LEFT: return -1; ;
            case Swipe.RIGHT: return 1;
            default:
                return 0;
        }
    }
}

public interface IInputHandlerBase
{
    bool isInputDown { get; }
    bool isInputUp { get; }
    Vector2 inputPosition { get; } //Screen(픽셀) 좌표
}

public class MouseHandler : IInputHandlerBase
{
    bool IInputHandlerBase.isInputDown => Input.GetButtonDown("Fire1");
    bool IInputHandlerBase.isInputUp => Input.GetButtonUp("Fire1");

    Vector2 IInputHandlerBase.inputPosition => Input.mousePosition;
}

public class InputManager
{
    Transform m_Container;

    IInputHandlerBase m_InputHandler = new MouseHandler();

    public InputManager(Transform container)
    {
        m_Container = container;
    }

    public bool isTouchDown => m_InputHandler.isInputDown;
    public bool isTouchUp => m_InputHandler.isInputUp;
    public Vector2 touchPosition => m_InputHandler.inputPosition;
    public Vector2 touch2BoardPosition => TouchToPosition(m_InputHandler.inputPosition);

    /*
     * 터치 좌표(Screen 좌표)를 보드의 루트인 컨테이너 기준으로 변경된 2차원 좌표를 리턴한다
     * @param vtInput Screen 좌표 즉, 스크린 픽셀 기준 좌표 (좌하(0,0) -> 우상(Screen.Width, Screen.Height))
     * */
    Vector2 TouchToPosition(Vector3 vtInput)
    {
        //1. 스크린 좌표 -> 월드 좌표
        Vector3 vtMousePosW = Camera.main.ScreenToWorldPoint(vtInput);

        //2. 컨테이너 local 좌표계로 변환(컨테이너 위치 이동시에도 컨테이너 기준의 로컬 좌표계이므로 화면 구성이 유연하다)
        Vector3 vtContainerLocal = m_Container.transform.InverseTransformPoint(vtMousePosW);

        return vtContainerLocal;
    }


        /*
        * 두 지점을 사용하여 Swipe 방향을 구한다.
        * UP : 45~ 135, LEFT : 135 ~ 225, DOWN : 225 ~ 315, RIGHT : 0 ~ 45, 0 ~ 315
        */
    public Swipe EvalSwipeDir(Vector2 vtStart, Vector2 vtEnd)
    {
        float angle = EvalDragAngle(vtStart, vtEnd);
        if (angle < 0)
            return Swipe.NA;

        int swipe = (((int)angle + 45) % 360) / 90;

        switch (swipe)
        {
            case 0: return Swipe.UP;
            case 1: return Swipe.RIGHT;
            case 2: return Swipe.DOWN;
            case 3: return Swipe.LEFT;
        }

        return Swipe.NA;
    }

    /*
     * 두 포인트 사이의 각도를 구한다.
     * Input(마우스, 터치) 장치 드래그시에 드래그한 각도를 구하는데 활용한다.
     */
    float EvalDragAngle(Vector2 vtStart, Vector2 vtEnd)
    {
        Vector2 dragDirection = vtEnd - vtStart;
        if (dragDirection.magnitude <= 0.3f)    // 스왑인식 거리
            return -1f;

        //Debug.Log($"eval angle : {vtStart} , {vtEnd}, magnitude = {dragDirection.magnitude}");

        float aimAngle = Mathf.Atan2(dragDirection.y, dragDirection.x);
        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        return aimAngle * Mathf.Rad2Deg;
    }
}

