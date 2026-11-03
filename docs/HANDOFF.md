# Handoff tecnico completo — SimRisorse RC14 / G1-D29-F1

**Baseline e provenienza:** RC12 / G1-D28, con **test Windows dichiarati tutti passati dall'utente** (468 attesi). RC13 / G1-D29 ha aggiornato roadmap e audit GUI ma **non ha compilato su Windows** per 2 errori CS0136 nel tooltip Matite; test RC13 non eseguiti. RC14 corregge **solo il conflitto di nomi locali**, con **470 test attesi** ancora da validare. Non chiudere il gate visivo P0.

## Ordine di lettura

1. [Stato corrente](CURRENT_STATE.md) — evidenze reali e gate.
2. [Roadmap ufficiale in sei fasi](ROADMAP.md) — sequenza, decisioni, stop rule e risultati attesi.
3. [Audit UI-01…UI-22](validation/G1_D29_AUDIT_GUI_V13_V14.md) e [matrice originale](validation/G1_D19_MATRICE_PARITA_GUI.md).
4. [Piano validazione RC14](validation/G1_D29_RC14_TEST_PLAN.md), [diagnosi CS0136](design/G1_D29_RC14_FIX_CS0136.md), [piano RC13](validation/G1_D29_RC13_TEST_PLAN.md), [rapporto D29](design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md), [ADR 0038](adr/0038-roadmap-gate-gui-e-audit-matite.md).
5. [Registro delle decisioni](AUTHORITY_REGISTRY.md), [architettura](ARCHITECTURE.md) e [contratto GUI](PIXEL_ART_UI.md).

## Vincoli funzionali da non perdere

- **v13 immutata:** GUI `MainWindow`, 16 risorse, campagna `civil-growth-13`, SQLite v2 in `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`.
- **v14 opt-in:** `--experimental-v14`, SQLite v3 in `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`; nessuna migrazione o sovrascrittura automatica.
- **Risorse:** sette colonne (Risorsa, Quantità, Capacità, Produzione, Consumo, Saldo, Si riempie tra), quantità tabella arrotondate, dati precisi a tooltip, capacità reale e fonte, ordine alfabetico, scrollbar e selezione ocra; Pane uniformato.
- **Edifici:** Base/Grande/Avanzato **distinti**, no ComboBox livello. Prerequisiti mancanti ⇒ riga grigia ma cliccabile, requisiti leggibili, pulsanti costruzione nascosti. Risorse mancanti ⇒ pulsanti grigi/disabilitati, costo e causa visibili. Riga `Costruisci | +1 | +5 | +10`; riga `Lavoratore (−1) | +1 | +5 | +10`. Lotti cumulativi, upgrade separati, hover ocra immobile.
- **Scuola:** costruibile solo da Edifici, nella pagina Scuola ci sono progressi come Esercito, iscrizione/annullamento, assegnazione ricercatori e laboratorio Matite. Ogni iscritto spende **almeno 2 Matite**. Ricetta candidata: 1 Legno lavorato + 1 Carbone → 4 Matite. Niente «Esci senza salvare».
- **Albero:** saldo e prezzi visualizzati a due decimali; acquisto su comando esplicito, confronto esatto in millipunti, disabilitato se risorse/prerequisiti non soddisfatti. Le tecnologie senza effetto non sono acquistabili.
- **Esercito:** non perdere Caserme, Spade, reclute, tempi individuali, arruolamento/annullamento. Residenti non duplicati con lavoratori, ricercatori, studenti o soldati.
- **Economia:** prezzi +15%, livelli ×1/2,6/6,5, +50/+150/+500 di storage, tempo formazione 120 s, 0,1 punti/s ricercatore sono **candidati**, non valori approvati; livelli 1/3/10 personale sono requisito confermato.

## Modifica RC13 e correzione compilazione RC14

`src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.Parity.cs`: il tooltip Matite non usa più l'elenco generico dei bonus potenziali. Calcola la quota degli edifici realmente posseduti dalla definizione `StoragePerBuilding` e usa la capacità effettiva Core per la parte Scuola; indica quantità, spazio libero e totali. Due casi aggiunti a `tests/SimRisorse.Desktop.Tests/V14UiParityRegressionTests.cs`. **RC14:** rinominati esclusivamente `owned`→`ownedBuilding` e `free`→`freePencilSpace` all’interno del ramo Matite per risolvere il conflitto CS0136; invariato il risultato della formula.

**Non modificati:** `src/SimRisorse.Desktop/Views/*.axaml`, v13, Core, Application, Infrastructure, catalogo, schema e migrazioni SQLite.

## Comandi e prossimo lavoro

Windows 11 + .NET SDK 10 (`global.json`: 10.0.200, rollForward latestFeature):

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --experimental-v14
```

Eseguire 470 test previsti RC14 e riportare log/numero reale; RC13 non è arrivata ai test perché non compilava. Poi D30: confronto visivo per ciascuna delle 22 voci con screenshot alle due risoluzioni; correggere prima gli scostamenti P0. Non iniziare E4 o navale senza gate.

**Consegna obbligatoria a ogni incremento:** ZIP integrale, test e stato esplicito, changelog, roadmap, handoff, ADR se cambiano decisioni. Conservare snapshot originali e dati personali, senza incorporarli nello ZIP.

Per il contesto dettagliato precedente, la versione G1-D28 di questo documento è in [archivio storico](history/operational/HANDOFF_G1_D28_ARCHIVE.md).
