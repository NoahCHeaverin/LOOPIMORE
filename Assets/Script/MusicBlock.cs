using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicBlock : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private Camera cam;
    private Vector3 startPosition;
    private float startPlaneY;

    [Header("Drag config")]
    public float snapRadius = 1.5f;          // plus grand = plus indulgent // bigger = more forgiving
    public bool destroyIfDropFailed = false; // si true (depuis UI), détruit si pas de slot valide // if true (from UI), destroyed if no valid slot

    private DropSlot hoveredSlot;            // slot le plus proche (highlight) // nearest slot (highlight)

    [Header("Audio (optionnel)")]
    public AudioClip clip;

    void Start()
    {
        cam = Camera.main;
        startPosition = transform.position;
        startPlaneY = startPosition.y;
    }

    // Appelée par l'UI pour démarrer un drag immédiatement // Called by the UI to start a drag immediately
    public void BeginDragFromUI()
    {
        cam = Camera.main;
        isDragging = true;
        destroyIfDropFailed = false; // <-- TIP: Leave this at false to avoid disappearances
        Vector3 p = GetMouseWorldPosition(startPlaneY);
        transform.position = p;
        startPosition = p;
        startPlaneY = p.y;

        // Détacher d'un slot éventuel // Detach from a possible slot
        var parentSlot = GetComponentInParent<DropSlot>();
        if (parentSlot != null)
        {
            parentSlot.ClearSlot();
            transform.SetParent(null, true);
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        Debug.Log("Dragging block");
        Vector3 p = GetMouseWorldPosition(startPlaneY);
        offset = transform.position - p;

        var parentSlot = GetComponentInParent<DropSlot>();
        if (parentSlot != null)
        {
            parentSlot.ClearSlot();
            transform.SetParent(null, true);
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // Essayer d'abord le slot "hover" // Try the "hover" slot first
        DropSlot target = hoveredSlot ?? FindNearestSlot(transform.position, snapRadius);
        if (target != null && target.IsEmpty())
        {
            if (target.TryPlaceBlock(gameObject))
            {
                startPosition = transform.position;
                hoveredSlot?.SetHover(false);
                hoveredSlot = null;
                return;
            }
        }

        // drop invalide // invalid drop
        hoveredSlot?.SetHover(false);
        hoveredSlot = null;

        if (destroyIfDropFailed)
            Destroy(gameObject);
        else
            transform.position = startPosition;
    }

    void Update()
    {
        if (!isDragging) return;

        // Plan de drag = hauteur du slot survolé si dispo, sinon hauteur de départ // Drag plane = height of the slot flown over if available, otherwise starting height
        float planeY = (hoveredSlot != null) ? hoveredSlot.transform.position.y : startPlaneY;

        Vector3 mousePos = GetMouseWorldPosition(planeY);
        transform.position = mousePos + offset;

        // Highlight : slot le plus proche dans le rayon // Highlight: closest slot in radius
        DropSlot nearest = FindNearestSlot(transform.position, snapRadius);
        if (nearest != hoveredSlot && hoveredSlot != null)
        {
            hoveredSlot.SetHover(false);
            hoveredSlot = nearest;
            if (hoveredSlot != null && hoveredSlot.IsEmpty()) hoveredSlot.SetHover(true);
        }
    }

    Vector3 GetMouseWorldPosition(float planeY)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
        if (plane.Raycast(ray, out float d)) return ray.GetPoint(d);
        return transform.position;
    }

    DropSlot FindNearestSlot(Vector3 pos, float radius)
    {
        DropSlot best = null;
        float bestSqr = radius * radius;

        // Utilise le registre global → pas besoin de colliders // Uses global registry → no need for colliders
        foreach (var slot in DropSlot.All)
        {
            if (slot == null || !slot.isActiveAndEnabled) continue;
            if (!slot.IsEmpty()) continue;

            float sqr = (slot.transform.position - pos).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                best = slot;
            }
        }
        return best;
    }

    public void SetStartPosition(Vector3 pos)
    {
        startPosition = pos;
        startPlaneY = pos.y;
    }
}
