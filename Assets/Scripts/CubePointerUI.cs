using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePointerUI : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private RectTransform pointerRect;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float screenEdgeOffset = 60f;

    private void Start()
    {
        if (!cam) cam = Camera.main;
    }

    private void Update()
    {
        Transform target = GameManager.Instance != null ? GameManager.Instance.CurrentCubeTarget : null;

        if (target == null)
        {
            pointerRect.gameObject.SetActive(false);
            return;
        }

        pointerRect.gameObject.SetActive(true);

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        bool isBehind = screenPos.z < 0f;

        if (isBehind)
        {
            screenPos *= -1f;
        }

        Vector2 screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 dir = ((Vector2)screenPos - screenCenter).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        pointerRect.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Vector2 clampedPos = screenCenter + dir * (Mathf.Min(Screen.width, Screen.height) * 0.5f - screenEdgeOffset);

        clampedPos.x = Mathf.Clamp(clampedPos.x, screenEdgeOffset, Screen.width - screenEdgeOffset);
        clampedPos.y = Mathf.Clamp(clampedPos.y, screenEdgeOffset, Screen.height - screenEdgeOffset);

        pointerRect.position = clampedPos;
    }
}
