using UnityEngine;


public class PlayerInput : MonoBehaviour
{
    public static Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    public static bool GetJump()
    {
        return Input.GetButtonDown("Jump");
    }

    public static bool ReleaseJump()
    {
        return Input.GetButtonUp("Jump");
    }

    public static bool Attack()
    {
        return Input.GetButtonDown("Attack");
    }

    public static bool Grab()
    {
        return Input.GetButtonDown("Grab");
    }
}
