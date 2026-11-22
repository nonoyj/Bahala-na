# BAHALA NA? — Card Content (distributable)

**Place:** Barangay Bayanihan, bayan ng San Isidro (fictional)

**Format:** Card template (Full Handoff §B.15). Tagalog-leaning Taglish.
Every card has a *Huwag makialam* option. No partial success (6B).
Kasama simplified (7B). Echoes pre-written (8B). Career: player picks (9B).

> Canonical card set for frontend (UI) and backend (data model).
> Mirrored into the Full Handoff §E.

---

## Chapter 1: Kabataan

*6-card pool, 4 drawn per run. Ages ~8–12.*

### Card 1 — Pondo ng Klase

`ch1_pondo_01` · Tags: school, integridad, padrino

Setup: *"Iniwan ni Ma'am Lourdes ang kahon ng ambagan sa mesa niya at lumabas saglit. Bumulong si Pia, ang katabi mo: '₱20 lang, babayaran ko bukas. Walang makakaalam.'"*

- **"Huwag. Sabihin natin kay Ma'am."** (Honest) — Integridad DC 8.
  Success: Integridad +1; Pia upset, Ma'am notices.
  Fail: "sumbungera," Komunidad −1.

- **"Sige, isang beses lang."** (Padrino) — No roll.
  Pera +1, Integridad −1, **Utang +1**.

- **"Pabayaan, hindi ko kasalanan."** (Huwag makialam) — No immediate effect.
  Flag `saw_it`.

Callback: `saw_it` → Ch2 Card 3 (Pia asks for a bigger favor).

---

### Card 2 — Baon

`ch1_baon_02` · Tags: school, komunidad, pera

Setup: *"Tanghalian na. Si Rico, ang kaklaseng laging maaga, ay walang baon — nakalimutan daw ng nanay niya. May sobra kang tinapay at itlog. Tumingin siya sa pagkain mo, tapos sa'yo."*

- **"Hati tayo."** (Collective) — Komunidad DC 8.
  Success: Komunidad +1, flag `shared_food`.
  Fail: nahiya siya; `shared_food` still set.

- **"Palit tayo ng meryenda."** (Diskarte) — Diskarte DC 10.
  Success: Pera +1, Komunidad +1.
  Fail: "ayaw ng palit," flag `rico_proud`.

- **"Hindi ko napansin."** (Huwag makialam) — Flag `looked_away`.

Callback: `shared_food` → Ch2 Card 7. `looked_away` → Ch2 Card 7.

---

### Card 3 — Kopya

`ch1_kopya_03` · Tags: school, integridad, utang

Setup: *"May quiz bukas at hindi nakapag-review si Bea, ang kaibigan mong magaling mag-drawing. 'Pahiram ng sagot mo mamaya, isang tingin lang. May utang ako sa'yo, promise.'"*

- **"Hindi pwede. Sabay tayo mag-review."** (Collective) — Talino DC 10.
  Success: Talino +1, Komunidad +1, flag `studied_together`.
  Fail: naubusan ng oras; `studied_together` still set.

- **"Sige, kopyahin mo."** (Padrino) — No roll.
  Integridad −1, **Utang +1**.

- **"Bahala ka."** (Huwag makialam) — Kinopya ka pa rin ni Bea.
  Flag `silent_copy`.

Callback: Utang +1 → Singil seed. `silent_copy` → Ch2 Card 4.

---

### Card 4 — Pirma ni Nanay

`ch1_pirma_04` · Tags: family, padrino, koneksyon

Setup: *"Field trip bukas, pero kailangan ng pirma ng magulang. Nasa abroad si Nanay at si Tatay ay nasa bukid pa. Sabi ni Ate Pia: 'Ako na ang pipirma. Para lang makasama ka.'"*

- **"Salamat, Ate."** (Padrino) — Auto-success.
  **Utang +1**, Integridad −1, Koneksyon +1.

- **"Magpapaalam ako kay Ma'am nang diretso."** (Honest) — Talino DC 10.
  Success: Talino +1.
  Fail: miss trip, Diskarte +1, flag `missed_trip`.

- **"Hindi na lang ako sasama."** (Huwag makialam) — Flag `stayed_home`.

Callback: Utang +1 → Singil. Presentation: Ate Pia addresses you per presentation.

---

### Card 5 — Baha sa Kalye

`ch1_baha_05` · Tags: barangay, komunidad, diskarte

Setup: *"Malakas ang ulan at binaha ang kalye. Si Aling Cora ay nagbubuhat ng mga paninda papasok sa bahay niya, pero hanggang bewang na ang tubig. 'Tulungan mo ako, anak.'"*

- **"Sige po, buhatin natin."** (Collective) — Komunidad DC 10.
  Success: Komunidad +1, flag `helped_cora`.
  Fail: nadulas, Diskarte +1; `helped_cora` still set.

- **"May payong at bota ako, gamitin natin."** (Diskarte) — Diskarte DC 12.
  Success: Pera +1, Komunidad +1.
  Fail: Pera −1 (nabasa gamit).

- **"Umuwi na lang ako."** (Huwag makialam) — Flag `walked_past`.

Callback: `helped_cora` → Ch2 Card 5. `walked_past` → Ch3.

---

### Card 6 — Tinda sa Palengke

`ch1_tinda_06` · Tags: family, padrino, pera

Setup: *"Hapon na at nagsasara na ang pamilya mo ng tindahan sa palengke. Bumulong ang tanod: 'Maliit lang 'to… pang-merienda lang. Para hindi na maabala.'"*

- **"Sige po, pang-merienda."** (Padrino) — No roll.
  Pera +1, **Utang +1**, Integridad −1.

- **"Ayusin namin ang pwesto, ngayon na."** (Honest) — Diskarte DC 12.
  Success: Diskarte +1, no fine.
  Fail: Pera −1, flag `fixed_stall`.

- **"Si Nanay na lang ang bahala."** (Huwag makialam) — Flag `let_nanay`.

Callback: Utang +1 → Singil. `let_nanay` → Ch2 Card 5 / Ch3.

---

## Chapter 2: Sangandaan

*8-card pool, 5 drawn per run. Ages ~16–22. Sets career flags.*

### Card 1 — Scholarship ni Tita Baby

`ch2_scholarship_01` · Tags: education, padrino

Setup: *"Bukas na ang deadline ng scholarship list. Nag-text si Tita Baby: 'Ako na bahala. Isang tawag lang sa kaibigan ko sa munisipyo.'"*

- **"Salamat po, Tita."** (Padrino) — Auto-success.
  **Utang +2**, Integridad −1, Koneksyon +1.

- **"Mag-aapply ako nang patas."** (Honest) — Talino DC 13.
  Success: Talino +1, scholarship earned honestly.
  Fail: lose out, Pera −1.

- **"Magtanong tayo ng kriteria, kasama ang mga kaklase."** (Collective) — Komunidad DC 14 + Kasama.
  Success: Komunidad +2, flag `spoke_up`.
  Fail: nothing changes, but `spoke_up` still set.

- **"Hindi na lang."** (Huwag makialam) — Lose the chance. Pera −1.

Callback: Utang +2 → Singil. `spoke_up` → Organizer career + Ch4.

---

### Card 2 — Halalan sa Org

`ch2_org_02` · Tags: school, koneksyon, politika

Setup: *"Halalan na ng student org. Sabi ni Bea: 'Tumakbo ka. Alam kong mananalo ka.' Pero may narinig kang balita — may nag-aalok ng pera para sa boto."*

- **"Tatakbo ako, patas."** (Honest) — Koneksyon DC 13.
  Success: Koneksyon +1, flag `ran_for_org`.
  Fail: lose, but `ran_for_org` still set; Komunidad +1.

- **"Sabihin kay Kuya Jun, ayusin niya."** (Padrino) — Auto-win.
  **Utang +2**, Integridad −1, flag `ran_for_org`.

- **"Suportahan ko na lang si Bea."** (Collective) — Komunidad DC 12.
  Success: Komunidad +1, flag `supported_friend`.
  Fail: Bea loses, bond remains.

- **"Hindi ako makikialam."** (Huwag makialam) — Flag `sat_out`.

Callback: `ran_for_org` → Politician career. Utang +2 → Singil.

---

### Card 3 — Pia, Mas Malaking Pabor

`ch2_pia_03` · Tags: school, utang, integridad
Requires: flag `saw_it` OR Utang ≥ 1.

Setup: *"Lumapit si Pia, kinakabahan. 'Kailangan kong sabihin kay Ma'am Lourdes na ikaw ang kumuha ng ambagan. Hindi ko kaya… pero ikaw, pwede.'"*

- **"Hindi. Hindi ko 'yan gagawin."** (Honest) — Integridad DC 13.
  Success: Integridad +1; Pia upset but respects you.
  Fail: rumor spreads, Komunidad −1, but you held firm.

- **"Sige, ako na ang magsabi."** (Padrino) — You take the blame.
  **Utang +2**, Integridad −1, Komunidad −1.

- **"Sabihin natin ang totoo, tayong dalawa."** (Collective) — Komunidad DC 14.
  Success: Komunidad +1, flag `told_truth`.
  Fail: Ma'am disappointed in both, but you did right.

- **"Bahala ka na, Pia."** (Huwag makialam) — Flag `left_pia`.

Callback: Utang +2 → Singil. `left_pia` → Ch3.

---

### Card 4 — Bea, Mas Malaking Pabor

`ch2_bea_04` · Tags: school, utang, integridad
Requires: flag `silent_copy` OR Utang ≥ 1.

Setup: *"May malaking exam bukas at hindi pa rin nakapag-review si Bea. 'Isang beses na lang, please. Pag hindi ako pumasa, lagot ako kay Nanay.'"*

- **"Hindi. Pero tutulungan kitang mag-review ngayon."** (Collective) — Talino DC 12.
  Success: Talino +1, Bea passes honestly.
  Fail: you both stay up late, exhausted, but you tried.

- **"Sige, kopya ka na lang."** (Padrino) — Bea copies and passes.
  **Utang +2**, Integridad −1.

- **"Hindi ko 'yan pipirmahan."** (Honest) — Integridad DC 12.
  Success: Integridad +1; Bea hurt but understands.
  Fail: Bea angry, Komunidad −1.

- **"Bahala ka."** (Huwag makialam) — Flag `left_bea`.

Callback: Utang +2 → Singil. `left_bea` → Ch3.

---

### Card 5 — Kuya Jun, ang Fixer

`ch2_fixer_05` · Tags: barangay, padrino, koneksyon

Setup: *"May problema ang pamilya mo — hindi maayos ang papeles sa munisipyo. Lumapit si Kuya Jun, nakangiti. 'Ako na bahala. Kilala ko ang lahat doon.'"*

- **"Salamat, Kuya."** (Padrino) — Auto-success.
  **Utang +2**, Integridad −1, Koneksyon +1 (Kuya Jun becomes a contact).

- **"Aayusin ko nang ako lang."** (Honest) — Diskarte DC 13.
  Success: Diskarte +1, papers fixed.
  Fail: papers delayed, Pera −1, but you learn the process.

- **"Isama ko si Aling Cora, baka alam niya."** (Collective) — Komunidad DC 12 (DC 10 if `helped_cora`).
  Success: Komunidad +1, papers fixed.
  Fail: still delayed, but Aling Cora helps you understand.

- **"Hayaan ko na lang si Nanay."** (Huwag makialam) — Flag `let_nanay2`.

Callback: Utang +2 → Singil. Kuya Jun → recurring fixer (Ch3).

---

### Card 6 — Bilhan ng Boto

`ch2_vote_06` · Tags: family, padrino, politika

Setup: *"Pista sa baryo at may nag-aalok ng ₱500 sa bawat boto. 'Pang-merienda lang 'to,' sabi ni Kuya Jun. 'Wala namang masama.'"*

- **"Sige, isang boto lang."** (Padrino) — Pera +1, Integridad −1, **Utang +1**.

- **"Hindi po, salamat."** (Honest) — Integridad DC 12.
  Success: Integridad +1.
  Fail: Kuya Jun offended, Koneksyon −1.

- **"Sabihin ko sa mga kapitbahay."** (Collective) — Komunidad DC 15.
  Success: 3 neighbors refuse, town Tiwala +1, flag `spoke_up`.
  Fail: awkward, Komunidad −1, but `spoke_up` set.

- **"Hindi ko problema."** (Huwag makialam) — Tracked by Walang Kibo.

Callback: Utang +1 → Singil. `spoke_up` → Organizer career.

---

### Card 7 — Rico, sa Barkada

`ch2_rico_07` · Tags: barkada, komunidad
Requires: flag `looked_away` OR `shared_food`.

Setup: *"Na-ospital ang nanay ni Rico at kailangan ng pera. Sabi ng barkada: 'Tulungan natin siya.' Pero ang iba, ayaw madamay."*

- **"Tulungan natin siya, lahat tayo."** (Collective) — Komunidad DC 13.
  Success: Komunidad +2, Rico's family helped, flag `helped_rico`.
  Fail: you raise some but not enough; Komunidad +1, `helped_rico` still set.

- **"Magbibigay ako ng pera."** (Pera) — No roll.
  Pera −1, Komunidad +1, flag `helped_rico`.

- **"Tumayo ako para kay Rico."** (Honest) — Integridad DC 14.
  Success: Integridad +1, Komunidad +1, flag `spoke_up`.
  Fail: mocked, Komunidad −1, but `spoke_up` still set.

- **"Hindi ko kaya."** (Huwag makialam) — Flag `abandoned_rico`.

Callback: `helped_rico` → Ch3/Ch4 ally. `spoke_up` → Organizer career.

---

### Card 8 — Pagsusulit o Trabaho

`ch2_exam_08` · Tags: crossroads, karera

Setup: *"Dumating ang sangandaan. May pagsusulit para sa serbisyo — o pwede kang magtrabaho agad para tumulong sa pamilya. Hindi pwedeng pareho."*

- **"Kukuha ako ng pagsusulit."** (Honest) — Talino DC 13.
  Success: Talino +1, flag `took_exam_or_followed_family_service`.
  Fail: fail the exam, Diskarte +1, but flag still set.

- **"Sabihin kay Kuya Jun, ayusin ang resulta."** (Padrino) — Auto-pass.
  **Utang +2**, Integridad −1, flag `took_exam_or_followed_family_service`.

- **"Magtrabaho na lang ako."** (Ordinary) — Flag `went_to_work_early`.
  Pera +1 (start earning), Talino −1 (stop studying).

- **"Hindi ko pa alam."** (Huwag makialam) — Flag `undecided`.

Callback: `took_exam_or_followed_family_service` → Police career.
`went_to_work_early` → Ordinary Citizen. Utang +2 → Singil.

---

## Chapter 3: Tungkulin

*22-card pool. 6 drawn per run: 3 role-specific + 2 shared adult + 1 Singil (if Utang ≥ 3). Ages ~25–35.*

### Role-specific — Official (Politician)

#### O1 — Ang Consultant

`ch3_official_consultant_01` · Tags: official, budget

Setup: *"May bagong linya sa budget: 'consultant,' ₱300,000. Walang consultant na nakalista. Sabi ng treasurer: 'Para sa kaibigan ng mayor. Pirma na lang.'"*

- **"Hindi ko pipirmahan."** (Honest) — Integridad DC 15.
  Success: Integridad +1, you block it.
  Fail: pressured, Koneksyon −1, but you refused.

- **"Sige, pirma na."** (Padrino) — **Utang +2**, Integridad −1, Tiwala −2.

- **"Ipa-post natin ang budget sa court."** (Collective) — Komunidad DC 16.
  Success: budget posted, Tiwala +2, `spoke_up`.
  Fail: blocked, but `spoke_up` set.

- **"Hindi ko nakita."** (Huwag makialam) — Flag `ignored_budget`.

Callback: Utang +2 → Singil. `spoke_up` → Ch4.

---

#### O2 — Ang Pabor sa Kaibigan

`ch3_official_friend_02` · Tags: official, padrino

Setup: *"Lumapit ang kaibigan mong si RC Dizon. 'Isang kontrata lang. Hindi naman mahalaga kung sino ang manalo.' Alam mong siya ang magbubuild."*

- **"Hindi. Bidding ang tama."** (Honest) — Integridad DC 14.
  Success: Integridad +1, fair bidding.
  Fail: friend angry, Koneksyon −1.

- **"Sige, ikaw na."** (Padrino) — **Utang +2**, Integridad −1, Tiwala −2 (ghost project seed).

- **"Ipa-bid natin nang bukas."** (Collective) — Komunidad DC 15.
  Success: open bidding, Tiwala +1.
  Fail: rigged anyway, but you tried.

- **"Bahala na sila."** (Huwag makialam) — Flag `let_contract`.

Callback: Utang +2 → Singil. Ghost project → Ch4.

---

#### O3 — Ang Whistleblower

`ch3_official_whistle_03` · Tags: official, whistleblower

Setup: *"May lumapit sa'yo — isang kawani. 'May ghost project. May resibo pero walang ginawa.' May dala siyang ebidensya, pero natatakot siya."*

- **"Tulungan kitang i-expose."** (Collective) — Komunidad DC 16.
  Success: exposed, Tiwala +2, `spoke_up`.
  Fail: retaliation, but `spoke_up` set.

- **"I-report ko nang pribado."** (Honest) — Integridad DC 15.
  Success: Integridad +1, quiet investigation.
  Fail: buried, Koneksyon −1.

- **"Sabihin mo kay Kuya Jun."** (Padrino) — **Utang +1**, Integridad −1.

- **"Wala akong narinig."** (Huwag makialam) — Flag `ignored_whistle`.

Callback: `spoke_up` → Ch4. Ghost project → Ch4.

---

### Role-specific — Police

#### P1 — Ang Checkpoint

`ch3_police_checkpoint_01` · Tags: police, padrino

Setup: *"May nahuli kang driver na walang lisensya. Bumulong siya: 'Settle na lang, sir. Pang-merienda lang.' May dala siyang anak na may sakit."*

- **"Hindi. Ticket ang tama."** (Honest) — Integridad DC 14.
  Success: Integridad +1, by the book.
  Fail: driver begs, you feel it, but held.

- **"Sige, settle na."** (Padrino) — Pera +1, **Utang +1**, Integridad −1.

- **"Tulungan kitang makuha ang lisensya."** (Collective) — Komunidad DC 13.
  Success: Komunidad +1.
  Fail: slow process, but you tried.

- **"Bahala ka na."** (Huwag makialam) — Flag `let_driver`.

Callback: Utang +1 → Singil.

---

#### P2 — Ang Report

`ch3_police_report_02` · Tags: police, pressure

Setup: *"Sabi ng superior mo: 'Palambutin mo ang report. Huwag mong banggitin ang pangalan ng konsehal.' Alam mong may nangyari."*

- **"Hindi ko babaguhin."** (Honest) — Integridad DC 15.
  Success: Integridad +1, report stays true.
  Fail: reprimanded, Koneksyon −1.

- **"Sige, palambutin ko."** (Padrino) — **Utang +1**, Integridad −1, Tiwala −1.

- **"Isama ko ang katotohanan, pero maingat."** (Diskarte) — Diskarte DC 14.
  Success: Diskarte +1, truth in but safe.
  Fail: they notice, Koneksyon −1.

- **"Hindi ko alam."** (Huwag makialam) — Flag `softened_report`.

Callback: Utang +1 → Singil.

---

#### P3 — Ang Reklamo

`ch3_police_harass_03` · Tags: police, justice

Setup: *"May kapitbahay na nagreklamo — hinaharas daw siya ng isang makapangyarihan. 'Takot ako, pero kailangan kong magsalita.'"*

- **"Iimbestigahan ko."** (Honest) — Integridad DC 15.
  Success: Integridad +1, case moves.
  Fail: stalls, but you tried.

- **"Tulungan kitang magsampa nang ligtas."** (Collective) — Komunidad DC 15.
  Success: Komunidad +1, `spoke_up`.
  Fail: threat comes, but `spoke_up` set.

- **"Sabihin mo na lang kay Kap Eddie."** (Huwag makialam) — Flag `passed_complaint`.

- **"Wala akong magagawa."** (Huwag makialam) — Flag `ignored_harass`.

Callback: `spoke_up` → Ch4.

---

### Role-specific — Teacher / Engineer

#### T1 — Ang Semento

`ch3_teacher_cement_01` · Tags: engineer, integrity

Setup: *"Pinapapirma ka sa materyales ng proyekto. Alam mong kulang ang semento — mas maraming buhangin. 'Standard 'yan,' sabi ng contractor."*

- **"Hindi ko pipirmahan."** (Honest) — Integridad DC 15.
  Success: Integridad +1, you refuse.
  Fail: pressured, Koneksyon −1.

- **"Sige, pirma na."** (Padrino) — **Utang +2**, Integridad −1, Tiwala −2 (ghost project seed).

- **"Ipa-test natin ang materyales."** (Collective) — Talino DC 15.
  Success: test exposes it, Tiwala +1.
  Fail: test blocked, but you tried.

- **"Hindi ko napansin."** (Huwag makialam) — Flag `signed_cement`.

Callback: Utang +2 → Singil. Ghost project → Ch4.

---

#### T2 — Ang Resibo

`ch3_teacher_receipts_02` · Tags: teacher, integrity

Setup: *"Sabi ng principal: 'Itaas mo ang resibo ng classroom fund. Maliit lang, para may pang-ayos tayo.' Alam mong hindi totoo ang numero."*

- **"Hindi. Totoo ang ilalagay ko."** (Honest) — Integridad DC 14.
  Success: Integridad +1.
  Fail: principal cold, Koneksyon −1.

- **"Sige, itaas ko."** (Padrino) — **Utang +1**, Integridad −1, Tiwala −1.

- **"Ipa-audit natin."** (Collective) — Talino DC 14.
  Success: audit, Tiwala +1.
  Fail: they resist, but you tried.

- **"Bahala na."** (Huwag makialam) — Flag `inflated_receipts`.

Callback: Utang +1 → Singil.

---

#### T3 — Ang Ebidensya

`ch3_teacher_evidence_03` · Tags: teacher, whistleblower

Setup: *"May estudyanteng lumapit sa'yo na may larawan — may iregularidad sa proyekto ng paaralan. 'Ma'am/Sir, hindi ko alam kung kanino ko sasabihin.'"*

- **"Sasabihin ko sa kinauukulan."** (Honest) — Integridad DC 15.
  Success: Integridad +1, case opens.
  Fail: stalls, but you tried.

- **"Tulungan kitang i-document."** (Collective) — Komunidad DC 14.
  Success: Komunidad +1, `spoke_up`.
  Fail: threat, but `spoke_up` set.

- **"Sabihin mo kay Kuya Jun."** (Padrino) — **Utang +1**, Integridad −1.

- **"Huwag ka nang magsalita."** (Huwag makialam) — Flag `silenced_student`.

Callback: `spoke_up` → Ch4. Ghost project → Ch4.

---

### Role-specific — Organizer / Journalist

#### J1 — Ang Source

`ch3_journalist_source_01` · Tags: journalist, evidence

Setup: *"May source na gustong magsalita tungkol sa ghost project, pero gusto niyang manatiling anonymous. Manipis ang ebidensya mo."*

- **"Ilalabas ko nang maingat."** (Honest) — Talino DC 15.
  Success: Talino +1, story runs.
  Fail: challenged, Koneksyon −1.

- **"Kailangan ko ng mas matibay."** (Diskarte) — Diskarte DC 14.
  Success: Diskarte +1, you verify.
  Fail: source backs out, but you learned.

- **"Isama ko ang barkada sa pag-verify."** (Collective) — Komunidad DC 14.
  Success: Komunidad +1, verified.
  Fail: slow, but solid.

- **"Hindi ko ilalabas."** (Huwag makialam) — Flag `killed_story`.

Callback: Ghost project → Ch4.

---

#### J2 — Ang Alok

`ch3_journalist_offer_02` · Tags: journalist, threat

Setup: *"May lumapit sa'yo na may alok — pera, para tumigil ka sa pag-iimbestiga. 'Para sa pamilya mo,' sabi niya. Hindi ito banta, pero parang banta."*

- **"Hindi ako titigil."** (Honest) — Integridad DC 16.
  Success: Integridad +1, you continue.
  Fail: pressure grows, but you held.

- **"Tatanggapin ko."** (Padrino) — Pera +2, **Utang +2**, Integridad −1, Tiwala −2.

- **"I-document ko ang alok."** (Diskarte) — Diskarte DC 15.
  Success: Diskarte +1, you have proof.
  Fail: they deny it, but you have notes.

- **"Titigil na lang ako."** (Huwag makialam) — Flag `stopped_investigating`.

Callback: Utang +2 → Singil. `stopped_investigating` → Ch4 (absent from cascade).

---

#### J3 — Ang Pagod na Grupo

`ch3_journalist_group_03` · Tags: organizer, burnout

Setup: *"Ang grupo mo ay pagod na. 'Wala namang nangyayari,' sabi nila. May nagsasabing umalis na lang."*

- **"Magpahinga tayo, tapos tuloy."** (Collective) — Komunidad DC 14.
  Success: Komunidad +1, group rests and returns.
  Fail: some leave, core stays.

- **"Mag-isa akong tutuloy."** (Honest) — Integridad DC 15.
  Success: Integridad +1, you continue alone.
  Fail: burnout — flag `burned_out` (→ Ending 8).

- **"Maghanap ng bagong kaalyado."** (Diskarte) — Diskarte DC 14.
  Success: Diskarte +1, new allies.
  Fail: slow, but you tried.

- **"Kayo na lang ang bahala."** (Huwag makialam) — Flag `left_group`.

Callback: `burned_out` → Ending 8. `left_group` → Ch4 (absent).

---

### Role-specific — Ordinary Citizen

#### C1 — Ang Assembly

`ch3_citizen_assembly_01` · Tags: citizen, assembly

Setup: *"May barangay assembly tungkol sa baha. 'Pupunta ka ba?' tanong ni Aling Cora. 'Wala namang nangyayari doon,' sabi ng iba."*

- **"Pupunta ako."** (Collective) — Komunidad DC 12.
  Success: Komunidad +1, you speak, `spoke_up`.
  Fail: ignored, but `spoke_up` set.

- **"Magpapadala ako ng sulat."** (Diskarte) — Diskarte DC 12.
  Success: Diskarte +1, letter read.
  Fail: ignored, but you tried.

- **"Sabihin kay Kuya Jun."** (Padrino) — **Utang +1**, Integridad −1.

- **"Hindi na lang."** (Huwag makialam) — Flag `skipped_assembly`.

Callback: `spoke_up` → Ch4. `skipped_assembly` → Ch4 (absent).

---

#### C2 — Ang Co-sign

`ch3_citizen_cosign_02` · Tags: citizen, padrino

Setup: *"May kapitbahay na humihingi na pumirma ka sa isang kahilingan — para sa tubig. 'Kailangan ko ng maraming pirma.'"*

- **"Pipirma ako."** (Collective) — Komunidad DC 11.
  Success: Komunidad +1, request moves.
  Fail: stalls, but you signed.

- **"Hindi ako pipirma."** (Honest) — Integridad DC 11.
  Success: Integridad +1.
  Fail: neighbor hurt, Komunidad −1.

- **"Sabihin kay Kuya Jun."** (Padrino) — **Utang +1**, Integridad −1.

- **"Bahala ka na."** (Huwag makialam) — Flag `refused_cosign`.

Callback: Utang +1 → Singil.

---

#### C3 — Ang ₱500

`ch3_citizen_vote_03` · Tags: citizen, padrino

Setup: *"Gabi bago ang eleksyon. May nag-abot sa'yo ng ₱500. 'Pang-merienda lang. Wala namang masama.'"*

- **"Tatanggapin ko."** (Padrino) — Pera +1, Integridad −1, **Utang +1**.

- **"Hindi po, salamat."** (Honest) — Integridad DC 12.
  Success: Integridad +1.
  Fail: Koneksyon −1.

- **"Sabihin ko sa mga kapitbahay."** (Collective) — Komunidad DC 15.
  Success: 3 refuse, Tiwala +1, `spoke_up`.
  Fail: Komunidad −1, `spoke_up` set.

- **"Hindi ko problema."** (Huwag makialam) — Walang Kibo tracked.

Callback: Utang +1 → Singil. `spoke_up` → Ch4.

---

### Shared Adult

#### S1 — Ang Sakit ni Nanay

`ch3_shared_sick_01` · Tags: family, pera, padrino

Setup: *"Na-ospital si Nanay. Kailangan ng pera ngayon na. Lumapit si Tita Baby: 'May kilala akong makakatulong. Isang pirma lang.'"*

- **"Salamat, Tita."** (Padrino) — Pera +2, **Utang +2**, Integridad −1.

- **"Maghahanap ako ng paraan."** (Diskarte) — Diskarte DC 14.
  Success: Diskarte +1, you find funds.
  Fail: Pera −1, but you tried.

- **"Hihingi ako ng tulong sa barkada."** (Collective) — Komunidad DC 13.
  Success: Komunidad +1, funds raised.
  Fail: partial, but community helps.

- **"Bahala na ang ospital."** (Huwag makialam) — Flag `couldnt_pay`.

Callback: Utang +2 → Singil. (Sympathetic reason for padrino.)

---

#### S2 — Ang Sirang Kalsada

`ch3_shared_road_02` · Tags: barangay, komunidad

Setup: *"Baha na naman sa kalye ninyo. Walang gumagawa. Sabi ni Kap Eddie: 'Mag-aantay tayo ng pondo.'"*

- **"Mag-organize tayo ng bayanihan."** (Collective) — Komunidad DC 14.
  Success: Komunidad +1, street fixed, `spoke_up`.
  Fail: partial, but `spoke_up` set.

- **"Magreklamo ako kay Kap Eddie."** (Honest) — Koneksyon DC 13.
  Success: Koneksyon +1, action promised.
  Fail: nothing, but you tried.

- **"Sabihin kay Kuya Jun."** (Padrino) — **Utang +1**, Integridad −1.

- **"Mag-antay na lang."** (Huwag makialam) — Flag `waited`.

Callback: `spoke_up` → Ch4. Utang +1 → Singil.

---

#### S3 — Ang Utos ng Boss

`ch3_shared_boss_03` · Tags: work, integrity

Setup: *"Sabi ng boss mo: 'Baguhin mo ang numero. Maliit lang, walang makakaalam.' Kung hindi, baka mawala ang trabaho mo."*

- **"Hindi ko babaguhin."** (Honest) — Integridad DC 15.
  Success: Integridad +1, you keep/find work.
  Fail: Pera −1 (job at risk), but you held.

- **"Sige, babaguhin ko."** (Padrino) — **Utang +1**, Integridad −1, Pera +1 (keep the job).

- **"I-document ko muna."** (Diskarte) — Diskarte DC 14.
  Success: Diskarte +1, you have proof.
  Fail: boss notices, Koneksyon −1.

- **"Bahala na."** (Huwag makialam) — Flag `changed_numbers`.

Callback: Utang +1 → Singil.

---

#### S4 — Ang Pagbabalik ni Pia

`ch3_shared_pia_04` · Tags: callback, loyalty

Setup: *"Nagkita kayo ni Pia pagkatapos ng maraming taon. 'May kailangan ako,' sabi niya. 'Isang pirma lang. Para sa kapatid ko.'"*
(Varies by `left_pia` / `saw_it` / Utang with Pia.)

- **"Sige, pipirma ako."** (Padrino) — **Utang +2**, Integridad −1.

- **"Hindi ko kaya."** (Honest) — Integridad DC 13.
  Success: Integridad +1.
  Fail: Pia hurt, Komunidad −1.

- **"Tulungan kitang mahanap ng ibang paraan."** (Collective) — Komunidad DC 13.
  Success: Komunidad +1, alternative found.
  Fail: partial.

- **"Hindi na kita kilala."** (Huwag makialam) — Flag `rejected_pia`.

Callback: Utang +2 → Singil.

---

### Singil (Utang ≥ 3)

#### G1 — Ang Pirma

`ch3_singil_sign_01` · Tags: singil, utang

Setup: *"Tumawag si Tita Baby. 'Anak, may papel lang… pirma lang. Alam mo namang tumulong ako sa'yo noon.'"*

- **"Pipirma ako."** (Comply) — Utang −2, Integridad −1, Tiwala −2.

- **"Hindi ako pipirma."** (Honest) — Integridad DC (10 + Utang).
  Success: debt cleared, Komunidad +1.
  Fail: lose Koneksyon, debt stays.

- **"Hihingi ako ng tulong kay Nurse Lea."** (Collective) — Add Kasama/echo to the refusal roll.

- **"Pag-iisipan ko."** (Huwag makialam) — Utang +1, returns harder.

Callback: Debt cleared → Ch4 with lighter burden.

---

#### G2 — Ang Pirma ni Kuya Jun

`ch3_singil_fixer_02` · Tags: singil, utang, fixer

Setup: *"Sabi ni Kuya Jun: 'Isang pirma lang. Vouch mo lang ako sa isang proyekto. Hindi ito malaking bagay.'"*

- **"Pipirma ako."** (Comply) — Utang −2, Integridad −1, Tiwala −2.

- **"Hindi."** (Honest) — Integridad DC (10 + Utang).
  Success: debt cleared, Komunidad +1.
  Fail: lose Pera, debt stays.

- **"Isama ko ang barkada."** (Collective) — Add Kasama/echo.

- **"Pag-iisipan ko."** (Huwag makialam) — Utang +1, returns harder.

Callback: Debt cleared → Ch4.

---

#### G3 — Ang Utang ni Bea

`ch3_singil_bea_03` · Tags: singil, utang, callback

Setup: *"Tumawag si Bea. 'Naalala mo ang utang mo sa akin? Kailangan ko ng pabor — ngayon na.'"*
(Callback to Bea arc.)

- **"Sige, gagawin ko."** (Comply) — Utang −2, Integridad −1, Tiwala −1.

- **"Hindi."** (Honest) — Integridad DC (10 + Utang).
  Success: debt cleared, Komunidad +1.
  Fail: lose Koneksyon, debt stays.

- **"Hihingi ako ng tulong."** (Collective) — Add Kasama/echo.

- **"Hindi pa ngayon."** (Huwag makialam) — Utang +1, returns harder.

Callback: Debt cleared → Ch4.

---

## Chapter 4: Ang Pagsubok (The Climax)

*4-card pool, 3 drawn per run. The flood, the ghost project, the town assembly.*
*Voice count (cascade) is tracked across these cards. Echoes add voices.*

### Card 1 — Baha

`ch4_flood_01` · Tags: climax, flood

Setup: *"Tatlong araw ng ulan at bumaha ang bayan. Ang proyektong 'tapos na' tatlong taon na ang nakalipas — wala. Ang arko, maganda. Nagkukumpol ang mga tao sa barangay hall, basang-basa at galit."*

- **"Magsasalita ako nang una."** (Honest) — Integridad DC 18 (before assists).
  Success or fail, you add the **first voice**. Flag `spoke_first`.

- **"Maghihintay ako kung sino ang magsasalita."** (Huwag makialam) — You join only if the cascade reaches your threshold.

- **"Dadalhin ko ang mga kapitbahay ko."** (Collective) — Komunidad DC 16 + Kasama/echoes.
  Success: adds **2 voices**.
  Fail: adds 1 voice (public).

- **"Aalis na lang ako."** (Escape) — Safe. Walang Kibo +1.

Callback: Voice count begins. Echoes add voices.

---

### Card 2 — Ang Pagkukumpol

`ch4_assembly_02` · Tags: climax, assembly

Setup: *"Nagsalita si Nurse Lea — ang unang boses. 'Hindi ito aksidente. May resibo, pero walang proyekto.' Tahimik ang lahat. Hinihintay kung sino ang susunod."*

- **"Susuporta ako kay Nurse Lea."** (Collective) — Komunidad DC 16.
  Success: you add a voice; **Angry Neighbors** join (+1).
  Fail: you add a voice anyway (public).

- **"Magsasalita ako nang mag-isa."** (Honest) — Integridad DC 17.
  Success: you add a voice.
  Fail: you add a voice (public), but it's hard.

- **"Mananatili akong tahimik."** (Huwag makialam) — Join only if threshold reached.

Callback: Voice count +1/+2. Cascade builds toward Worried Parents.

---

### Card 3 — Ang Ebidensya

`ch4_evidence_03` · Tags: climax, evidence

Setup: *"May nagpakita ng ebidensya — ang supply log, ang resibo, ang larawan ng walang ginawa. Hindi na ito usap lang. Totoo na ito."*

- **"Ipapakita ko ang ebidensya ko."** (Honest) — Talino DC 16.
  Success: undeniable; **Worried Parents** join (+1).
  Fail: questioned, but you spoke (+1).

- **"Isama ko ang mga echo ng ibang tao."** (Collective) — Uses echoes.
  Adds **2 voices**.

- **"Mananatili akong tahimik."** (Huwag makialam) — Join only if threshold reached.

Callback: Voice count +1/+2. **Cautious Vendors** may join.

---

### Card 4 — Ang Umaga

`ch4_morning_04` · Tags: climax, decision

Setup: *"Ang mga boses ay dumami. Ang Silent Majority ay nagsalita na. Ang bayan ay kailangang magdesisyon — reporma o tanggapin ang dati."*

- **"Reporma. Ngayon na."** (Collective) — Komunidad DC 18 + all assists/echoes.
  Success: **Reform**.
  Fail: partial reform, but you tried.

- **"Unti-unti lang."** (Honest) — Integridad DC 16.
  Success: slow reform.
  Fail: status quo, but you spoke.

- **"Tanggapin na lang ang dati."** (Huwag makialam) — No reform. Walang Kibo +1.

Callback: Determines ending (Reform vs status quo). Leads to final screen.

---

## Endings (10 core)

*Personal epilogues. Complicit endings are humanizing, not punishing.*
*Each ends with the "starting line" note. Then the final screen "Ikaw at ang Bayan."*

### 1 — Ang Kapitang Hindi Yumuko

*Official, Integridad high, Utang low.*

*"Hindi ka yumuko. Sa huling assembly, ikaw ang nagsalita nang una — at ang bayan ay sumunod. Hindi ka naging bayani. Naging halimbawa ka lang. At minsan, sapat na 'yon."*

Starting line: *"Nagsimula ka bilang [background]. Ang iba, nagsimula sa iba."*

---

### 2 — Ang Pirma

*Official, Utang high, complied with Singil.*

*"Pumirma ka. Alam mong mali, pero may nanay kang may sakit, may utang kang binabayaran. Hindi ka masamang tao. Isa ka lang taong napagod. Ang bayan ay nagpatuloy — pero ikaw, hindi na makatulog."*

---

### 3 — Ang Badge

*Police, Integridad high, spoke up.*

*"Suot mo ang badge nang may dangal. Sa huling gabi, tumayo ka sa gitna ng assembly at nagsalita. Ang ilan ay nagalit. Ang bayan ay nakinig. Ang badge ay hindi na mabigat."*

---

### 4 — Ang Sumunod

*Police, complied under pressure.*

*"Sinunod mo ang utos. Mas madali 'yon. Pero sa gabi, naaalala mo ang mukha ng kapitbahay na humingi ng tulong. Hindi ka masama. Natatakot ka lang. At ang takot, minsan, ay sapat na para sumunod."*

---

### 5 — Ang Tamang Semento

*Teacher/Engineer, refused or reported.*

*"Tumanggi kang pumirma. Ang semento ay tama, ang resibo ay totoo. Nawalan ka ng kaibigan sa munisipyo, pero nakuha mo ang sarili mo. Ang mga estudyante mo ay natuto — hindi lang sa aralin, kundi sa halimbawa."*

---

### 6 — Ang Pumikit

*Teacher/Engineer, stayed silent.*

*"Pumikit ka. Hindi dahil gusto mo, kundi dahil pagod ka na. Ang proyekto ay itinayo nang mali, at ang baha ay dumating. Minsan, ang katahimikan ay isang pagpili rin — at may bayad ito."*

---

### 7 — Ang Boses

*Organizer/Journalist, cascade succeeded.*

*"Ang boses mo ay hindi nag-iisa. Sa assembly, ang isa ay naging marami. Ang bayan ay nagbago — hindi dahil sa'yo, kundi dahil sa lahat ng nagsalita. Ikaw ay isa lang sa kanila. At 'yon ang buong punto."*

---

### 8 — Ang Pagod na Boses

*Organizer/Journalist, acted alone and burned out.*

*"Nagsalita ka nang mag-isa, at napagod ka. Walang sumama, kaya walang nagbago. Hindi ito pagkabigo mo — ito ang gastos ng pagkilos nang mag-isa. Ang bayan ay naghintay ng iba. Sana ay may sumama."*

---

### 9 — Ang Sumama

*Ordinary Citizen who joined the cascade.*

*"Hindi ka nagsalita nang una. Pero nang may nagsalita, sumama ka. At ang pagsama mo ang naging dahilan kung bakit naging marami ang boses. Hindi ka bayani. Isa kang kapitbahay. At ang kapitbahay, minsan, ang pinakamahalaga."*

---

### 10 — Ang Nanood

*Ordinary Citizen who stayed silent.*

*"Nanood ka lang. Ang baha ay dumating at umalis. Ang bayan ay nagbago — o hindi. Hindi ka masama. Isa ka lang taong hindi pa handa. Ang pinto ay bukas pa rin."*

---

## Final Screen: "Ikaw at ang Bayan"

1. **Personal epilogue card** (tarot-style illustration, 3–4 lines).
2. **Barangay impact:** your Pag-asa contribution + a threshold fact
   (e.g., "37% of players in this barangay spoke up. The tipping point is 40%.").
3. **Utang ledger reveal:** each favor you accepted and its later cost.
4. **Real-world call to action:** one concrete, location-specific step (see below).

---

## Bayanihan Echo Pool (pre-written, anonymous)

*From "earlier players." Each = one die (+1d6), max 2 echoes per roll. Usable in Ch4.*

- *"Nurse Lea leaked the supply log: +1d6 once."*
- *"Aling Cora kept the receipts: +1d6 once."*
- *"A teacher photographed the empty site: +1d6 once."*
- *"The barkada chipped in for the printing: +1d6 once."*
- *"Kap Eddie looked the other way — for once: +1d6 once."*
- *"Someone posted the budget on the court wall: +1d6 once."*
- *"The vendor association kept the records: +1d6 once."*
- *"A student drew the missing project: +1d6 once."*
- *"The tanod's nephew testified: +1d6 once."*
- *"The OFW family sent money for the assembly: +1d6 once."*
- *"Ma'am Lourdes kept her old lesson plans: +1d6 once."*
- *"The whole street signed the petition: +1d6 once."*

---

## Location-Specific Final Ask (13C)

*Template:* "This week, [one concrete action] in [your barangay/town]. Bring one neighbor."

*Examples by location type (app shows the one matching the player's locale):*

- **Small town / rural barangay:** "Attend your next barangay assembly. Ask to see the posted budget. Bring one neighbor."
- **City barangay:** "Find your barangay's posted budget (barangay hall wall or website). Ask one question about it at the next assembly."
- **School community:** "Ask your school's PTA or SGC to post the budget. Offer to help make it readable."
- **General:** "Ask one local official one question about a local project. Write down the answer. Share it with one neighbor."

---

*End of card content. All 40 cards, 10 endings, Echo pool, and final-ask list are complete.*
