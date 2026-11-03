# ADR 0016 — Distinguere capacità militare controllata e progressione reale

- **Data:** 2026-11-03
- **Stato:** Accettata come decisione tecnica di misurazione; non approva nuovi parametri di gameplay.
- **Milestone:** M5.3c

## Contesto

La v58-RC5 contiene reclutamento individuale con 5 Spade per recluta, 1 posto/Caserma, 30 s per persona. Il confronto dei numeri teorici con una campagna che parte da 8 abitanti confonde potenza dell'addestramento e disponibilità economica delle Spade e della popolazione.

## Decisione

1. Aggiungere un benchmark deterministico sul Core per confrontare 5/10/20 reclute con 1/2/4 Caserme, avendo persone e Spade disponibili subito, **senza cambiare i ruleset**.
2. Registrare separatamente le misure del già esistente percorso `--strategy military --hours 8`, che parte dalla colonia reale e usa i normali comandi di costruzione, assegnazione e arruolamento.
3. Non proclamare una durata di campagna dai dati del benchmark controllato. Nessun tempo numerico nuovo diventa vincolante senza un gate di validazione e una decisione esplicita sul gameplay.
4. Conservare i benchmark civili e industriali storici nei rispettivi cataloghi versionati.

## Conseguenze

- I risultati sono confrontabili e i prezzi/tempi in vigore sono protetti da test.
- L'analisi successiva dovrà includere saldo alimentare, popolazione civile libera e costo-opportunità delle Caserme.
- Nessuna modifica a formati SQLite, cataloghi, GUI, stato delle partite o condizioni di vittoria.

[Protocollo M5.3c](../validation/M5_3C_V59_MILITARY_BALANCE.md)
