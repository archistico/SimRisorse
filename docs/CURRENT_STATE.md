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
