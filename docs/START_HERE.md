# Ripresa rapida — SimRisorse G1-D28

**AGGIORNAMENTO G1-D28 / RC12 (03/11/2026):** RC11 dichiarata verde dall'utente (**466 test attesi**, numero esatto non comunicato). RC12: ricerca a **due decimali**, pulsante «Acquista ricerca» disabilitato e grigio se saldo o prerequisiti insufficienti, verde con hover ocra solo quando acquistabile. Controllo prezzi/saldi sul valore **non arrotondato** in millipunti. **468 test attesi, non eseguiti** in questo ambiente privo di SDK .NET 10. GUI v13, core e SQLite invariati; audit visivo P0 ancora aperto. [Report RC12](design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano](validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).


**AGGIORNAMENTO G1-D26 / RC10 (03/11/2026):** la compilazione Windows della RC9 si è interrotta con **CS0246** in `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs` (riga 15): `GameScenario` è definito in `SimRisorse.Core.Content`, ma mancava il relativo `using`. **RC10 aggiunge esclusivamente tale import nel codice dei test.** Tutti gli altri progetti sono stati compilati nel log dell’utente; la suite xUnit RC9 non è partita. **Ultima suite verde: RC8, 456/456 test confermati. RC10: 464 test attesi, non ancora eseguiti; audit GUI P0 ancora aperto.** [Diagnosi RC10](design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano RC10](validation/G1_D26_RC10_TEST_PLAN.md).


**AGGIORNAMENTO G1-D25 / RC9 (03/11/2026):** **RC8 456/456 test superati, comunicato dall'utente.** RC9 rimuove «Esci senza salvare» in entrambe le UI; la tab Scuola segue l'Esercito con coda, formazione/progresso, ricercatori; introduce la risorsa **Matite** solo nella v14 (1 Legno lavorato + 1 Carbone → 4 Matite, fabbricazione manuale, 2 Matite + 10 Cibo per studente), persistenza v3 retrocompatibile e rimborsi non fittizi. **464 test attesi, NON eseguiti**; confronto visivo ancora aperto. [Rapporto RC9](design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Piano RC9](validation/G1_D25_RC9_TEST_PLAN.md) · [ADR 0035](adr/0035-scuola-formazione-matite-v14.md).


**Per proseguire leggi [`NEW_CHAT_HANDOFF.md`](NEW_CHAT_HANDOFF.md), poi [`HANDOFF.md`](HANDOFF.md), [`ROADMAP.md`](ROADMAP.md) e [`AUTHORITY_REGISTRY.md`](AUTHORITY_REGISTRY.md).** L'archivio integrale corrente è **v60-E3b4-RC12 / G1-D28**: conserva le correzioni RC8 e aggiunge la Scuola con barra di progresso, Matite e rimozione di «Esci senza salvare».

## Stato in 30 secondi

- **RC8: 456/456 test superati, dichiarati dall'utente**; confronto visivo con la GUI v13 ancora aperto.
- **RC9: build Windows fallita con CS0246, poi corretta in RC10; RC10 e RC11 test confermati passati dall’utente. RC12: 468 test attesi, da validare**; nell’ambiente di consegna manca .NET 10.
- **Novità:** pagina Scuola come Esercito (posti, coda, progresso e ricercatori), Matite da 1 Legno lavorato + 1 Carbone, **2 Matite per studente**, salvataggi v14 compatibili; rimosso «Esci senza salvare» da entrambe le modalità.
- **Vincolo:** v13 con 16 risorse, salvataggi SQLite v2 e GUI storica invariati salvo la rimozione del pulsante richiesta. La v14 usa un DB SQLite v3 separato.
- **Priorità:** eseguire [piano RC9](validation/G1_D25_RC9_TEST_PLAN.md), poi completare matrice di parità P0 e soltanto dopo avviare E4.

## Comandi

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --experimental-v14
```

**Da verificare sulla RC12 (comprende RC5–RC11):** sette colonne Risorse con tooltip e tempi, Scuola e Albero nel menu, Esercito completo, edificio bloccato senza pulsanti attivi, +1/+5/+10, upgrade, diagnosi produttiva e Pane, salvataggi separati e chiusura senza blocco. Verificare prima il [piano RC8](validation/G1_D24_RC8_TEST_PLAN.md). Usa [matrice P0](validation/G1_D19_MATRICE_PARITA_GUI.md), [piano RC7](validation/G1_D23_RC7_TEST_PLAN.md), [piano RC6](validation/G1_D22_RC6_TEST_PLAN.md) e [piano RC5](validation/G1_D21_RC5_TEST_PLAN.md).

## File autorevoli

[README](../README.md) · [Handoff integrale](HANDOFF.md) · [Passaggio in nuova chat](NEW_CHAT_HANDOFF.md) · [Stato](CURRENT_STATE.md) · [Roadmap dettagliata](ROADMAP.md) · [Decisioni](AUTHORITY_REGISTRY.md) · [Changelog](../CHANGELOG.md).

I vecchi checkpoint sono in `docs/history/checkpoints`; le note di rilascio in `docs/history/releases`; i documenti operativi superati in `docs/history/operational`. **Non trattare i vecchi titoli «stato attivo» come stato corrente.**
