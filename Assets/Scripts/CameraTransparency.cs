using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransparency : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Transform target;

    private Renderer lastRenderer;

    void Update()
    {
        if (!target) return;

        HandleTransparency();
    }

    private void HandleTransparency()
    {
        Ray ray = new Ray(target.position, transform.position - target.position);

        if (Physics.Raycast(ray, out RaycastHit hit, Vector3.Distance(target.position, transform.position), obstacleMask))
        {
            HandleHit(hit);
        }
        else
        {
            if (lastRenderer)
            {
                SetAlpha(lastRenderer, 1f);
                lastRenderer = null;
            }
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        Renderer renderer = hit.collider.GetComponent<Renderer>();
        if (!renderer) return;

        if (lastRenderer && lastRenderer != renderer)
        {
            SetAlpha(lastRenderer, 1f);
        }

        SetAlpha(renderer, 0.3f);
        lastRenderer = renderer;
    }

    void SetAlpha(Renderer r, float alpha)
    {
        foreach (var mat in r.materials)
        {
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;
        }
    }
}
