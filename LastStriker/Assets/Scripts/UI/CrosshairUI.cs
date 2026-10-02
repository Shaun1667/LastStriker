using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    public RectTransform crosshairRect;
    public PlayerAim aim;

    void Update()
    {
        if (aim == null || crosshairRect == null) return;
        crosshairRect.position = new Vector3(aim.ScreenPosition.x, aim.ScreenPosition.y, 0f);
    }
}
