using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class DropSlot : MonoBehaviour
{
    // --- Registre global de tous les slots actifs --- // --- Global register of all active slots ---
    public static readonly List<DropSlot> All = new List<DropSlot>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    private GameObject currentBlock;

    [Header("Placement & Contraintes")]
    public bool autoClampInside = true;
    public bool rejectIfTooBig = true;
    public bool clampX = true;
    public bool clampY = true;
    public bool clampZ = false;     // Z libre par défaut // Z free by default

    [Header("Offsets de placement")]
    public float placeYOffset = 0f;
    public float placeZOffset = 0.25f;
    public bool useSlotForward = true;

    [Header("Marge intérieure")]
    public float extraPaddingX = 0.02f;
    public float extraPaddingY = 0.02f;
    public float extraPaddingZ = 0.0f;

    [Header("Visuel du slot (optionnel)")]
    public Renderer slotRenderer;                 // quad / plane indicateur // quad / plane indicator
    public string slotColorProperty = "_BaseColor";
    public Color emptyColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color filledColor = new Color(0.25f, 0.9f, 0.25f, 1f);
    public Color hoverColor = new Color(0.25f, 0.6f, 1f, 1f);

    private SegmentConfig segmentConfig;
    private bool isHovered;
    private MaterialPropertyBlock _mpb;

    void Awake()
    {
        if (segmentConfig == null)
            segmentConfig = GetComponentInParent<SegmentConfig>();
        _mpb = new MaterialPropertyBlock();
    }

    void Start()
    {
        Refresh();
        UpdateVisual();
    }

    // --- API état --- // --- API status ---
    public bool IsEmpty() => currentBlock == null;

    public void PlaceBlock(GameObject block)
    {
        currentBlock = block;
        UpdateVisual();
    }

    public void ClearSlot()
    {
        currentBlock = null;
        UpdateVisual();
    }

    public void SetHover(bool on)
    {
        if (isHovered == on) return;
        isHovered = on;
        UpdateVisual();
    }

    public void Refresh()
    {
        currentBlock = (transform.childCount > 0) ? transform.GetChild(0).gameObject : null;
        if (segmentConfig == null)
            segmentConfig = GetComponentInParent<SegmentConfig>();
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (slotRenderer == null) return;

        Color c = emptyColor;
        if (!IsEmpty()) c = filledColor;
        else if (isHovered) c = hoverColor;

        slotRenderer.GetPropertyBlock(_mpb);
        var mat = slotRenderer.sharedMaterial;
        if (mat != null && mat.HasProperty(slotColorProperty))
            _mpb.SetColor(slotColorProperty, c);
        else
            _mpb.SetColor("_Color", c); // fallback Standard
        slotRenderer.SetPropertyBlock(_mpb);
    }

    /// <summary>
    /// Placement robuste : clamp centre des bounds en X/Y à l’intérieur du segment (Z libre par défaut). // Robust placement: clamp center of bounds in X/Y inside the segment (Z free by default).
    /// </summary>
    public bool TryPlaceBlock(GameObject block)
    {
        // Parenter d’abord // Parent first
        block.transform.SetParent(transform, worldPositionStays: true);

        // Direction offset Z
        Vector3 zDir = useSlotForward ? transform.forward : Vector3.forward;

        // Centre cible initial (slot + offsets) // Initial target center (slot + offsets)
        Vector3 targetCenter = transform.position
                             + new Vector3(0f, placeYOffset, 0f)
                             + zDir * placeZOffset;

        // Sans SegmentConfig : aligner le centre et accepter // Without SegmentConfig: Align center and accept
        if (segmentConfig == null || !autoClampInside)
        {
            var r0 = block.GetComponentInChildren<Renderer>();
            if (r0 != null)
            {
                Bounds b0 = r0.bounds;
                block.transform.position += targetCenter - b0.center;
            }
            currentBlock = block;
            UpdateVisual();
            return true;
        }

        // Zone autorisée // Authorized area
        Bounds inner = segmentConfig.GetWorldInnerBounds();
        inner.Expand(new Vector3(-extraPaddingX, -extraPaddingY, -extraPaddingZ));

        // Bounds bloc // Bounds block
        var rend = block.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"[DropSlot] Le bloc {block.name} n'a pas de Renderer.");
            return false;
        }
        Bounds bb = rend.bounds;

        Vector3 halfBlock = bb.extents;
        Vector3 halfInner = inner.extents;

        // Trop grand ? // Too big?
        bool tooBigX = clampX && (halfBlock.x > halfInner.x);
        bool tooBigY = clampY && (halfBlock.y > halfInner.y);
        bool tooBigZ = clampZ && (halfBlock.z > halfInner.z);

        if (rejectIfTooBig && (tooBigX || tooBigY || tooBigZ))
        {
            block.transform.SetParent(null, true);
            Debug.Log($"[DropSlot] Refus : {block.name} trop grand (X:{tooBigX} Y:{tooBigY} Z:{tooBigZ}).");
            return false;
        }

        // Clamp du centre // Center Clamp
        Vector3 minCenter = inner.center - (halfInner - halfBlock);
        Vector3 maxCenter = inner.center + (halfInner - halfBlock);

        Vector3 desiredCenter = targetCenter;
        if (clampX) desiredCenter.x = Mathf.Clamp(desiredCenter.x, minCenter.x, maxCenter.x);
        if (clampY) desiredCenter.y = Mathf.Clamp(desiredCenter.y, minCenter.y, maxCenter.y);
        if (clampZ) desiredCenter.z = Mathf.Clamp(desiredCenter.z, minCenter.z, maxCenter.z);

        // Ajuster position pour faire coïncider centre(bounds) et desiredCenter // Adjust position to match center(bounds) and desiredCenter
        Vector3 moveDelta = desiredCenter - bb.center;
        block.transform.position += moveDelta;

        currentBlock = block;
        UpdateVisual();
        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = IsEmpty() ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.6f, 0.6f, 0.6f));
    }
}
