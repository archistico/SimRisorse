# ADR 0044 — Verifica unica del laboratorio Matite

- Data: 2026-11-04
- Milestone: P1-D47 / RC33
- Stato: proposta implementata; in attesa di validazione RC33 su Windows

## Contesto

Il comando di produzione Matite e il ViewModel della Scuola verificavano separatamente Scuola, input e stoccaggio. L'eventuale divergenza avrebbe reso il pulsante disponibile anche quando il dominio rifiuta l'operazione.

## Decisione

La sessione v14 espone `CheckPencilCraft(int batches)`, controllo di sola lettura che usa la stessa funzione interna del comando `CraftPencils`. Il ViewModel delega la disponibilità del comando a tale verifica; i suggerimenti quantitativi prendono i valori dallo snapshot corrente. Le operazioni di fabbricazione rimangono atomiche.

Non cambiano ricetta 1+1→4, capacità, 2 Matite per studente, ordine dei rimborsi, progressi, GUI v13, salvataggi o schema SQLite.

## Alternative rinviate

Produzione automatica in Scuola ed edificio dedicato non sono stati selezionati: richiedono prima misurazioni E4a e decisione esplicita sui coefficienti e sull'esperienza di gioco.

## Verifica

6 test aggiunti (4 Application, 2 Desktop), 524 cumulativi attesi. Compilazione e xUnit da eseguire su Windows. [Rapporto D47](../design/P1_D47_RC33_FILIERA_MATITE.md) · [Piano RC33](../validation/P1_D47_RC33_TEST_PLAN.md)
