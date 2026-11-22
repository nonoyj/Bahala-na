"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const { eraForPagAsa, dcModForEra, clamp, defaultBarangayState, ERA_TABLE } = require("../lib/era");

test("era boundaries", () => {
  assert.equal(eraForPagAsa(0), "mabulok");
  assert.equal(eraForPagAsa(29), "mabulok");
  assert.equal(eraForPagAsa(30), "karaniwan");
  assert.equal(eraForPagAsa(59), "karaniwan");
  assert.equal(eraForPagAsa(60), "gumigising");
  assert.equal(eraForPagAsa(84), "gumigising");
  assert.equal(eraForPagAsa(85), "bagong_umaga");
  assert.equal(eraForPagAsa(100), "bagong_umaga");
});

test("era DC modifiers", () => {
  assert.equal(dcModForEra("mabulok"), 2);
  assert.equal(dcModForEra("karaniwan"), 0);
  assert.equal(dcModForEra("gumigising"), -2);
  assert.equal(dcModForEra("bagong_umaga"), -2);
});

test("clamp + seed", () => {
  assert.equal(clamp(150, 0, 100), 100);
  assert.equal(clamp(-5, 0, 100), 0);
  const seed = defaultBarangayState();
  assert.equal(seed.pagAsa, 45);
  assert.equal(seed.era, "karaniwan");
  assert.equal(seed.version, 0);
});

test("era table covers 0..100", () => {
  for (let v = 0; v <= 100; v++) {
    const era = eraForPagAsa(v);
    const def = ERA_TABLE.find((e) => e.id === era);
    assert.ok(def && v >= def.min && v <= def.max, `value ${v} mapped wrong`);
  }
});
