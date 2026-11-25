// Bahala Na? — world layer: brings the player back to the plaza after a card.
//
// WHY THIS EXISTS
//   EncounterFlow shows the existing GameScreen for the card. GameScreen was
//   written for the menu flow, so when the run ends it moves on to the outcome /
//   ending screens by itself. We do not want to edit GameScreens.cs while the
//   encounter flow is still being proven — too much blast radius.
//
//   Instead this component watches for the moment the run has moved past the
//   card and then restores the world layer. It is a temporary shim: once the
//   EncounterFlow is verified, this behaviour should move INTO GameScreens.cs
//   properly and this file should be deleted.
//
// Behaviour:
//   - If no world is active, does nothing (menu flow unaffected).
//   - If a world is active and the CURRENT card changed while the UI is showing,
//     it hides the UI canvas and re-enables the world.

using UnityEngine;
using BahalaNa.UI;

namespace BahalaNa.World
{
    public class GameScreenReturnHook : MonoBehaviour
    {
        private WorldBootstrap _boot;
        private string _lastCardId;
        private bool _armed;

        private void Start()
        {
            _boot = GetComponent<WorldBootstrap>();
        }

        /// <summary>Called by EncounterFlow right before showing the card screen.</summary>
        public void Arm(string cardId)
        {
            _lastCardId = cardId;
            _armed = true;
        }

        private void Update()
        {
            if (!_armed || _boot == null || _boot.WorldRoot == null) return;
            if (GameApp.Game == null || GameApp.Game.Run == null) return;

            // The card screen is only active while the UI canvas is showing.
            var ui = GameApp.UI;
            if (ui == null || ui.Canvas == null || !ui.Canvas.enabled) return;

            var card = GameApp.Game.CurrentCard;
            string currentId = card != null ? card.id : null;

            // Returning to the plaza happens when the current card is no longer
            // the one we left on — i.e. it resolved and the run advanced.
            if (currentId != null && currentId != _lastCardId)
            {
                _armed = false;
                _boot.ShowWorldAfterEncounter();
            }
        }
    }
}
