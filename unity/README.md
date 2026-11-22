# Bahala Na? — Unity 6.3 client

Unity 6.3 LTS · 2D URP · C# · Firebase (Firestore / Auth / Functions / Remote Config) · Addressables-ready content.

> **Status:** authored scaffold — the full game logic and UI flow are implemented in C#; art/animation and scene polish are the remaining frontend work. Open `unity/` as a Unity project (see README steps).

## Quick start
1. Open Unity Hub → **Add project from disk** → select the `unity/` folder (Unity **6.3 LTS**, version `6000.3.29f1`).
2. Let packages resolve (`manifest.json` pins UGUI, TextMeshPro, Newtonsoft.Json, Addressables).
3. **Bahala Na → Export Content** (menu) — copies `../../content/*` into `Assets/StreamingAssets/Content/` (already committed, but re-run after content edits).
4. Open any scene (or create an empty one) and press Play — `GameApp` bootstraps itself (`RuntimeInitializeOnLoadMethod`), builds a Canvas, and shows the main menu.
5. Without the Firebase Unity SDK the game runs on `LocalBackend` (offline, local Shared Barangay). To go live, add the Firebase Unity SDK + `google-services.json`/`GoogleService-Info.plist`, then define **`BAHALANA_FIREBASE`** in Player Settings → Scripting Define Symbols.

## Layout
```
unity/
  Packages/manifest.json      UGUI 2.0.0 · TMP 3.0.6 · Newtonsoft.Json 3.2.1 · Addressables 2.1.19
  ProjectSettings/ProjectVersion.txt   6000.3.29f1 (Unity 6.3 LTS)
  Assets/
    Editor/ContentExport.cs   menu: re-export content from ../../content
    StreamingAssets/Content/  runtime content source (manifest + cards/ + extras)
    Scripts/
      Models.cs               DTOs mirroring API_CONTRACT.md / content JSON
      ContentDatabase.cs      loads + queries content; eras; modifiers
      Dice.cs                 d20 / dN (System.Random)
      GameManager.cs          rules: run, chapters, careers, climax, endings
      SaveManager.cs          local save, settings, offline contribution queue
      Contribution.cs         §C.4 contribution + era recompute
      Backend.cs              IBarangayBackend + LocalBackend (offline)
      FirebaseBackend.cs      real Firebase impl (#if BAHALANA_FIREBASE)
      GameApp.cs              bootstrap + service locator
      UI/
        UIBuilder.cs          code-built uGUI primitives (no prefabs)
        UIManager.cs          canvas + screen switching
        ScreenBase.cs         screen contract + shared widgets
        MenuScreens.cs        menu, Character Spark, chapter intro, career pick
        GameScreens.cs        card, roll sheet, outcome, climax, ending
        InfoScreens.cs        barangay, echoes, how-to, settings, about
```

## UI flow
Character Spark → Chapter intro → **card** (choices) → **roll sheet** (assists/kasama/echoes + d20) → **outcome** → next card → … → Ch4 climax → **ending** (+ final ask) → menu.
Every choice is resolved through `GameManager` (the single rules entry point); screens never mutate state directly.
Utang na Loob is never rendered as a number — only `SaveManager.UtangHint()` flavor lines.

## Assemblies
No `.asmdef` is included: the scripts compile into the predefined `Assembly-CSharp` (and `Assembly-CSharp-Editor` for `Assets/Editor`), which auto-references `UnityEngine.UI`, `Unity.TextMeshPro` and `Newtonsoft.Json`. If you split the client into assemblies later, add `UnityEngine.UI` / `Unity.TextMeshPro` / `Newtonsoft.Json` references explicitly (verify the exact uGUI assembly name for your Unity 6 patch — 2.0.0 has shipped as both `UnityEngine.UI` and `Unity.UI`).

## Rules parity
`GameManager` is a direct port of `js/engine.js` (the verified reference). When changing rules, change both (or port from the reference) and re-run `node scripts/simulate.js`.

## Verification
This sandbox has no Unity/.NET toolchain, so the client is **authored + statically reviewed, not compiled/run**. Every `.cs` file parses clean (tree-sitter C# grammar, 0 ERROR/MISSING nodes) and the rules were validated headlessly via `js/engine.js` + `scripts/simulate.js`. Open `unity/` in Unity 6.3 to compile and play; fix any compile errors there and re-run the parse check:
```bash
python3 - <<'PY'
import glob
from tree_sitter_languages import get_parser
p = get_parser('c_sharp')
def errs(n, out):
    if n.type == 'ERROR' or n.is_missing: out.append(n.start_point)
    for c in n.children: errs(c, out)
for f in sorted(glob.glob('unity/Assets/**/*.cs', recursive=True)):
    out = []; errs(p.parse(open(f,'rb').read()).root_node, out)
    print(f, 'OK' if not out else ('ERRORS ' + str(out[:3])))
PY
```

## Firebase wiring
- Reads: `barangay/state`, `echoes` (direct Firestore reads; public-read rules).
- Writes: **only** `submitRunContribution` (callable) — never direct writes to `barangay/state`.
- Offline: the run is saved locally after every card; contributions queue in `SaveManager` and flush on reconnect (idempotent per `runId`).

See `../API_CONTRACT.md` for the frozen contract and `../backend/` for the Cloud Function implementation.
