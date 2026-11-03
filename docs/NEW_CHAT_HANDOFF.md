# Nuova chat — SimRisorse RC20 / G1-D35

**Allegare il progetto integrale RC20.** Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e `docs/validation/G1_D35_RC20_TEST_PLAN.md`.

> Continua da RC20. **Ultimo gate Windows confermato: RC16, 476 test.** RC17 (481), RC18 (484), RC19 (487) e **RC20 (490 attesi)** non sono ancora state testate. RC20 include tutte le precedenti, senza cambiarne il codice applicativo.
>
> **D35:** audit statico della tabella Risorse: v14 conserva le sette colonne della v13, anche la scrollbar e il dato di capienza. Aggiunti 3 casi di test in `V14ResourcesAuditTests.cs` (due viewport Avalonia + identità/ordine/aggiornamento). Nessuna modifica arbitraria al layout. Eseguire un solo `./validate.cmd` su RC20 quando possibile, poi controllo visivo a 1040×740 e 1240×850. Non dichiarare P0 chiuso senza conferma visiva.
>
> **Priorità D36:** Edifici (grigi ma selezionabili, due righe Costruisci/Lavoratore, disabilitazione e hover, costo cumulativo, diagnosi). **RC17:** protezione Nuova partita/autosalvataggio: conferma Windows pendente, con salvataggio di prova.
>
> Roadmap: P0 grafico → P1 Scuola/tecnologie → E4a analisi → E4b bilanciamento → E4c consolidamento/migrazione → M5.4–M7. **Preservare v13 e SQLite**. Consegna sempre archivio integrale, changelog, roadmap, handoff e test. Separare prove statiche dai risultati Windows.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Piano RC20](validation/G1_D35_RC20_TEST_PLAN.md) · [Rapporto](design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md).

---

# Nuova chat — SimRisorse RC19 / G1-D34

**Allegare lo ZIP integrale RC19**. Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e `docs/validation/G1_D34_RC19_TEST_PLAN.md`.

> Continua da **SimRisorse RC19 / G1-D34**. Ultimo checkpoint Windows dichiarato superato: **RC16, 476 test**. RC17 (**481**), RC18 (**484**) e RC19 (**487 attesi**) non sono state testate dall'utente. RC19 include tutte le modifiche RC17+RC18. **Non dichiararle verdi** prima del prossimo `validate.cmd`.
>
> **RC19:** `V14PreviewViewModel.Parity.cs` rende coerenti i suggerimenti Esercito con il blocco dei comandi durante «Nuova partita?»; mostra disponibili/richiesti per Spade, abitanti, reclute annullabili e spazio rimborso; singolare «1 recluta». +3 test Desktop in `V14MilitaryUiParityTests.cs`, incluso Avalonia headless. Nessuna variazione delle regole militari, del motore, del layout XAML, della GUI v13 o SQLite.
>
> **RC18:** suggerimenti Scuola coerenti con il lotto +5/+10, posti e ricercatori qualificati. **RC17:** protezione del checkpoint v14 fra creazione nuova colonia, autosalvataggio e chiusura; **richiede ancora test Windows di sicurezza su copia del salvataggio**.
>
> **Roadmap confermata:** Fase 1 P0 audit GUI 22 voci, 1040×740/1240×850 (ora Risorse/Edifici prioritari e verifica Esercito); poi P1 Scuola/tecnologie, E4a misurazioni, E4b bilanciamento, E4c migrazione con backup, M5.4–M7 navale/esplorazione/conquista. No E4 prima del gate P0 e nessun cambio di coefficienti.
>
> **Metodo:** consegna ZIP integrale, changelog, roadmap, handoff e test a ogni incremento. Evidenze statiche e Windows sempre separate; preservare la v13, percorsi SQLite, dati personali. Il confronto grafico necessita verifica Windows reale, non è ancora approvato.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Piano RC19](validation/G1_D34_RC19_TEST_PLAN.md) · [Rapporto D34](design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md).

---

# Nuova chat — SimRisorse RC18 / G1-D33

**Allegare ZIP integrale RC18**. Leggere `docs/CURRENT_STATE.md`, `docs/ROADMAP.md`, `docs/HANDOFF.md`, `docs/validation/G1_D29_AUDIT_GUI_V13_V14.md` e `docs/validation/G1_D33_RC18_TEST_PLAN.md`.

> Continua da **SimRisorse RC18 / G1-D33**. Ultimo gate confermato su Windows: **RC16, 476 test passati**. RC17 (481 attesi) **non è stata testata**; RC18 include RC17 e ha 3 test aggiuntivi, **484 attesi**, ancora da verificare. Non dichiarare verdi RC17 o RC18 prima dell'esito Windows. L'utente ha esplicitamente chiesto di continuare anche in assenza temporanea di test.
>
> RC18 corregge il suggerimento della Scuola che presentava un lotto +5/+10 di ricercatori come assegnabile pur avendo meno posti liberi; distingue ora Scuola assente, posti insufficienti e qualificati insufficienti. Solo ViewModel v14, test, documentazione.
>
> RC17 protegge il checkpoint v14 durante «Nuova partita», salvataggi manuali e automatici; richiede verifica. Mai toccare GUI classica, file v13/SQLite, cataloghi economici o testimoni di una partita personale.
>
> Roadmap approvata: P0 audit GUI (22 voci, risoluzioni 1040×740 e 1240×850); P1 Scuola/tecnologie; E4a misure; E4b bilanciamento approvato; E4c migrazione con backup; M5.4–M7 navale/esplorazione/conquista. Prima validare RC17+RC18 quando possibile, poi Risorse/Edifici/Esercito. Test automatici non sostituiscono audit visivo.
>
> Ogni avanzamento: ZIP integrale, changelog, roadmap, handoff, piano test e ADR solo se decisioni architetturali nuove. Separare chiaramente verifiche statiche da test xUnit non eseguiti.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Piano RC18](validation/G1_D33_RC18_TEST_PLAN.md).
