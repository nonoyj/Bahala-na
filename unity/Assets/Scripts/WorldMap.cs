// Bahala Na? — world layer: the plaza map + collision table.
//
// Collision is a bool grid in PIXEL space of the plaza image:
//   32 columns x 18 rows, each cell 64x64 px.
//   row 0 = BOTTOM of the image, col 0 = LEFT.
//
// The default table below is a starting point measured from the plaza art. Use
// CollisionPainter (F1 overlay, drag to paint, F2 save) to correct it in-game,
// then F3 to dump the corrected rects back into this file.

using UnityEngine;

namespace BahalaNa.World
{
    public class WorldMap : MonoBehaviour
    {
        public Sprite PlazaSprite;

        private bool[] _blocked = new bool[GridUtil.Cols * GridUtil.Rows];
        private SpriteRenderer _plazaRenderer;
        private Transform _plazaTransform;

        public int Cols => GridUtil.Cols;
        public int Rows => GridUtil.Rows;

        // ---------------------------------------------------------------- build
        public void Build(Sprite plaza)
        {
            PlazaSprite = plaza;

            var go = new GameObject("PlazaArt");
            _plazaTransform = go.transform;
            _plazaTransform.SetParent(transform, false);

            _plazaRenderer = go.AddComponent<SpriteRenderer>();
            _plazaRenderer.sprite = plaza;
            _plazaRenderer.sortingOrder = -100;

            float spriteWorldWidth = plaza.rect.width / plaza.pixelsPerUnit;
            float wantWorldWidth  = GridUtil.ImageWidth  / GridUtil.PixelsPerUnit;
            float wantWorldHeight = GridUtil.ImageHeight / GridUtil.PixelsPerUnit;
            float scale = wantWorldWidth / spriteWorldWidth;
            _plazaTransform.localScale = new Vector3(scale, scale, 1f);

            // Pivot is Center, so offset by half the size to put the image's
            // bottom-left corner at world origin.
            _plazaTransform.position = new Vector3(
                wantWorldWidth * 0.5f,
                wantWorldHeight * 0.5f,
                0f);

            ApplyDefaultCollision();
            CollisionPainter.TryLoad(this);   // painted-over corrections win
        }

        // ------------------------------------------------------------ collision
        private int Index(int col, int row) => row * GridUtil.Cols + col;

        public bool IsBlocked(int col, int row)
        {
            if (col < 0 || col >= GridUtil.Cols || row < 0 || row >= GridUtil.Rows) return true;
            return _blocked[Index(col, row)];
        }

        public bool IsBlockedWorld(Vector3 worldPos)
        {
            var cell = GridUtil.PixelToCell(GridUtil.WorldToPixel(worldPos));
            return IsBlocked(cell.x, cell.y);
        }

        public void SetBlocked(int col, int row, bool blocked)
        {
            if (col < 0 || col >= GridUtil.Cols || row < 0 || row >= GridUtil.Rows) return;
            _blocked[Index(col, row)] = blocked;
        }

        public void BlockRect(int colMin, int rowMin, int colMax, int rowMax)
        {
            for (int r = rowMin; r < rowMax; r++)
                for (int c = colMin; c < colMax; c++)
                    SetBlocked(c, r, true);
        }

        // --------------------------------------------------- default collision
        // Grid: 32 wide x 18 tall, row 0 = bottom of the plaza image.
        // Measured against barangay_plaza_16x9.png. Expect to refine with the painter.
        private void ApplyDefaultCollision()
        {
            // ---- water along the very bottom edge ----
            BlockRect(0, 0, 32, 1);

            // ---- TOP BAND (grid rows 15-17) : houses above the main road ----
            BlockRect(0, 15, 8, 18);       // top-left q
            BlockRect(9, 16, 14, 18);      // top-centre hut
            BlockRect(15, 15, 21, 18);     // top-centre-right houses
            BlockRect(22, 15, 32, 18);     // top-right blue-roof house

            // ---- COVERED BASKETBALL COURT (left third, grid rows 9-14) ----
            // The roof is solid: the player must walk around, not on it.
            BlockRect(1, 9, 8, 14);

            // ---- SARI-SARI STORE block (centre-left, below the court) ----
            BlockRect(9, 9, 13, 13);

            // ---- BARANGAY HALL + immediate frontage (centre) ----
            BlockRect(16, 9, 22, 14);
            // keep the steps + flagpole approach walkable
            for (int c = 17; c <= 20; c++)
            {
                SetBlocked(c, 9, false);
                SetBlocked(c, 8, false);
            }

            // ---- CHAPEL (right third) ----
            BlockRect(26, 9, 31, 14);

            // ---- BOTTOM-LEFT houses ----
            BlockRect(0, 3, 6, 7);
            BlockRect(0, 1, 3, 3);

            // ---- BOTTOM-CENTRE house + garden ----
            BlockRect(15, 2, 20, 6);
            BlockRect(21, 2, 24, 5);

            // ---- BOTTOM-RIGHT houses ----
            BlockRect(27, 1, 32, 6);
        }

        // ------------------------------------------------------------ spawning
        public Vector3 DefaultSpawnWorld()
        {
            // Open road just south of the barangay hall frontage.
            return GridUtil.CellToWorld(18, 7);
        }

        public static void SetDepth(SpriteRenderer r, float worldY)
        {
            float pixelY = worldY * GridUtil.PixelsPerUnit;
            r.sortingOrder = 2000 - Mathf.RoundToInt(pixelY);
        }
    }
}
