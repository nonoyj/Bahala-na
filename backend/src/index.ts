/**
 * Cloud Functions entry point.
 * Deploy: `npm run deploy` (from backend/).
 */
import { initializeApp } from "firebase-admin/app";

initializeApp();

export { submitRunContribution } from "./submitRunContribution";
