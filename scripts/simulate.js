/* Headless balance/crash simulation for the Bahala Na? engine.
 * Run: node scripts/simulate.js [runs]
 */
"use strict";
const fs = require("fs");
const path = require("path");
const vm = require("vm");

const ROOT = path.dirname(__dirname);
const sandbox = { window: {}, console };
sandbox.global = sandbox;
vm.createContext(sandbox);
vm.runInContext(fs.readFileSync(path.join(ROOT, "content/cards-bundle.js"), "utf8"), sandbox);
vm.runInContext(fs.readFileSync(path.join(ROOT, "js/engine.js"), "utf8"), sandbox);
const BN = sandbox.window.BN;

function playRun(barangay, smart) {
  const profile = { name: "Test", presentation: "neutral" };
  const run = BN.newRun(profile, barangay);
  BN.startChapter(run, 1, barangay);
  let guard = 0;
  const singilSeen = run.utang >= 3;
  while (!run.ended && guard++ < 100) {
    const card = BN.currentCard(run);
    if (!card) {
      if (run.chapter === 1) { BN.startChapter(run, 2, barangay); }
      else if (run.chapter === 2) {
        const opts = BN.careerOptions(run);
        run.career = opts[Math.floor(Math.random() * opts.length)];
        BN.startChapter(run, 3, barangay);
      } else if (run.chapter === 3) { BN.startChapter(run, 4, barangay); }
      else {
        run.endingId = BN.determineEnding(run);
        run.ended = true;
        BN.applyContribution(barangay, run);
      }
      continue;
    }
    // choose: smart = prefer collective/honest, else random; 60% padrino on first run
    let idx;
    if (smart) {
      const pref = card.choices.findIndex((c) => c.type === "collective");
      const hon = card.choices.findIndex((c) => c.type === "honest");
      idx = pref >= 0 && Math.random() < 0.7 ? pref : hon >= 0 ? hon : 0;
      if (run.chapter === 4) {
        const ci = card.choices.findIndex((c) => c.type === "collective" || c.type === "honest");
        idx = ci >= 0 ? ci : 0;
      }
    } else {
      idx = Math.floor(Math.random() * card.choices.length);
    }
    const choice = card.choices[idx];
    if (choice.roll) {
      const opts = {
        kasama: choice.roll.kasama && BN.kasamaAvailable(run) > 0,
        echoes: choice.roll.echoes && BN.echoAvailable(run, barangay) > 0,
      };
      const res = BN.computeRoll(run, card, choice, barangay, opts);
      BN.resolveChoice(run, card, choice, res, barangay);
    } else {
      BN.resolveChoice(run, card, choice, null, barangay);
    }
    run.cardIndex += 1;
  }
  return { run, singilSeen };
}

function main() {
  const N = parseInt(process.argv[2] || "2000", 10);
  const endings = {};
  let spokeFirst = 0, joined = 0, silent = 0, utangHeavy = 0, utangSum = 0, reform = 0;
  const careers = {};
  const barangay = BN.defaultBarangay();
  let crash = null;
  for (let i = 0; i < N; i++) {
    try {
      const smart = i % 2 === 0;
      const { run } = playRun(barangay, smart);
      endings[run.endingId] = (endings[run.endingId] || 0) + 1;
      careers[run.career] = (careers[run.career] || 0) + 1;
      if (run.spokeFirst) spokeFirst++;
      if (run.joinedCascade) joined++;
      if (run.silentClimax || (!run.spokeFirst && !run.joinedCascade)) silent++;
      if (run.utang >= 5) utangHeavy++;
      utangSum += run.utang;
      if (run.reform) reform++;
      if (run.cardIndex === 0) throw new Error("no cards played");
      for (const [k, v] of Object.entries(run.stats)) {
        if (v < 0 || v > 10 || !Number.isFinite(v)) throw new Error(`stat out of range: ${k}=${v}`);
      }
      if (run.utang < 0 || !Number.isFinite(run.utang)) throw new Error("utang out of range");
    } catch (e) {
      crash = e;
      break;
    }
  }
  if (crash) {
    console.error("CRASH on run:", crash.stack);
    process.exit(1);
  }
  const pct = (x) => `${((x / N) * 100).toFixed(1)}%`;
  console.log(`runs: ${N}`);
  console.log("endings:", Object.entries(endings).map(([k, v]) => `#${k}:${v} (${pct(v)})`).join("  "));
  console.log("careers:", Object.entries(careers).map(([k, v]) => `${k}:${v}`).join("  "));
  console.log(`spokeFirst: ${pct(spokeFirst)}  joined: ${pct(joined)}  silent: ${pct(silent)}`);
  console.log(`reform: ${pct(reform)}  utang>=5: ${pct(utangHeavy)}  avgUtang: ${(utangSum / N).toFixed(2)}`);
  console.log("barangay after:", JSON.stringify({ pagAsa: barangay.pagAsa, tiwala: barangay.tiwala, walangKibo: barangay.walangKibo, runs: barangay.runs, era: BN.eraOf(barangay).id, pct: BN.spokeUpPct(barangay) }));
}

main();
