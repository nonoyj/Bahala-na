/**
 * Eras and Shared Barangay defaults (HANDOFF §B.13 / §C.4, 10B).
 * Pure functions — unit-tested in test/era.test.js.
 */
import { BarangayState, Era } from "./types";

export interface EraDef {
  id: Era;
  min: number;
  max: number;
  dcMod: number; // applied to systemic DCs
}

export const ERA_TABLE: EraDef[] = [
  { id: "mabulok", min: 0, max: 29, dcMod: 2 },
  { id: "karaniwan", min: 30, max: 59, dcMod: 0 },
  { id: "gumigising", min: 60, max: 84, dcMod: -2 },
  { id: "bagong_umaga", min: 85, max: 100, dcMod: -2 },
];

export function clamp(n: number, min: number, max: number): number {
  if (Number.isNaN(n)) return min;
  return Math.max(min, Math.min(max, n));
}

export function eraForPagAsa(pagAsa: number): Era {
  const v = clamp(Math.round(pagAsa), 0, 100);
  const def = ERA_TABLE.find((e) => v >= e.min && v <= e.max);
  return def ? def.id : "karaniwan";
}

export function dcModForEra(era: Era): number {
  return ERA_TABLE.find((e) => e.id === era)?.dcMod ?? 0;
}

/** Seed state so the demo has history (§C.4 seeding). */
export function defaultBarangayState(): BarangayState {
  return {
    tiwala: 60,
    imprastruktura: 35,
    pagAsa: 45, // Karaniwan
    walangKibo: 0,
    era: "karaniwan",
    version: 0,
  };
}
