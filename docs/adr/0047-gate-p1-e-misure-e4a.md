# ADR 0047 — Gate Scuola/Ricerca e misurazioni economiche senza rebalance

- Data: 2026-11-04
- Milestone: P1-D50 / RC36
- Stato: protocollo adottato, gate P1 in attesa della validazione Windows

## Decisione

Terminare P1 con un gate cumulativo di test e una tabella esplicita delle invarianti, senza modificare le regole economiche. Rafforzare la protezione dei vecchi salvataggi Matite e rifiutare checkpoint con rimborsi inventati prima di scrivere su SQLite. Preparare per E4a un insieme versionato di scenari naturali e controllati; i loro risultati devono essere misurati, non ipotizzati.

## Motivazione

Una prova end-to-end finanziata artificialmente dimostra integrazione, non raggiungibilità economica. Non deve diventare una falsa approvazione dei costi. Separare le due verifiche impedisce di cambiare ricette, coefficienti e priorità senza un dato quantitativo e una decisione esplicita.

## Alternative escluse ora

- Dichiarare verde P1 senza test Windows.
- Modificare i 120 s, i 10 Cibo, la ricetta 1+1→4, la produzione manuale o gli 0,1 punti/s durante D50.
- Sostituire lo slot v14 esistente o convertire automaticamente il database v13.

## Impatti

Due nuovi test SQLite; nessuna modifica a `src/`, UI, schema SQLite, meccaniche o coefficienti. Preparati manifest scenari E4a e template risultati. In caso di test fallito la milestone resta aperta e si applica una correzione dedicata.

[Rapporto](../design/P1_D50_RC36_CONSOLIDAMENTO_E_GATE_E4A.md) · [Piano test](../validation/P1_D50_RC36_TEST_PLAN.md) · [Protocollo E4a](../design/E4A_PROTOCOLLO_BENCHMARK.md)
