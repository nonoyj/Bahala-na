// Bahala Na? — world layer: ambient NPC patrol.
//
// Background villagers that make the plaza feel inhabited. They never interact
// and never block. They step between two cells on a timer.

using UnityEngine;

namespace BahalaNa.World
{
    public class AmbientWalker : MonoBehaviour
    {
        private WorldMap _map;
        private Vector2Int _home;
        private int _range;
        private Vector2Int _target;
        private float _timer;

        private SpriteRenderer _sr;

        public void Init(WorldMap map, Vector2Int home, int range)
        {
            _map = map;
            _home = home;
            _range = Mathf.Max(1, range);
            _target = home;
            _timer = Random.Range(0.8f, 2.5f);
            _sr = GetComponent<SpriteRenderer>();
            transform.position = GridUtil.CellToWorld(home.x, home.y) + new Vector3(0f, 0.35f, 0f);
        }

        private void Update()
        {
            if (_map == null) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = Random.Range(1.2f, 3.0f);

            // Pick a neighbouring cell within range, on open ground.
            for (int tries = 0; tries < 8; tries++)
            {
                int dc = Random.Range(-_range, _range + 1);
                int dr = Random.Range(-_range, _range + 1);
                int c = Mathf.Clamp(_home.x + dc, _home.x - _range, _home.x + _range);
                int r = Mathf.Clamp(_home.y + dr, _home.y - _range, _home.y + _range);
                if (_map.IsBlocked(c, r)) continue;

                _target = new Vector2Int(c, r);
                transform.position = GridUtil.CellToWorld(c, r) + new Vector3(0f, 0.35f, 0f);
                if (_sr != null) WorldMap.SetDepth(_sr, transform.position.y);
                return;
            }
        }
    }
}
