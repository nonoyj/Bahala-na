// Bahala Na? — world layer: NPC encounter triggers.
//
// Design decision (important, and it keeps this simple):
//   The world is a HUB, not a second game. Walking up to an NPC and interacting
//   plays ONE CARD of the current run, framed by that NPC's dialogue. So:
//
//       NPC interact → EncounterScreen (interior + portrait + lines)
//                    → GameScreen (the current card, existing code)
//                    → resolves as it already does
//
//   This means the card system needs no changes at all. The NPC is presentation.
//
// Attach to the same GameObject as WorldBootstrap.

using System.Collections.Generic;
using UnityEngine;
using BahalaNa.UI;

namespace BahalaNa.World
{
    public class EncounterFlow : MonoBehaviour
    {
        public float InteractRadius = 1.1f;      // world units
        public KeyCode InteractKey = KeyCode.F;

        private WorldBootstrap _boot;
        private WorldMap _map;
        private PlayerController _player;
        private readonly List<NpcInstance> _npcs = new List<NpcInstance>();
        private NpcInstance _near;

        private class NpcInstance
        {
            public EncounterDef Def;
            public GameObject Go;
            public SpriteRenderer Sr;
            public Color BaseColor;      // what it looks like when NOT highlighted
        }

        // ---------------------------------------------------------------- setup
        private void Start()
        {
            _boot = GetComponent<WorldBootstrap>();
            if (_boot == null)
            {
                Debug.LogError("[Encounter] Needs to sit on the same GameObject as WorldBootstrap.");
                enabled = false;
                return;
            }
            // WorldBootstrap.Launch() runs in its own Start(); we defer one frame
            // so Map and Player exist by the time we place NPCs.
        }

        private void Update()
        {
            if (_map == null)
            {
                if (_boot != null && _boot.Map != null && _boot.Player != null)
                {
                    _map = _boot.Map;
                    _player = _boot.Player;
                    PlaceNpcs();
                }
                return;
            }

            RefreshNearest();

            bool pressed = Input.GetKeyDown(InteractKey) ||
                           Input.GetKeyDown(KeyCode.Space) ||
                           Input.GetKeyDown(KeyCode.Return);

            if (pressed && _near != null) Trigger(_near);
        }

        // ----------------------------------------------------------- placement
        private void PlaceNpcs()
        {
            var defs = EncounterDef.All();

            foreach (var def in defs)
            {
                var go = new GameObject("NPC_" + def.Id);
                go.transform.SetParent(transform, false);
                go.transform.position = GridUtil.CellToWorld(def.CellX, def.CellY);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = LoadMapSprite(def);

                Color baseColor;
                if (sr.sprite == null)
                {
                    // Visible placeholder so placement can be checked before art exists.
                    sr.sprite = StandInSprite();
                    baseColor = new Color(0.95f, 0.69f, 0.22f, 0.9f);   // amber
                    go.transform.localScale = new Vector3(0.45f, 0.8f, 1f);
                    go.transform.position += new Vector3(0f, 0.3f, 0f);
                }
                else
                {
                    baseColor = Color.white;
                    go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
                    go.transform.position += new Vector3(0f, 0.42f, 0f);   // stand on the cell
                }
                sr.color = baseColor;
                WorldMap.SetDepth(sr, go.transform.position.y);

                // Block the cell so the player can't walk through the NPC.
                _map.SetBlocked(def.CellX, def.CellY, true);

                _npcs.Add(new NpcInstance { Def = def, Go = go, Sr = sr, BaseColor = baseColor });
            }

            Debug.Log("[Encounter] Placed " + _npcs.Count + " NPCs. Press F next to one.");
        }

        private static Sprite LoadMapSprite(EncounterDef def)
        {
            var s = Resources.Load<Sprite>("Art/NPCs/" + def.Id + "_walk");
            if (s != null) return s;
            return Resources.Load<Sprite>("Art/NPCs/" + def.Id);
        }

        private static Sprite _standIn;

        /// <summary>White 16x16 sprite; tinted per-NPC so it is visible on the map.</summary>
        private static Sprite StandInSprite()
        {
            if (_standIn != null) return _standIn;
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var px = new Color32[16 * 16];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.Apply();
            _standIn = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0f), 16f);
            return _standIn;
        }

        // ----------------------------------------------------------- proximity
        private void RefreshNearest()
        {
            // Facing-aware: prefer the cell directly in front, fall back to radius.
            Vector3 p = _player.transform.position;
            Vector3 facingWorld = new Vector3(_player.Facing.x, _player.Facing.y, 0f) * 0.64f;

            NpcInstance best = null;
            float bestDist = float.MaxValue;

            foreach (var n in _npcs)
            {
                float d = Vector3.Distance(p, n.Go.transform.position);
                if (d > InteractRadius) { SetHighlight(n, false); continue; }
                if (d < bestDist) { bestDist = d; best = n; }
            }

            if (_near != best)
            {
                if (_near != null) SetHighlight(_near, false);
                _near = best;
                if (_near != null) SetHighlight(_near, true);
            }
        }

        private void SetHighlight(NpcInstance n, bool on)
        {
            if (n == null || n.Sr == null) return;
            n.Sr.color = on
                ? new Color(1f, 1f, 1f, n.BaseColor.a)     // brighten on highlight
                : n.BaseColor;                              // restore the stand-in colour
            if (n.Go != null)
                n.Go.transform.localScale = on
                    ? new Vector3(n.Go.transform.localScale.x * 1.06f, n.Go.transform.localScale.y, 1f)
                    : n.Go.transform.localScale;
        }

        // ------------------------------------------------------------- trigger
        private void Trigger(NpcInstance npc)
        {
            SetHighlight(npc, false);
            _near = null;

            // Hide the world while the encounter UI is up.
            if (_boot != null) _boot.HideWorldForEncounter();

            GameApp.UI.Show(new EncounterScreen(npc.Def, OnEncounterFinished));
        }

        private void OnEncounterFinished()
        {
            // Dialogue done — hand off to the existing card screen.
            if (GameApp.Game != null && GameApp.Game.Run != null && GameApp.Game.CurrentCard != null)
            {
                var hook = GetComponent<GameScreenReturnHook>();
                if (hook == null) hook = gameObject.AddComponent<GameScreenReturnHook>();
                hook.Arm(GameApp.Game.CurrentCard.id);

                var ui = GameApp.UI;
                if (ui != null && ui.Canvas != null) ui.Canvas.enabled = true;
                var es = UnityEngine.EventSystems.EventSystem.current;
                if (es != null) es.gameObject.SetActive(true);

                GameApp.UI.Show(new GameScreen());
            }
            else
            {
                if (_boot != null) _boot.ShowWorldAfterEncounter();
            }
        }
    }
}
