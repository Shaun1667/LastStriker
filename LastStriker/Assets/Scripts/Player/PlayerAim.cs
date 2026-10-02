using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    public static PlayerAim Instance { get; private set; }
    public Vector2 ScreenPosition { get; private set; }
    public Camera aimCamera;

    void Awake()
    {
        Instance = this;
        if (aimCamera == null) aimCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
            pos.y = Mathf.Clamp(pos.y, 0, Screen.height);
            ScreenPosition = pos;
        }
    }

    public Ray GetAimRay()
    {
        return aimCamera.ScreenPointToRay(ScreenPosition);
    }
}
