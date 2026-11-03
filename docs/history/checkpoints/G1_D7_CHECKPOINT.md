# Checkpoint G1-D7 — SimRisorse v60-E3-RC1

**3 novembre 2026 — CANDIDATO NON VALIDATO.** Questo ZIP integrale aggiunge E3 sperimentale al ramo E1+E2; la campagna `civil-growth-13`, UI Avalonia e SQLite restano invariati. Ultimo gate Windows confermato: **v59-RC1**.

## Implementazione nel solo Core

- Scuola parametrica: studenti (coda, progressi individuali), qualificati, ricercatori attivi, disponibilità dei posti e riserva di popolazione.
- Iscrizione con consumo di Cibo, annullamento con rimborso soggetto a deposito, assegnazione e rientro civile; politiche ancora **candidate**.
- Punti liberamente accumulabili, volontariamente spendibili; 8 tecnologie produttive candidate con grafo valido, 3 differite non acquistabili; collegamento ai livelli 1/3/10 di E2.
- DTO `research-prototype-e3-1` e controlli di restore/round-trip: NON esiste un nuovo schema SQLite o `GameSnapshot` v3.
- Corretto anche `TieredProductionCatalog.FromCatalog` ereditato da E2: `ToImmutable()` al posto di `MoveToImmutable()` per i cataloghi storici con 3 famiglie. È **l’unico file tecnico preesistente modificato**.
- 2 classi xUnit nuove, **non eseguite**; esempi: 100 punti/2 ricercatori/500 secondi, percorso completo Scuola→Segheria grande, annullamento atomico, restore anti-doppia occupazione.

## Decisioni

**Approvata:** ricerca accumulabile senza progetto, spendibile dal giocatore quando desidera (ADR 0021). **Da approvare:** scuola 1 posto formazione / 2 ricerca, 10 Cibo, 120s, 0,1 punti/s, prezzi tecnologie, rimborsi e rientri, incremento prezzi 15%, multipliers tier.

## Prossima milestone

**E3b:** progettare integrazione `civil-growth-14`/snapshot SQLite versionato e UI, senza attivarla finché costi/ricerca/upgrade non sono presentabili insieme; validazione su copie e test Windows. **E4:** benchmark anti-softlock e regolazione dei numeri. Poi G1 navale.

Dettaglio [E3](../../design/G1_D7_E3_SCUOLA_RICERCA_IMPLEMENTAZIONE_ISOLATA.md) · [ADR 0023](../../adr/0023-e3-school-technology-ledger-isolated-prototype.md).
