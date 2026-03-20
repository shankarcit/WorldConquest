using UnityEngine;
using UnityEngine.UI;
using WorldConquest.Map;
using WorldConquest.Game;

namespace WorldConquest.UI
{
    /// <summary>
    /// Minimap — bottom-right territory overview (GDD §5.2).
    ///
    /// Architecture:
    ///   • A secondary orthographic camera (MinimapCam) sits far above the map,
    ///     looking straight down, and renders everything to a RenderTexture.
    ///   • A UI RawImage displays that RenderTexture in the bottom-right corner.
    ///   • A small RectTransform overlay (ViewportIndicator) shows the region
    ///     the main camera is currently viewing.
    ///   • A coloured quad (PlayerMarker) sits in world-space on the minimap
    ///     layer to highlight the player's country centroid.
    ///
    /// Scene setup:
    ///   1. Create a RenderTexture asset (256×144, Depth 16) → assign to MinimapRenderTexture.
    ///   2. Create a Camera child of this GameObject → assign to MinimapCamera.
    ///      Set it: Orthographic, Size=100, ClearFlags=SolidColor (dark bg),
    ///      TargetTexture = the RenderTexture above, Culling Mask = Everything.
    ///   3. In Canvas, create UI > Raw Image in bottom-right → assign to MinimapDisplay.
    ///      Set its Texture to the same RenderTexture.
    ///   4. Create a child RectTransform named "ViewportIndicator" inside MinimapDisplay
    ///      (white border image, no fill) → assign to ViewportIndicator.
    ///   5. Assign MainCamera reference.
    ///
    /// Attach to: any persistent GameObject (e.g. GameManager).
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [Header("Cameras")]
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private Camera mainCamera;

        [Header("UI")]
        [SerializeField] private RawImage   minimapDisplay;
        [SerializeField] private RectTransform viewportIndicator;

        [Header("Render Texture")]
        [SerializeField] private RenderTexture minimapRenderTexture;

        [Header("World Bounds (degrees)")]
        [SerializeField] private float worldWidth  = 360f;  // –180 to +180 lon
        [SerializeField] private float worldHeight = 180f;  // –90 to +90 lat

        // Player country centroid marker (created at runtime)
        private GameObject playerMarker;
        private static readonly Color PlayerColor = new Color(0f, 0.8f, 1f, 1f); // cyan

        void Awake()
        {
            MapEventBus.OnCountryConquered += OnCountryConquered;
        }

        void OnDestroy()
        {
            MapEventBus.OnCountryConquered -= OnCountryConquered;
        }

        void Start()
        {
            SetupMinimapCamera();
            CreatePlayerMarker();
        }

        void LateUpdate()
        {
            UpdateViewportIndicator();
            UpdatePlayerMarker();
        }

        // ── Setup ────────────────────────────────────────────────────────────

        private void SetupMinimapCamera()
        {
            if (minimapCamera == null) return;

            // Position directly above the map centre, looking straight down
            minimapCamera.transform.position   = new Vector3(0f, 500f, 0f);
            minimapCamera.transform.rotation   = Quaternion.Euler(90f, 0f, 0f);
            minimapCamera.orthographic         = true;
            minimapCamera.orthographicSize     = worldHeight * 0.5f; // fits full map vertically
            minimapCamera.nearClipPlane        = 1f;
            minimapCamera.farClipPlane         = 1000f;

            if (minimapRenderTexture != null)
                minimapCamera.targetTexture = minimapRenderTexture;

            if (minimapDisplay != null && minimapRenderTexture != null)
                minimapDisplay.texture = minimapRenderTexture;
        }

        private void CreatePlayerMarker()
        {
            // Small quad in world space, rendered by minimap camera only
            playerMarker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            playerMarker.name = "MinimapPlayerMarker";
            playerMarker.transform.localScale = new Vector3(8f, 8f, 1f);
            playerMarker.transform.rotation   = Quaternion.Euler(90f, 0f, 0f);
            playerMarker.transform.position   = new Vector3(0f, 1f, 0f); // just above map plane

            // Destroy collider — it's a visual-only marker
            Destroy(playerMarker.GetComponent<Collider>());

            // Apply colour via material instance
            var rend = playerMarker.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                rend.material.color = PlayerColor;
            }

            playerMarker.SetActive(false); // hidden until game starts
        }

        // ── Per-frame ────────────────────────────────────────────────────────

        private void UpdatePlayerMarker()
        {
            if (playerMarker == null) return;

            var gm = GameManager.Instance;
            if (gm == null || !gm.IsGameStarted || gm.PlayerCountry == null)
            {
                playerMarker.SetActive(false);
                return;
            }

            var p = gm.PlayerCountry;

            // Use centroid of first polygon as marker position
            if (p.Polygons != null && p.Polygons.Count > 0 && p.Polygons[0].Length > 0)
            {
                Vector2 centroid = ComputeCentroid(p.Polygons[0]);
                playerMarker.transform.position = new Vector3(centroid.x, 1f, centroid.y);
                playerMarker.SetActive(true);
            }
        }

        private void UpdateViewportIndicator()
        {
            if (viewportIndicator == null || minimapCamera == null || mainCamera == null) return;
            if (minimapDisplay == null) return;

            // Project main camera frustum corners onto the minimap RawImage rect
            Rect displayRect = minimapDisplay.rectTransform.rect;

            // World bounds of the minimap (orthographic)
            float camSize  = minimapCamera.orthographicSize;
            float aspect   = minimapCamera.aspect;
            float worldL   = minimapCamera.transform.position.x - camSize * aspect;
            float worldR   = minimapCamera.transform.position.x + camSize * aspect;
            float worldB   = minimapCamera.transform.position.z - camSize;
            float worldT   = minimapCamera.transform.position.z + camSize;
            float ww       = worldR - worldL;
            float wh       = worldT - worldB;

            // Main camera's visible area in world space (orthographic)
            float mSize    = mainCamera.orthographicSize;
            float mAspect  = mainCamera.aspect;
            float mL       = mainCamera.transform.position.x - mSize * mAspect;
            float mB       = mainCamera.transform.position.z - mSize;
            float mW       = mSize * mAspect * 2f;
            float mH       = mSize * 2f;

            // Convert to normalised minimap space
            float nx = (mL  - worldL) / ww;
            float ny = (mB  - worldB) / wh;
            float nw = mW / ww;
            float nh = mH / wh;

            // Map to RectTransform (pivot 0,0 bottom-left)
            viewportIndicator.anchorMin        = Vector2.zero;
            viewportIndicator.anchorMax        = Vector2.zero;
            viewportIndicator.pivot            = Vector2.zero;
            viewportIndicator.anchoredPosition = new Vector2(nx * displayRect.width,
                                                              ny * displayRect.height);
            viewportIndicator.sizeDelta        = new Vector2(nw * displayRect.width,
                                                              nh * displayRect.height);
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void OnCountryConquered(CountryData loser, CountryData winner)
        {
            // Conquered countries are already turned dark grey by CountryMesh.
            // The minimap camera renders the same meshes, so it updates automatically.
            // Nothing extra needed here.
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static Vector2 ComputeCentroid(Vector2[] ring)
        {
            Vector2 sum = Vector2.zero;
            foreach (var v in ring)
                sum += v;
            return sum / ring.Length;
        }
    }
}
