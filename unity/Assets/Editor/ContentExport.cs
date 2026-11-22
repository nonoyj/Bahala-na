// Bahala Na? — editor tool: re-export content JSON into StreamingAssets.
// Menu: Bahala Na → Export Content  (also run from CI before the build)
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BahalaNa.EditorTools
{
    public static class ContentExport
    {
        [MenuItem("Bahala Na/Export Content")]
        public static void Export()
        {
            var dataPath = Application.dataPath;                    // <project>/Assets
            var projectRoot = Path.GetFullPath(Path.Combine(dataPath, "..", ".."));
            var contentDir = Path.Combine(projectRoot, "content");
            if (!Directory.Exists(contentDir))
            {
                Debug.LogError("[BahalaNa] content/ not found next to the Unity project: " + contentDir);
                EditorUtility.DisplayDialog("Bahala Na?", "content/ folder not found at:\n" + contentDir, "OK");
                return;
            }

            var dst = Path.Combine(dataPath, "StreamingAssets", "Content");
            Directory.CreateDirectory(dst);
            Directory.CreateDirectory(Path.Combine(dst, "cards"));

            int copied = 0;
            foreach (var f in Directory.GetFiles(contentDir, "*.json"))
            {
                File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), true);
                copied++;
            }
            foreach (var f in Directory.GetFiles(Path.Combine(contentDir, "cards"), "*.json"))
            {
                File.Copy(f, Path.Combine(dst, "cards", Path.GetFileName(f)), true);
                copied++;
            }

            AssetDatabase.Refresh();
            Debug.Log("[BahalaNa] Exported " + copied + " content files to Assets/StreamingAssets/Content");
            EditorUtility.DisplayDialog("Bahala Na?", "Exported " + copied + " content files.", "OK");
        }
    }
}
#endif
