# ADR 0024 — Confini dello snapshot candidato v14 e migrazione dalla v13

- **Data:** 2026-11-03
- **Stato:** PROPOSTO — implementazione Core isolata, nessuna migrazione DB effettuata
- **Decisione già accettata:** ADR 0021 — saldo ricerca libero e spesa volontaria.

## Contesto

Il formato v13 conta edifici per tipo e applica un solo lavoratore per ogni produttore. I nuovi livelli 3/10 e i ricercatori richiedono un nuovo modello dati. Reinterpretare `building_stock` o `GameSnapshot` v2 romperebbe l'identità dei salvataggi e dei benchmark storici.

## Decisione proposta

Introdurre `V14CampaignSnapshot` (formato candidato 3, ruleset `civil-growth-14`) come contratto **separato**, e `V14CampaignSnapshots.MigrateFromV13` per trasferire esclusivamente gli stati v13 verificati. I 16 produttori partono dal livello base; gli acquisti inizializzano contatori coerenti, i dati della colonia e dell'esercito restano identici. Scuola e conoscenza partono da zero. I punti ricerca si accumuleranno senza alcuna tecnologia selezionata.

La conversione viene prima validata in memoria, poi in una milestone futura estesa a SQLite con backup S1 e transazioni atomiche. Non cambiare fino ad allora il lettore v13 o il formato 2.

## Conseguenze

- **Benefici:** contratto esplicito, test di consistenza, nessun rischio immediato sui database in uso; possibile calcolare in futuro capacità e occupazione 1/3/10.
- **Limiti:** non include ancora scrittura/lettura SQLite v3, comandi di sessione v14, Scuola visibile o attivazione del prezzo progressivo.
- **Rischi aperti:** scelta definitiva dei parametri, eventuali acquisti storici non recuperabili se esistesse una demolizione, dimensione dei checkpoint, politiche di importazione su salvataggi non v13.
- **Gate:** compila/test .NET sul PC, integrazione persistente, verifica rollback, benchmark di raggiungibilità, approvazione dei numeri.
