// Bahala Na? — world layer: on-screen guidance HUD.
//
// Lives on its own ScreenSpaceOverlay canvas so it does not fight the main uGUI
// canvas (which needs to stay enabled for encounters).
//
// Shows:
//   - chapter name, top centre
//   - card progress "1/4", top right
//   - "Talk to Aling Nena" hint, bottom centre
//   - the interact key, bottom right
//
// This exists because the player had no way to know what to do next or in what
// order — the plaza looked like a sandbox with no objective.

using UnityEngine;
using TMPro;

namespace BahalaNa.World
{
    public class WorldHud : MonoBehaviour
    {
        private Canvas _canvas;
        private TextMeshProUGUI _chapter;
        private TextMeshProUGUI _progress;
        private TextMeshProUGUI _hint;
        private TextMeshProUGUI _key;

        private void Awake()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;   // above the world, below nothing

            var scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            _chapter  = MakeText("Chapter", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                                 new Vector2(0f, -22f), 30f, new Color32(242, 177, 52, 255),
                                 TextAlignmentOptions.Center);
            _progress = MakeText("Progress", new Vector2(1f, 1f), new Vector2(1f, 1f),
                                 new Vector2(-28f, -22f), 24f, new Color32(184, 169, 201, 255),
                                 TextAlignmentOptions.Right);
            _hint     = MakeText("Hint", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                                 new Vector2(0f, 44f), 28f, new Color32(244, 234, 216, 255),
                                 TextAlignmentOptions.Center);
            _key      = MakeText("Key", new Vector2(1f, 0f), new Vector2(1f, 0f),
                                 new Vector2(-28f, 44f), 20f, new Color32(184, 169, 201, 255),
                                 TextAlignmentOptions.Right);
        }

        private TextMeshProUGUI MakeText(string name, Vector2 anchorMin, Vector2 anchorMax,
                                         Vector2 pos, float size, Color color,
                                         TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(anchorMin.x, anchorMin.y);
            rt.sizeDelta = new Vector2(900f, 80f);
            rt.anchoredPosition = pos;

            var t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.text = "";

            // dark backing so text reads over bright pixel art
            var sh = go.AddComponent<UnityEngine.UI.Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.85f);
            sh.effectDistance = new Vector2(2f, -2f);

            return t;
        }

        public void Set(string chapter, string progress, string hint, string key)
        {
            if (_chapter != null) _chapter.text = chapter;
            if (_progress != null) _progress.text = progress;
            if (_hint != null) _hint.text = hint;
            if (_key != null) _key.text = string.IsNullOrEmpty(hint) ? "" : "[" + key + "]";
        }

        public void SetVisible(bool v)
        {
            if (_canvas != null) _canvas.enabled = v;
        }
    }
}
