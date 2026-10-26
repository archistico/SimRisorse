# 0009 — Crescita civile e generazioni di salvataggio

- Stato: Accettato
- Data: 2026-10-26
- Ambito: primo incremento M4, con parametri di scenario provvisori
- Origine: proseguimento della [proposta civile](../scenarios/CIVIL_GROWTH_DRAFT.md)

## Contesto

La popolazione M3 è fissa. La prima progressione civile deve collegare case, cibo e disponibilità dei lavoratori, conservare il progresso parziale e consentire di recuperare da investimenti inefficienti. I vecchi checkpoint non devono cambiare formule quando viene aggiornato il programma.

## Decisione

Introdurre un effetto `HousingPerBuilding` nelle definizioni e regole `PopulationRules` opzionali nello scenario. `GameSession` possiede popolazione effettiva e secondi maturati verso il prossimo arrivo. Gli abitanti rimangono persone intere; assegnazioni e crescita passano dalle invarianti del Core.

Uno scenario senza queste regole mantiene la popolazione fissa e il contratto snapshot v1. `civil-growth-1` usa il contratto v2 e applica i valori della [scheda eseguibile](../scenarios/CIVIL_GROWTH.md). Sono parametri di lavoro dello scenario, non un bilanciamento definitivo o una scelta già estesa a esercito e isole.

Nel secondo economico il consumo civile completo viene prenotato prima delle ricette. Una razione insufficiente non viene consumata parzialmente. Il pianificatore conserva lo spazio libero iniziale: il cibo prodotto e lo spazio liberato diventano utilizzabili nel passo successivo. Crescita e previsione usano lo stesso piano. Una carenza ferma gli arrivi senza eliminare persone o fermare i produttori del primo scenario.

La crescita è unica per insediamento, non per casa. Progresso parziale fermo in pausa, durante carenze o a capacità piena; nessun accumulo di arrivi oltre il limite. Il nuovo abitante consuma dal passo successivo. Gli acquisti che supererebbero il limite numerico abitativo falliscono atomicamente.

### Compatibilità

- M3 resta `economy-foundation`, regole `economy-foundation-1`, file e snapshot v1, percorso `game.sqlite3`.
- Il nuovo default è `civil-growth`, regole `civil-growth-1`, file e snapshot v2, percorso `civil-growth.sqlite3`.
- Lo schema v2 aggiunge `population_state(slot, growth_seconds)` alle tre tabelle precedenti. Popolazione, progresso, scorte ed edifici vengono scritti nella medesima transazione.
- Il loader supporta entrambe le generazioni, ma il runtime carica solo lo scenario e la versione delle regole attivi. Dati v2 senza progresso non diventano automaticamente progresso zero.
- Lo store rifiuta la sovrascrittura tra generazioni diverse anche se gli viene passato un percorso condiviso. Non vengono migrate automaticamente partite o formule.
- Per riprendere M3 si usa `run-game.cmd --scenario economy-foundation`. "Nuova partita" riparte dallo scenario corrente.

## Alternative e conseguenze

Rendere ogni abitante una risorsa frazionaria confonderebbe persone, assegnazioni e accumulo temporale. Derivare la crescita dal tempo totale perderebbe pause dovute a fame o case piene. Riutilizzare il solo slot precedente renderebbe ambiguo il cambio di regole.

Due slot mantengono semplice la compatibilità di questa fase e preservano le partite precedenti. La selezione grafica degli scenari e una futura conversione di salvataggi richiederanno un lavoro dedicato. Il Core rimane indipendente da UI, SQL e orologio; non vengono introdotti servizi esterni o un motore di regole generico.

## Verifica

[Report M4](../validation/M4_CIVIL_GROWTH.md): soglia dei 60 secondi, tempo frazionario, consumo esatto, contesa e capacità, ripresa a 59 secondi, carenza e recupero, rollback SQLite v2 e protezione dei checkpoint v1. I flussi proiettati sono confrontati con la variazione effettiva delle scorte anche nei secondi di arrivo.
