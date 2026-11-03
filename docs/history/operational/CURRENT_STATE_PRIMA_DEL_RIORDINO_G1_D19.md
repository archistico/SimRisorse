# Stato corrente — SimRisorse v60-E3b4-RC3 / G1-D18

**Data:** 2026-11-03. **Ultimo gate Windows confermato dall'utente:** v60-E3b4-RC1 con **427 test passati**. La RC2 è stata **respinta nella verifica visiva**: la scheda Risorse era stata semplificata, Esercito incompleto e Costruisci visibile su edifici bloccati. La RC3 è un **candidato correttivo non ancora compilato o testato su Windows**.

## Rischi e protezioni

- Modalità normale `civil-growth-13`: `MainWindow`, `MainViewModel`, SQLite v2 e `GameSession` invariati rispetto a RC2/ultima validazione; nessuna migrazione automatica.
- Modalità `--experimental-v14`: GUI v14 e DB v3 separati; prezzi/tier/schermate Scuola e Albero ancora sperimentali.
- Regola definitiva: ricerca accumulata liberamente e spesa volontaria; coefficienti +15% e altri numeri restano proposte.
- La **v13 GUI è baseline** per tutte le aree esistenti, non solo per i colori. La v14 aggiunge funzionalità; non sottrae colonne o ordini.

## Correzioni RC3

Recupero sette colonne complete Risorse con `ResourceRow`, layout classico e capacità con tooltip, stessi pianificatori della simulazione. Ripristino crescita e pannello Esercito con arruolamento/annullamento, diagnosi materiali e blocchi, comandi costruzione +1/+5/+10 senza ComboBox nei dettagli, nuova partita con conferma e autosalvataggio separato. Test automatici di regressione aggiunti. **L'audit visivo è indispensabile e non ancora superato.**

## Roadmap

Gate RC3 (compilazione/test + audit grafico) → completare eventuali discrepanze (es. diagnosi avanzata Pane) → E4 verifiche anti-blocco/bilanciamento/migrazione → M5.4 navigazione.

[Ripresa](../../START_HERE.md) · [Rapporto RC3](../../design/G1_D18_E3B4_RC3_RIPRISTINO_FUNZIONI_GUI.md) · [Test](../../validation/V60_E3B4_RC3_PARITY_TEST_PLAN.md) · [ADR 0033](../../adr/0033-preservazione-funzionale-gui-v13-v14.md).
