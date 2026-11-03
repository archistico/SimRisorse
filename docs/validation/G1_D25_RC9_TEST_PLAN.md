# G1-D25 — Piano di verifica RC9, Scuola e Matite

**Baseline automatica confermata:** RC8 **456 test superati** (comunicato dall'utente il 3/11/2026). **Nuovo obiettivo RC9:** 464 test (8 nuovi), non ancora eseguiti in questo ambiente; audit grafico manuale aperto.

## Test automatici — Windows 11, .NET 10

Estrarre lo ZIP RC9 in una cartella pulita, **non sovrascrivere la cartella RC8**. Eseguire:

```powershell
.\validate.cmd
```

Verificare nel `.trx` **464/464**, senza fallimenti o test ignorati. Test aggiunti: `V14SchoolSuppliesTests` (4 Core/Application), `V14SchoolPencilPersistenceTests` (2 Infrastructure), `SchoolReadModelShowsPencilRequirementsAndBatchCosts` (1 Desktop), `SchoolPanelUsesMilitaryStyleProgressAndHasNoDiscardExit` (1 Desktop). Altri test esistenti aggiornati per la 17ª risorsa sperimentale, senza modificare aspettative v13 a 16 risorse.

## Prove manuali obbligatorie

Eseguire prima un **backup** del file v14 già esistente. Avviare `./run-game.cmd --experimental-v14` con la RC9 e controllare separatamente la v13 con `./run-game.cmd`. Ripetere le prove a **1040×740** e **1240×850**.

| Prova | Risultato previsto | Esito |
| --- | --- | --- |
| Footer nelle due modalità | Non appare «Esci senza salvare»; Salva/Carica e chiusura standard disponibili | [ ] |
| Scuola senza costruzione | Non compare il tasto Costruisci; riga Scuola ancora acquistabile da Edifici | [ ] |
| Scuola vuota | Coda, in formazione, qualificati e ricercatori tutti a zero; barra a 0%; nessun testo tagliato | [ ] |
| Fabbricazione impossibile senza Scuola | Pulsante «Produci +4 Matite» non attivo, causa esplicita | [ ] |
| Fabbricazione con Scuola e materiali | 1 Legno lavorato + 1 Carbone consumati, 4 Matite prodotte in magazzino | [ ] |
| Scorte insufficienti | Nessuna fabbricazione o iscrizione; pulsante disabilitato e spiegazione leggibile | [ ] |
| Matite in Risorse | 17ª riga alfabetica, icona, quantità/capacità, tooltip capienza; **sette colonne ancora presenti** | [ ] |
| Iscrizione di 1 / 5 / 10 | Ogni studente richiede 10 Cibo e **2 Matite**; consumo esatto, nessun abitante duplicato | [ ] |
| Formazione | Primo studente avanza per 120 s, barra e countdown aggiornati, coda visualizzata | [ ] |
| Qualifica e ricerca | Studente diventa ricercatore qualificato; dopo Assegna genera ricerca senza acquisto automatico | [ ] |
| Annullamento | Restituisce Cibo e Matite pagate **soltanto se esiste spazio**, altrimenti transazione respinta | [ ] |
| Salvataggio RC9 | Craft/Studenti/Progresso/Matite/contatore conservati a ricaricamento | [ ] |
| Vecchio salvataggio RC8 con studenti | Caricato e preservato; annullamento senza rimborso fittizio di Matite; backup v14 creato su nuovo Salva | [ ] |
| Baseline v13 | Rimane a 16 risorse, SQLite v2 immutato, Esercito/Risorse/Edifici operativi | [ ] |
| Grafica e pulsanti RC8 | Edifici grigi e selezionabili, Costruisci/hover, lavoratori, Esercito e Pane invariati | [ ] |

## Gate e reporting

Annotare test totali e `.trx`, eventuali eccezioni, confronto della tabella Risorse e screenshot in entrambe le dimensioni. **Solo una suite verde Windows confermerà la RC9 automaticamente**, e solo il confronto grafico potrà chiudere P0. E4 e navale restano in attesa.

[Report RC9](../design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [ADR 0035](../adr/0035-scuola-formazione-matite-v14.md) · [Stato](../CURRENT_STATE.md).
