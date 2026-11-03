# Handoff aggiornato — RC20 / G1-D35

**Ultimo test Windows dichiarato superato:** RC16 (476 test). **Versioni non testate:** RC17 (481), RC18 (484), RC19 (487), **RC20 (490 attesi)**. RC20 include tutte le versioni intermedie; testare direttamente lo ZIP integrale RC20.

**RC20 contiene solo test e documentazione.** Nuovo `tests/SimRisorse.Desktop.Tests/V14ResourcesAuditTests.cs` (3 casi) per verificare le sette colonne della GUI v14, gutter scrollbar, sei colonne numeriche, selezione ocra, tooltip Capacità, progressione tick, ordinamento e 17 risorse. La GUI v13, la GUI v14, il motore e gli schemi SQLite sono invariati. Nessuna correzione grafica è stata inventata senza una regressione dimostrata.

**Prossimo G1-D36:** audit Edifici. Non perdere le due righe Costruisci/Lavoratore, edifici bloccati grigi ma selezionabili, costi cumulativi e stati `IsEffectivelyEnabled`. I test RC17 del checkpoint sono un gate delicato ancora da validare. P0 resta aperto finché Windows e screenshot non confermano le 22 voci; E4 e navale sospesi.

[Rapporto](design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Piano RC20](validation/G1_D35_RC20_TEST_PLAN.md) · [Roadmap](ROADMAP.md).

---

# Handoff aggiornato — RC19 / G1-D34

**RC16 è l'ultimo gate confermato:** 476 test Windows. **RC17** (481), **RC18** (484) non testate. **RC19** include RC17+RC18 e introduce 3 test, **487 totali attesi**, senza compilazione/test Windows in questo ambiente.

**Patch RC19:** `src/SimRisorse.Desktop/Presentation/V14PreviewViewModel.Parity.cs` allinea i suggerimenti Esercito al blocco dei comandi durante la conferma «Nuova partita?» e mostra deficit numerici secondo lotto. Corretto «1 recluta». Nuovo `tests/SimRisorse.Desktop.Tests/V14MilitaryUiParityTests.cs` (3 casi). Invariati GUI v13, XAML, Core, Application, Infrastructure, SQLite, coefficienti, salvataggi. **RC17 è una modifica importante della protezione del checkpoint, ancora da convalidare su copia.**

**Prossimo lavoro:** eseguire quando possibile ` .\validate.cmd` su RC19 (487 previsti); poi audit Risorse/Edifici e delle 22 voci a due risoluzioni. Non dichiarare P0 approvato con soli test automatici. E4 e navale restano sospesi.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Rapporto D34](design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano RC19](validation/G1_D34_RC19_TEST_PLAN.md).

---

# Handoff aggiornato — RC18 / G1-D33

**RC16:** 476 test Windows confermati dall'utente. **RC17:** 481 attesi, utente temporaneamente impossibilitato a testarli. **RC18:** 484 attesi, non compilati/eseguiti qui. Partire **dallo ZIP integrale RC18**, che contiene anche RC17; non dichiarare verde fino al test Windows.

**RC18:** migliorati i messaggi di assegnazione dei ricercatori nei lotti, con posti di ricerca insufficienti anche se ne resta almeno uno; 3 test nuovi. Invariati algoritmo economico, Core, SQLite, v13, XAML v14. **RC17:** continua a proteggere checkpoint/autosalvataggio in nuova partita; richiede la validazione su Windows.

Preservare tabella Risorse a sette colonne, lista Edifici Base/Grande/Avanzato, pulsanti Costruisci +1/+5/+10 e Lavoratore −1/+1/+5/+10, Esercito, Scuola/Matite, ricerca a due decimali, GUI retro v13. Gate P0 non chiuso. Prossima priorità: audit Risorse, Edifici ed Esercito e screenshot a 1040×740 / 1240×850. Niente E4, navale né cambi di DB.

[Stato](CURRENT_STATE.md) · [Roadmap](ROADMAP.md) · [Matrice](validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Rapporto D33](design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Piano test D33](validation/G1_D33_RC18_TEST_PLAN.md).

---

# Handoff aggiornato — RC17 / G1-D32

**Ultimo gate Windows confermato:** RC16 con **476 test passati**. **Candidata RC17:** **481 test attesi** (+5 Desktop), test .NET non eseguiti in questo ambiente. D32 corregge la collisione Salva/Carica con la conferma «Nuova partita» e impedisce che il timer/chiusura sovrascriva il vecchio checkpoint v14 dopo Avvia nuova prima di un **Salva v14** manuale. Chiudere durante la conferma la annulla e salva la vecchia colonia. I salvataggi v13 e v14 restano separati. **Gate P0 visivo aperto, E4 rimandato.**

**Leggere per la ripresa:** [CURRENT_STATE](CURRENT_STATE.md), [ROADMAP](ROADMAP.md), [audit 22 voci](validation/G1_D29_AUDIT_GUI_V13_V14.md), [rapporto G1-D32](design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md), [piano RC17](validation/G1_D32_RC17_TEST_PLAN.md), [ADR 0039](adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md).

---

# Handoff tecnico completo — SimRisorse RC16 / G1-D31

**Baseline validata:** RC15 (473 test attesi, dichiarati superati dall’utente). **Candidata RC16:** +3 test Desktop, 476 attesi e non eseguiti nell’ambiente di produzione. La RC16 blocca le azioni che modificano la colonia durante «Nuova partita?», fino a Conferma o Annulla; mantiene visibilità e dettagli della GUI e preserva l’intero modello/salvataggi. Patch D30: capienza Matite nel dettaglio Edifici (Municipio/Magazzino/Scuola) e disabilitazione Pausa/Riprendi durante conferma nuova partita, come v13. Il Core v14 introduce soltanto una costante nominata per `50` posti Matite/Scuola senza modifica di formula; GUI v13, XAML, SQLite e salvataggi invariati. **Gate grafico P0 aperto; E4 sospeso.**

## Ordine di lettura

1. [Stato corrente](CURRENT_STATE.md) — evidenze reali e gate.
2. [Roadmap ufficiale in sei fasi](ROADMAP.md) — sequenza, decisioni, stop rule e risultati attesi.
3. [Audit UI-01…UI-22](validation/G1_D29_AUDIT_GUI_V13_V14.md) e [matrice originale](validation/G1_D19_MATRICE_PARITA_GUI.md).
4. [Piano validazione RC16](validation/G1_D31_RC16_TEST_PLAN.md), [rapporto RC16](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md), [Piano validazione RC15](validation/G1_D30_RC15_TEST_PLAN.md), [report D30](design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md), [piano storico RC14](validation/G1_D29_RC14_TEST_PLAN.md), [diagnosi CS0136](design/G1_D29_RC14_FIX_CS0136.md), [piano RC13](validation/G1_D29_RC13_TEST_PLAN.md), [rapporto D29](design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md), [ADR 0038](adr/0038-roadmap-gate-gui-e-audit-matite.md).
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

Eseguire **476 test previsti RC16** e riportare log/numero reale (RC15 già validata); RC13 non è arrivata ai test perché non compilava. Poi D30+: confronto visivo per ciascuna delle 22 voci con screenshot alle due risoluzioni; correggere prima gli scostamenti P0. Non iniziare E4 o navale senza gate.

**Consegna obbligatoria a ogni incremento:** ZIP integrale, test e stato esplicito, changelog, roadmap, handoff, ADR se cambiano decisioni. Conservare snapshot originali e dati personali, senza incorporarli nello ZIP.

Per il contesto dettagliato precedente, la versione G1-D28 di questo documento è in [archivio storico](history/operational/HANDOFF_G1_D28_ARCHIVE.md).

[Report RC15](design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Piano RC15](validation/G1_D30_RC15_TEST_PLAN.md).

**RC16 / D31:** guardia `CanMutateColony` nelle azioni mutanti dei ViewModel v14 e test di disabilitazione; il salvataggio di prova e le verifiche visuali restano pendenti. [Rapporto RC16](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](validation/G1_D31_RC16_TEST_PLAN.md).
