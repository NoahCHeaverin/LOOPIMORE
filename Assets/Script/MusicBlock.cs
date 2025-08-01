using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MusicBlock : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private Camera cam;
    private Vector3 startPosition;

    public AudioClip clip;
    public float snapRadius = 0.6f; // rayon de recherche d'un slot

    void Start()
    {
        cam = Camera.main;
        startPosition = transform.position;
    }

    void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - GetMouseWorldPosition();

        // Si on était dans un slot, on libère ce slot
        var parentSlot = GetComponentInParent<DropSlot>();
        if (parentSlot != null)
        {
            parentSlot.ClearSlot();
            transform.SetParent(null, true);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Trouver le DropSlot le plus proche dans un rayon limité
        DropSlot nearest = FindNearestSlot(transform.position, snapRadius);
        if (nearest != null && nearest.IsEmpty())
        {
            nearest.PlaceBlock(gameObject); // => FitBlock appliqué ici
            startPosition = transform.position; // nouvelle "base"
            return;
        }

        // Sinon, retour
        transform.position = startPosition;
    }

    void Update()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float d)) return ray.GetPoint(d);
        return transform.position;
    }

    DropSlot FindNearestSlot(Vector3 pos, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(pos, radius);
        DropSlot best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            var slot = h.GetComponent<DropSlot>();
            if (slot == null) continue;

            float dist = Vector3.SqrMagnitude(slot.transform.position - pos);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = slot;
            }
        }
        return best;
    }

    public void SetStartPosition(Vector3 pos) => startPosition = pos;
}
