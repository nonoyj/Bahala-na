/* Bahala Na? — bootstrap. */
(function () {
  "use strict";
  window.addEventListener("DOMContentLoaded", function () {
    if (!window.BAHALANA_CONTENT) {
      document.getElementById("app").innerHTML =
        '<div class="screen"><h2>Content bundle missing</h2><p>Run <code>python3 scripts/build_content.py</code> first.</p></div>';
      return;
    }
    window.UI.init();
  });
})();
