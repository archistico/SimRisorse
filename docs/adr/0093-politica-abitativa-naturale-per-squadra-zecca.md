# ADR 0093 — RC85: politica abitativa naturale prima della Zecca

- **Data:** 06/11/2026
- **Stato:** implementata come **esperimento isolato**, candidata alla validazione .NET; non approva nuovi costi, prezzi o modalità predefinite.
- **Dipendenze:** ADR 0089 (MintTen), 0090 (naturale), 0091 (seconda Zecca), 0092 (audit di evidenze).

## Contesto

Le strategie `research-first` e `bread-first` RC82 differiscono principalmente per il momento di costruzione della Scuola. Entrambe chiedono 10 addetti alla Zecca, ma nessuna investe esplicitamente in abitazioni **aggiuntive** dopo la filiera del Pane. Un'attesa di popolazione a fine catena non giustifica aumentare artificialmente gli abitanti né concludere che la partita abbia un softlock. L'utente ha confermato RC82 (1002 test); RC83 e RC84 non sono ancora stati eseguiti su .NET in questo ambiente.

## Decisione

1. Aggiungere `housing-buffer` come **terza strategia opzionale** esclusiva dello scenario Zecca `V15-I03`, applicabile a `MintTen` e al suo controllo `Legacy`, senza cambiare i due script storici o il loro comportamento predefinito.
2. `housing-buffer` riproduce `bread-first` e acquista **due Case piccole aggiuntive**, con i normali prezzi e comandi di costruzione, subito prima della filiera dell'Oro. Si tratta di un investimento deliberato, non di popolazione o materiali regalati. La crescita degli abitanti usa esclusivamente il clock della campagna.
3. Conservare l'assegnazione atomica `staff-mint-team` di **10 persone** nel regime MintTen, con regole versionate. Il nuovo studio non abilita MintTen nella GUI.
4. Estendere la CLI naturale con `--policy housing-buffer` e `--include-housing-buffer`. Il default continua a eseguire soltanto `research-first` e `bread-first`. `--compare-legacy` produce un confronto omogeneo anche per la terza strategia.
5. Introdurre `E4bMintHousingBufferStudy` e comando **offline** `--review-e4b-housing-buffer --input RESULTS_JSON [--output NEW_DIRECTORY]`. Rileggono l'audit RC84 e richiedono una coppia `bread-first`/`housing-buffer` nel regime MintTen; il controllo Legacy è facoltativo.
6. I pagamenti di legname e pietra comprendono i **costi effettivamente registrati** per tutti gli acquisti, non prezzi stimati. Si registrano soltanto case effettivamente acquistate. Il delta dei tempi è calcolato **solo** quando entrambe le strategie completano il run naturale a orizzonte dichiarato, con evento di Monete reale e prova dell'assegnazione.
7. Correggere il contatore RC84 `CompleteNaturalPolicyPairObserved`: deve richiedere *specificamente* `research-first` e `bread-first`, non due politiche qualsiasi; `housing-buffer` non può sostituire una delle due.
8. Il gate usa solo **sei diagnostiche a t=0** e un riesame offline; nessuna misura lunga o probabilità di successo è inventata.

## Non-obiettivi

Nessuna modifica a Core, Desktop, SQLite, salvataggi o economia attiva. La curva A/B/C, la capienza aggiuntiva, l'escrow e l'abilitazione di default restano decisioni aperte. Aggiungere case può aumentare le spese iniziali: la nuova politica potrebbe anche peggiorare il tempo della prima Moneta; questo è un risultato ammissibile. Un timeout prova soltanto il mancato completamento **di quello script**, non il blocco del gioco.

## Uscita RC85

Validare il gate cumulativo (**1058 test attesi**); eseguire successivamente le tre politiche naturali a orizzonte pieno con e senza Legacy, conservare il JSON grezzo, analizzarlo con RC84 e RC85 e verificare dove si incontrano eventuali attese. Se nessuno script completa, ricercare ulteriori strategie senza alterare il motore per far diventare verde il benchmark. Solo dopo approvare la politica economica e gli interventi di bilanciamento.
