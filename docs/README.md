# Documentazione SimRisorse — indice corrente G1-D26

**AGGIORNAMENTO G1-D28 / RC12 (03/11/2026):** RC11 dichiarata verde dall'utente (**466 test attesi**, numero esatto non comunicato). RC12: ricerca a **due decimali**, pulsante «Acquista ricerca» disabilitato e grigio se saldo o prerequisiti insufficienti, verde con hover ocra solo quando acquistabile. Controllo prezzi/saldi sul valore **non arrotondato** in millipunti. **468 test attesi, non eseguiti** in questo ambiente privo di SDK .NET 10. GUI v13, core e SQLite invariati; audit visivo P0 ancora aperto. [Report RC12](design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano](validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).


**AGGIORNAMENTO G1-D26 / RC10 (03/11/2026):** la compilazione Windows della RC9 si è interrotta con **CS0246** in `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs` (riga 15): `GameScenario` è definito in `SimRisorse.Core.Content`, ma mancava il relativo `using`. **RC10 aggiunge esclusivamente tale import nel codice dei test.** Tutti gli altri progetti sono stati compilati nel log dell’utente; la suite xUnit RC9 non è partita. **Ultima suite verde: RC8, 456/456 test confermati. RC10: 464 test attesi, non ancora eseguiti; audit GUI P0 ancora aperto.** [Diagnosi RC10](design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano RC10](validation/G1_D26_RC10_TEST_PLAN.md).


**AGGIORNAMENTO G1-D25 / RC9 (03/11/2026):** **RC8 456/456 test superati, comunicato dall'utente.** RC9 rimuove «Esci senza salvare» in entrambe le UI; la tab Scuola segue l'Esercito con coda, formazione/progresso, ricercatori; introduce la risorsa **Matite** solo nella v14 (1 Legno lavorato + 1 Carbone → 4 Matite, fabbricazione manuale, 2 Matite + 10 Cibo per studente), persistenza v3 retrocompatibile e rimborsi non fittizi. **464 test attesi, NON eseguiti**; confronto visivo ancora aperto. [Rapporto RC9](design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Piano RC9](validation/G1_D25_RC9_TEST_PLAN.md) · [ADR 0035](adr/0035-scuola-formazione-matite-v14.md).


**Fonte dello stato attuale:** **v60-E3b4-RC10 / G1-D26 candidata**, **464 test attesi e non eseguiti**; RC9 non compilava per un namespace mancante nel test. RC8 è stata dichiarata **456/456 verde** dall'utente il 3 novembre 2026. RC9 introduce Scuola con progressi, Matite e rimozione del comando «Esci senza salvare». Parità grafica generale v13/v14 ancora aperta.

## Per iniziare

- [Ripresa rapida](START_HERE.md)
- [Handoff tecnico completo per una nuova chat](HANDOFF.md)
- [Messaggio di avvio da riutilizzare in una nuova chat](NEW_CHAT_HANDOFF.md)
- [Stato corrente e incertezze](CURRENT_STATE.md)
- [Roadmap dettagliata con priorità e gate](ROADMAP.md)
- [Registro delle decisioni confermate/candidate](AUTHORITY_REGISTRY.md)
- [Changelog consolidato](../CHANGELOG.md)

## Ultime modifiche

- [RC10 correzione del compilatore CS0246](design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano RC10](validation/G1_D26_RC10_TEST_PLAN.md)

- [RC9 Scuola, formazione e Matite](design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Piano di test RC9](validation/G1_D25_RC9_TEST_PLAN.md) · [ADR 0035](adr/0035-scuola-formazione-matite-v14.md)

## GUI e regressioni

- [RC8 / D24 — Rapporto correttivo Edifici e Scuola](design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md) · [piano di validazione RC8](validation/G1_D24_RC8_TEST_PLAN.md) · [ADR 0034](adr/0034-v14-stati-edifici-e-layout-azioni.md)

- [Matrice di parità v13/v14 — verifiche obbligatorie](validation/G1_D19_MATRICE_PARITA_GUI.md)
- [Analisi RC7 / problema del test](design/G1_D23_E3B4_RC7_AVALONIA_TEST_ENABLED.md) · [Piano prova RC7](validation/G1_D23_RC7_TEST_PLAN.md)
- [Piano prova RC6](validation/G1_D22_RC6_TEST_PLAN.md) · [Piano prova RC5](validation/G1_D21_RC5_TEST_PLAN.md) · [Piano RC4](validation/G1_D20_RC4_TEST_PLAN.md) · [Test plan RC3](validation/V60_E3B4_RC3_PARITY_TEST_PLAN.md)
- [Rapporto G1-D22 diagnostica produttiva](design/G1_D22_E3B4_RC6_DIAGNOSTICA_PRODUZIONE.md)
- [Rapporto G1-D21 preventivi e upgrade](design/G1_D21_E3B4_RC5_LOTTI_UPGRADE_GUI.md)
- [Rapporto G1-D20 diagnosi Pane](design/G1_D20_E3B4_RC4_DIAGNOSTICA_PANE.md)
- [Design e rapporto RC3](design/G1_D18_E3B4_RC3_RIPRISTINO_FUNZIONI_GUI.md)
- [Contratto grafico originale](PIXEL_ART_UI.md)
- [ADR 0032 — livelli come edifici](adr/0032-gui-v14-identica-navigazione-classica-tier-come-edifici.md)
- [ADR 0033 — mantenere tutte le funzioni v13](adr/0033-preservazione-funzionale-gui-v13-v14.md)

## Motore, salvataggi e sviluppo

- [Architettura](ARCHITECTURE.md) · [Convenzioni e comandi](DEVELOPMENT.md) · [Formato salvataggi](SAVE_FORMAT.md)
- [Contratto economico ancora in proposta](design/G1_ECONOMIA_RICERCA_CONTRATTO_QUANTITATIVO.md) · [Albero tecnologico candidato](design/G1_D4_ALBERO_TECNOLOGICO_E_GAMEPLAY.md)
- [ADR e loro storico](adr/README.md) · [Resoconto audit documentale](validation/G1_D19_DOCUMENTATION_AUDIT.md)

## Storia conservata

- [Checkpoint](history/checkpoints/README.md) · [Note di rilascio](history/releases/README.md) · [Manifest](history/integrity/README.md)
- [Changelog completo precedente](history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md) · [Indice archivi operativi](history/operational/README.md) · [Checkpoint G1-D19](history/checkpoints/G1_D19_CHECKPOINT.md)
- [Roadmap precedente](history/operational/ROADMAP_PRIMA_DEL_RIORDINO_G1_D19.md)
- [Handoff precedente](history/operational/HANDOFF_PRIMA_DEL_RIORDINO_G1_D19.md)

**Indicazione:** in caso di conflitto tra documenti storici e questi riferimenti, seguire `AUTHORITY_REGISTRY.md`, `CURRENT_STATE.md` e i requisiti dell'utente; non inventare validazioni o autorizzazioni economiche.
