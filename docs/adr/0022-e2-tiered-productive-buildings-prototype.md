# ADR 0022 — Prototipo isolato per edifici produttivi a livelli

**Data:** 2026-11-03  
**Stato:** Proposta tecnica sperimentale, NON approvazione dei valori di gioco.

## Contesto

L'utente chiede edifici produttivi Base/Grandi/Avanzati (1/3/10 lavoratori), nuovi acquisti con costo crescente per famiglia e migliorie da ricerca. La partita attuale è la `civil-growth-13`, con salvataggi v2 e catalogo aggregato per edificio; in E1 esistono già funzioni pure per le quote e per il saldo ricerca libero.

## Decisione di implementazione candidata

- Modellare quantità e lavoratori per famiglia+livello con tre gruppi aggregati; non alterare `BuildingState` usato dalla v13.
- Gli upgrade di un solo edificio conservano i lavoratori già presenti nel gruppo scelto (riempimento prioritario del primo edificio), senza riassegnarne di nuovi o prelevarli dal pool civico. Il sistema conserva il totale e non decrementa la storia acquisti.
- Usare la stessa ricetta **per lavoratore** di prima, riusando `ProductionPlanner`; stoccaggio locale proporzionale ai posti 1/3/10.
- Bloccare acquisto di livelli senza tecnologie appropriate e respingere operazioni non finanziabili senza aggiornare parzialmente dati.
- Separare il prototipo dalla partita reale: nessuna nuova interfaccia, migrazione SQLite, versione di ruleset o prezzi attivi finché Scuola, ricerche e benchmark non sono completi.

## Questioni da approvare

Moltiplicatori prezzi 1/2,6/6,5; crescita per famiglia 15%; numero/costo dei ricercatori; elenco e costo delle tecnologie; comportamento UI sugli upgrade aggregati; regola di vittoria a 23 famiglie. La ricerca libera è già approvata nell'ADR 0021; non lo sono questi numeri.

## Verifica

Test Core/Application predisposti, da eseguire con SDK .NET 10 su Windows. L'ultima v59 testata rimane il riferimento conservativo. Vedere `docs/design/G1_D6_E2_IMPLEMENTAZIONE_ISOLATA.md`.
