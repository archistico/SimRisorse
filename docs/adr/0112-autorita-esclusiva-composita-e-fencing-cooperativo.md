# ADR 0112 — Registro dell'autorità e cambio controllato del ramo composito

**Data:** 07/11/2026. **Stato:** sperimentale RC101, non ancora validato con .NET 10/Windows.

## Problema

RC100 può ripristinare un finanziamento anche incompleto in un secondo database. Se originale e copia vengono modificati separatamente, i due salvataggi divergono e il gioco non dispone di un criterio sicuro per fonderli. Un semplice confronto del numero di revisione non basta: due rami possono avere revisioni uguali e contenuti/ricevute diversi.

## Decisione

- In `Infrastructure.Experimental`, introdurre `E4bV15FundingAuthorityRegistry`: file **SQLite separato** dal composito e dal salvataggio giocabile. Un registro identifica il **solo percorso attivo**, l'**epoca** progressiva e un token di fencing.
- L'operatore registra **una volta** un database composito esistente e già validato. Tutti gli scrittori **cooperanti** usano `Apply` e `AdvanceClock` del registro, anziché chiamare direttamente lo store. Ogni operazione tiene una transazione SQLite immediata sul registro per l'intera mutazione dello store, serializzando i processi che usano lo stesso registro.
- `Promote(lease, restoredPath)` accetta soltanto una copia presente, valida e con **uguale contenuto durevole**: immagine SQLite v15 incorporata, JSON del finanziamento e tutte le ricevute (ID, payload e revisione). Fonte e destinazione devono essere file distinti. Non è ammesso promuovere un backup che ha perso pagamenti successivi o ricevute differenti.
- La promozione incrementa l'epoca e sostituisce il token con un nuovo valore **nella stessa transazione del registro**. Le sessioni che conservano il token precedente non possono più scrivere attraverso questa API. Il ramo vecchio **non viene cancellato né sovrascritto**.
- Nessuna promozione automatica dopo backup/ripristino. La nuova copia diventa autorevole solo con un comando esplicito e una corrispondenza completa.

## Confini delle garanzie

È un protocollo **cooperativo**, non un sistema di sicurezza e non un lock universale dei file SQLite. Chi apre direttamente `E4bV15FundingCompositeSqliteStore`, copia il file del registro, usa un registro diverso, modifica gli archivi fuori dal protocollo o usa un altro host senza il registro condiviso **può aggirare il fencing**. Il token non è una credenziale segreta. Inoltre il controllo del contenuto non è una prova di autenticità contro un avversario capace di manipolare tutti i file. Non usare la nuova API per garantire un'autorità globale distribuita o per abilitare il finanziamento nella GUI.

Il registro rimane **esterno** al composito: non esiste una transazione distribuita tra i due database. L'operazione ordinaria del composito resta atomicamente protetta nel suo database; il blocco dell'autorità resta detenuto per l'intera operazione. La promozione confronta i contenuti prima di trasferire il token. Non introdurre scadenze o takeover automatici che potrebbero consentire due proprietari contemporanei.

Il normale salvataggio v15 generation 6, i prezzi, i costi e la GUI rimangono invariati. Non è ancora consentita una migrazione in-place con cantieri pendenti.

## Verifiche e rollback

12 nuovi casi `Rc101FundingAuthorityTests` verificano inizializzazione, formati estranei, riavvio, replay, clock, promozione di una copia identica, revoca della vecchia lease, rifiuto di backup obsoleti, alterazioni delle ricevute e token falsi. Gate atteso **1306 test**, baseline Windows confermata RC95 **1245 test**. Un fallimento della promozione conserva il registro precedente; i file sorgente non sono cancellati. La validazione Windows avviene con il solo `validate.cmd`.
