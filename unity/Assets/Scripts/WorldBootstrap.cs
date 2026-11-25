// Bahala Na? — world layer: entry point.
//
// Zero inspector setup. Everything loads from Resources by path:
//   Resources/Art/Backgrounds/barangay_plaza_16x9
//   Resources/Art/Characters/Boy/boy_walk_atlas
//   Resources/Art/Characters/Girl/girl_walk_atlas
//
// So the only manual step is: put the art inside Assets/Resources/Art/... and
// attach this component to one empty GameObject.

using System;
using System.Linq;
using UnityEngine;

namespace BahalaNa.World
{
    public class WorldBootstrap : MonoBehaviour
    {
        public bool UseGirl = false;
        public bool AutoLaunch = true;

        public WorldMap Map { get; private set; }
        public PlayerController Player { get; private set; }
        public GameObject WorldRoot { get; private set; }

        // EventSystem.current returns null while the object is INACTIVE, so we
        // must remember the GameObject itself in order to switch it back on.
        public static GameObject EventSystemGo { get; private set; }

        private bool _launched;

        private void Start()
        {
            if (AutoLaunch) Launch();
        }

        public void Launch()
        {
            if (_launched) return;
            _launched = true;

            // ---- plaza ----
            var plaza = Resources.Load<Sprite>("Art/Backgrounds/barangay_plaza_16x9");
            if (plaza == null)
            {
                Debug.LogError("[World] Plaza sprite not found at Resources/Art/Backgrounds/barangay_plaza_16x9. " +
                               "Check the folder is named Resources (capital R) and the image is imported as Sprite.");
                return;
            }

            WorldRoot = new GameObject("WorldRoot");
            WorldRoot.transform.SetParent(transform, false);

            var mapGo = new GameObject("WorldMap");
            mapGo.transform.SetParent(WorldRoot.transform, false);
            Map = mapGo.AddComponent<WorldMap>();
            Map.Build(plaza);

            // ---- camera rig ----
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            var follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();

            // ---- player ----
            string atlasPath = UseGirl
                ? "Art/Characters/Girl/girl_walk_atlas"
                : "Art/Characters/Boy/boy_walk_atlas";

            var slices = Resources.LoadAll<Sprite>(atlasPath);
            if (slices == null || slices.Length < 8)
            {
                Debug.LogError($"[World] Expected 8 sprite slices at Resources/{atlasPath}, found " +
                               $"{(slices == null ? 0 : slices.Length)}. Make sure Sprite Mode is Multiple and " +
                               "the atlas was sliced 2 columns x 4 rows.");
                return;
            }
            // Sort by trailing slice number so index 0,1 = down, 2,3 = up, 4,5 = left, 6,7 = right
            slices = slices.OrderBy(s =>
            {
                int i = s.name.LastIndexOf('_');
                return (i >= 0 && int.TryParse(s.name.Substring(i + 1), out int n)) ? n : 0;
            }).ToArray();

            var pGo = new GameObject("Player");
            pGo.transform.SetParent(WorldRoot.transform, false);
            Player = pGo.AddComponent<PlayerController>();
            Player.Init(Map, slices, Map.DefaultSpawnWorld());

            follow.Target = Player.transform;

            // ---- hide the uGUI canvas while testing the world ----
            var ui = GameApp.UI;
            if (ui != null && ui.Canvas != null)
            {
                ui.Canvas.enabled = false;
                var es = UnityEngine.EventSystems.EventSystem.current;
                if (es != null)
                {
                    EventSystemGo = es.gameObject;      // remember BEFORE disabling
                    es.gameObject.SetActive(false);
                }
            }

            Debug.Log("[World] Launched. WASD or arrow keys to walk.");
        }

        /// <summary>Called by EncounterFlow before an encounter UI takes over.</summary>
        public void HideWorldForEncounter()
        {
            if (WorldRoot != null) WorldRoot.SetActive(false);
        }

        /// <summary>Called when the encounter sequence is finished.</summary>
        public void ShowWorldAfterEncounter()
        {
            if (WorldRoot != null) WorldRoot.SetActive(true);
            var ui = GameApp.UI;
            if (ui != null && ui.Canvas != null) ui.Canvas.enabled = false;
            if (EventSystemGo != null) EventSystemGo.SetActive(false);
        }

        public void Shutdown()
        {
            if (Player != null) Destroy(Player.gameObject);
            if (Map != null) Destroy(Map.gameObject);
            var ui = GameApp.UI;
            if (ui != null && ui.Canvas != null) ui.Canvas.enabled = true;
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es != null) es.gameObject.SetActive(true);
            _launched = false;
        }
    }
}
