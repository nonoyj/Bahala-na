// Bahala Na? — world layer: smooth camera follow, clamped to the plaza image.
//
// Attach to the Main Camera. The camera is orthographic (required) and its size
// is derived from the plaza so the map fills the screen without letterboxing.

using UnityEngine;

namespace BahalaNa.World
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public float Smooth = 8f;
        // 5.76 = the whole plaza height, which leaves the camera nothing to pan.
        // 3.2 shows about a third of the map, so following is visible and the
        // character stays readable.
        public float OrthographicSize = 3.2f;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _cam.orthographicSize = OrthographicSize;
            _cam.backgroundColor = new Color32(20, 16, 28, 255);
            _cam.clearFlags = CameraClearFlags.SolidColor;
            // Landscape game: no rotation, no perspective.
            transform.rotation = Quaternion.identity;
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            Vector3 want = new Vector3(Target.position.x, Target.position.y, -10f);

            // Clamp so the camera never shows past the plaza edges.
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float minX = GridUtil.ImageMinWorld.x + halfW;
            float maxX = GridUtil.ImageMaxWorld.x - halfW;
            float minY = GridUtil.ImageMinWorld.y + halfH;
            float maxY = GridUtil.ImageMaxWorld.y - halfH;

            // If the viewport is wider than the map, lock to centre instead of
            // producing min > max (which would flip the camera).
            want.x = (minX <= maxX) ? Mathf.Clamp(want.x, minX, maxX)
                                    : (GridUtil.ImageMinWorld.x + GridUtil.ImageMaxWorld.x) * 0.5f;
            want.y = (minY <= maxY) ? Mathf.Clamp(want.y, minY, maxY)
                                    : (GridUtil.ImageMinWorld.y + GridUtil.ImageMaxWorld.y) * 0.5f;

            transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-Smooth * Time.deltaTime));
        }
    }
}
