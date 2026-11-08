# Stato corrente — SimRisorse RC105 (secondo hotfix test Zecca)

**08/11/2026. Ultima baseline completamente confermata: RC95 (1245 test).** RC96–RC105 ancora candidate; **1360 test cumulativi attesi**. Dopo il primo hotfix di compilazione (`CS0119`, `xUnit2013`), il nuovo report Windows conferma **compilazione riuscita** e **1359/1360 test superati**: Core 248/248, Application 602/603, Infrastructure 262/262, Desktop 247/247. Un solo test Application RC98 è fallito perché modificava il checkpoint della Fonderia d'oro senza aggiornare la capacità dell'escrow. **Secondo hotfix incluso: fixture coerente e controllo negativo su capienza incoerente; nessuna riesecuzione di `validate.cmd` o F1 dopo la correzione.**

## A che punto siamo

- RC95–RC104: laboratorio con salvataggio composito, cantieri, ricevute, backup, identità, registro di autorità e preflight del ramo attivo.
- **RC105:** nuovo prototipo **SQLite generation 7**, con **tabelle cittadine v15 reali e tabelle finanziarie nello stesso database**, identità e ricevute. Migrazione solo da composito autorevole; supporto a cantieri aperti. **Non diventa il salvataggio attivo** e il loader del gioco continua a utilizzare generation 6. [ADR 0116](adr/0116-prototipo-sqlite-generation7-citta-e-cantieri.md).
- Punteggio e classifica locale pianificati dopo il gameplay stabile; **classifica online tramite API soltanto alla fine**.

## Cosa ci aspetta

1. **Validazione Windows:** rieseguire ` .\validate.cmd ` sul secondo hotfix RC105; confermare **1360/1360 test** e gate F1 completo. Gli esiti precedenti sono 1359/1360, non una validazione della nuova consegna.
2. **RC106+:** autorità nel nuovo formato, transazioni uniche di partita/cantiere/ricevuta, concorrenza, backup e ripristino resistenti alle interruzioni; solo dopo integrazione nei comandi e GUI.
3. Bilanciamento naturale della curva A e squadre Zecca; poi punteggio e classifica locale, in ultima fase classifica online.

**Invariati:** GUI pixel art approvata, Municipio 100 Carbone + 2 Matite all'avvio, Depositi alimentari superiori, `MintTen` opt-in, prezzi correnti, salvataggi generation 6, README con screenshot autentici, documentazione unica, nessun manifest o `.local-cache` nello ZIP.

[Roadmap](ROADMAP.md) · [Decisioni](AUTHORITY_REGISTRY.md) · [Handoff](NEW_CHAT_HANDOFF.md) · [Piano RC105](validation/RC105_UNIFIED_SAVE_PROTOTYPE.md).
