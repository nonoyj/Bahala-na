// Bahala Na? — world layer: grid-step player movement.
//
// Pokemon-style: one press = one tile step, smoothly interpolated. Input is
// sampled each frame but only consumed when the current step finishes, so
// holding a direction walks continuously and taps move exactly one tile.
//
// Animation: 2-frame atlas per direction, sourced from the 8 sliced sprites of
// boy_walk_atlas / girl_walk_atlas. Row order in the atlas is DOWN, UP, LEFT, RIGHT.

using UnityEngine;

namespace BahalaNa.World
{
    public class PlayerController : MonoBehaviour
    {
        // ---- tuning ----
        public float StepDuration = 0.16f;      // seconds per tile
        public float AnimFrameTime = 0.12f;     // seconds per walk frame

        // ---- state ----
        public Vector2Int Cell { get; private set; }
        public Vector2Int Facing { get; private set; } = Vector2Int.down;

        private WorldMap _map;
        private SpriteRenderer _sr;
        private Vector3 _from, _to;
        private float _t;
        private bool _stepping;

        // frames[dir][frame]  dir order: 0=down 1=up 2=left 3=right
        private Sprite[][] _frames;
        private float _animTimer;
        private int _animFrame;

        // ---------------------------------------------------------------- setup
        public void Init(WorldMap map, Sprite[] atlasSlices, Vector3 spawnWorld)
        {
            _map = map;

            _sr = gameObject.GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

            // atlasSlices must be length 8: [0,1]=down [2,3]=up [4,5]=left [6,7]=right
            _frames = new Sprite[4][];
            for (int d = 0; d < 4; d++)
            {
                if (atlasSlices != null && atlasSlices.Length >= (d + 1) * 2)
                    _frames[d] = new[] { atlasSlices[d * 2], atlasSlices[d * 2 + 1] };
                else
                    _frames[d] = new[] { atlasSlices != null && atlasSlices.Length > 0 ? atlasSlices[0] : null };
            }

            // Scale so the character reads at a sensible size against a 64px grid.
            // Atlas cell is ~89x170 px at import PPU 100 => 0.89 x 1.70 world units
            // at scale 1. We want roughly 2 tiles tall on screen (~1.28 units), so
            // scale ~0.75. Adjust if it looks wrong in play.
            transform.localScale = new Vector3(0.55f, 0.55f, 1f);

            Cell = GridUtil.PixelToCell(GridUtil.WorldToPixel(spawnWorld));
            transform.position = spawnWorld;
            _from = _to = spawnWorld;
            ApplySprite();
        }

        // ---------------------------------------------------------------- update
        private void Update()
        {
            if (_map == null) return;

            if (_stepping)
            {
                _t += Time.deltaTime;
                float k = Mathf.Clamp01(_t / StepDuration);
                transform.position = Vector3.Lerp(_from, _to, k);
                TickAnim();
                if (k >= 1f) { _stepping = false; transform.position = _to; }
                return;
            }

            var dir = ReadDirection();
            if (dir == Vector2Int.zero) { _animFrame = 0; ApplySprite(); return; }

            Facing = dir;
            ApplySprite();

            var target = Cell + dir;
            if (_map.IsBlocked(target.x, target.y)) return;   // wall: stand still, keep facing

            Cell = target;
            _from = transform.position;
            _to = GridUtil.CellToWorld(Cell.x, Cell.y);
            _t = 0f;
            _stepping = true;
        }

        private Vector2Int ReadDirection()
        {
            // Keyboard
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    return Vector2Int.up;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  return Vector2Int.down;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  return Vector2Int.left;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) return Vector2Int.right;

            // Touch / mouse: tap a point near the character and walk toward it.
            // Simple and good enough for a demo; replace with a virtual joystick later.
            Vector3? target = null;
#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                target = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
#else
            if (Input.GetMouseButtonDown(0))
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
#endif
            if (target.HasValue)
            {
                Vector2 delta = (Vector2)(target.Value - transform.position);
                if (delta.magnitude < 0.4f) return Vector2Int.zero;   // tapped on self
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            return Vector2Int.zero;
        }

        // ------------------------------------------------------------- animation
        private void TickAnim()
        {
            _animTimer += Time.deltaTime;
            if (_animTimer >= AnimFrameTime)
            {
                _animTimer = 0f;
                var set = _frames[DirIndex(Facing)];
                _animFrame = (_animFrame + 1) % Mathf.Max(1, set.Length);
                ApplySprite();
            }
        }

        private void ApplySprite()
        {
            if (_sr == null || _frames == null) return;
            var set = _frames[DirIndex(Facing)];
            if (set == null || set.Length == 0) return;
            _sr.sprite = set[Mathf.Clamp(_animFrame, 0, set.Length - 1)];

            // Y-depth: lower on screen draws in front. The 2000 base keeps every
            // character above the plaza art, which sits at -100.
            float pixelY = transform.position.y * GridUtil.PixelsPerUnit;
            _sr.sortingOrder = 2000 - Mathf.RoundToInt(pixelY);
        }

        private static int DirIndex(Vector2Int d)
        {
            if (d == Vector2Int.up)    return 1;
            if (d == Vector2Int.left)  return 2;
            if (d == Vector2Int.right) return 3;
            return 0; // down
        }

        // ------------------------------------------------------------- queries
        /// <summary>The cell the player is facing — used by interaction checks.</summary>
        public Vector2Int FacingCell => Cell + Facing;

        public bool IsMoving => _stepping;
    }
}
