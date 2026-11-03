# G1-D31 / RC16 — Piano di validazione su Windows

**Baseline validata (comunicata dall'utente): RC15, 473 test attesi.** **Candidata: RC16, 476 test attesi (+3 Desktop).** Nessun test .NET eseguito nel confezionamento.

## Gate automatico

1. Estrarre lo ZIP **integrale** RC16 in una cartella nuova, senza sovrapporlo ad altre versioni. Requisiti: Windows 11, SDK .NET 10 come `global.json`.
2. Eseguire `./validate.cmd` dalla root. Atteso: **476 test passati, zero falliti e zero ignorati**; verificare anche compilazione `SimRisorse.Desktop` e `SimRisorse.Desktop.Tests`.
3. Se fallisce, conservare primo messaggio `error CS...` o nome/stack trace del test e, se disponibile, percorso `.trx`. Non dichiarare verde RC16 prima della conferma.

## Prova manuale UI-20 (v14)

- Avviare `./run-game.cmd --experimental-v14` **su copia di sicurezza del DB v14**, non su dati personali senza backup.
- Con **Segheria Base** sbloccata e materiali disponibili, verificare `Costruisci +1` verde/abilitato; cliccare **Nuova partita**: appare la conferma, il pulsante resta visibile ma diventa grigio e non cliccabile, come **Pausa/Riprendi**. Selezionare altre schede e verificare che non si possano mutare Esercito, Scuola, Matite o tecnologie.
- Cliccare **Annulla**: la vecchia colonia non viene modificata, i comandi tornano attivi secondo risorse e prerequisiti; la sessione resta in pausa finché non viene ripresa esplicitamente.
- Cliccare **Nuova partita → Avvia nuova**: viene creata una nuova colonia v14 in pausa. **Non premere Salva** su una partita reale se non vuoi sostituire il checkpoint sperimentale.
- Ripetere a **1040×740** e **1240×850**: nessuno spostamento di layout, pulsanti e informazioni leggibili, gli edifici bloccati restano grigi e selezionabili, Risorse conservano 7 colonne.

## Rischi aperti e stop rule

- `Salva`/`Carica` durante la conferma sono ancora gestiti secondo i comandi preesistenti della v13: controllare a parte la loro UX, senza presumerne la sicurezza totale in UI-21.
- Le altre voci UI-01…UI-22 non risultano automaticamente approvate da questi test. **P0 grafico aperto**, E4 non avviato; nessuna modifica ai salvataggi v13/v14.

[Rapporto RC16](../design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Audit](G1_D29_AUDIT_GUI_V13_V14.md) · [Roadmap](../ROADMAP.md).
