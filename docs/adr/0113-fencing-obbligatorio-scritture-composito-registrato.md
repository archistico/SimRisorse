# ADR 0113 — Protezione delle scritture sui compositi registrati

**07/11/2026 · RC102 · Stato: candidata, test .NET 10 non ancora confermati.**

## Problema

RC101 serializza `Apply` e `AdvanceClock` quando l'operatore usa il registro d'autorità, ma lo store composito espone ancora API di scrittura dirette. Un'istanza vecchia può cambiare il file registrato ignorando il token, e gli snapshot potrebbero divergere.

## Decisione

- La registrazione `RegisterInitial` lega il composito al **percorso canonico del registro** in una tabella `composite_authority_guard` e installa otto trigger SQLite su scritture di `composite_state`, `composite_receipt` e modifiche al legame d'autorità. Le scritture ordinarie non dispongono della funzione SQLite locale richiesta dai trigger e vengono rifiutate. Nessun riaccredito di risorse o cambio al motore di gioco.
- `E4bV15FundingAuthorityRegistry.Apply` e `AdvanceClock` continuano a mantenere la transazione di scrittura sul registro per tutta l'operazione e autorizzano la singola connessione del composito **solo dopo** aver verificato il legame. `Create` e i metodi diretti `Apply`/`AdvanceClock` falliscono su file protetti, anche da oggetti creati in precedenza.
- Backup e ripristini SQLite ereditano tabella e trigger. Una promozione richiede digest identici prima e dopo la protezione della destinazione, aggiorna epoca/token e lascia protetta la copia originaria. Una copia del registro salvata **in un percorso diverso** non può autorizzare il composito originale.
- I registri RC101 esistenti vengono protetti in-place alla riapertura, sotto il lock del registro. I compositi **mai registrati** restano scrivibili direttamente per compatibilità con le prove storiche; non sono considerati salvataggi autorevoli.

## Garanzie e limiti

La protezione è una barriera contro accessi diretti accidentali tramite le API applicative e SQLite ordinario, **non** un controllo di sicurezza contro un editor che può cambiare lo schema SQLite, registrare artificialmente la funzione o sostituire file sul disco. Il registro è un secondo database: non esiste una transazione distribuita tra registro e composito; se un'operazione di inizializzazione si interrompe dopo l'installazione dei trigger, il file può restare protetto ma senza autorità registrata (fail closed, recupero manuale consapevole). Copie preesistenti non ancora registrate restano indipendenti. Nessuna garanzia multi-host contro manomissioni, dischi condivisi non affidabili o copia del registro allo stesso percorso in un altro ambiente.

Il salvataggio giocabile v15 generation 6, GUI pixel art, prezzi normali, Municipio (100 Carbone e 2 Matite), squadre MintTen e produzione non vengono modificati. La curva A e i cantieri restano sperimentali.

## Validazione

12 nuovi test `Rc102MandatoryCompositeFenceTests` verificano i percorsi legacy, SQL diretto, marcatura d'autorità, backup, restore, promozione e registri esterni. Il test RC101 di alterazione deliberata della ricevuta viene adattato per simulare un editor privilegiato, conservando il controllo del digest. **1318 test cumulativi attesi**; ultimo gate confermato RC95 con **1245 superati**. Comando unico su Windows: ` .\validate.cmd `. Non distribuire `.local-cache`, manifest o `validate-f1.cmd`.
