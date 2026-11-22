/* Bahala Na? — UI layer (DOM rendering, screens, dice overlay). */
(function () {
  "use strict";

  const BN = window.BN;
  const C = BN.C;

  let S = {
    profile: null,      // {name, presentation}
    settings: null,
    barangay: null,
    run: null,
  };
  let ui = {
    screen: "menu",
    pending: null,      // {card, choice, idx}
    rollOpts: { kasama: false, echoes: false },
    diceTimer: null,
    lastContrib: null,
  };

  const $ = (sel) => document.querySelector(sel);
  const esc = (s) => String(s == null ? "" : s).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

  function txt(tl, en) { return S.settings.lang === "tl" ? tl : (en || tl); }
  function pick(obj, key) { return S.settings.lang === "tl" ? obj[key] : (obj[key + "_en"] || obj[key]); }

  function fill(t) {
    if (!t) return "";
    const p = S.profile || { name: "", presentation: "neutral" };
    const maam = p.presentation === "girl" ? "Ma'am" : p.presentation === "boy" ? "Sir" : "Sir/Ma'am";
    return t.replace(/\{\{maam\}\}/g, maam).replace(/\{\{name\}\}/g, p.name || "ikaw");
  }

  // ---------- persistence ----------
  const KEYS = { profile: "bn_profile", settings: "bn_settings", barangay: "bn_barangay", run: "bn_run" };
  function save() {
    try {
      localStorage.setItem(KEYS.profile, JSON.stringify(S.profile));
      localStorage.setItem(KEYS.settings, JSON.stringify(S.settings));
      localStorage.setItem(KEYS.barangay, JSON.stringify(S.barangay));
      localStorage.setItem(KEYS.run, JSON.stringify(S.run));
    } catch (e) { /* storage unavailable */ }
  }
  function load() {
    try {
      S.profile = JSON.parse(localStorage.getItem(KEYS.profile) || "null");
      S.settings = Object.assign(BN.defaultSettings(), JSON.parse(localStorage.getItem(KEYS.settings) || "{}"));
      S.barangay = Object.assign(BN.defaultBarangay(), JSON.parse(localStorage.getItem(KEYS.barangay) || "{}"));
      S.run = JSON.parse(localStorage.getItem(KEYS.run) || "null");
    } catch (e) {
      S.settings = BN.defaultSettings();
      S.barangay = BN.defaultBarangay();
    }
  }

  // ---------- audio ----------
  let AC = null;
  function sfx(kind) {
    if (!S.settings.sfx) return;
    try {
      AC = AC || new (window.AudioContext || window.webkitAudioContext)();
      const o = AC.createOscillator();
      const g = AC.createGain();
      o.connect(g); g.connect(AC.destination);
      const now = AC.currentTime;
      const conf = {
        click:  [440, 0.05, 0.05],
        roll:   [220, 0.12, 0.08],
        good:   [523, 0.18, 0.10],
        bad:    [160, 0.25, 0.10],
        voice:  [392, 0.20, 0.10],
      }[kind] || [440, 0.08, 0.06];
      o.frequency.setValueAtTime(conf[0], now);
      o.frequency.exponentialRampToValueAtTime(conf[0] * (kind === "good" ? 1.5 : kind === "bad" ? 0.6 : 1), now + conf[1]);
      g.gain.setValueAtTime(conf[2], now);
      g.gain.exponentialRampToValueAtTime(0.0001, now + conf[1]);
      o.start(now); o.stop(now + conf[1] + 0.02);
    } catch (e) { /* no audio */ }
  }
  function vibrate(ms) {
    if (S.settings.vibration && navigator.vibrate) { try { navigator.vibrate(ms); } catch (e) {} }
  }

  // ---------- shared render helpers ----------
  function statChips() {
    const r = S.run;
    if (!r) return "";
    return BN.STAT_KEYS.map((k) => {
      const info = BN.STAT_INFO[k];
      const m = BN.mod(r.stats[k]);
      return `<div class="stat-chip" title="${esc(info.desc || "")}">
        <span class="stat-ico">${info.icon}</span>
        <span class="stat-val">${r.stats[k]}</span>
        <span class="stat-mod">${m >= 0 ? "+" : ""}${m}</span>
      </div>`;
    }).join("");
  }

  function hudBar() {
    const r = S.run;
    if (!r) return "";
    const kasama = BN.kasamaAvailable(r);
    const echoes = BN.echoAvailable(r, S.barangay);
    const hint = BN.utangHint(r, S.settings.lang);
    const chapter = BN.CHAPTER_INFO[r.chapter];
    const total = r.deck.length;
    const idx = Math.min(r.cardIndex + 1, total);
    return `
      <div class="hud">
        <div class="hud-top">
          <span class="hud-chapter">${esc(txt(chapter.name, chapter.name_en))}</span>
          <span class="hud-progress">${idx} / ${total}</span>
          <button class="icon-btn" data-act="stats" title="${esc(txt("Stats", "Stats"))}">📊</button>
          <button class="icon-btn" data-act="quit" title="${esc(txt("Menu", "Menu"))}">🏠</button>
        </div>
        <div class="stats-bar">${statChips()}</div>
        <div class="hud-sub">
          <span class="kasama" title="${esc(txt("Kasama: +1d4 bawat isa, max 2", "Kasama: +1d4 each, max 2"))}">🤝 Kasama: ${kasama}</span>
          <span class="echoes">📣 Echo: ${echoes}</span>
          ${hint ? `<span class="utang-hint">🕯️ ${esc(hint)}</span>` : ""}
        </div>
      </div>`;
  }

  function effectChips(effects) {
    if (!effects) return "";
    const chips = [];
    if (effects.stats) {
      for (const [k, v] of Object.entries(effects.stats)) {
        if (!v) continue;
        chips.push(`<span class="chip ${v > 0 ? "up" : "down"}">${BN.STAT_INFO[k].icon} ${BN.STAT_INFO[k].name} ${v > 0 ? "+" : ""}${v}</span>`);
      }
    }
    if (effects.utang > 0) chips.push(`<span class="chip down">🕯️ ${esc(txt("May bagong binabalakid…", "A new debt…"))}</span>`);
    if (effects.utang < 0) chips.push(`<span class="chip up">🕯️ ${esc(txt("Utang bawas", "Debt reduced"))}</span>`);
    if (effects.utangClear) chips.push(`<span class="chip up">🕯️ ${esc(txt("Nawala ang utang", "Debt cleared"))}</span>`);
    if (effects.tiwala) chips.push(`<span class="chip ${effects.tiwala > 0 ? "up" : "down"}">🏛️ ${esc(txt("Tiwala ng bayan", "Town trust"))} ${effects.tiwala > 0 ? "+" : ""}${effects.tiwala}</span>`);
    if (effects.voices) chips.push(`<span class="chip up">📣 ${esc(txt("Boses", "Voice"))} +${effects.voices}</span>`);
    if (effects.walangKibo) chips.push(`<span class="chip down">🤫 ${esc(txt("Walang kibo", "No voice"))}</span>`);
    if (effects.reform) chips.push(`<span class="chip up">🌅 ${esc(txt("Reporma", "Reform"))}!</span>`);
    if (effects.spokeFirst) chips.push(`<span class="chip up">⭐ ${esc(txt("Unang boses", "First voice"))}</span>`);
    return chips.length ? `<div class="chips">${chips.join("")}</div>` : "";
  }

  function typeBadge(type) {
    const map = {
      honest: ["Honest", "Totoo", "⚖️"],
      padrino: ["Padrino", "Padrino", "🕯️"],
      collective: ["Collective", "Sama-sama", "🏘️"],
      huwag: ["Huwag makialam", "Huwag makialam", "🤫"],
      ordinary: ["Ordinary", "Karanihatan", "🧑‍🌾"],
      comply: ["Comply", "Sunduin", "✍️"],
      pera: ["Pera", "Pera", "💰"],
      escape: ["Escape", "Umalis", "🚪"],
      diskarte: ["Diskarte", "Diskarte", "🛠️"],
    }[type] || [type, type, "•"];
    return `<span class="type-badge type-${type}">${map[2]} ${esc(txt(map[0], map[1]))}</span>`;
  }

  function choiceButton(card, choice, i) {
    const roll = choice.roll;
    let meta = "";
    if (roll) {
      const stat = BN.STAT_INFO[roll.stat];
      let dc = roll.dc;
      if (roll.dc_plus_utang) dc = `${10}+${esc(txt("utang", "debt"))}`;
      meta = `<span class="roll-meta">${stat.icon} ${esc(txt("DC", "DC"))} ${dc}${roll.kasama ? " · 🤝" : ""}${roll.echoes ? " · 📣" : ""}</span>`;
    } else if (choice.type === "padrino" || choice.type === "comply") {
      meta = `<span class="roll-meta auto">${esc(txt("Auto", "Auto"))}</span>`;
    }
    return `<button class="choice-btn" data-choice="${i}">
      <span class="choice-label">${esc(fill(pick(choice, "label")))}</span>
      ${typeBadge(choice.type)}
      ${meta}
    </button>`;
  }

  // ---------- screens ----------
  function setScreen(name, html) {
    ui.screen = name;
    $("#app").innerHTML = html;
    try { window.scrollTo(0, 0); } catch (e) { /* unsupported */ }
  }

  function showMenu() {
    const era = BN.eraOf(S.barangay);
    const hasSave = !!(S.run && !S.run.ended);
    const pct = BN.spokeUpPct(S.barangay);
    setScreen("menu", `
      <div class="screen menu">
        <div class="menu-art">
          <img src="assets/title.png" alt="Barangay Bayanihan" onerror="this.style.display='none'">
        </div>
        <h1 class="game-title">BAHALA NA?</h1>
        <p class="tagline">${esc(txt(
          "Isang buhay, isang d20, isang barangay. Ang Desisyon mo, Desisyon ng Bayan.",
          "One life, one d20, one barangay. Your choice, the town's choice."
        ))}</p>
        <div class="menu-buttons">
          <button class="btn primary big" data-act="play">${esc(txt("▶ Maglaro", "▶ Play"))}</button>
          ${hasSave ? `<button class="btn big" data-act="continue">${esc(txt("↻ Magpatuloy", "↻ Continue"))}</button>` : ""}
          <button class="btn big" data-act="barangay">${esc(txt("🏘️ Barangay", "🏘️ Barangay"))}</button>
          <button class="btn big" data-act="echoes">${esc(txt("📣 Mga Echo", "📣 Echoes"))}</button>
          <button class="btn big" data-act="howto">${esc(txt("❓ Paano Maglaro", "❓ How to Play"))}</button>
          <button class="btn big" data-act="settings">${esc(txt("⚙️ Settings", "⚙️ Settings"))}</button>
          <button class="btn big" data-act="about">${esc(txt("ℹ️ Tungkol", "ℹ️ About"))}</button>
        </div>
        <div class="menu-era">
          <span class="era-pill era-${era.id}">${esc(txt(era.name, era.name_en))}</span>
          <span class="menu-era-sub">${esc(txt(
            `Barangay Bayanihan · ${pct}% nagsalita · tipping point 40%`,
            `Barangay Bayanihan · ${pct}% spoke up · tipping point 40%`
          ))}</span>
        </div>
      </div>`);
  }

  function showSpark() {
    ui.spark = ui.spark || { name: (S.profile && S.profile.name) || "", presentation: (S.profile && S.profile.presentation) || "neutral", rolled: false, bg: null };
    const sp = ui.spark;
    const presentations = [
      { id: "boy", label: "👦 Batang lalaki", label_en: "Boy" },
      { id: "girl", label: "👧 Batang babae", label_en: "Girl" },
      { id: "neutral", label: "🧒 Neutral", label_en: "Neutral" },
    ];
    setScreen("spark", `
      <div class="screen spark">
        <h2>${esc(txt("Character Spark", "Character Spark"))}</h2>
        <p class="sub">${esc(txt("30 segundo. Pangalan, presentation, at suerte.", "30 seconds. Name, presentation, and luck."))}</p>
        <label class="field-label">${esc(txt("Pangalan / alias", "Name / alias"))}</label>
        <input id="spark-name" class="text-input" maxlength="20" value="${esc(sp.name)}" placeholder="${esc(txt("Hal. Nonoy", "e.g. Nonoy"))}">
        <label class="field-label">${esc(txt("Presentation", "Presentation"))}</label>
        <div class="pres-row">
          ${presentations.map((p) => `<button class="pres-btn ${sp.presentation === p.id ? "sel" : ""}" data-pres="${p.id}">${esc(txt(p.label, p.label_en))}</button>`).join("")}
        </div>
        <div id="spark-result">
          ${sp.rolled && sp.bg ? sparkResultHtml(sp.bg) : `<p class="hint">${esc(txt("I-roll ang d6 para sa iyong Family Circumstance.", "Roll the d6 for your Family Circumstance."))}</p>`}
        </div>
        <div class="row">
          <button class="btn" data-act="menu">${esc(txt("← Bumalik", "← Back"))}</button>
          <button class="btn primary" data-act="roll-bg" ${sp.rolled ? "disabled" : ""}>${esc(txt("🎲 I-roll ang d6", "🎲 Roll d6"))}</button>
          <button class="btn primary" data-act="start-run" ${sp.rolled ? "" : "disabled"}>${esc(txt("Simulan ▶", "Start ▶"))}</button>
        </div>
      </div>`);
  }

  function sparkResultHtml(bg) {
    const bonus = Object.entries(bg.bonus || {}).map(([k, v]) => `<span class="chip up">${BN.STAT_INFO[k].icon} ${BN.STAT_INFO[k].name} +${v}</span>`).join("");
    const penalty = Object.entries(bg.penalty || {}).map(([k, v]) => `<span class="chip down">${BN.STAT_INFO[k].icon} ${BN.STAT_INFO[k].name} ${v}</span>`).join("");
    return `
      <div class="bg-result">
        <div class="bg-roll">d6 → <b>${bg.roll}</b></div>
        <div class="bg-name">${esc(txt(bg.name, bg.name_en))}</div>
        <p class="bg-text">${esc(txt(bg.text, bg.text_en))}</p>
        <div class="chips">${bonus}${penalty}</div>
        ${bg.roll === 4 ? `<p class="hint">${esc(txt("Mas maraming alok na pabor sa simula (mataas na Utang risk).", "More favors offered early (higher Utang risk)."))}</p>` : ""}
      </div>`;
  }

  function showCareer(opts) {
    const r = S.run;
    const list = opts && opts.length > 1 ? opts : BN.careerOptions(r);
    const single = list.length === 1;
    setScreen("career", `
      <div class="screen career">
        <h2>${esc(txt("Ang Sangandaan", "The Crossroads"))}</h2>
        <p class="sub">${esc(txt(
          single ? "Isang tungkulin ang bumagay sa'yo." : "Maraming tungkulin ang bumagay sa'yo. Ikaw ang pipili (9B).",
          single ? "One path fits you." : "Several paths fit you. You pick (9B)."
        ))}</p>
        <div class="career-list">
          ${list.map((c) => `
            <button class="career-btn" data-career="${c}">
              <span class="career-icon">${BN.CAREERS[c].icon}</span>
              <span class="career-name">${esc(txt(BN.CAREERS[c].name, BN.CAREERS[c].name_en))}</span>
              <span class="career-desc">${esc(careerReason(c, r))}</span>
            </button>`).join("")}
        </div>
        <button class="btn" data-act="menu">${esc(txt("← Bumalik", "← Back"))}</button>
      </div>`);
  }

  function careerReason(c, r) {
    const s = r.stats;
    const reasons = {
      official: txt(`Koneksyon ${s.koneksyon} · ${BN.hasFlag(r, "ran_for_org") ? "nagpakandidato" : "utang " + r.utang}`, `Connections ${s.koneksyon} · ${BN.hasFlag(r, "ran_for_org") ? "ran for org" : "debt " + r.utang}`),
      police: txt(`Pagsusulit · Diskarte ${s.diskarte} · Pera ${s.pera}`, `Exam · Resourcefulness ${s.diskarte} · Money ${s.pera}`),
      teacher: txt(`Talino ${s.talino}`, `Wits ${s.talino}`),
      organizer: txt(`Komunidad ${s.komunidad}${BN.hasFlag(r, "spoke_up") ? " · nagsalita" : ""}`, `Community ${s.komunidad}${BN.hasFlag(r, "spoke_up") ? " · spoke up" : ""}`),
      citizen: txt("Ordinaryong mamamayan", "Ordinary citizen"),
    };
    return reasons[c] || "";
  }

  function showChapterIntro(chapter) {
    const ch = BN.CHAPTER_INFO[chapter];
    const titles = {
      1: ["Kabataan", "Childhood", "Mga edad ~8–12. Mainit, maliit, medyo nakakatawa."],
      2: ["Sangandaan", "Crossroads", "Mga edad ~16–22. Tumatindi ang pressure."],
      3: ["Tungkulin", "Duty", "Mga edad ~25–35. Bigat ng moralidad."],
      4: ["Ang Pagsubok", "The Test", "Ang baha. Ang ghost project. Ang town assembly."],
    }[chapter];
    setScreen("chapter", `
      <div class="screen chapter-intro">
        <div class="chapter-num">${esc(txt("Kabanata", "Chapter"))} ${chapter}</div>
        <h2>${esc(txt(titles[0], titles[1]))}</h2>
        <p class="sub">${esc(titles[2])}</p>
        <p class="ages">${esc(txt("Edad", "Ages"))}: ${ch.ages} · ${ch.cards} ${esc(txt("na card", "cards"))}</p>
        <button class="btn primary big" data-act="begin-chapter">${esc(txt("Magpatuloy ▶", "Continue ▶"))}</button>
      </div>`);
  }

  function showGame() {
    const r = S.run;
    if (!r) { showMenu(); return; }
    if (r.chapter === 0) { showChapterIntro(1); return; }
    const card = BN.currentCard(r);
    if (!card) { advanceChapter(); return; }
    const ch = BN.CHAPTER_INFO[r.chapter];
    const cascadeHtml = r.chapter === 4 ? cascadeBar(r) : "";
    setScreen("game", `
      <div class="screen game">
        ${hudBar()}
        ${cascadeHtml}
        <div class="card">
          <div class="card-kicker">${esc(txt(ch.name, ch.name_en))} · ${esc(txt("Card", "Card"))} ${r.cardIndex + 1}/${r.deck.length}</div>
          <h2 class="card-title">${esc(fill(pick(card, "title")))}</h2>
          <p class="card-setup">${esc(fill(pick(card, "setup")))}</p>
          <div class="choices" id="choices">
            ${card.choices.map((c, i) => choiceButton(card, c, i)).join("")}
          </div>
        </div>
      </div>`);
  }

  function cascadeBar(r) {
    const groups = BN.GROUPS.map((g) => {
      const on = r.cascade[g.id];
      return `<span class="cascade-node ${on ? "on" : ""}" title="${esc(txt(g.name, g.name_en))} (${g.threshold})">${g.icon}</span>`;
    }).join('<span class="cascade-link"></span>');
    return `<div class="cascade-bar"><span class="cascade-label">📣 ${r.voices} ${esc(txt("boses", "voices"))}</span><div class="cascade-track">${groups}</div></div>`;
  }

  function showPreRoll() {
    const { card, choice } = ui.pending;
    const roll = choice.roll;
    const stat = BN.STAT_INFO[roll.stat];
    const kasama = BN.kasamaAvailable(S.run);
    const echoes = roll.echoes ? BN.echoAvailable(S.run, S.barangay) : 0;
    const era = BN.eraOf(S.barangay);
    const systemic = roll.systemic || card.systemic;
    let dc = roll.dc;
    if (roll.dc_if_flag && BN.hasFlag(S.run, roll.dc_if_flag.flag)) dc = roll.dc_if_flag.dc;
    if (roll.dc_plus_utang) dc = 10 + S.run.utang;
    const eraMod = systemic ? era.dc_mod : 0;
    ui.rollOpts = { kasama: false, echoes: false };
    $("#choices").innerHTML = `
      <div class="preroll">
        <div class="preroll-title">${esc(fill(pick(choice, "label")))}</div>
        <div class="preroll-math">
          <span>${stat.icon} ${esc(txt(stat.name, stat.name_en))} <b>+${BN.mod(S.run.stats[roll.stat])}</b></span>
          <span class="preroll-dc">${esc(txt("DC", "DC"))} ${dc}${eraMod ? ` <small>(${eraMod > 0 ? "+" : ""}${eraMod} ${esc(txt(era.name, era.name_en))})</small>` : ""}</span>
        </div>
        <div class="preroll-opts">
          ${roll.kasama && kasama > 0 ? `<button class="opt-btn" data-opt="kasama">🤝 Kasama +1d4 ×${kasama} <span class="opt-state">${esc(txt("OFF", "OFF"))}</span></button>` : ""}
          ${roll.echoes && echoes > 0 ? `<button class="opt-btn" data-opt="echoes">📣 Echo +1d6 ×${echoes} <span class="opt-state">${esc(txt("OFF", "OFF"))}</span></button>` : ""}
        </div>
        <div class="row">
          <button class="btn" data-act="back-choice">${esc(txt("← Bumalik", "← Back"))}</button>
          <button class="btn primary" data-act="do-roll">${esc(txt("🎲 I-roll", "🎲 Roll"))}</button>
        </div>
      </div>`;
    document.querySelectorAll(".opt-btn").forEach((b) => {
      b.addEventListener("click", () => {
        const k = b.dataset.opt;
        ui.rollOpts[k] = !ui.rollOpts[k];
        b.querySelector(".opt-state").textContent = ui.rollOpts[k] ? "ON" : "OFF";
        b.classList.toggle("on", ui.rollOpts[k]);
        sfx("click");
      });
    });
  }

  function animateD20(result, done) {
    const speed = S.settings.dice === "off" ? 0 : S.settings.dice === "fast" ? 120 : 500;
    const overlay = document.createElement("div");
    overlay.className = "overlay dice-overlay";
    overlay.innerHTML = `
      <div class="d20-wrap">
        <div class="d20" id="d20face">?</div>
        <div class="d20-result" id="d20result"></div>
      </div>`;
    document.body.appendChild(overlay);
    const face = overlay.querySelector("#d20face");
    const res = overlay.querySelector("#d20result");
    if (speed === 0) { face.textContent = result.d20; finish(); return; }
    sfx("roll");
    let n = 0;
    const total = Math.max(3, Math.round(speed / 90));
    ui.diceTimer = setInterval(() => {
      face.textContent = 1 + Math.floor(Math.random() * 20);
      if (++n >= total) {
        clearInterval(ui.diceTimer);
        face.textContent = result.d20;
        finish();
      }
    }, 90);
    function finish() {
      vibrate(result.success ? [30, 40, 30] : 80);
      sfx(result.success ? "good" : "bad");
      const stat = BN.STAT_INFO[result.stat];
      const bonusParts = [];
      if (result.mod) bonusParts.push(`${stat.icon} +${result.mod}`);
      result.rolls.forEach((rr) => bonusParts.push(`${rr.kind === "kasama" ? "🤝" : "📣"} +${rr.v}`));
      res.innerHTML = `
        <div class="d20-total">${result.total} <small>vs</small> ${result.dc}</div>
        <div class="d20-breakdown">d20 <b>${result.d20}</b>${bonusParts.length ? " · " + bonusParts.join(" · ") : ""}</div>
        <div class="d20-verdict ${result.success ? "win" : "lose"}">
          ${result.crit ? "🌟 " + esc(txt("Natural 20! Kritikal.", "Natural 20! Critical.")) :
            result.fumble ? "💀 " + esc(txt("Natural 1. Biggest consequence — pero hindi huli ang lahat.", "Natural 1. A bigger cost — but never a dead end.")) :
            result.success ? "✅ " + esc(txt("Tagumpay", "Success")) : "❌ " + esc(txt("Nabigo", "Fail"))}
        </div>`;
      setTimeout(() => {
        overlay.classList.add("show");
        const btn = document.createElement("button");
        btn.className = "btn primary big";
        btn.textContent = txt("Ituloy ▶", "Continue ▶");
        btn.addEventListener("click", () => { overlay.remove(); done(); });
        overlay.querySelector(".d20-wrap").appendChild(btn);
      }, speed * 0.4);
    }
  }

  function showOutcome(card, choice, rollResult, out) {
    const r = S.run;
    const textObj = rollResult ? (rollResult.success ? choice.success : choice.fail) : null;
    const bodyText = textObj ? pick(textObj, "text") : outcomeFallback(choice);
    const chips = effectChips(out.effects);
    const verdict = rollResult
      ? `<div class="outcome-verdict ${rollResult.success ? "win" : "lose"}">${rollResult.success ? "✅" : "❌"} ${esc(txt(rollResult.success ? "Tagumpay" : "Nabigo", rollResult.success ? "Success" : "Fail"))}</div>`
      : "";
    $("#choices").innerHTML = `
      <div class="outcome">
        ${verdict}
        <p class="outcome-text">${esc(fill(bodyText))}</p>
        ${chips}
        <button class="btn primary big" data-act="next-card">${esc(txt("Ituloy ▶", "Continue ▶"))}</button>
      </div>`;
  }

  function outcomeFallback(choice) {
    const map = {
      padrino: ["Tumango ka. May bagong utang, kahit hindi mo pa kita.", "You agree. A new debt, even if you don't see it yet."],
      huwag: ["Umuwi ka nang tahimik. May binenta kang ano… hindi mo pa alam.", "You go home quietly. Something was sold… you don't know yet."],
      comply: ["Sinusunod mo. Mas madali 'yon.", "You comply. That was easier."],
      escape: ["Umalis ka. Ligtas ka, pero may naiwan.", "You leave. Safe, but something stays behind."],
      ordinary: ["Pinili mo ang karaniwan. Walang êxplica.", "You choose the ordinary path. No excuses."],
      pera: ["Binigay mo ang pera.", "You give the money."],
      honest: ["Sinabi mo ang totoo.", "You tell the truth."],
      collective: ["Sama-sama kayong kumilos.", "You act together."],
      diskarte: ["Nilutas mo sa paraang diskarte.", "You handle it with street smarts."],
    }[choice.type] || ["Nagawa mo.", "You do it."];
    return txt(map[0], map[1]);
  }

  function nextCard() {
    const r = S.run;
    r.cardIndex += 1;
    save();
    sfx("click");
    if (r.cardIndex >= r.deck.length) { advanceChapter(); return; }
    showGame();
  }

  function advanceChapter() {
    const r = S.run;
    if (r.chapter === 1) { BN.startChapter(r, 2, S.barangay); save(); showChapterIntro(2); }
    else if (r.chapter === 2) {
      const opts = BN.careerOptions(r);
      if (opts.length <= 1) {
        r.career = opts[0];
        BN.startChapter(r, 3, S.barangay);
        save();
        showChapterIntro(3);
      } else {
        showCareer(opts);
      }
    }
    else if (r.chapter === 3) { BN.startChapter(r, 4, S.barangay); save(); showChapterIntro(4); }
    else if (r.chapter === 4) { finishRun(); }
  }

  function pickCareer(c) {
    S.run.career = c;
    BN.startChapter(S.run, 3, S.barangay);
    save();
    sfx("click");
    showChapterIntro(3);
  }

  function finishRun() {
    const r = S.run;
    r.endingId = BN.determineEnding(r);
    r.ended = true;
    r.endedAt = Date.now();
    ui.lastContrib = BN.applyContribution(S.barangay, r);
    save();
    showEnding();
  }

  // ---------- ending ----------
  function showEnding() {
    const r = S.run;
    const e = C.endings.find((x) => x.id === r.endingId);
    const bg = C.backgrounds.find((b) => b.id === r.background);
    const era = BN.eraOf(S.barangay);
    const contrib = ui.lastContrib || S.barangay.lastContribution;
    const pct = BN.spokeUpPct(S.barangay);
    const redeemed = BN.redemption(r);
    const ask = C.finalAsks.find((a) => a.id === S.settings.location) || C.finalAsks[3];
    const ledger = r.ledger.length
      ? r.ledger.map((l) => {
          if (l.kind === "utang") return `<li>🕯️ <b>+${l.amount}</b> · ${esc(l.card)} <span class="ledger-ch">Ch${l.chapter}</span></li>`;
          if (l.kind === "cleared") return `<li>🌤️ ${esc(txt("Nawala ang utang", "Debt cleared"))} · ${esc(l.card)} <span class="ledger-ch">Ch${l.chapter}</span></li>`;
          return `<li>✍️ ${esc(txt("Sumunod sa utang", "Complied with the debt"))} · ${esc(l.card)} <span class="ledger-ch">Ch${l.chapter}</span></li>`;
        }).join("")
      : `<li>${esc(txt("Walang natirang utang. Malinis ang loob mo.", "No debts remain. Your conscience is clean."))}</li>`;
    setScreen("ending", `
      <div class="screen ending">
        <div class="ending-card">
          <div class="ending-kicker">${esc(txt("Ikaw at ang Bayan", "You and the Town"))}</div>
          <h2 class="ending-title">${esc(txt(e.title, e.title_en))}</h2>
          <p class="ending-text">${esc(txt(e.text, e.text_en))}</p>
          <p class="starting-line">${esc(txt(
            `Nagsimula ka bilang ${txt(bg.name, bg.name_en).toLowerCase()}. Ang iba, nagsimula sa iba.`,
            `You began as ${txt(bg.name, bg.name_en).toLowerCase()}. Others began elsewhere.`
          ))}</p>
          ${redeemed ? `<p class="redemption">🕊️ ${esc(txt("Pagbawi: Hindi pa huli ang lahat. May pagkakataon pa.", "Redemption: it's never too late. There's still a chance."))}</p>` : ""}
        </div>
        <div class="impact">
          <h3>${esc(txt("Epekto sa Barangay", "Impact on the Barangay"))}</h3>
          <div class="impact-row"><span>${esc(txt("Pag-asa", "Reform progress"))}</span><b>${S.barangay.pagAsa}</b></div>
          <div class="impact-bar"><div style="width:${S.barangay.pagAsa}%"></div></div>
          <div class="impact-row"><span>${esc(txt("Tiwala", "Trust"))}</span><b>${S.barangay.tiwala}</b></div>
          <div class="impact-bar tiwala"><div style="width:${S.barangay.tiwala}%"></div></div>
          <div class="impact-row"><span>${esc(txt("Walang Kibo", "Silent ones"))}</span><b>${S.barangay.walangKibo}</b></div>
          <p class="impact-fact">${esc(txt(
            `${pct}% ng mga manlalaro sa barangay na ito ang nagsalita. Ang tipping point ay 40%.`,
            `${pct}% of players in this barangay spoke up. The tipping point is 40%.`
          ))}</p>
          ${contrib && contrib.crossed60 ? `<p class="era-event">🌅 ${esc(txt("Tumaas ang Pag-asa sa 60! Ang bayan ay Gumigising.", "Reform progress crossed 60! The town is Awakening."))}</p>` : ""}
          <span class="era-pill era-${era.id}">${esc(txt(era.name, era.name_en))}</span>
        </div>
        <div class="ledger">
          <h3>${esc(txt("Ang Ledger ng Utang", "The Debt Ledger"))}</h3>
          <ul>${ledger}</ul>
        </div>
        <div class="final-ask">
          <h3>${esc(txt("Ang Susunod na Hakbang", "The Next Step"))}</h3>
          <p>${esc(txt(ask.text, ask.text_en))}</p>
          <div class="ask-picker">
            ${C.finalAsks.map((a) => `<button class="ask-btn ${S.settings.location === a.id ? "sel" : ""}" data-ask="${a.id}">${esc(txt(a.label, a.label_en))}</button>`).join("")}
          </div>
        </div>
        <div class="echo-leave">
          <h3>${esc(txt("Iwan ang isang Echo", "Leave an Echo"))}</h3>
          <p class="hint">${esc(txt("Isang pre-written echo mula sa nakaraang manlalaro, +1d6 sa susunod na run.", "A pre-written echo from an earlier player, +1d6 in the next run."))}</p>
          <div class="echo-grid">
            ${C.echoes.map((ec) => `<button class="echo-leave-btn" data-echo="${ec.id}">${esc(txt(ec.text, ec.text_en))}</button>`).join("")}
          </div>
        </div>
        <div class="row">
          <button class="btn" data-act="menu">${esc(txt("🏠 Menu", "🏠 Menu"))}</button>
          <button class="btn primary big" data-act="play-again">${esc(txt("🔄 Maglaro ulit", "🔄 Play again"))}</button>
        </div>
      </div>`);
  }

  // ---------- info screens ----------
  function showBarangay() {
    const b = S.barangay;
    const era = BN.eraOf(b);
    const pct = BN.spokeUpPct(b);
    setScreen("barangay", `
      <div class="screen info">
        <h2>${esc(txt("Barangay Bayanihan", "Barangay Bayanihan"))}</h2>
        <p class="sub">${esc(txt("Bayan ng San Isidro (fictional)", "Town of San Isidro (fictional)"))}</p>
        <img class="hub-img" src="assets/hub.png" alt="Barangay hub" onerror="this.style.display='none'">
        <span class="era-pill era-${era.id}">${esc(txt(era.name, era.name_en))}</span>
        <p class="era-desc">${esc(txt(era.desc, era.desc_en))}</p>
        <div class="impact">
          <div class="impact-row"><span>${esc(txt("Tiwala", "Trust"))}</span><b>${b.tiwala}</b></div>
          <div class="impact-bar tiwala"><div style="width:${b.tiwala}%"></div></div>
          <div class="impact-row"><span>${esc(txt("Imprastruktura", "Infrastructure"))}</span><b>${b.imprastruktura}</b></div>
          <div class="impact-bar imprastra"><div style="width:${b.imprastruktura}%"></div></div>
          <div class="impact-row"><span>${esc(txt("Pag-asa", "Reform progress"))}</span><b>${b.pagAsa}</b></div>
          <div class="impact-bar"><div style="width:${b.pagAsa}%"></div></div>
          <div class="impact-row"><span>${esc(txt("Walang Kibo", "Silent ones"))}</span><b>${b.walangKibo}</b></div>
        </div>
        <div class="fact-grid">
          <div class="fact"><b>${b.runs}</b><span>${esc(txt("nakumpleto", "runs completed"))}</span></div>
          <div class="fact"><b>${pct}%</b><span>${esc(txt("nagsalita", "spoke up"))}</span></div>
          <div class="fact"><b>${b.echoes.length}</b><span>${esc(txt("mga echo", "echoes"))}</span></div>
        </div>
        <p class="hint">${esc(txt("Ang bawat run ay permanenteng nagbabago sa barangay na ito. (Demo: naka-local lang; sa tunay na laro, Firestore.)", "Every run permanently changes this barangay. (Demo: local only; in the real game, Firestore.)"))}</p>
        <button class="btn" data-act="menu">${esc(txt("← Bumalik", "← Back"))}</button>
      </div>`);
  }

  function showEchoes() {
    setScreen("echoes", `
      <div class="screen info">
        <h2>${esc(txt("Mga Echo", "Echoes"))}</h2>
        <p class="sub">${esc(txt("Pre-written, anonymous. +1d6 bawat isa, max 2 bawat roll. Gamitin sa Ch4.", "Pre-written, anonymous. +1d6 each, max 2 per roll. Used in Ch4."))}</p>
        <ul class="echo-list">
          ${S.barangay.echoes.map((id) => {
            const e = C.echoes.find((x) => x.id === id);
            return e ? `<li>${esc(txt(e.text, e.text_en))}</li>` : "";
          }).join("")}
        </ul>
        <button class="btn" data-act="menu">${esc(txt("← Bumalik", "← Back"))}</button>
      </div>`);
  }

  function showHowto() {
    setScreen("howto", `
      <div class="screen info">
        <h2>${esc(txt("Paano Maglaro", "How to Play"))}</h2>
        <div class="howto">
          <p>🎲 ${esc(txt("Roll = d20 + stat modifier + assists vs. DC. Natural 20 = kritikal. Natural 1 = malaking consequance, pero hindi huli ang lahat. Walang partial success.", "Roll = d20 + stat modifier + assists vs. DC. Natural 20 = critical. Natural 1 = a bigger cost, but never a dead end. No partial success."))}</p>
          <p>🕯️ ${esc(txt("Padrino (temptation): auto-success, maliit na cost ngayon (Integridad −1), pero Utang na Loob +1/+2. Hindi ipinapakita ang bilang — puro hint lang.", "Padrino (temptation): auto-success, small cost now (Integrity −1), but hidden Debt +1/+2. Never shown as a number — only hints."))}</p>
          <p>📣 ${esc(txt("Singil: kapag Utang ≥ 3 sa pagpasok ng Ch3, may kard na kokolekta. Pumirma (Utang −2), tumanggi (Integridad DC 10+Utang), o humingi ng tulong.", "Singil: when Debt ≥ 3 at Ch3, a debt-collection card appears. Sign (Debt −2), refuse (Integrity DC 10+Debt), or ask for help."))}</p>
          <p>🤝 ${esc(txt("Kasama: mula sa Komunidad (≥3 → 1, ≥6 → 2). Bawat isa: +1d4, max 2 bawat roll.", "Kasama: from Community (≥3 → 1, ≥6 → 2). Each: +1d4, max 2 per roll."))}</p>
          <p>🏘️ ${esc(txt("Bayanihan: samahan ang kapwa. Ang mga echo ng ibang manlalaro: +1d6, max 2 bawat roll, gamitin sa Ch4.", "Bayanihan: collective action. Other players' echoes: +1d6, max 2 per roll, used in Ch4."))}</p>
          <p>🌊 ${esc(txt("Ch4: ang boses mo ang gasolina ng cascade. Umabot sa Silent Majority (4) → reporma.", "Ch4: your voices fuel the cascade. Reach the Silent Majority (4) → reform."))}</p>
          <p>🌅 ${esc(txt("Ang bawat run ay nagpapabago sa Shared Barangay. Ang era (Mabulok → Bagong Umaga) ay nagbabago ng DC.", "Every run changes the Shared Barangay. The era (Rotten → New Morning) changes DCs."))}</p>
        </div>
        <button class="btn" data-act="menu">${esc(txt("← Bumalik", "← Back"))}</button>
      </div>`);
  }

  function showSettings() {
    const s = S.settings;
    const toggle = (key, label, labelEn) => `
      <label class="set-row"><span>${esc(txt(label, labelEn))}</span>
        <button class="switch ${s[key] ? "on" : ""}" data-set="${key}"><span class="knob"></span></button>
      </label>`;
    setScreen("settings", `
      <div class="screen info">
        <h2>${esc(txt("Settings", "Settings"))}</h2>
        <label class="set-row"><span>${esc(txt("Wika / Language", "Wika / Language"))}</span>
          <select class="select" data-set="lang">
            <option value="tl" ${s.lang === "tl" ? "selected" : ""}>Taglish</option>
            <option value="en" ${s.lang === "en" ? "selected" : ""}>English</option>
          </select>
        </label>
        ${toggle("sfx", "Sound FX", "Sound FX")}
        ${toggle("vibration", "Vibration", "Vibration")}
        <label class="set-row"><span>${esc(txt("Dice animation", "Dice animation"))}</span>
          <select class="select" data-set="dice">
            <option value="full" ${s.dice === "full" ? "selected" : ""}>${esc(txt("Buong", "Full"))}</option>
            <option value="fast" ${s.dice === "fast" ? "selected" : ""}>${esc(txt("Mabilis", "Fast"))}</option>
            <option value="off" ${s.dice === "off" ? "selected" : ""}>${esc(txt("Off", "Off"))}</option>
          </select>
        </label>
        <label class="set-row"><span>${esc(txt("Text size", "Text size"))}</span>
          <select class="select" data-set="textSize">
            <option value="standard" ${s.textSize === "standard" ? "selected" : ""}>${esc(txt("Standard", "Standard"))}</option>
            <option value="large" ${s.textSize === "large" ? "selected" : ""}>${esc(txt("Malaki", "Large"))}</option>
          </select>
        </label>
        ${toggle("contrast", "High contrast / colorblind", "High contrast / colorblind")}
        <label class="set-row"><span>${esc(txt("Location (para sa final ask)", "Location (for the final ask)"))}</span>
          <select class="select" data-set="location">
            ${C.finalAsks.map((a) => `<option value="${a.id}" ${s.location === a.id ? "selected" : ""}>${esc(txt(a.label, a.label_en))}</option>`).join("")}
          </select>
        </label>
        <button class="btn danger" data-act="reset">${esc(txt("🗑️ I-reset ang progreso", "🗑️ Reset progress"))}</button>
        <button class="btn" data-act="menu">${esc(txt("← Bumalik", "← Back"))}</button>
      </div>`);
  }

  function showAbout() {
    setScreen("about", `
      <div class="screen info">
        <h2>${esc(txt("Tungkol", "About"))}</h2>
        <div class="howto">
          <p><b>BAHALA NA?</b> ${esc(txt("— isang maikling, paulit-ulit, dice-driven na kuwento ng buhay na nasa isang fictional na barangay. Lumaki ka mula bata hanggang tanda sa pamamagitan ng pagpili ng mga card; bawat mahirap na sandali ay d20 roll.", "— a short, replayable, dice-driven life story set in a fictional Philippine barangay. You grow from childhood to adulthood by choosing cards; each hard moment is a d20 roll."))}</p>
          <p>${esc(txt("Ang core message (hindi sinasabi hanggang sa huli): ang bahala na ay maaaring 'bahala na' o 'tumalon.' Ang kapalaran ay nag-iiba kapag sabay kayong nagro-roll.", "The core message (never stated until the end): bahala na can mean 'leave it to fate' or 'take the leap.' Fate looks different when people roll together."))}</p>
          <p>${esc(txt("Demo na ito ng buong laro: 40 na card, 10 ending, Shared Barangay (local), echoes, career, Singil. Ang tunay na bersyon: Unity 6.3 + Firebase (Firestore, Cloud Functions).", "This is a full-game demo: 40 cards, 10 endings, Shared Barangay (local), echoes, careers, Singil. The real version: Unity 6.3 + Firebase (Firestore, Cloud Functions)."))}</p>
          <p class="hint">${esc(txt("Privacy: ang lahat ng data ay nasa device mo lang sa demo na ito.", "Privacy: all data stays on your device in this demo."))}</p>
        </div>
        <button class="btn" data-act="menu">${esc(txt("← Bumalik", "← Back"))}</button>
      </div>`);
  }

  function showStats() {
    const r = S.run;
    if (!r) return;
    const hint = BN.utangHint(r, S.settings.lang);
    openOverlay(`
      <div class="panel">
        <h3>📊 ${esc(txt("Stats", "Stats"))}</h3>
        ${BN.STAT_KEYS.map((k) => {
          const i = BN.STAT_INFO[k];
          return `<div class="stat-line"><span>${i.icon} ${esc(txt(i.name, i.name_en))}</span><b>${r.stats[k]}</b><span class="stat-mod">+${BN.mod(r.stats[k])}</span></div>`;
        }).join("")}
        <div class="stat-line"><span>🤝 Kasama</span><b>${BN.kasamaAvailable(r)}</b></div>
        <div class="stat-line"><span>📣 Echo</span><b>${BN.echoAvailable(r, S.barangay)}</b></div>
        ${hint ? `<p class="hint">🕯️ ${esc(hint)}</p>` : `<p class="hint">${esc(txt("Walang nararamdamang utang.", "No debt you can feel."))}</p>`}
        <p class="hint">${esc(txt("Modifier: 0–1→+0 · 2–3→+1 · 4–5→+2 · 6–7→+3 · 8–10→+4", "Modifier: 0–1→+0 · 2–3→+1 · 4–5→+2 · 6–7→+3 · 8–10→+4"))}</p>
        <button class="btn primary" data-act="close-overlay">${esc(txt("Isara", "Close"))}</button>
      </div>`);
  }

  function openOverlay(html) {
    closeOverlay();
    const o = document.createElement("div");
    o.className = "overlay";
    o.id = "overlay";
    o.innerHTML = html;
    document.body.appendChild(o);
    o.addEventListener("click", (ev) => { if (ev.target === o) closeOverlay(); });
  }
  function closeOverlay() { const o = $("#overlay"); if (o) o.remove(); }

  // ---------- events ----------
  function bind() {
    document.body.addEventListener("click", (ev) => {
      const t = ev.target.closest("[data-act],[data-choice],[data-career],[data-ask],[data-echo],[data-set],[data-pres]");
      if (!t) return;
      const act = t.dataset.act;
      const choice = t.dataset.choice;
      if (t.dataset.pres) {
        ui.spark.presentation = t.dataset.pres;
        sfx("click");
        showSpark();
        return;
      }
      if (choice != null) { chooseOption(parseInt(choice, 10)); return; }
      if (t.dataset.career) { pickCareer(t.dataset.career); return; }
      if (t.dataset.ask) {
        S.settings.location = t.dataset.ask;
        save();
        sfx("click");
        showEnding();
        return;
      }
      if (t.dataset.echo) {
        const id = t.dataset.echo;
        if (!S.barangay.echoes.includes(id)) S.barangay.echoes.push(id);
        save();
        sfx("voice");
        vibrate(40);
        showEnding();
        return;
      }
      if (t.dataset.set) return; // handled by change listener below
      switch (act) {
        case "play": sfx("click"); ui.spark = null; showSpark(); break;
        case "play-again": sfx("click"); S.run = null; save(); ui.spark = null; showSpark(); break;
        case "continue": sfx("click"); if (S.run && !S.run.ended) { if (S.run.chapter === 0) showChapterIntro(1); else showGame(); } else { showMenu(); } break;
        case "barangay": sfx("click"); showBarangay(); break;
        case "echoes": sfx("click"); showEchoes(); break;
        case "howto": sfx("click"); showHowto(); break;
        case "settings": sfx("click"); showSettings(); break;
        case "about": sfx("click"); showAbout(); break;
        case "menu": sfx("click"); showMenu(); break;
        case "roll-bg": {
          sfx("roll"); vibrate(30);
          const roll = BN.d(6);
          ui.spark.rolled = true;
          ui.spark.bg = C.backgrounds.find((b) => b.roll === roll);
          const name = ($("#spark-name") || {}).value || "";
          ui.spark.name = name;
          showSpark();
          break;
        }
        case "start-run": {
          S.profile = { name: ui.spark.name || "Bata", presentation: ui.spark.presentation };
          S.run = BN.newRun(S.profile, S.barangay);
          save();
          sfx("good");
          showChapterIntro(1);
          break;
        }
        case "begin-chapter":
          sfx("click");
          if (S.run.chapter === 0) BN.startChapter(S.run, 1, S.barangay);
          save();
          showGame();
          break;
        case "back-choice": sfx("click"); showGame(); break;
        case "do-roll": {
          const { card, choice: ch } = ui.pending;
          const res = BN.computeRoll(S.run, card, ch, S.barangay, ui.rollOpts);
          animateD20(res, () => {
            const out = BN.resolveChoice(S.run, card, ch, res, S.barangay);
            save();
            showOutcome(card, ch, res, out);
          });
          break;
        }
        case "next-card": nextCard(); break;
        case "stats": sfx("click"); showStats(); break;
        case "close-overlay": closeOverlay(); break;
        case "quit": sfx("click"); save(); showMenu(); break;
        case "reset": {
          if (t.dataset.arm) {
            localStorage.removeItem(KEYS.run);
            localStorage.removeItem(KEYS.barangay);
            S.run = null;
            S.barangay = BN.defaultBarangay();
            save();
            sfx("bad");
            showMenu();
          } else {
            t.dataset.arm = "1";
            t.textContent = txt("⚠️ Sigurado ka? I-tap ulit.", "⚠️ Sure? Tap again.");
            setTimeout(() => { delete t.dataset.arm; t.textContent = txt("🗑️ I-reset ang progreso", "🗑️ Reset progress"); }, 2500);
          }
          break;
        }
      }
    });
    document.body.addEventListener("change", (ev) => {
      const t = ev.target.closest("[data-set]");
      if (!t) return;
      const k = t.dataset.set;
      S.settings[k] = t.type === "checkbox" ? t.checked : t.value;
      if (k === "textSize") document.body.classList.toggle("large-text", S.settings.textSize === "large");
      if (k === "contrast") document.body.classList.toggle("high-contrast", !!S.settings.contrast);
      save();
      sfx("click");
      if (k === "lang") rerender();
    });
    document.body.addEventListener("click", (ev) => {
      const t = ev.target.closest(".switch");
      if (!t) return;
      const k = t.dataset.set;
      S.settings[k] = !S.settings[k];
      t.classList.toggle("on", S.settings[k]);
      if (k === "contrast") document.body.classList.toggle("high-contrast", !!S.settings.contrast);
      save();
      sfx("click");
    });
  }

  function chooseOption(i) {
    const card = BN.currentCard(S.run);
    if (!card) return;
    const choice = card.choices[i];
    ui.pending = { card, choice, idx: i };
    sfx("click");
    if (choice.roll) showPreRoll();
    else {
      const out = BN.resolveChoice(S.run, card, choice, null, S.barangay);
      save();
      vibrate(20);
      showOutcome(card, choice, null, out);
    }
  }

  function applyBodySettings() {
    document.body.classList.toggle("large-text", S.settings.textSize === "large");
    document.body.classList.toggle("high-contrast", !!S.settings.contrast);
  }

  function rerender() {
    switch (ui.screen) {
      case "menu": showMenu(); break;
      case "spark": showSpark(); break;
      case "career": showCareer(); break;
      case "game": showGame(); break;
      case "ending": showEnding(); break;
      case "barangay": showBarangay(); break;
      case "echoes": showEchoes(); break;
      case "howto": showHowto(); break;
      case "settings": showSettings(); break;
      case "about": showAbout(); break;
      default: break;
    }
  }

  function init() {
    load();
    applyBodySettings();
    bind();
    showMenu();
  }

  window.UI = { init, get S() { return S; }, set S(v) { S = v; } };
})();
