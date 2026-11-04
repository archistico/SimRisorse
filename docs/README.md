# Documentazione corrente — RC40 / F1-D01

Approvata la riforma alimentare. Modello Core puro della lavorazione nel deposito, senza attivazione nel gioco e senza alterare salvataggi. [Specifica F1-D01](design/F1_D01_RC40_MODELLO_ALIMENTARE.md) · [ADR 0050](adr/0050-filiera-alimentare-unificata-deposito.md) · [Roadmap](ROADMAP.md) · [Piano test](validation/F1_D01_RC40_TEST_PLAN.md). Ultimo gate confermato RC38 (541 test), RC40 552 attesi.

---

# Documentazione corrente — RC36 / P1-D50

**538 test attesi (non eseguiti qui); ultimo checkpoint Windows confermato RC30 / 511.** [Rapporto consolidamento](design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md), [piano test](validation/P1_D50_RC36_TEST_PLAN.md), [roadmap](ROADMAP.md), [protocollo E4a](design/E4A_PROTOCOLLO_BENCHMARK.md) e [ADR 0047](adr/0047-gate-p1-e-misure-e4a.md). Nessun dato di benchmark E4a ancora misurato.

---

# Documentazione corrente — RC33

RC30: ultimo checkpoint Windows confermato (511 test). RC33: 524 test attesi, da verificare, include RC31 e RC32. Filiera Matite con un'unica verifica dominio/UI senza modificare la ricetta. [Rapporto D47](design/P1_D47_RC33_FILIERA_MATITE.md) · [Piano RC33](validation/P1_D47_RC33_TEST_PLAN.md) · [ADR 0044](adr/0044-unica-verifica-laboratorio-matite.md) · [Roadmap](ROADMAP.md).

---

# Documentazione corrente — RC31

La GUI v14 è stata approvata come modalità normale; `run-game.cmd` apre v14, `--legacy-v13` conserva la GUI precedente. **RC30 validata dall'utente**, RC31 candidata a 515 test. [Avvio rapido](START_HERE.md) · [Roadmap](ROADMAP.md) · [ADR 0042](adr/0042-promozione-v14-e-conservazione-slot.md).

---

# Documentazione operativa — aggiornamento G1-D42

RC27 è la baseline applicativa validata dall'utente con **507 test superati**. La nuova [roadmap dettagliata](ROADMAP.md) contiene attività, dipendenze e criteri di uscita D42–D50, E4a/b/c e M5.4–M7; il [runbook P0](validation/G1_D42_P0_RUNBOOK.md) specifica come raccogliere le prove visive delle 22 voci. G1-D42 è solo documentazione, senza incremento dei test o nuovo rilascio applicativo. P0 visivo aperto.

---

# Indice corrente — RC25 / G1-D39-F1

**RC20** ultimo gate Windows verde (490 test). **RC24** build fallita per due CS1061 nel test upgrade. **RC25 candidata:** corretto l'import Avalonia Headless, **501 test attesi**. [Rapporto F1](design/G1_D39_RC25_FIX_CS1061_HEADLESS.md) · [Piano RC25](validation/G1_D39_RC25_TEST_PLAN.md) · [Matrice P0](validation/G1_D39_P0_UI_MATRIX.md) · [Roadmap](ROADMAP.md). Il gate grafico resta aperto.

---

# Indice corrente — RC24 / G1-D39

RC20 (490) è l'ultimo checkpoint Windows confermato. RC24 (501 test attesi, RC21–RC23 inclusi) è una **candidata**. [Audit Scuola](design/G1_D39_RC24_AUDIT_SCUOLA_E_GATE_P0.md) · [Matrice gate P0](validation/G1_D39_P0_UI_MATRIX.md) · [Validazione RC24](validation/G1_D39_RC24_TEST_PLAN.md) · [Roadmap](ROADMAP.md). Il confronto grafico delle 22 voci è ancora aperto.

---

# Indice corrente — RC22 / G1-D37

**RC20 validata:** 490 test. **RC21 pendente:** 493. **RC22 candidata:** 496 attesi, +3 test. [Rapporto scorrimento Edifici / Esercito](design/G1_D37_RC22_SCORRIMENTO_DETTAGLI_ESERCITO.md) · [Piano Windows](validation/G1_D37_RC22_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Audit](validation/G1_D29_AUDIT_GUI_V13_V14.md). **P0 visivo aperto**, nessun E4.

---

# Indice corrente — RC21 / G1-D36

**RC20 validata dall'utente (490 attesi)**; **RC21 candidata (493 attesi, +3 test)**. Audit Edifici: [report D36](design/G1_D36_RC21_AUDIT_EDIFICI_UPGRADE.md) · [test Windows](validation/G1_D36_RC21_TEST_PLAN.md) · [roadmap](ROADMAP.md) · [audit 22 voci](validation/G1_D29_AUDIT_GUI_V13_V14.md). P0 visuale aperto, E4 sospeso.

---

# Indice corrente — RC19 / G1-D34

**RC16 validata (476)**; RC17 (481) e RC18 (484) non testate; **RC19 candidata (487)**. [Audit Esercito](design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano RC19](validation/G1_D34_RC19_TEST_PLAN.md) · [Roadmap](ROADMAP.md). P0 visuale aperto, E4 sospeso.

---

# Indice corrente — RC18 / G1-D33

**RC16 validata (476)**; **RC17 da validare (481)**; **RC18 candidata (484)**. [Correzione Scuola/lotti](design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Piano test RC18](validation/G1_D33_RC18_TEST_PLAN.md). Nessuna modifica economica. Gate P0 visivo aperto.

---

# Indice aggiornato — RC17 / G1-D32

**RC16:** 476 test dichiarati superati. **RC17:** 481 attesi, test Windows pendenti. [Rapporto sicurezza checkpoint](design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Piano RC17](validation/G1_D32_RC17_TEST_PLAN.md) · [ADR 0039](adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md). Gate grafico P0 ancora aperto.

# Documentazione SimRisorse — Indice RC16 / G1-D31

**Baseline confermata Windows:** RC15 (473 test attesi, dichiarati superati dall'utente). **Candidata RC16:** 476 test attesi, da validare su Windows. Gate grafico P0 non chiuso.

## Documenti autorevoli

- [Ripresa rapida](START_HERE.md) · [Stato operativo](CURRENT_STATE.md) · [Roadmap in sei fasi](ROADMAP.md)
- [Handoff completo](HANDOFF.md) · [Nuova chat](NEW_CHAT_HANDOFF.md)
- [Audit GUI UI-01…UI-22](validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Matrice storica](validation/G1_D19_MATRICE_PARITA_GUI.md)
- [Rapporto G1-D31 / RC16](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano test RC16](validation/G1_D31_RC16_TEST_PLAN.md)
- [Decisioni e priorità](AUTHORITY_REGISTRY.md) · [Architettura](ARCHITECTURE.md) · [Stile pixel art](PIXEL_ART_UI.md)
- [Changelog](../CHANGELOG.md) · [ADR](adr/README.md)

## Tappe recenti

- [RC15: dettagli Matite e pausa durante conferma](design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Test RC15](validation/G1_D30_RC15_TEST_PLAN.md)
- [RC14: correzione CS0136](design/G1_D29_RC14_FIX_CS0136.md) · [Test RC14](validation/G1_D29_RC14_TEST_PLAN.md)
- [RC13: primo audit e tooltip capacità Matite](design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md) · [Test RC13](validation/G1_D29_RC13_TEST_PLAN.md)
- [RC12: ricerca a due decimali](design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Test RC12](validation/G1_D28_RC12_TEST_PLAN.md)
- [RC11: layout Costruisci/Lavoratore](design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Test RC11](validation/G1_D27_RC11_TEST_PLAN.md)
- [RC9: Scuola e Matite](design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Test RC9](validation/G1_D25_RC9_TEST_PLAN.md)
- [Storico completo precedente G1-D19](history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md)

**Prevalenza:** richiesta utente e decisioni accettate → stato, roadmap, handoff correnti → archivi. I coefficienti di gioco sono candidati finché non approvati.
