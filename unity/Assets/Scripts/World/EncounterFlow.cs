// Bahala Na? — world layer: NPC encounter triggers. OPTION A.
//
// THE WORLD IS THE GAME:
//   - Each chapter defines its cast (ChapterPlan). The plaza is re-populated when
//     the chapter advances.
//   - Each NPC interaction plays ONE card of the current run deck.
//   - When the deck is exhausted, the chapter advances, the cast changes, and the
//     next chapter's cards load.
//   - After chapter 4 the climax / ending screens take over.
//
// Attach to the same GameObject as WorldBootstrap.

using System.Collections.Generic;
using UnityEngine;
using BahalaNa.UI;

namespace BahalaNa.World
{
    public class EncounterFlow : MonoBehaviour
    {
        public float InteractRadius = 1.1f;
        public KeyCode InteractKey = KeyCode.F;

        private WorldBootstrap _boot;
        private WorldMap _map;
        private PlayerController _player;
        private readonly List<NpcInstance> _npcs = new List<NpcInstance>();
        private NpcInstance _near;

        private int _plannedChapter = -1;
        private GameObject _castRoot;
        private readonly HashSet<string> _talked = new HashSet<string>();
        private NpcInstance _suggested;
        private WorldHud _hud;
        private float _pulse;

        private class NpcInstance
        {
            public EncounterDef Def;
            public GameObject Go;
            public SpriteRenderer Sr;
            public Color BaseColor;
            public Vector3 BaseScale;
            public Vector2Int Cell;
            public bool CellWasBlocked;
        }

        // ---------------------------------------------------------------- setup
        private void Start()
        {
            _boot = GetComponent<WorldBootstrap>();
            if (_boot == null)
            {
                Debug.LogError("[Encounter] Needs to sit on the same GameObject as WorldBootstrap.");
                enabled = false;
            }
        }

        private void Update()
        {
            if (_map == null)
            {
                if (_boot != null && _boot.Map != null && _boot.Player != null)
                {
                    _map = _boot.Map;
                    _player = _boot.Player;
                    EnsureRun();
                    RefreshCast();
                }
                return;
            }

            // Chapter may change while the world is hidden (card resolved).
            RefreshCast();

            UpdateSuggestion();
            UpdateHud();
            if (_hud != null) _hud.SetVisible(true);

            RefreshNearest();

            bool pressed = Input.GetKeyDown(InteractKey) ||
                           Input.GetKeyDown(KeyCode.Space) ||
                           Input.GetKeyDown(KeyCode.Return);

            if (pressed && _near != null) Trigger(_near);
        }

        // ------------------------------------------------------------------ run
        private void EnsureRun()
        {
            if (GameApp.Game == null) return;
            if (GameApp.Game.Run != null && GameApp.Game.CurrentCard != null) return;

            if (GameApp.Game.Run == null)
            {
                string name = GameApp.Save != null && GameApp.Save.Profile != null
                    ? GameApp.Save.Profile.name : "";
                if (string.IsNullOrEmpty(name)) name = "Ikaw";
                GameApp.Game.NewRun(name, "neutral");
                GameApp.Game.GoToChapter(1);
                Debug.Log("[Encounter] Started a run on demand.");
            }
        }

        private int CurrentChapter
        {
            get
            {
                if (GameApp.Game == null || GameApp.Game.Run == null) return 1;
                return Mathf.Clamp(GameApp.Game.Run.chapter, 1, 4);
            }
        }

        // ------------------------------------------------------------- the cast
        private void RefreshCast()
        {
            int chapter = CurrentChapter;
            if (chapter == _plannedChapter) return;

            _plannedChapter = chapter;
            ClearCast();

            var plan = ChapterPlan.For(chapter);
            Debug.Log($"[Encounter] Chapter {chapter}: {plan.Cast.Count} NPCs, {plan.Ambient.Count} ambient.");

            _castRoot = new GameObject("Cast");
            var rootParent = _boot != null && _boot.WorldRoot != null ? _boot.WorldRoot.transform : transform;
            _castRoot.transform.SetParent(rootParent, false);

            foreach (var placed in plan.Cast)
            {
                var def = EncounterDef.ById(placed.Id);
                if (def == null)
                {
                    Debug.LogWarning("[Encounter] No EncounterDef for id '" + placed.Id + "'");
                    continue;
                }
                SpawnNpc(def, placed.CellX, placed.CellY);
            }

            foreach (var amb in plan.Ambient)
                SpawnAmbient(amb);

            // Chapter tint on the plaza.
            if (_boot != null) _boot.SetChapterTint(plan.Tint, plan.AmbientDim);
        }

        private void ClearCast()
        {
            // Un-block the cells the old cast occupied, or the plaza gradually
            // fills with phantom walls every time the chapter advances.
            foreach (var n in _npcs)
            {
                if (n.Cell.x >= 0)
                {
                    _map.SetBlocked(n.Cell.x, n.Cell.y, false);
                    // restore whatever the painted collision said originally
                    if (n.CellWasBlocked) _map.SetBlocked(n.Cell.x, n.Cell.y, true);
                }
            }

            _npcs.Clear();
            _near = null;
            _talked.Clear();
            if (_castRoot != null) Destroy(_castRoot);
            _castRoot = null;
        }

        private void SpawnNpc(EncounterDef def, int authoredX, int authoredY)
        {
            var cell = FindOpenCell(authoredX, authoredY);

            var go = new GameObject("NPC_" + def.Id);
            go.transform.SetParent(_castRoot.transform, false);
            go.transform.position = GridUtil.CellToWorld(cell.x, cell.y);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadMapSprite(def);

            Color baseColor;
            if (sr.sprite == null)
            {
                sr.sprite = StandInSprite();
                baseColor = new Color(0.95f, 0.69f, 0.22f, 0.9f);
                go.transform.localScale = new Vector3(0.45f, 0.8f, 1f);
                go.transform.position += new Vector3(0f, 0.3f, 0f);
            }
            else
            {
                baseColor = Color.white;
                go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
                go.transform.position += new Vector3(0f, 0.42f, 0f);
            }
            sr.color = baseColor;
            WorldMap.SetDepth(sr, go.transform.position.y);

            bool wasBlocked = _map.IsBlocked(cell.x, cell.y);
            _map.SetBlocked(cell.x, cell.y, true);

            _npcs.Add(new NpcInstance
            {
                Def = def,
                Go = go,
                Sr = sr,
                BaseColor = baseColor,
                BaseScale = go.transform.localScale,
                Cell = cell,
                CellWasBlocked = wasBlocked,
            });
        }

        private void SpawnAmbient(ChapterPlan.AmbientNpc amb)
        {
            var cell = FindOpenCell(amb.CellX, amb.CellY);

            var go = new GameObject("AMB_" + amb.Id);
            go.transform.SetParent(_castRoot.transform, false);
            go.transform.position = GridUtil.CellToWorld(cell.x, cell.y);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Art/NPCs/" + amb.Id + "_walk");
            if (sr.sprite == null)
            {
                sr.sprite = StandInSprite();
                sr.color = new Color(0.6f, 0.7f, 0.85f, 0.7f);   // bluish, dimmer = crowds
                go.transform.localScale = new Vector3(0.4f, 0.7f, 1f);
            }
            else
            {
                go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            }
            go.transform.position += new Vector3(0f, 0.35f, 0f);
            WorldMap.SetDepth(sr, go.transform.position.y);

            // Ambient NPCs do not block and do not interact. Movement is optional;
            // a simple patrol between two cells if WalkRange > 0.
            if (amb.WalkRange > 0)
            {
                var patrol = go.AddComponent<AmbientWalker>();
                patrol.Init(_map, cell, amb.WalkRange);
            }
        }

        // ---------------------------------------------------------- cell search
        private Vector2Int FindOpenCell(int col, int row)
        {
            if (_map == null) return new Vector2Int(col, row);
            if (!_map.IsBlocked(col, row)) return new Vector2Int(col, row);

            for (int radius = 1; radius <= 12; radius++)
                for (int dc = -radius; dc <= radius; dc++)
                    for (int dr = -radius; dr <= radius; dr++)
                    {
                        if (Mathf.Abs(dc) != radius && Mathf.Abs(dr) != radius) continue;
                        int c = col + dc, r = row + dr;
                        if (!_map.IsBlocked(c, r)) return new Vector2Int(c, r);
                    }

            Debug.LogWarning($"[Encounter] No open cell near ({col},{row})");
            return new Vector2Int(col, row);
        }

        // ---------------------------------------------------------------- sprite
        /// <summary>Map sprite only — never the 1024x1024 dialogue portrait.</summary>
        private static Sprite LoadMapSprite(EncounterDef def)
        {
            return Resources.Load<Sprite>("Art/NPCs/" + def.Id + "_walk");
        }

        private static Sprite _standIn;
        private static Sprite StandInSprite()
        {
            if (_standIn != null) return _standIn;
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var px = new Color32[16 * 16];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px); tex.Apply();
            _standIn = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0f), 16f);
            return _standIn;
        }

        // ------------------------------------------------------- guidance/HUD
        /// <summary>
        /// Picks the NPC the player should talk to next and pulses them so the
        /// order of the chapter is obvious instead of guesswork.
        /// </summary>
        private void UpdateSuggestion()
        {
            _suggested = null;

            // Prefer someone not yet talked to this chapter.
            foreach (var n in _npcs)
                if (!_talked.Contains(n.Def.Id)) { _suggested = n; break; }

            // Everyone has been talked to — reuse the first one.
            if (_suggested == null && _npcs.Count > 0) _suggested = _npcs[0];

            // Pulse the suggested NPC's scale so the eye finds them.
            _pulse += Time.deltaTime * 3f;
            float k = 1f + Mathf.Sin(_pulse) * 0.05f;

            foreach (var n in _npcs)
            {
                if (n.Go == null) continue;
                bool isSuggested = n == _suggested;
                n.Go.transform.localScale = isSuggested
                    ? new Vector3(n.BaseScale.x * k, n.BaseScale.y, n.BaseScale.z)
                    : n.BaseScale;
            }
        }

        private void UpdateHud()
        {
            if (GameApp.Game == null || GameApp.Game.Run == null) return;

            if (_hud == null)
            {
                var go = new GameObject("WorldHud");
                go.transform.SetParent(transform, false);
                _hud = go.AddComponent<WorldHud>();
            }

            var run = GameApp.Game.Run;
            string chapterName = GameManager.ChapterName(run.chapter, GameApp.Lang);
            string progress = (run.cardIndex + 1) + "/" + run.deck.Count;

            string hint;
            if (_suggested != null && !_talked.Contains(_suggested.Def.Id))
            {
                string who = GameApp.Lang == "tl" ? _suggested.Def.DisplayName : _suggested.Def.DisplayNameEn;
                hint = GameApp.Lang == "tl" ? "Kausapin si " + who : "Talk to " + who;
            }
            else if (_suggested != null)
            {
                string who = GameApp.Lang == "tl" ? _suggested.Def.DisplayName : _suggested.Def.DisplayNameEn;
                hint = GameApp.Lang == "tl" ? "Bumalik kay " + who : "Back to " + who;
            }
            else hint = "";

            _hud.Set(chapterName, progress, hint, InteractKey.ToString());
        }

        // ----------------------------------------------------------- proximity
        private void RefreshNearest()
        {
            Vector3 p = _player.transform.position;
            NpcInstance best = null;
            float bestDist = float.MaxValue;

            foreach (var n in _npcs)
            {
                if (n.Go == null) continue;
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
            n.Sr.color = on ? new Color(1f, 1f, 1f, n.BaseColor.a) : n.BaseColor;
            if (n.Go != null)
                n.Go.transform.localScale = on ? n.BaseScale * 1.06f : n.BaseScale;
        }

        // ------------------------------------------------------------- trigger
        private void Trigger(NpcInstance npc)
        {
            SetHighlight(npc, false);
            _near = null;
            _talked.Add(npc.Def.Id);

            if (GameApp.Game == null || GameApp.Game.CurrentCard == null)
            {
                Debug.Log("[Encounter] No card available — deck may be exhausted.");
                return;
            }

            if (_hud != null) _hud.SetVisible(false);
            if (_boot != null) _boot.HideWorldForEncounter();

            var ui = GameApp.UI;
            if (ui != null && ui.Canvas != null) ui.Canvas.enabled = true;
            if (WorldBootstrap.EventSystemGo != null) WorldBootstrap.EventSystemGo.SetActive(true);

            GameApp.UI.Show(new EncounterScreen(npc.Def, OnEncounterFinished));
        }

        private void OnEncounterFinished()
        {
            if (GameApp.Game != null && GameApp.Game.Run != null && GameApp.Game.CurrentCard != null)
            {
                var hook = GetComponent<GameScreenReturnHook>();
                if (hook == null) hook = gameObject.AddComponent<GameScreenReturnHook>();
                hook.Arm(GameApp.Game.CurrentCard.id);

                var ui = GameApp.UI;
                if (ui != null && ui.Canvas != null) ui.Canvas.enabled = true;
                if (WorldBootstrap.EventSystemGo != null) WorldBootstrap.EventSystemGo.SetActive(true);

                GameApp.UI.Show(new GameScreen());
            }
            else
            {
                if (_boot != null) _boot.ShowWorldAfterEncounter();
            }
        }

    }
}
