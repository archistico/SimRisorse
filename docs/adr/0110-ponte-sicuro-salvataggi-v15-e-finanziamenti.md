# ADR 0110 — Ponte opt-in fra salvataggio giocabile v15 e finanziamento composito

**Stato:** sperimentale, da verificare con `validate.cmd` su Windows. **Data:** 07/11/2026.

## Contesto

RC95–RC98 hanno creato uno store composito che conserva città v15, depositi e ricevute di finanziamento con commit atomico, ma è separato dai salvataggi giocabili. Non è sicuro aggiungere semplicemente un campo cantiere al file nativo né sostituire un salvataggio esistente con il database sperimentale. I prezzi della curva A restano proposte non approvate.

## Decisione RC99

È introdotto `E4bV15FundingSaveBridge` in Infrastructure.Experimental, **non collegato alla GUI**:

1. **Importazione non distruttiva:** legge un normale salvataggio v15 generation 6 in modalità sola lettura e crea un **nuovo** file composito sperimentale. Il sorgente originale resta intatto. Il file di destinazione deve non esistere e appartenere a una directory già esistente.
2. **Esportazione controllata:** legge e valida lo stato composito ed esporta lo snapshot della città in un **nuovo** file nativo v15 generation 6. Rifiuta qualunque preventivo/versamento pendente, anche se non è ancora stata effettuata alcuna rata, perché il formato giocabile non rappresenta quel contratto.
3. **Promozione con staging:** la nuova destinazione viene preparata con un file temporaneo nella stessa directory, caricata e verificata prima di essere pubblicata con `File.Move` senza overwrite. Un errore lascia intatti sorgente e destinazione preesistenti.
4. **Conservazione delle prove:** il file composito originale, contenente ricevute e storico dei versamenti, resta disponibile. Le ricevute **non vengono serializzate nel formato v15 nativo**: esportare non equivale ad attivare il finanziamento nel gioco. Nel salvataggio nativo vengono esportati soltanto la città e gli edifici completati e già registrati nello snapshot.
5. **Nessun automatismo:** non viene implementato alcun comando di gameplay che importi, esporti o sovrascriva il salvataggio dell'utente. Le operazioni sono API esplicite riservate al laboratorio e ai test.

## Garanzie e limiti

- Non eseguire un export mentre l'applicazione può continuare a scrivere il contenitore composito: l'export rappresenta uno **snapshot coerente di una revisione**, non un blocco persistente della sessione sorgente. Il flusso di cambio autorità tra salvataggi è ancora una fase futura.
- `File.Move` non sovrascrive i salvataggi esistenti. Il bridge rifiuta path identici, destinazione già occupata, file esteri e finanziamenti incompleti.
- Nessuna migrazione automatica della GUI, nessuna nuova tabella SQLite del salvataggio giocabile, nessun nuovo prezzo in partita, nessun rifornimento sintetico.
- L'esportazione di una Zecca pagata in laboratorio non prova la raggiungibilità naturale della curva A e non conserva lo storico escrow nel salvataggio di gioco.

## Test

12 casi in `Rc99NativeSaveBridgeTests` coprono import, export, Zecca completata, deposito incompleto, riuso dei path, destinazioni esistenti, file di formato estraneo, percorsi mancanti e ripulitura dei temporanei. **1282 test cumulativi attesi**, con RC95 (1245) ultima baseline confermata; RC96–RC99 devono essere verificati su Windows.

## Prossimo passo

Decidere un modello autorevole unico (città e cantieri nello stesso salvataggio giocabile) e gestire cambio di autorità con consenso, backup e rollback prima dell'attivazione della GUI. Poi benchmark di sostenibilità naturale della curva A.
