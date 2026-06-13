using UnityEngine;
using UnityEngine.UI;
 
/// <summary>
/// Attach this script to the CIRCLE GameObject.
///
/// Scene setup (all inside one Canvas):
///   Canvas
///   ├── UIBackground1   (RectTransform – the "trigger zone")
///   ├── UIBackground2   (RectTransform – shown when circle overlaps UI1)
///   └── Circle          ← attach this script here
///
/// In the Inspector, drag:
///   • UIBackground1  →  uiBackground1
///   • UIBackground2  →  uiBackground2
/// </summary>
public class CircleReveal : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [Tooltip("The UI background that acts as the trigger zone.")]
    public RectTransform uiBackground1;
 
    [Tooltip("The UI background to show/hide.")]
    public RectTransform uiBackground2;
 
    // ── private ──────────────────────────────────────────────────────────
    private RectTransform _circleRect;
    private bool          _wasOverlapping = false;
 
    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _circleRect = GetComponent<RectTransform>();
 
        if (uiBackground1 == null)
            Debug.LogError("[CircleOverlapDetector] uiBackground1 is not assigned!");
 
        if (uiBackground2 == null)
            Debug.LogError("[CircleOverlapDetector] uiBackground2 is not assigned!");
 
        // Start hidden
        SetBackground2Visible(false);
    }
 
    // ─────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (uiBackground1 == null || uiBackground2 == null) return;
 
        bool overlapping = RectOverlaps(_circleRect, uiBackground1);
 
        // Only react on state change (enter / exit)
        if (overlapping != _wasOverlapping)
        {
            _wasOverlapping = overlapping;
            SetBackground2Visible(overlapping);
        }
    }
 
    // ─────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Returns true when the world-space axis-aligned bounding rectangles
    /// of two RectTransforms intersect.
    /// Works correctly regardless of Canvas render mode (Overlay / Camera / World).
    /// </summary>
    private static bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect worldA = GetWorldRect(a);
        Rect worldB = GetWorldRect(b);
        return worldA.Overlaps(worldB);
    }
 
    /// <summary>
    /// Converts a RectTransform into a world-space Rect (axis-aligned).
    /// </summary>
    private static Rect GetWorldRect(RectTransform rt)
    {
        // GetWorldCorners fills: [0]=bottom-left [1]=top-left [2]=top-right [3]=bottom-right
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
 
        float xMin = corners[0].x;
        float yMin = corners[0].y;
        float xMax = corners[2].x;
        float yMax = corners[2].y;
 
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
 
    // ─────────────────────────────────────────────────────────────────────
    private void SetBackground2Visible(bool visible)
    {
        // Toggle via GameObject active state so it can't interfere with layout
        uiBackground2.gameObject.SetActive(visible);
 
        Debug.Log($"[CircleOverlapDetector] UIBackground2 → {(visible ? "VISIBLE" : "HIDDEN")}");
    }
}