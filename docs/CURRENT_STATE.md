# Stato corrente — RC20 / G1-D35

- **Baseline verde Windows:** RC16, 476 test. RC17 (481), RC18 (484), RC19 (487) non testate. **RC20 candidata: 490 test attesi**, di cui +3 test Desktop, non eseguiti qui.
- **Audit Risorse UI-02/UI-03/UI-04:** nuova protezione automatica per 7 colonne, scrollbar, selezione ocra, tooltip, refresh e ordine delle 17 risorse. Nessuna variazione del codice di produzione perché il confronto statico non dimostra una regressione.
- **P0 visivo aperto** su 22 voci, alle risoluzioni 1040×740 e 1240×850. **G1-D36 Edifici** segue dopo questo checkpoint. P1 Scuola/tecnologie ed E4 differiti.
- [Rapporto](design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Piano Windows](validation/G1_D35_RC20_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md).

---

# Stato corrente — RC19 / G1-D34

**Baseline Windows confermata:** RC16, **476 test superati**. RC17 (481) e RC18 (484) **non verificate**; nuova candidata **RC19: 487 test attesi (+3 Desktop)**. .NET 10 e xUnit non eseguiti nell'ambiente di confezionamento. La RC19 incorpora RC17 e RC18.

- **UI-15/UI-20:** suggerimenti Esercito allineati ai comandi durante la conferma «Nuova partita?»; quantificati deficit Spade, abitanti, reclute annullabili e capienza rimborso; corregge «1 recluta».
- Tre test, di cui uno headless Avalonia. Un solo file C# v14 di presentazione modificato. Invariati XAML, motore, regole militari, GUI v13, SQLite e salvataggi.
- **P0 visuale ancora aperto** sulle 22 voci e sulle due risoluzioni 1040×740 / 1240×850. Risorse ed Edifici restano nell'audit successivo; E4 sospeso.

[Rapporto](design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano Windows](validation/G1_D34_RC19_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md).

---

# Stato corrente — RC18 / G1-D33

**RC16:** ultima versione validata dall'utente, **476 test passati**. **RC17 non verificata:** 481 attesi. **RC18 candidata:** 484 test attesi, nessuna esecuzione .NET 10 nell'ambiente di confezionamento. P0 visivo **aperto**; E4 non avviato.

- G1-D33 corregge solo il testo del suggerimento di assegnazione ricercatori: quantità +1/+5/+10, posti disponibili effettivi, assenza Scuola e qualificati liberi insufficienti. I comandi mantengono i controlli originali.
- Tre test Desktop aggiunti. Nessun cambiamento alle regole di popolazione, produzione, capacità, ricerca, persistenza, GUI v13, né allo XAML della v14.
- Il pacchetto RC18 **include la protezione dei checkpoint RC17 ancora da validare**, con tutti i 5 test introdotti da RC17. Non trattare RC17 come verde.

[Rapporto](design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Test](validation/G1_D33_RC18_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Handoff](NEW_CHAT_HANDOFF.md).

---

# Stato corrente — RC17 / G1-D32

**Baseline validata Windows:** RC16 (**476 test superati dichiarati dall'utente, log/TRX non allegati**). **Candidata RC17:** +5 test Desktop, **481 attesi** e non ancora compilati/eseguiti qui (manca SDK .NET 10). **P0 parità grafica:** ancora aperto. **E4:** non avviato.

- **UI-20/UI-21:** Salva, Carica e ripetizione di Nuova partita bloccati durante la conferma; annullamento ripristina la sessione precedente.
- Dopo conferma «Avvia nuova», il **checkpoint precedente resta intatto anche con autosalvataggio a minuto e chiusura**, fino a un Salva v14 riuscito; quel Save riattiva l'autosalvataggio.
- Chiudere durante la conferma equivale ad annullarla, consentendo il normale salvataggio della colonia precedente. Protezione dei DB incompatibili invariata.
- Solo **2 file C# v14 di presentazione/lifecycle** e **1 file test Desktop** modificati, oltre a documentazione. GUI v13, XAML, motore e SQLite invariati.

[Rapporto](design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Piano test](validation/G1_D32_RC17_TEST_PLAN.md) · [Roadmap](ROADMAP.md) · [Audit](validation/G1_D29_AUDIT_GUI_V13_V14.md).

---

# Stato operativo — SimRisorse RC16 / G1-D31

**Data:** 03/11/2026. **Ultimo gate Windows verde comunicato:** **RC15, 473 test attesi dichiarati passati**. **RC16:** protegge tutti i comandi che mutano la colonia durante la conferma «Nuova partita?»; **476 test attesi non eseguiti qui** per assenza SDK .NET 10. **Audit visivo P0:** resta aperto.

## Cosa è implementato e cosa resta invariato

- **RC16:** l’avviso di nuova partita non lascia più operativi acquisti di edifici, personale, ricerche, Scuola/Matite o Esercito; i comandi tornano disponibili dopo «Annulla». Nessun XAML o motore modificato; +3 test Desktop. Salva/Carica restano da approfondire in UI-20/21.

- RC15 completa i dettagli «Capacità» di Municipio, Magazzino e Scuola aggiungendo le rispettive Matite; la v14 blocca Pausa/Riprendi durante la conferma di nuova partita, come la v13. Tre nuovi test Desktop.
- RC13 ha introdotto nella **sola presentazione v14** il tooltip Capacità di Matite: conta Municipio/Magazzini/Scuole realmente costruiti, mostra scorta e spazio libero; 2 test nuovi per stato iniziale e caricamento checkpoint.
- **Invariati:** `MainWindow` v13, catalogo e motore economico, scuola/formazione/ricetta Matite, XAML, SQLite v2/v3 e percorsi di salvataggio.
- **Gioco v13:** `civil-growth-13`, 16 risorse, avvio normale `./run-game.cmd`, SQLite v2; da preservare.
- **Anteprima v14:** avvio `./run-game.cmd --experimental-v14`, 17 risorse con Matite, SQLite v3 isolato, GUI pixel art classica con nuove Scuola e Albero tecnologico.
- Edifici Base/Grande/Avanzato sono righe separate; costruzione +1/+5/+10, lavoratori −1/+1/+5/+10; bloccati grigi ma selezionabili; ricerca a due decimali e acquisto condizionato ai punti effettivi.
- I valori quantitativi del prototipo non sono ancora bilanciati; **2 Matite per studente** e **1/3/10 lavoratori** sono requisiti confermati.

## Gate aperti

1. **RC16 Windows:** `./validate.cmd`, **476 test attesi**; confermare build e test, registrare conteggio e TRX.
2. **P0 UI:** completare checklist UI-01…UI-22 e confronto 1040×740/1240×850; un test verde non sostituisce l'approvazione grafica.
3. Dopo P0: verifiche complete Scuola/tecnologie, E4a numerico, E4b con approvazione costi, E4c con backup/migrazione, infine M5.4–M7.

**Prossimo passo consigliato:** validare RC16 su Windows e confrontare la GUI alle due viewport. Continuare l’audit D30+ fino a chiusura UI-01…UI-22.

[Roadmap](ROADMAP.md) · [Report RC13](design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md) · [Test plan](validation/G1_D29_RC13_TEST_PLAN.md) · [Handoff](HANDOFF.md).

[Diagnosi RC14](design/G1_D29_RC14_FIX_CS0136.md) · [Piano RC14](validation/G1_D29_RC14_TEST_PLAN.md).

[Correzioni RC15](design/G1_D30_RC15_PARITA_DETTAGLI_E_CONFERMA.md) · [Piano RC15](validation/G1_D30_RC15_TEST_PLAN.md).

[Rapporto RC16](design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](validation/G1_D31_RC16_TEST_PLAN.md).
