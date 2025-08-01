using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DropSlot : MonoBehaviour
{
    private GameObject currentBlock;

    [Header("Placement & Contraintes")]
    public bool autoClampInside = true;  // Clamper le CENTRE des bounds dans la zone du segment
    public bool rejectIfTooBig = true;   // Refuser si le bloc ne tient pas (selon axes clampés)

    [Tooltip("Contraindre X à l'intérieur du segment")]
    public bool clampX = true;

    [Tooltip("Contraindre Y à l'intérieur du segment")]
    public bool clampY = true;

    [Tooltip("Contraindre Z à l'intérieur du segment (laisser false pour autoriser le dépassement en Z)")]
    public bool clampZ = false;          // <- Z libre par défaut

    [Header("Offsets")]
    [Tooltip("Décalage vertical de base du CENTRE (laisser 0 si tu ne veux pas qu'il monte en Y)")]
    public float placeYOffset = 0f;

    [Tooltip("Décalage Z depuis le slot. Si useSlotForward=true, c’est le forward du slot ; sinon l’axe Z monde.")]
    public float placeZOffset = 0.2f;

    [Tooltip("Utiliser le forward du slot pour l’offset Z (souvent préférable si la piste est orientée).")]
    public bool useSlotForward = true;

    [Header("Marge intérieure (rétrécit la zone)")]
    public float extraPaddingX = 0.0f;
    public float extraPaddingY = 0.0f;
    public float extraPaddingZ = 0.0f;

    private SegmentConfig segmentConfig;

    void Awake()
    {
        if (segmentConfig == null)
            segmentConfig = GetComponentInParent<SegmentConfig>();
    }

    void Start()
    {
        Refresh();
    }

    public bool IsEmpty() => currentBlock == null;

    public void PlaceBlock(GameObject block) { currentBlock = block; }

    public void ClearSlot() { currentBlock = null; }

    public void Refresh()
    {
        currentBlock = (transform.childCount > 0) ? transform.GetChild(0).gameObject : null;
        if (segmentConfig == null)
            segmentConfig = GetComponentInParent<SegmentConfig>();
    }

    /// <summary>
    /// Place le bloc en garantissant :
    /// - Clamp du CENTRE des bounds en X/Y à l'intérieur du segment (optionnel par axe)
    /// - Z libre par défaut (clampZ=false) ; décalage en Z appliqué depuis le slot
    /// - Correction par delta = desiredCenter - currentBounds.center (pivot-agnostic)
    /// </summary>
    public bool TryPlaceBlock(GameObject block)
    {
        // Parentage d’abord (pour que bounds s’actualisent correctement)
        block.transform.SetParent(transform, worldPositionStays: true);

        // Direction pour l’offset Z
        Vector3 zDir = useSlotForward ? transform.forward : Vector3.forward;

        // Centre cible initial (slot + offsets)
        Vector3 targetCenter = transform.position
                             + new Vector3(0f, placeYOffset, 0f)
                             + zDir * placeZOffset;

        // Si pas de config, on aligne juste le centre sur targetCenter
        if (segmentConfig == null || !autoClampInside)
        {
            var r0 = block.GetComponentInChildren<Renderer>();
            if (r0 != null)
            {
                Bounds b0 = r0.bounds;
                block.transform.position += (targetCenter - b0.center);
            }
            currentBlock = block;
            return true;
        }

        // Zone autorisée (monde)
        Bounds inner = segmentConfig.GetWorldInnerBounds();
        inner.Expand(new Vector3(-extraPaddingX, -extraPaddingY, -extraPaddingZ));

        // Bounds monde du bloc
        var rend = block.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"[DropSlot] Le bloc {block.name} n'a pas de Renderer.");
            return false;
        }
        Bounds bb = rend.bounds;

        Vector3 halfBlock = bb.extents;
        Vector3 halfInner = inner.extents;

        // Trop grand selon les axes clampés
        bool tooBigX = clampX && (halfBlock.x > halfInner.x);
        bool tooBigY = clampY && (halfBlock.y > halfInner.y);
        bool tooBigZ = clampZ && (halfBlock.z > halfInner.z);

        if (rejectIfTooBig && (tooBigX || tooBigY || tooBigZ))
        {
            block.transform.SetParent(null, true);
            Debug.Log($"[DropSlot] Refus : {block.name} trop grand (X:{tooBigX} Y:{tooBigY} Z:{tooBigZ}).");
            return false;
        }

        // Limites autorisées pour le centre du bloc
        Vector3 minCenter = inner.center - (halfInner - halfBlock);
        Vector3 maxCenter = inner.center + (halfInner - halfBlock);

        // Centre désiré clampé (par axe)
        Vector3 desiredCenter = targetCenter;
        if (clampX) desiredCenter.x = Mathf.Clamp(desiredCenter.x, minCenter.x, maxCenter.x);
        if (clampY) desiredCenter.y = Mathf.Clamp(desiredCenter.y, minCenter.y, maxCenter.y);
        if (clampZ) desiredCenter.z = Mathf.Clamp(desiredCenter.z, minCenter.z, maxCenter.z);
        // sinon Z libre : targetCenter.z reste tel quel

        // Déplacer le bloc de sorte que le centre de ses bounds = desiredCenter
        Vector3 moveDelta = desiredCenter - bb.center;
        block.transform.position += moveDelta;

        currentBlock = block;
        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = IsEmpty() ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.6f, 0.6f, 0.6f));
    }
}
