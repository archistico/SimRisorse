# 0006 — Precisione delle risorse e prenotazioni nel passo economico

- Stato: Accettato
- Data: 2026-10-26
- Ambito: Core M2 e scenario economico di riferimento
- Origine: generalizzare la prova M1 mantenendo risultati ripetibili e rendendo esplicita la contesa tra attività

## Contesto

Il catalogo definitivo deve poter descrivere risorse e ricette senza modificare un enum per ogni nuovo contenuto. La produzione può richiedere quantità frazionarie e più attività possono contendersi lo stesso materiale o spazio di deposito. Una previsione calcolata separatamente per ogni edificio potrebbe promettere risorse che non bastano per tutti.

## Decisione

### Quantità e contenuti

Usare `ResourceAmount`, valore non negativo in millesimi di unità rappresentati da `long`. La conversione da `decimal` rifiuta quantità con più di tre decimali, senza arrotondamento implicito. Somme e prodotti controllano l'overflow. Persone, assegnazioni e quantità di edifici rimangono intere.

Usare vettori immutabili di risorse e identificatori stabili ASCII minuscoli. Catalogo, ricette, requisiti e dotazione dello scenario sono separati dallo stato mutabile della partita. Il catalogo controlla duplicati, riferimenti e cicli nei prerequisiti edilizi; non dimostra da solo la raggiungibilità economica di ogni contenuto.

### Passo economico

Il passo economico dura un secondo. Ogni lavoratore assegnato può eseguire una ricetta completa per passo. Tutte le attività prenotano input e spazio dalle disponibilità all'inizio del passo. Gli output prodotti e lo spazio liberato dai consumi sono utilizzabili al passo successivo.

Le prenotazioni seguono `AllocationPriority` crescente, poi l'identificatore dell'edificio con confronto ordinale. L'ordine di costruzione, inserimento in dizionario o caricamento delle definizioni non stabilisce la precedenza. Con risorse insufficienti lavorano solo le persone per cui una ricetta completa è sostenibile.

Il medesimo pianificatore calcola avanzamento e previsioni, considerando insieme tutti gli edifici. Le letture non modificano la partita. La priorità è per tipo di edificio; una ripartizione equa o priorità modificabili dal giocatore non sono implementate in M2.

### Azioni, tempo e proprietà dello stato

Acquisti multipli immediati e variazioni delle assegnazioni sono atomici. Gli errori attesi producono codici di dominio e non modificano lo stato. I prerequisiti richiedono almeno una struttura del tipo indicato.

Il tempo conserva tick di 100 ns, compreso il residuo inferiore al secondo; solo le frontiere di secondo eseguono produzione. Ogni chiamata ad `Advance` accetta al massimo 24 ore, per evitare richieste temporalmente illimitate. La durata totale della partita può superarle: strumenti e chiamanti suddividono gli avanzamenti. Il limite non è una garanzia di latenza con cataloghi arbitrariamente grandi.

Gli snapshot sono immutabili, identificano scenario, regole e versione del contratto e vengono validati prima della ripresa. La ripresa inizia in pausa. In M2 popolazione e capacità sono fisse nello scenario; crescita e nuovi depositi appartengono a M4.

`GameRuntime` possiede una sola sessione e serializza comandi e proiezioni con un lock locale. Il Core resta un aggregato a singolo proprietario, utilizzabile direttamente in test e strumenti. Le funzionalità applicative espongono operazioni nominate e risultati coerenti, senza un bus o un mediator.

## Alternative

- `float`/`double`: rappresentazione approssimata e maggiore necessità di gestire tolleranze e arrotondamenti.
- `decimal` senza granularità dichiarata: precisione elevata, ma nessuna regola esplicita sul minimo quantitativo del gioco.
- Flussi proporzionali continui: permettono lavorazioni parziali, ma cambiano la semantica semplice verificata in M1.
- Usare subito gli output dello stesso passo: rende l'ordine delle filiere parte implicita della velocità di produzione.
- Ripartizione proporzionale degli input: possibile scelta futura, richiede regole sui resti e cambia il bilanciamento.

## Conseguenze e limiti

La simulazione è ripetibile per la stessa cronologia di azioni e le stesse definizioni. Una priorità elevata può lasciare senza input attività successive: è un compromesso dichiarato, non una politica di equità.

Una ricetta con output di 0,001 unità al secondo è rappresentabile; una quantità inferiore non viene arrotondata automaticamente. Cicli con durata diversa da un secondo e crescita frazionaria degli abitanti richiederanno un'estensione esplicita delle regole.

L'identità delle regole è assegnata dal catalogo: chi cambia definizioni o capacità deve aggiornare la versione. Non esiste ancora un fingerprint automatico del contenuto. `GameSnapshot` è un contratto in memoria; formato su disco e migrazioni restano lavoro di M3.

## Verifica

Test su quantità frazionarie, overflow atomici, acquisti multipli, competizione per input e spazio, ordine di definizione invertito, priorità a pari valore, previsioni coerenti, pausa e residui temporali. Verifiche concorrenti dei casi d'uso impediscono doppie spese o assegnazioni.

## Collegamenti

- [Architettura](../ARCHITECTURE.md).
- [Scenario M2](../scenarios/ECONOMY_FOUNDATION.md).
- [Report M2](../validation/M2_CORE.md).
