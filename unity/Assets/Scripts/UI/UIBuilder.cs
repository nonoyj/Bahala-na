// Bahala Na? — tiny code-built uGUI helpers (no prefabs required).
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BahalaNa.UI
{
    public static class UIBuilder
    {
        public static Color Ink = new Color32(244, 234, 216, 255);
        public static Color InkDim = new Color32(184, 169, 201, 255);
        public static Color Gold = new Color32(242, 177, 52, 255);
        public static Color Teal = new Color32(46, 196, 182, 255);
        public static Color Red = new Color32(231, 111, 81, 255);
        public static Color Green = new Color32(123, 201, 80, 255);
        public static Color Panel = new Color32(36, 28, 49, 255);
        public static Color Panel2 = new Color32(45, 35, 64, 255);
        public static Color Line = new Color32(58, 47, 77, 255);

        public static RectTransform FullRect(Transform parent)
        {
            var go = new GameObject("Rect", typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform VList(Transform parent, float spacing = 8f)
        {
            var rt = FullRect(parent);
            var go = rt.gameObject;
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
            var csf = go.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        public static Image PanelBg(Transform parent, Color? color = null)
        {
            var go = new GameObject("Panel", typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color ?? Panel;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return img;
        }

        public static TextMeshProUGUI Text(Transform parent, string content, float size, Color? color = null,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = size;
            tmp.color = color ?? Ink;
            tmp.alignment = align;
            tmp.richText = true;
            tmp.enableWordWrapping = true;
            if (bold) tmp.fontStyle = FontStyles.Bold;
            var rt = tmp.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = size * 1.35f;
            le.flexibleWidth = 1f;
            return tmp;
        }

        public static Button Button(Transform parent, string label, Action onClick, float minHeight = 56f, Color? bg = null)
        {
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bg ?? Panel2;
            var btn = go.AddComponent<Button>();
            var rt = btn.GetComponent<RectTransform>();
            // fixed anchors + LayoutElement: works inside Vertical/Horizontal layout groups
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(0f, minHeight);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = minHeight;
            le.flexibleWidth = 1f;
            btn.onClick.AddListener(() => onClick());

            var t = Text(btn.transform, label, 18f, Ink, TextAlignmentOptions.Center, true);
            t.GetComponent<LayoutElement>().minHeight = minHeight;
            return btn;
        }

        public static Toggle Toggle(Transform parent, string label, bool value, Action<bool> onChange)
        {
            var row = new GameObject("ToggleRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRt = row.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.5f, 0.5f); rowRt.anchorMax = new Vector2(0.5f, 0.5f);
            rowRt.pivot = new Vector2(0.5f, 0.5f);
            rowRt.sizeDelta = new Vector2(0f, 52f);
            var le = row.AddComponent<LayoutElement>();
            le.minHeight = 52f;
            le.flexibleWidth = 1f;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;

            var t = Text(rowRt, label, 16f, InkDim, TextAlignmentOptions.Left, false);
            var tle = t.GetComponent<LayoutElement>();
            tle.flexibleWidth = 1f;

            var togGo = new GameObject("Toggle", typeof(RectTransform));
            togGo.transform.SetParent(rowRt, false);
            var tog = togGo.AddComponent<Toggle>();
            var trt = tog.GetComponent<RectTransform>();
            trt.sizeDelta = new Vector2(56f, 30f);
            var tle2 = togGo.AddComponent<LayoutElement>();
            tle2.minWidth = 56f;
            // simple on/off visual (background + checkmark), no art assets needed
            var offImg = togGo.AddComponent<Image>();
            offImg.color = UIBuilder.Line;
            var onGo = new GameObject("On", typeof(RectTransform));
            onGo.transform.SetParent(togGo.transform, false);
            var onRt = onGo.GetComponent<RectTransform>();
            onRt.anchorMin = Vector2.zero; onRt.anchorMax = Vector2.one;
            onRt.offsetMin = Vector2.zero; onRt.offsetMax = Vector2.zero;
            var onImg = onGo.AddComponent<Image>();
            onImg.color = value ? UIBuilder.Teal : UIBuilder.Line;
            tog.targetGraphic = onImg;
            tog.transition = UnityEngine.UI.Selectable.Transition.None;
            tog.isOn = value;
            tog.onValueChanged.AddListener(v =>
            {
                onImg.color = v ? UIBuilder.Teal : UIBuilder.Line;
                onChange(v);
            });
            return tog;
        }

        public static TMP_InputField Input(Transform parent, string value, int maxChars, Action<string> onChange)
        {
            var go = new GameObject("Input", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = Panel;
            var inp = go.AddComponent<TMP_InputField>();
            var rt = inp.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(0f, 56f);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 56f;
            le.flexibleWidth = 1f;
            inp.characterLimit = maxChars;
            var t = Text(inp.transform, value, 18f, Ink, TextAlignmentOptions.Left, false);
            t.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
            t.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            t.GetComponent<RectTransform>().offsetMin = new Vector2(14f, 0f);
            t.GetComponent<RectTransform>().offsetMax = new Vector2(-14f, 0f);
            inp.textComponent = t;
            inp.text = value;
            inp.onValueChanged.AddListener(v => onChange(v));
            return inp;
        }

        public static ScrollRect Scroll(Transform parent, out RectTransform content)
        {
            var go = new GameObject("Scroll", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<ScrollRect>();
            var rt = sr.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(rt, false);
            var vrt = viewportGo.GetComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero; vrt.offsetMax = Vector2.zero;
            var mask = viewportGo.AddComponent<RectMask2D>();
            sr.viewport = vrt;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(vrt, false);
            var crt = contentGo.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f); crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.sizeDelta = new Vector2(0f, 0f);
            crt.anchoredPosition = Vector2.zero;
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = crt;
            sr.horizontal = false;
            sr.scrollSensitivity = 24f;
            content = crt;
            return sr;
        }
    }
}
