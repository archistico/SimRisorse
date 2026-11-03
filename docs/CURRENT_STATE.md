# Stato operativo — SimRisorse RC14 / G1-D29-F1

**Data:** 03/11/2026. **Ultimo gate Windows verde comunicato dall'utente:** **RC12**, 468 test attesi, test dichiarati passati (TRX non allegato). **RC13:** build fallita per due CS0136 in `V14PreviewViewModel.Parity.cs`, quindi test RC13 non eseguiti. **RC14:** patch di soli identificatori locali; **470 test attesi**, non eseguiti nell'ambiente di confezionamento per assenza SDK .NET 10. **Audit visivo P0:** ancora aperto.

## Cosa è implementato e cosa resta invariato

- RC13 ha introdotto nella **sola presentazione v14** il tooltip Capacità di Matite: conta Municipio/Magazzini/Scuole realmente costruiti, mostra scorta e spazio libero; 2 test nuovi per stato iniziale e caricamento checkpoint.
- **Invariati:** `MainWindow` v13, catalogo e motore economico, scuola/formazione/ricetta Matite, XAML, SQLite v2/v3 e percorsi di salvataggio.
- **Gioco v13:** `civil-growth-13`, 16 risorse, avvio normale `./run-game.cmd`, SQLite v2; da preservare.
- **Anteprima v14:** avvio `./run-game.cmd --experimental-v14`, 17 risorse con Matite, SQLite v3 isolato, GUI pixel art classica con nuove Scuola e Albero tecnologico.
- Edifici Base/Grande/Avanzato sono righe separate; costruzione +1/+5/+10, lavoratori −1/+1/+5/+10; bloccati grigi ma selezionabili; ricerca a due decimali e acquisto condizionato ai punti effettivi.
- I valori quantitativi del prototipo non sono ancora bilanciati; **2 Matite per studente** e **1/3/10 lavoratori** sono requisiti confermati.

## Gate aperti

1. **RC14 Windows:** `./validate.cmd`, **470 test attesi**; verificare assenza CS0136 e registrare conteggio e TRX.
2. **P0 UI:** completare checklist UI-01…UI-22 e confronto 1040×740/1240×850; un test verde non sostituisce l'approvazione grafica.
3. Dopo P0: verifiche complete Scuola/tecnologie, E4a numerico, E4b con approvazione costi, E4c con backup/migrazione, infine M5.4–M7.

**Prossimo passo consigliato:** validare RC14 su Windows; poi continuare da D30 con audit visuale e correzioni dei problemi reali, non nuove meccaniche.

[Roadmap](ROADMAP.md) · [Report RC13](design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md) · [Test plan](validation/G1_D29_RC13_TEST_PLAN.md) · [Handoff](HANDOFF.md).

[Diagnosi RC14](design/G1_D29_RC14_FIX_CS0136.md) · [Piano RC14](validation/G1_D29_RC14_TEST_PLAN.md).
