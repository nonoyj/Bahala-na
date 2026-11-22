/* Bahala Na? — game engine (pure logic, no DOM).
 * Implements Full Handoff §B.5-§B.14, §C.3-§C.5.
 * Exposed as window.BN.
 */
(function () {
  "use strict";

  const C = window.BAHALANA_CONTENT;
  const CARDS = {};
  C.cards.forEach((c) => { CARDS[c.id] = c; });

  const STAT_KEYS = ["talino", "diskarte", "pera", "koneksyon", "integridad", "komunidad"];
  const STAT_INFO = {
    talino:     { name: "Talino",     name_en: "Wits",        icon: "🧠", desc: "Kaalaman, edukasyon, kritikal na pag-iisip." },
    diskarte:   { name: "Diskarte",   name_en: "Resourcefulness", icon: "🛠️", desc: "Tuso, matatag, marunong makibagay." },
    pera:       { name: "Pera",       name_en: "Money",       icon: "💰", desc: "Pera at materyal na seguridad." },
    koneksyon:  { name: "Koneksyon",  name_en: "Connections", icon: "🤝", desc: "Kilala mo, clout, padrino." },
    integridad: { name: "Integridad", name_en: "Integrity",   icon: "⚖️", desc: "Moral na kompas, reputasyon sa katapatan." },
    komunidad:  { name: "Komunidad",  name_en: "Community",   icon: "🏘️", desc: "Tunay na samahan, kapwa, bayanihan." },
  };

  const CHAPTER_INFO = {
    1: { name: "Kabataan", name_en: "Childhood", ages: "~8–12", cards: 4 },
    2: { name: "Sangandaan", name_en: "Crossroads", ages: "~16–22", cards: 5 },
    3: { name: "Tungkulin", name_en: "Duty", ages: "~25–35", cards: 6 },
    4: { name: "Ang Pagsubok", name_en: "The Test", ages: "~35+", cards: 3 },
  };

  const CAREERS = {
    official:  { name: "Opisyal / Barangay-City Official", name_en: "Barangay/City Official", icon: "🏛️" },
    police:    { name: "Pulis", name_en: "Police Officer", icon: "🛡️" },
    teacher:   { name: "Guro / Municipal Engineer", name_en: "Teacher / Municipal Engineer", icon: "📐" },
    organizer: { name: "Community Organizer / Mamamahayag", name_en: "Community Organizer / Journalist", icon: "📣" },
    citizen:   { name: "Ordinaryong Mamamayan", name_en: "Ordinary Working Citizen", icon: "🧑‍🌾" },
  };

  const GROUPS = [
    { id: "brave",   threshold: 0, name: "Ang Matapang (Nurse Lea)", name_en: "The Brave (Nurse Lea)", icon: "✊" },
    { id: "angry",   threshold: 1, name: "Mga Galit na Kapitbahay", name_en: "The Angry Neighbors", icon: "🔥" },
    { id: "worried", threshold: 2, name: "Mga Nag-aalalang Magulang", name_en: "The Worried Parents", icon: "👪" },
    { id: "vendors", threshold: 3, name: "Mga Maingat na Vendedor", name_en: "The Cautious Vendors", icon: "🧺" },
    { id: "silent",  threshold: 4, name: "Ang Silent Majority", name_en: "The Silent Majority", icon: "🤫" },
  ];

  // ---------- helpers ----------
  function clamp(v, min, max) { return Math.max(min, Math.min(max, v)); }
  function mod(stat) {
    if (stat <= 1) return 0;
    if (stat <= 3) return 1;
    if (stat <= 5) return 2;
    if (stat <= 7) return 3;
    return 4;
  }
  function sample(arr, n) {
    const a = arr.slice();
    for (let i = a.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [a[i], a[j]] = [a[j], a[i]];
    }
    return a.slice(0, Math.min(n, a.length));
  }
  function d(n) { return 1 + Math.floor(Math.random() * n); }

  function eraOf(barangay) {
    const v = clamp(barangay.pagAsa || 0, 0, 100);
    return C.eras.find((e) => v >= e.min && v <= e.max) || C.eras[1];
  }
  function eraDcMod(barangay) { return eraOf(barangay).dc_mod; }

  function defaultBarangay() {
    return {
      tiwala: 60,
      imprastruktura: 35,
      pagAsa: 45,
      walangKibo: 0,
      version: 0,
      runs: 24,
      spokeUp: 9,
      echoes: C.echoes.map((e) => e.id),
      lastContribution: null,
    };
  }

  function defaultSettings() {
    return {
      lang: "tl",
      sfx: true,
      vibration: true,
      dice: "full", // full | fast | off
      textSize: "standard",
      contrast: false,
      location: "general",
    };
  }

  // ---------- run ----------
  function newRun(profile, barangay) {
    const roll = d(6);
    const bg = C.backgrounds.find((b) => b.roll === roll);
    const stats = {};
    STAT_KEYS.forEach((k) => { stats[k] = 3; });
    Object.entries(bg.bonus || {}).forEach(([k, v]) => { stats[k] += v; });
    Object.entries(bg.penalty || {}).forEach(([k, v]) => { stats[k] += v; });
    STAT_KEYS.forEach((k) => { stats[k] = clamp(stats[k], 0, 10); });
    const run = {
      runId: "run_" + Date.now() + "_" + Math.floor(Math.random() * 1e6),
      chapter: 0,
      deck: [],
      cardIndex: 0,
      stats,
      utang: 0,
      flags: [],
      kasama: 0,
      background: bg.id,
      backgroundRoll: roll,
      career: null,
      ledger: [],
      voices: 0,
      voicesAdded: 0,
      cascade: {},
      spokeUp: false,
      spokeFirst: false,
      joinedCascade: false,
      silentClimax: false,
      reform: false,
      complied: false,
      stoodFirm: false,
      padrinoCount: 0,
      pendingTiwala: 0,
      echoesUsed: [],
      startedAt: Date.now(),
      phase: "spark",
    };
    return run;
  }

  function hasFlag(run, f) { return run.flags.includes(f); }

  function reqOk(card, run) {
    const r = card.requires;
    if (!r) return true;
    if (r.any_flags && r.any_flags.some((f) => hasFlag(run, f))) return true;
    if (r.utang_min && run.utang >= r.utang_min) return true;
    return false;
  }

  function pool(name) { return C.cards.filter((c) => c.pool === name); }

  function startChapter(run, chapter, barangay) {
    run.chapter = chapter;
    run.cardIndex = 0;
    let ids = [];
    if (chapter === 1) {
      ids = sample(pool("ch1"), 4).map((c) => c.id);
    } else if (chapter === 2) {
      const p = pool("ch2");
      const eligible = p.filter((c) => reqOk(c, run));
      const chosen = sample(eligible, 5);
      if (chosen.length < 5) {
        const rest = p.filter((c) => !chosen.includes(c));
        chosen.push(...sample(rest, 5 - chosen.length));
      }
      ids = chosen.map((c) => c.id);
    } else if (chapter === 3) {
      const rolePool = pool("ch3_role").filter((c) => c.role === run.career);
      const sharedPool = pool("ch3_shared");
      const roleIds = sample(rolePool, 3).map((c) => c.id);
      const sharedIds = sample(sharedPool, 2).map((c) => c.id);
      let last;
      if (run.utang >= 3) {
        last = sample(pool("ch3_singil"), 1)[0];
      } else {
        const remaining = sharedPool.filter((c) => !sharedIds.includes(c.id));
        last = sample(remaining.length ? remaining : sharedPool, 1)[0];
      }
      ids = [roleIds[0], sharedIds[0], roleIds[1], last.id, sharedIds[1], roleIds[2]];
    } else if (chapter === 4) {
      ids = sample(pool("ch4"), 3).map((c) => c.id);
      run.voices = 0;
      run.voicesAdded = 0;
      run.cascade = {};
      // The Brave (Nurse Lea) speaks at threshold 0 — the first voice.
      run.cascade.brave = true;
      run.voices = 1;
    }
    run.deck = ids;
    run.phase = "chapter";
  }

  function currentCard(run) {
    if (run.cardIndex >= run.deck.length) return null;
    return CARDS[run.deck[run.cardIndex]];
  }

  // ---------- careers (§B.10 / 9B) ----------
  function careerOptions(run) {
    const s = run.stats;
    const opts = [];
    if (s.koneksyon >= 5 && (hasFlag(run, "ran_for_org") || run.utang >= 2)) opts.push("official");
    if (hasFlag(run, "took_exam_or_followed_family_service") && (s.diskarte >= 4 || s.pera <= 3)) opts.push("police");
    if (s.talino >= 5) opts.push("teacher");
    if (s.komunidad >= 5 || hasFlag(run, "spoke_up")) opts.push("organizer");
    if (hasFlag(run, "went_to_work_early")) opts.push("citizen");
    if (opts.length === 0) opts.push("citizen");
    return opts;
  }

  // ---------- cascade (§B.12) ----------
  function addVoices(run, n) {
    if (n <= 0) return [];
    run.voices += n;
    run.voicesAdded += n;
    if (run.chapter === 4 && !run.spokeFirst) run.joinedCascade = true;
    // chain reaction: any group whose threshold is met joins
    const joined = [];
    let changed = true;
    while (changed) {
      changed = false;
      for (const g of GROUPS) {
        if (!run.cascade[g.id] && run.voices >= g.threshold) {
          run.cascade[g.id] = true;
          run.voices += 1;
          joined.push(g.id);
          changed = true;
        }
      }
    }
    return joined;
  }

  // ---------- effects ----------
  function applyEffects(run, effects, barangay) {
    if (!effects) return;
    if (effects.stats) {
      for (const [k, v] of Object.entries(effects.stats)) {
        if (STAT_KEYS.includes(k)) run.stats[k] = clamp(run.stats[k] + v, 0, 10);
      }
    }
    if (effects.utang) {
      run.utang = Math.max(0, run.utang + effects.utang);
      if (effects.utang > 0) {
        run.ledger.push({ kind: "utang", amount: effects.utang, chapter: run.chapter, card: cardTitle(run) });
      }
    }
    if (effects.utangClear) {
      run.utang = 0;
      run.ledger.push({ kind: "cleared", chapter: run.chapter, card: cardTitle(run) });
    }
    if (effects.flags) {
      effects.flags.forEach((f) => { if (!run.flags.includes(f)) run.flags.push(f); });
    }
    if (effects.tiwala) run.pendingTiwala += effects.tiwala;
    if (effects.kasama) run.kasama = Math.max(0, run.kasama + effects.kasama);
    if (effects.voices) addVoices(run, effects.voices);
    if (effects.walangKibo && run.chapter === 4) run.silentClimax = true;
    if (effects.spokeUp) run.spokeUp = true;
    if (effects.spokeFirst) { run.spokeFirst = true; run.spokeUp = true; }
    if (effects.complied) {
      run.complied = true;
      run.ledger.push({ kind: "complied", chapter: run.chapter, card: cardTitle(run) });
    }
    if (effects.reform) run.reform = true;
    if (effects.usesEchoes) {
      for (let i = 0; i < effects.usesEchoes; i++) {
        const avail = barangay.echoes.filter((e) => !run.echoesUsed.includes(e));
        if (avail.length) run.echoesUsed.push(avail[0]);
      }
    }
  }

  function cardTitle(run) {
    const card = currentCard(run);
    return card ? card.title : "";
  }

  // ---------- dice (§B.7) ----------
  function kasamaAvailable(run) {
    const k = run.stats.komunidad;
    return k >= 6 ? 2 : k >= 3 ? 1 : 0;
  }
  function echoAvailable(run, barangay) {
    const total = (barangay.echoes || []).length;
    return Math.max(0, Math.min(2, total - run.echoesUsed.length));
  }

  function computeRoll(run, card, choice, barangay, opts) {
    const r = choice.roll;
    let dc = r.dc;
    if (r.dc_if_flag && hasFlag(run, r.dc_if_flag.flag)) dc = r.dc_if_flag.dc;
    if (r.dc_plus_utang) dc = 10 + run.utang;
    const systemic = !!(r.systemic || card.systemic);
    let eraNote = null;
    if (systemic) {
      const em = eraDcMod(barangay);
      if (em) {
        dc += em;
        const era = eraOf(barangay);
        eraNote = (em > 0 ? "+" : "") + em + " (" + era.name + ")";
      }
    }
    const m = mod(run.stats[r.stat]);
    const kasamaDice = opts.kasama ? Math.min(2, kasamaAvailable(run)) : 0;
    const echoDice = opts.echoes ? echoAvailable(run, barangay) : 0;
    const d20 = d(20);
    let bonus = 0;
    const rolls = [];
    for (let i = 0; i < kasamaDice; i++) { const v = d(4); bonus += v; rolls.push({ kind: "kasama", v }); }
    for (let i = 0; i < echoDice; i++) { const v = d(6); bonus += v; rolls.push({ kind: "echo", v }); }
    const total = d20 + m + bonus;
    const crit = d20 === 20;
    const fumble = d20 === 1;
    const success = crit || (!fumble && total >= dc);
    return { d20, mod: m, bonus, rolls, total, dc, systemic, eraNote, crit, fumble, success, stat: r.stat };
  }

  function resolveChoice(run, card, choice, rollResult, barangay) {
    let effs;
    if (choice.roll && rollResult) {
      effs = rollResult.success ? choice.success.effects : choice.fail.effects;
    } else {
      effs = choice.effects || {};
    }
    if (choice.type === "padrino" || choice.type === "comply") run.padrinoCount += 1;
    if (choice.type === "honest" || choice.type === "collective") run.stoodFirm = true;
    applyEffects(run, effs, barangay);
    return { effects: effs, text: (rollResult ? (rollResult.success ? choice.success : choice.fail) : null) };
  }

  // ---------- endings (§B.14) ----------
  function determineEnding(run) {
    const integ = run.stats.integridad;
    const highInteg = integ >= 6;
    const spoke = run.spokeUp || hasFlag(run, "spoke_up");
    switch (run.career) {
      case "official":
        return highInteg && run.utang <= 2 ? 1 : 2;
      case "police":
        return highInteg && (spoke || run.stoodFirm) ? 3 : 4;
      case "teacher":
        return (run.stoodFirm || spoke) ? 5 : 6;
      case "organizer":
        if (hasFlag(run, "burned_out")) return 8;
        if (run.reform) return 7;
        return spoke ? 7 : 8;
      case "citizen":
      default:
        return (run.joinedCascade || run.reform) ? 9 : 10;
    }
  }

  function redemption(run) {
    return run.stats.komunidad >= 4 && [2, 4, 6, 8, 10].includes(run.endingId);
  }

  // ---------- shared barangay contribution (§B.13 / §C.4) ----------
  function applyContribution(barangay, run) {
    const beforeEra = eraOf(barangay).id;
    const beforePag = barangay.pagAsa;
    let dPag = 0, dTiw = 0, dWK = 0, spoke = false;
    if (run.spokeFirst) { dPag += 4; spoke = true; }
    else if (run.joinedCascade) { dPag += 2; spoke = true; }
    else { dWK += 1; dTiw -= 1; }
    if (run.utang >= 5) dTiw -= 3;
    dTiw += run.pendingTiwala || 0;
    barangay.pagAsa = clamp(barangay.pagAsa + dPag, 0, 100);
    barangay.tiwala = clamp(barangay.tiwala + dTiw, 0, 100);
    barangay.walangKibo += dWK;
    barangay.runs += 1;
    if (spoke) barangay.spokeUp += 1;
    const afterEra = eraOf(barangay).id;
    const crossed60 = beforePag < 60 && barangay.pagAsa >= 60;
    const summary = { pagAsa: dPag, tiwala: dTiw, walangKibo: dWK, spoke, beforeEra, afterEra, crossed60 };
    barangay.lastContribution = summary;
    barangay.version += 1;
    return summary;
  }

  function spokeUpPct(barangay) {
    if (!barangay.runs) return 0;
    return Math.floor((barangay.spokeUp / barangay.runs) * 100);
  }

  function utangHint(run, lang) {
    const u = run.utang;
    if (u <= 0) return null;
    if (u <= 2) return lang === "tl" ? "May binabalakid ka bang utang…" : "You feel a debt hanging over you…";
    if (u <= 4) return lang === "tl" ? "May naghihintay sa'yo." : "Something is waiting for you.";
    return lang === "tl" ? "Mabigat ang nararamdaman mo." : "It feels heavy.";
  }

  window.BN = {
    C, CARDS, STAT_KEYS, STAT_INFO, CHAPTER_INFO, CAREERS, GROUPS,
    clamp, mod, sample, d, eraOf, eraDcMod,
    defaultBarangay, defaultSettings,
    newRun, hasFlag, reqOk, startChapter, currentCard,
    careerOptions, addVoices, applyEffects, kasamaAvailable, echoAvailable,
    computeRoll, resolveChoice, determineEnding, redemption,
    applyContribution, spokeUpPct, utangHint,
  };
})();
