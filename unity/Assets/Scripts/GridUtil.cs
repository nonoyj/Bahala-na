// Bahala Na? — world layer: grid maths. One source of truth for tile size.
//
// Coordinate system choice (important):
//   The plaza sprite is a single 2048x1152 image placed on a SpriteRenderer whose
//   transform sits at world origin. We therefore treat WORLD UNITS AS PIXELS of that
//   sprite, so a player at (1024, 576) stands in the middle of the plaza image.
//   Grid cells are computed in pixel space and converted back.
//
//   This avoids any dependency on Pixels-Per-Unit import settings: if the sprite is
//   imported at 100 PPU the SpriteRenderer draws it 20.48 x 11.52 units wide and we
//   still address it in the same 0..2048 / 0..1152 space, because we scale the
//   renderer transform to match PIXELS_PER_UNIT regardless of import settings.

using UnityEngine;

namespace BahalaNa.World
{
    public static class GridUtil
    {
        // ---- plaza image geometry (must match barangay_plaza_16x9.png) ----
        public const int ImageWidth  = 2048;
        public const int ImageHeight = 1152;

        // ---- tile geometry ----
        // 64 px cells => 32 x 18 grid over the plaza. Character frames are ~89x170 px,
        // so a character is ~1.4 tiles wide and ~2.7 tiles tall on screen. That reads
        // correctly for a top-down village and keeps the collision table small enough
        // to author by hand.
        public const int CellSize = 64;

        public const int Cols = ImageWidth  / CellSize;   // 32
        public const int Rows = ImageHeight / CellSize;   // 18

        /// <summary>Pixels per world unit for the world layer. Fixed, not import-driven.</summary>
        public const float PixelsPerUnit = 100f;

        public static Vector2 CellToPixel(int col, int row) => new Vector2(
            col * CellSize + CellSize * 0.5f,
            row * CellSize + CellSize * 0.5f);

        public static Vector2Int PixelToCell(Vector2 px) => new Vector2Int(
            Mathf.FloorToInt(px.x / CellSize),
            Mathf.FloorToInt(px.y / CellSize));

        public static Vector3 PixelToWorld(Vector2 px) => new Vector3(
            px.x / PixelsPerUnit,
            px.y / PixelsPerUnit,
            0f);

        public static Vector2 WorldToPixel(Vector3 w) => new Vector2(
            w.x * PixelsPerUnit,
            w.y * PixelsPerUnit);

        public static Vector3 CellToWorld(int col, int row) =>
            PixelToWorld(CellToPixel(col, row));

        public static bool InBounds(Vector2Int c) =>
            c.x >= 0 && c.x < Cols && c.y >= 0 && c.y < Rows;

        /// <summary>Bottom-left world corner of the plaza image.</summary>
        public static Vector3 ImageMinWorld => PixelToWorld(Vector2.zero);
        /// <summary>Top-right world corner of the plaza image.</summary>
        public static Vector3 ImageMaxWorld => PixelToWorld(new Vector2(ImageWidth, ImageHeight));
    }
}
