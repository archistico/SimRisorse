# G1-D10 — Piano operativo E3b/3: sessione economia v14 isolata

**Stato:** piano tecnico, non implementazione della sessione. **Base prerequisita:** v60-E3b2-RC3 con controllo del numero degli studenti in formazione.

## Proposta di incrementi

| Gate | Contenuto | Accettazione |
| --- | --- | --- |
| E3b/3a | `V14CampaignSession` isolata, bootstrap e restore dello snapshot v14 | Stato identico dopo restore, nessun accesso allo store v2 |
| E3b/3b | Comandi Scuola: costruzione, iscrizione, annullamento, assegnazione, ritorno civile | Nessun doppio impiego della popolazione, pagamenti atomici, niente negative balance |
| E3b/3c | Avanzamento unico del tempo: generazione punti, formazione e produzione tier | Grande passo temporale = passi piccoli, pausa, determinatezza, overflow protetto |
| E3b/3d | Comandi edifici tier: preventivo, acquisto, upgrade, assegna/libera lavoratori | Ricerca necessaria, contatori coerenti, capacità e prezzi anti-softlock |
| E3b/3e | Salvataggio v3 e restore su una sessione separata | Round-trip e replay identici, S1 coerente, v13 immutata |
| E3b/4 | UI Scuola/Ricerca e livelli | Blocco azioni valide/invalid e informazioni leggibili |
| E4 | Benchmark e gate campagna | raggiungibilità, bilanciamento, migrazione su copie |

## Regole di integrità per E3b/3

- Lo snapshot è l’unica fonte autorevole del saldo di ricerca, delle tecnologie e dei contatori; non conservarne copie divergenti in ViewModel o nello store.
- Una singola transizione restituisce un nuovo snapshot validato; fallimenti lasciano il precedente invariato.
- Il tempo della simulazione resta unico; ricerca, addestramento militare, razioni e produzione devono avanzare in modo consistente, senza duplicare i residenti.
- Gli edifici produttivi sono rappresentati per famiglia e livello (Base 1 / Grande 3 / Avanzato 10); un upgrade non aumenta il contatore degli acquisti.
- L’accumulo di punti ricerca è libero. Nessuna tecnologia si acquista automaticamente.
- I comandi v14 **non modificano** lo schema, il catalogo o il comportamento del ruleset storico `civil-growth-13`.
- L’attivazione nella partita giocabile sarà un gate separato dopo UI e verifiche anti-blocco.

## Decisioni da ratificare prima di attivare v14

Crescita del prezzo (+15% candidato), moltiplicatori tier (1/2,6/6,5), prezzo e tempi della Scuola, ricompense tecnologiche, capacità e obiettivi della vittoria. La sola regola già approvata è **accumulare liberamente punti ricerca e spenderli quando si vuole**.
