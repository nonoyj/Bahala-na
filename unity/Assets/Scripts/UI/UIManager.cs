// Bahala Na? — UI manager: owns the Canvas and the current screen.
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BahalaNa.UI
{
    // Concrete MonoBehaviour so UIManager can host coroutines without being a component itself.
    public class UIRunner : MonoBehaviour { }

    public class UIManager
    {
        public Canvas Canvas { get; private set; }
        public RectTransform ScreenRoot { get; private set; }
        public IScreen Current { get; private set; }
        public MonoBehaviour Host { get; private set; }

        public UIManager()
        {
            var host = new GameObject("UIHost");
            UnityEngine.Object.DontDestroyOnLoad(host);
            Host = host.AddComponent<UIRunner>();
            var go = new GameObject("BahalaNaCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            UnityEngine.Object.DontDestroyOnLoad(go);
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                UnityEngine.Object.DontDestroyOnLoad(es);
            }
            Canvas = go.GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(520f, 960f);
            scaler.matchWidthOrHeight = 0.5f;

            var bg = new GameObject("Background", typeof(Image));
            bg.transform.SetParent(go.transform, false);
            var bimg = bg.GetComponent<Image>();
            bimg.color = new Color32(20, 16, 28, 255);
            var brt = bg.GetComponent<RectTransform>();
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

            var root = new GameObject("ScreenRoot", typeof(RectTransform));
            root.transform.SetParent(go.transform, false);
            ScreenRoot = root.GetComponent<RectTransform>();
            ScreenRoot.anchorMin = Vector2.zero; ScreenRoot.anchorMax = Vector2.one;
            ScreenRoot.offsetMin = Vector2.zero; ScreenRoot.offsetMax = Vector2.zero;
        }

        public void Show(IScreen screen)
        {
            if (Current != null) Current.OnHide();
            foreach (Transform child in ScreenRoot) UnityEngine.Object.Destroy(child.gameObject);
            Current = screen;
            screen.Build(this);
            screen.OnShow();
        }

        public void Reload()
        {
            if (Current != null) Show(Current);
        }

        public Coroutine StartRoutine(IEnumerator routine) => Host.StartCoroutine(routine);
    }
}
