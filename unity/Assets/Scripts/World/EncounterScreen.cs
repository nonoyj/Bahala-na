// Bahala Na? — world layer: NPC encounter screen.
//
// Landscape layout layered over the interior art (or the plaza if the NPC has no
// interior). Uses the project's existing UIBuilder palette so it matches.
//
// Layout (1920x1080 reference, inside the 1180-wide content column):
//   - full-bleed background art, dimmed
//   - portrait panel on the LEFT
//   - dialogue panel on the RIGHT with a Continue button
//
// This screen does not touch run state. It ends by calling onDone.

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BahalaNa.UI
{
    public class EncounterScreen : Screen
    {
        private readonly BahalaNa.World.EncounterDef _def;
        private readonly Action _onDone;
        private int _line;

        private TextMeshProUGUI _body;

        public EncounterScreen(BahalaNa.World.EncounterDef def, Action onDone)
        {
            _def = def;
            _onDone = onDone;
        }

        protected override void BuildContent()
        {
            bool tl = GameApp.Lang == "tl";

            // ------------------------------------------------ background art
            var bgGo = new GameObject("Bg", typeof(RectTransform));
            bgGo.transform.SetParent(Root, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            // bleed wider than the content column so it reads as full-screen
            bgRt.offsetMin = new Vector2(-400f, 0f);
            bgRt.offsetMax = new Vector2(400f, 0f);

            var bgImg = bgGo.AddComponent<Image>();
            bgImg.raycastTarget = false;

            string artPath = _def.InteriorArt ?? "Art/Backgrounds/barangay_plaza_16x9";
            var art = Resources.Load<Sprite>(artPath);
            if (art != null)
            {
                bgImg.sprite = art;
                bgImg.preserveAspect = false;
                bgImg.color = Color.white;
            }
            else
            {
                bgImg.color = UIBuilder.Panel;
            }

            // dim layer for legibility
            var dimGo = new GameObject("Dim", typeof(RectTransform));
            dimGo.transform.SetParent(bgGo.transform, false);
            var dimRt = dimGo.GetComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero; dimRt.offsetMax = Vector2.zero;
            var dimImg = dimGo.AddComponent<Image>();
            dimImg.color = new Color(0.06f, 0.05f, 0.09f, 0.55f);
            dimImg.raycastTarget = false;

            // ------------------------------------------------ content row
            var rowGo = new GameObject("Row", typeof(RectTransform));
            rowGo.transform.SetParent(Root, false);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.anchorMin = Vector2.zero; rowRt.anchorMax = Vector2.one;
            rowRt.offsetMin = new Vector2(40f, 40f);
            rowRt.offsetMax = new Vector2(-40f, -40f);

            var hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 28f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            // ------------------------------------------------ portrait (left)
            var pGo = new GameObject("Portrait", typeof(RectTransform));
            pGo.transform.SetParent(rowRt, false);
            var ple = pGo.AddComponent<LayoutElement>();
            ple.preferredWidth = 420f;
            ple.minWidth = 300f;
            ple.flexibleWidth = 0f;

            var pImg = pGo.AddComponent<Image>();
            var portrait = Resources.Load<Sprite>("Art/NPCs/" + _def.Id);
            if (portrait != null)
            {
                pImg.sprite = portrait;
                pImg.preserveAspect = true;
                pImg.color = Color.white;
            }
            else
            {
                pImg.color = UIBuilder.Panel2;
                Debug.LogWarning("[Encounter] No portrait at Resources/Art/NPCs/" + _def.Id);
            }

            // ------------------------------------------------ dialogue (right)
            var dGo = new GameObject("Dialogue", typeof(RectTransform));
            dGo.transform.SetParent(rowRt, false);
            var dle = dGo.AddComponent<LayoutElement>();
            dle.flexibleWidth = 1f;
            dle.minWidth = 400f;

            var dImg = dGo.AddComponent<Image>();
            dImg.color = new Color32(28, 22, 40, 235);

            var col = UIBuilder.VList(dGo.GetComponent<RectTransform>(), 12f);
            col.offsetMin = new Vector2(30f, 26f);
            col.offsetMax = new Vector2(-30f, -26f);

            UIBuilder.Text(col, tl ? _def.DisplayName : _def.DisplayNameEn,
                28f, UIBuilder.Gold, TextAlignmentOptions.Left, true);

            _body = UIBuilder.Text(col, CurrentLine(tl), 20f, UIBuilder.Ink,
                TextAlignmentOptions.TopLeft);

            // continue button
            var btn = Widgets.Primary(col, tl ? "Magpatuloy" : "Continue", Advance);
            var ble = btn.gameObject.AddComponent<LayoutElement>();
            ble.minHeight = 54f;
        }

        private string CurrentLine(bool tl)
        {
            var arr = tl ? _def.Lines : _def.LinesEn;
            if (arr == null || arr.Length == 0) return "";
            return arr[Mathf.Clamp(_line, 0, arr.Length - 1)];
        }

        private void Advance()
        {
            bool tl = GameApp.Lang == "tl";
            var arr = tl ? _def.Lines : _def.LinesEn;
            _line++;

            if (arr != null && _line < arr.Length)
            {
                if (_body != null) _body.text = CurrentLine(tl);
                return;
            }

            if (_onDone != null) _onDone();
        }
    }
}
