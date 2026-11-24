// Bahala Na? — world layer: in-game collision painter.
//
// Why this exists: authoring collision against a painted illustration by editing
// numbers is slow and error-prone. This lets you paint it while looking at the art.
//
// USAGE
//   1. Attach this component to the WorldBootstrap GameObject (same object).
//   2. Press Play. The world launches as usual.
//   3. Press F1 to show/hide the collision overlay (red = blocked cells).
//   4. Hold LEFT MOUSE and drag to BLOCK cells. Hold RIGHT MOUSE to UNBLOCK.
//   5. Press F2 to save. Press F3 to print C# you can paste into WorldMap.cs.
//   6. Stop play mode. Collision persists in PlayerPrefs across runs.
//
// The overlay is a 32x18 texture stretched over the plaza, so what you see is
// exactly the grid the player collides against.

using UnityEngine;

namespace BahalaNa.World
{
    public class CollisionPainter : MonoBehaviour
    {
        public KeyCode ToggleOverlayKey = KeyCode.F1;
        public KeyCode SaveKey         = KeyCode.F2;
        public KeyCode DumpCodeKey     = KeyCode.F3;
        public KeyCode ClearAllKey     = KeyCode.F4;

        private WorldMap _map;
        private WorldBootstrap _boot;
        private bool _visible;
        private SpriteRenderer _overlay;
        private Texture2D _tex;
        private Camera _cam;

        private static readonly Color BlockedColor = new Color(1f, 0.25f, 0.2f, 0.45f);
        private static readonly Color FreeColor    = new Color(0f, 0f, 0f, 0f);

        private void Start()
        {
            _boot = GetComponent<WorldBootstrap>();
            _cam = Camera.main;
        }

        private void Update()
        {
            if (_boot == null || _boot.Map == null)
            {
                if (_boot != null && _boot.Map == null) { }
            }
            _map = _boot != null ? _boot.Map : null;
            if (_map == null) return;

            if (Input.GetKeyDown(ToggleOverlayKey)) ToggleOverlay();
            if (Input.GetKeyDown(SaveKey)) Save();
            if (Input.GetKeyDown(DumpCodeKey)) DumpCode();
            if (Input.GetKeyDown(ClearAllKey)) { ClearAll(); }

            if (_visible) Paint();
        }

        // ------------------------------------------------------------- overlay
        private void ToggleOverlay()
        {
            _visible = !_visible;
            if (_visible) BuildOverlay();
            else if (_overlay != null) _overlay.enabled = false;
        }

        private void BuildOverlay()
        {
            if (_tex == null)
            {
                _tex = new Texture2D(GridUtil.Cols, GridUtil.Rows, TextureFormat.RGBA32, false);
                _tex.filterMode = FilterMode.Point;
            }
            if (_overlay == null)
            {
                var go = new GameObject("CollisionOverlay");
                go.transform.SetParent(transform, false);
                _overlay = go.AddComponent<SpriteRenderer>();
                _overlay.sortingOrder = 5000;     // above characters
                _overlay.color = new Color(1f, 1f, 1f, 0.85f);
            }

            var sprite = Sprite.Create(
                _tex,
                new Rect(0, 0, GridUtil.Cols, GridUtil.Rows),
                new Vector2(0f, 0f),      // bottom-left pivot
                GridUtil.CellSize);       // 1 texel = 1 cell = 64 px

            _overlay.sprite = sprite;
            _overlay.enabled = true;
            _overlay.transform.position = Vector3.zero;
            _overlay.transform.localScale = Vector3.one;

            RefreshTexture();
        }

        private void RefreshTexture()
        {
            if (_tex == null || _map == null) return;
            for (int r = 0; r < GridUtil.Rows; r++)
                for (int c = 0; c < GridUtil.Cols; c++)
                    _tex.SetPixel(c, r, _map.IsBlocked(c, r) ? BlockedColor : FreeColor);
            _tex.Apply();
        }

        // -------------------------------------------------------------- paint
        private void Paint()
        {
            bool block   = Input.GetMouseButton(0);
            bool unblock = Input.GetMouseButton(1);

            if (!block && !unblock) return;
            if (_cam == null) { _cam = Camera.main; if (_cam == null) return; }

            Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
            var cell = GridUtil.PixelToCell(GridUtil.WorldToPixel(wp));
            if (!GridUtil.InBounds(cell)) return;

            if (_map.IsBlocked(cell.x, cell.y) != block)
            {
                _map.SetBlocked(cell.x, cell.y, block);
                RefreshTexture();
            }
        }

        // ------------------------------------------------------------ persist
        private const string PrefKey = "bahalana.collision.v2";

        private void Save()
        {
            var sb = new System.Text.StringBuilder();
            for (int r = 0; r < GridUtil.Rows; r++)
            {
                for (int c = 0; c < GridUtil.Cols; c++)
                    sb.Append(_map.IsBlocked(c, r) ? '1' : '0');
                if (r < GridUtil.Rows - 1) sb.Append(';');
            }
            PlayerPrefs.SetString(PrefKey, sb.ToString());
            PlayerPrefs.Save();
            Debug.Log("[Collision] Saved " + GridUtil.Cols + "x" + GridUtil.Rows + " grid to PlayerPrefs.");
        }

        public static bool TryLoad(WorldMap map)
        {
            string data = PlayerPrefs.GetString(PrefKey, "");
            if (string.IsNullOrEmpty(data)) return false;
            var rows = data.Split(';');
            if (rows.Length != GridUtil.Rows) return false;
            for (int r = 0; r < GridUtil.Rows; r++)
                for (int c = 0; c < GridUtil.Cols && c < rows[r].Length; c++)
                    map.SetBlocked(c, r, rows[r][c] == '1');
            Debug.Log("[Collision] Loaded saved grid.");
            return true;
        }

        private void ClearAll()
        {
            for (int r = 0; r < GridUtil.Rows; r++)
                for (int c = 0; c < GridUtil.Cols; c++)
                    _map.SetBlocked(c, r, false);
            RefreshTexture();
            Debug.Log("[Collision] Cleared all cells.");
        }

        // ------------------------------------------------------- dump as code
        private void DumpCode()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[Collision] Paste into WorldMap.ApplyDefaultCollision():");
            for (int r = 0; r < GridUtil.Rows; r++)
            {
                int runStart = -1;
                for (int c = 0; c <= GridUtil.Cols; c++)
                {
                    bool blocked = c < GridUtil.Cols && _map.IsBlocked(c, r);
                    if (blocked && runStart < 0) runStart = c;
                    else if (!blocked && runStart >= 0)
                    {
                        sb.AppendLine($"BlockRect({runStart}, {r}, {c}, {r + 1});");
                        runStart = -1;
                    }
                }
            }
            Debug.Log(sb.ToString());
        }
    }
}
