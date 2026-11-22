// Bahala Na? — screen contracts + shared widgets.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BahalaNa.UI
{
    public interface IScreen
    {
        void Build(UIManager ui);
        void OnShow();
        void OnHide();
    }

    public abstract class Screen : IScreen
    {
        protected UIManager UI;
        protected RectTransform Root;

        public void Build(UIManager ui)
        {
            UI = ui;
            Root = ui.ScreenRoot;
            BuildContent();
        }

        protected abstract void BuildContent();
        public virtual void OnShow() { }
        public virtual void OnHide() { }
    }

    /// <summary>Shown while content loads (before the menu).</summary>
    public class LoadingScreen : Screen
    {
        protected override void BuildContent()
        {
            UIBuilder.PanelBg(Root, new Color32(20, 16, 28, 255));
            UIBuilder.Text(Root, "BAHALA NA?", 40f, UIBuilder.Gold, TextAlignmentOptions.Center, true);
            UIBuilder.Text(Root, "Naglo-load…", 18f, UIBuilder.InkDim);
        }
    }

    // ---------------------------------------------------------------- shared widgets
    public static class Widgets
    {
        public static TextMeshProUGUI Title(Transform parent, string text)
            => UIBuilder.Text(parent, text, 30f, UIBuilder.Gold, TextAlignmentOptions.Center, true);

        public static TextMeshProUGUI Sub(Transform parent, string text)
            => UIBuilder.Text(parent, text, 16f, UIBuilder.InkDim);

        public static TextMeshProUGUI Body(Transform parent, string text, float size = 18f)
            => UIBuilder.Text(parent, text, size, UIBuilder.Ink, TextAlignmentOptions.Left);

        public static Button Primary(Transform parent, string label, System.Action action)
            => UIBuilder.Button(parent, label, action, 58f, UIBuilder.Gold);

        public static Button Normal(Transform parent, string label, System.Action action)
            => UIBuilder.Button(parent, label, action, 52f, UIBuilder.Panel2);

        public static TextMeshProUGUI HudStat(Transform parent, string icon, string name, int value, int mod)
        {
            var go = new GameObject("Stat", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(0f, 54f);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 54f;
            le.flexibleWidth = 1f;
            var img = go.AddComponent<Image>();
            img.color = UIBuilder.Panel2;
            var t = UIBuilder.Text(rt, $"{icon} {value}\n<size=11>{name} +{mod}</size>", 15f, UIBuilder.Ink);
            return t;
        }

        public static Image Bar(Transform parent, float pct, Color? color = null, float height = 12f)
        {
            var go = new GameObject("Bar", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = new Vector2(0f, height);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.flexibleWidth = 1f;
            var bg = go.AddComponent<Image>();
            bg.color = UIBuilder.Panel2;
            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(rt, false);
            var frt = fillGo.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            var img = fillGo.AddComponent<Image>();
            img.color = color ?? UIBuilder.Green;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = Mathf.Clamp01(pct);
            return img;
        }

        public static void Segmented(Transform parent, string[] options, int selected, System.Action<int> onPick)
        {
            var go = new GameObject("Segmented", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = new Vector2(0f, 44f);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 44f;
            le.flexibleWidth = 1f;
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            for (int i = 0; i < options.Length; i++)
            {
                int idx = i;
                var bgo = new GameObject("Opt", typeof(RectTransform));
                bgo.transform.SetParent(rt, false);
                var img = bgo.AddComponent<Image>();
                img.color = i == selected ? UIBuilder.Gold : UIBuilder.Panel2;
                var btn = bgo.AddComponent<Button>();
                var ble = bgo.AddComponent<LayoutElement>();
                ble.flexibleWidth = 1f;
                ble.minHeight = 40f;
                btn.onClick.AddListener(() => onPick(idx));
                UIBuilder.Text(bgo.transform, options[i], 14f, i == selected ? new Color(0.14f, 0.1f, 0.02f, 1f) : UIBuilder.Ink);
            }
        }

        public static TextMeshProUGUI Chip(Transform parent, string text, Color color)
            => UIBuilder.Text(parent, text, 13f, color);

        /// <summary>Padded scrollable page (standard screen body).</summary>
        public static RectTransform Page(Transform root, out RectTransform content)
        {
            var pad = UIBuilder.FullRect(root);
            pad.offsetMin = new Vector2(14f, 14f);
            pad.offsetMax = new Vector2(-14f, -14f);
            UIBuilder.Scroll(pad, out content);
            return pad;
        }
    }
}
