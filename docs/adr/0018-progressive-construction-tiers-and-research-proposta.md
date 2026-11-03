# ADR 0018 — Costi progressivi, taglie degli edifici e ricerca (PROPOSTA)

Data: 2026-11-03  
Stato: **Proposto — non approvato**  
Contesto: SimRisorse v59-RC1 `civil-growth-13`, test Windows dichiarati superati; G1-D2 modifica soltanto documentazione.

## Problema

Gli edifici a costo fisso rendono troppo semplice moltiplicare la capacità produttiva e raggiungere strutture avanzate. Si vuole una strategia di sviluppo che combini aumento dei prezzi, investimenti in taglie superiori e sblocchi tramite ricerca.

## Decisioni da chiedere all'utente

- Costo crescente esponenziale per famiglia o formula alternativa; coefficienti e trattamento degli acquisti in lotto.
- Strutture base/grandi/avanzate con 1/3/10 posti, costruzione diretta e/o upgrade; quantità/famiglie interessate.
- Scuola che forma abitanti come ricercatori e genera progresso tecnologico; ordinamento e costi delle ricerche.
- Conteggio degli obiettivi della prima campagna: 22 attuali o 23 includendo la Scuola.
- Rinvio Cantiere navale fino all'introduzione e al bilanciamento delle nuove regole.

## Vincoli

- Nessuna perdita automatica di risorse, lavoratori o salvataggi; compatibilità S1 con backup.
- Conservare benchmark storici, Municipio fisso, stoccaggi diffusi, Pane, addestramento individuale e orologio simulato.
- Sblocchi e unità di ricerca non duplicano la popolazione o il conteggio dei lavoratori.

Dettagli: `docs/design/G1_ECONOMIA_RICERCA_LIVELLI_PROPOSTA.md`.
