using UnityEngine;
using System.Collections.Generic;

public class TrackSegmentLooper : MonoBehaviour
{
    [Header("Track")]
    public GameObject trackSegmentPrefab;
    public int segmentCount = 20;

    [Tooltip("Longueur d’un segment en X. Peut être mesurée auto.")]
    public float segmentLength = 1f;

    public float moveSpeed = 2f;
    public Transform endPoint;

    [Header("Auto length")]
    [Tooltip("Mesure automatiquement la longueur réelle du visuel du segment (en X) au Start().")]
    public bool autoDetectSegmentLength = true;

    [Header("Visuals (segments)")]
    [Tooltip("Nom du sous-objet dans le prefab de segment à teinter (ex.: 'Visual'). Laisse vide pour teinter la racine.")]
    public string segmentVisualNodeName = "Visual";
    public Color filledColor = Color.green;
    public Color emptyColor = Color.red;
    [Tooltip("URP Lit = _BaseColor, Built-in Standard = _Color")]
    public string colorPropertyName = "_BaseColor";

    [Header("Music Blocks")]
    [Tooltip("1..N prefabs musicaux; un sera choisi aléatoirement quand un segment doit être rempli.")]
    public List<GameObject> musicPrefabs = new List<GameObject>();

    [Header("Pattern mode")]
    public bool usePattern = true;     // si false, on utilise holeChance
    [Tooltip("Segments de référence (chacun avec un DropSlot). Si le DropSlot a un enfant, on considère 'rempli'.")]
    public List<GameObject> patternSegments = new List<GameObject>();

    [Header("Random holes (si usePattern = false)")]
    [Range(0f, 1f)] public float holeChance = 0.25f;

    // Runtime
    public List<GameObject> segments = new List<GameObject>();

    // Renderers « du segment » (exclut tout renderer sous un DropSlot)
    private readonly List<List<Renderer>> _segmentOnlyRenderers = new List<List<Renderer>>();
    private MaterialPropertyBlock _mpb;

    void Start()
    {
        if (musicPrefabs == null || musicPrefabs.Count == 0)
        {
            Debug.LogError("[TrackSegmentLooper] musicPrefabs est vide : assigne au moins 1 prefab musical.");
            enabled = false; return;
        }

        if (usePattern)
        {
            if (patternSegments == null || patternSegments.Count == 0)
            {
                Debug.LogError("[TrackSegmentLooper] usePattern = true mais patternSegments est vide.");
                enabled = false; return;
            }
            foreach (var p in patternSegments)
            {
                var ds = p ? p.GetComponentInChildren<DropSlot>(true) : null;
                if (ds != null) ds.Refresh();
            }
        }

        if (autoDetectSegmentLength)
        {
            float measured = MeasurePrefabWorldLength(trackSegmentPrefab, segmentVisualNodeName);
            if (measured > 0f) segmentLength = measured;
        }

        _mpb = new MaterialPropertyBlock();

        // Instancier segments + collecter leurs renderers + appliquer état/couleur
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = transform.position + Vector3.right * (segmentLength * i);
            GameObject seg = Instantiate(trackSegmentPrefab, pos, Quaternion.identity, transform);
            segments.Add(seg);

            _segmentOnlyRenderers.Add(CollectSegmentRenderers(seg, segmentVisualNodeName));

            ApplyStateToSegment(seg, i);
        }
    }

    void Update()
    {
        foreach (var seg in segments)
            seg.transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        for (int i = 0; i < segments.Count; i++)
        {
            GameObject seg = segments[i];
            float rightEdge = seg.transform.position.x + (segmentLength * 0.5f);

            if (rightEdge <= endPoint.position.x)
            {
                float maxX = float.MinValue;
                for (int k = 0; k < segments.Count; k++)
                    if (segments[k].transform.position.x > maxX) maxX = segments[k].transform.position.x;

                seg.transform.position = new Vector3(maxX + segmentLength, seg.transform.position.y, seg.transform.position.z);

                ApplyStateToSegment(seg, i);
            }
        }
    }

    // ---------- Helpers ----------

    void ApplyStateToSegment(GameObject seg, int segIndex)
    {
        DropSlot segSlot = SafeGetDropSlot(seg, $"segment[{segIndex}]");
        if (segSlot == null) return;

        // Nettoyer le slot (supprimer ancien bloc)
        foreach (Transform child in segSlot.transform)
            Destroy(child.gameObject);
        segSlot.ClearSlot();

        bool shouldHaveBlock = DecideShouldHaveBlock(segIndex);

        if (shouldHaveBlock)
        {
            // Choix aléatoire d'un prefab musical
            GameObject prefab = musicPrefabs[Random.Range(0, musicPrefabs.Count)];
            GameObject block = Instantiate(prefab);

            // Placement sécurisé : clamp X/Y dans le segment, Z libre (selon DropSlot)
            if (segSlot.TryPlaceBlock(block))
            {
                var mb = block.GetComponent<MusicBlock>();
                if (mb != null) mb.SetStartPosition(block.transform.position);

                TintSegment(segIndex, filledColor);
            }
            else
            {
                Destroy(block);
                TintSegment(segIndex, emptyColor);
            }
        }
        else
        {
            TintSegment(segIndex, emptyColor);
        }
    }

    bool DecideShouldHaveBlock(int segIndex)
    {
        if (usePattern)
        {
            int refIndex = segIndex % patternSegments.Count;
            var refSlot = SafeGetDropSlot(patternSegments[refIndex], $"pattern[{refIndex}]");
            return refSlot != null && (!refSlot.IsEmpty() || refSlot.transform.childCount > 0);
        }
        else
        {
            return Random.value > holeChance; // true = bloc ; false = trou
        }
    }

    DropSlot SafeGetDropSlot(GameObject root, string label)
    {
        if (root == null)
        {
            Debug.LogWarning($"[TrackSegmentLooper] {label} est null.");
            return null;
        }
        var slot = root.GetComponentInChildren<DropSlot>(true);
        if (slot == null)
            Debug.LogWarning($"[TrackSegmentLooper] {label} n'a pas de DropSlot !");
        return slot;
    }

    List<Renderer> CollectSegmentRenderers(GameObject seg, string visualNodeName)
    {
        var list = new List<Renderer>();
        Transform rootToScan = seg.transform;

        if (!string.IsNullOrEmpty(visualNodeName))
        {
            var visual = seg.transform.Find(visualNodeName);
            if (visual != null) rootToScan = visual;
        }

        var rends = rootToScan.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            // On exclut les renderers situés SOUS un DropSlot (ce sont les blocs)
            if (r.GetComponentInParent<DropSlot>() != null) continue;
            list.Add(r);
        }
        return list;
    }

    void TintSegment(int segIndex, Color color)
    {
        if (_segmentOnlyRenderers == null || segIndex < 0 || segIndex >= _segmentOnlyRenderers.Count) return;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();

        var rends = _segmentOnlyRenderers[segIndex];
        foreach (var r in rends)
        {
            r.GetPropertyBlock(_mpb);

            // URP Lit : _BaseColor ; Built-in Standard : _Color
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(colorPropertyName))
            {
                _mpb.SetColor(colorPropertyName, color);
            }
            else
            {
                _mpb.SetColor("_Color", color);
            }

            r.SetPropertyBlock(_mpb);
        }
    }

    float MeasurePrefabWorldLength(GameObject prefab, string visualNodeName)
    {
        if (prefab == null) return 0f;

        GameObject temp = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        temp.SetActive(false);

        var rends = CollectSegmentRenderers(temp, visualNodeName);
        if (rends.Count == 0) { Destroy(temp); return 0f; }

        Bounds b = new Bounds(rends[0].bounds.center, rends[0].bounds.size);
        for (int i = 1; i < rends.Count; i++) b.Encapsulate(rends[i].bounds);

        float length = b.size.x;
        Destroy(temp);
        return length;
    }
}
