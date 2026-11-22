"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const { applyContribution } = require("../lib/contribute");
const { defaultBarangayState } = require("../lib/era");

function input(over = {}) {
  return {
    runId: "run_test_1",
    background: "mahirap",
    career: "citizen",
    endingId: 9,
    spokeUp: false,
    joinedCascade: false,
    silent: false,
    utangFinal: 0,
    pendingTiwala: 0,
    ...over,
  };
}

test("speak up → +4 Pag-asa, version++", () => {
  const { next, summary, crossed60 } = applyContribution(defaultBarangayState(), input({ spokeUp: true }));
  assert.equal(summary.pagAsa, 4);
  assert.equal(summary.spoke, true);
  assert.equal(next.pagAsa, 49);
  assert.equal(next.version, 1);
  assert.equal(crossed60, false);
});

test("join cascade → +2 Pag-asa", () => {
  const { next, summary } = applyContribution(defaultBarangayState(), input({ joinedCascade: true }));
  assert.equal(summary.pagAsa, 2);
  assert.equal(next.pagAsa, 47);
});

test("silent → Walang Kibo +1, Tiwala −1", () => {
  const { next, summary } = applyContribution(defaultBarangayState(), input({ silent: true }));
  assert.equal(summary.walangKibo, 1);
  assert.equal(summary.pagAsa, 0);
  assert.equal(next.walangKibo, 1);
  assert.equal(next.tiwala, 59);
});

test("padrino-heavy (Utang ≥ 5) → Tiwala −3", () => {
  const { next } = applyContribution(defaultBarangayState(), input({ spokeUp: true, utangFinal: 6 }));
  assert.equal(next.tiwala, 60 - 3);
});

test("crossing 60 fires the town event", () => {
  const prev = { ...defaultBarangayState(), pagAsa: 58 };
  const { next, crossed60 } = applyContribution(prev, input({ spokeUp: true }));
  assert.equal(crossed60, true);
  assert.equal(next.era, "gumigising");
});

test("era recompute from Pag-asa", () => {
  const { next } = applyContribution({ ...defaultBarangayState(), pagAsa: 99 }, input({ spokeUp: true }));
  assert.equal(next.era, "bagong_umaga");
  assert.equal(next.pagAsa, 100); // clamped
});

test("pendingTiwala applies and clamps at 0/100", () => {
  const { next } = applyContribution({ ...defaultBarangayState(), tiwala: 1 }, input({ silent: true, pendingTiwala: -5 }));
  assert.equal(next.tiwala, 0);
});

test("server derives silence when neither flag set", () => {
  const { summary } = applyContribution(defaultBarangayState(), input({ silent: false }));
  assert.equal(summary.walangKibo, 1);
});
