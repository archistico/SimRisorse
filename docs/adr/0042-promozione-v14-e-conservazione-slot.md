# ADR 0042 — Promozione della GUI v14 e preservazione dei salvataggi

**Data:** 2026-11-04  
**Stato:** approvato per l'uso della GUI v14, implementazione candidata RC31

## Contesto

Dopo la validazione cumulativa RC30 (511 test dichiarati superati), il proprietario del progetto ha espresso approvazione per la GUI v14 e ha indicato di non utilizzare più la vecchia interfaccia. Le 44 prove formali P0 con screenshot non sono state raccolte: tale dossier rimane storico e **non costituisce una certificazione visuale eseguita**.

## Decisione

1. `run-game.cmd`, senza argomenti, apre la campagna e la GUI v14.
2. `--v14` e lo storico `--experimental-v14` aprono lo stesso percorso v14. Il nome «experimental» è un alias di compatibilità, non cambia il contenuto del gioco.
3. `--legacy-v13` apre la GUI classica v13 per recupero e confronto. `--scenario civil-growth|economy-foundation` conserva la semantica **legacy** degli script pubblicati; `--smoke-test` continua a usare la GUI v13 e salvataggi temporanei.
4. Nessuna migrazione o spostamento automatico: l'utente ritrova la propria campagna v14 in `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`. Il nome della cartella `experiments` rimane **intenzionalmente** per compatibilità e potrà cambiare soltanto con migrazione esplicita, backup e test futuri.
5. La v13 rimane su `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3` (scenario storico: `game.sqlite3`). La CLI rifiuta i percorsi riservati dell'altra modalità.
6. Non rimuovere il codice v13 né il supporto alla sua persistenza. Non modificare regole economiche o formati SQLite in questo passaggio.

## Conseguenze e gate

- P0: **accettazione d'uso esplicita** da parte del proprietario, distinta dalla **mancata raccolta** di 44 evidenze visive formali. Non dichiarare «44/44 verificati».
- RC31 richiede suite .NET 10 cumulativa, test di routing/default/compatibilità e prova manuale dell'apertura sullo slot v14, senza scritture sullo slot v13.
- Lavoro successivo: **P1-D46** revisione ciclo Scuola, formazione e rimborsi; P1-D47 Matite; P1-D48 albero tecnologico; P1-D49 salvataggi/integrato; P1-D50 gate, poi E4a→E4b→E4c→M5.4–M7.
