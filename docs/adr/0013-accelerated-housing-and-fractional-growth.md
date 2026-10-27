# ADR 0013 — Crescita accelerata dalle abitazioni senza perdita di progresso

- Data: 2026-10-27
- Stato: **Accettato, implementato in v24; verifica Windows ancora da eseguire**
- Ambito: M4.10, `civil-growth-5`, Core/Application/Desktop/SQLite

## Contesto

Nella baseline `civil-growth-4` una Casa piccola o grande aumentava soltanto la capienza. L'utente ha chiesto che investire in più abitazioni aumenti anche il ritmo con cui nasce un abitante, senza perdere i vincoli alimentari o introdurre la futura GUI retro 8-bit prima del suo gate.

## Decisione

La colonia conserva **un unico progresso verso il prossimo residente**. La prima Casa piccola non accelera; ogni Casa piccola aggiuntiva aggiunge il 15% e ogni Casa grande il 30%, **fino al 200%** del ritmo base. A velocità massima il periodo nominale è 30 s anziché 60 s. Non esiste accumulo quando manca Cibo, manca Pane richiesto per il prossimo residente, non ci sono posti liberi o la partita è in pausa.

Il calcolo usa **20 unità intere di progresso per secondo base**: +3 per Casa piccola oltre la prima, +6 per Casa grande, tetto 40. La soglia resta 1.200 unità (60 s × 20). Il progresso in eccesso dopo un arrivo è conservato, così costruire una casa non azzera un avanzamento in corso. I secondi inferiori al secondo già presenti in `elapsed_ticks` conservano la loro semantica originaria.

## Persistenza e compatibilità

La versione delle regole cambia da `civil-growth-4` a `civil-growth-5`, mentre file SQLite e snapshot restano **v2**:

- `population_state.growth_seconds` continua a contenere secondi interi.
- La nuova tabella facoltativa `population_growth_fraction` contiene `twentieths` (intero 0–19).
- File v2 storici senza la nuova tabella sono letti con resto zero; se la tabella esiste ma manca la riga, il file è invalido.
- La conversione esplicita `-4 → -5` preserva secondi, abitanti, scorte, edifici e tempo. Al primo salvataggio che sostituisce regole diverse si effettua una copia dell'originale.
- Nessuna crescita offline, riscrittura silenziosa o copia del passato viene simulata.

## Alternativa rifiutata

Il contatore non viene più interpretato come un numero di secondi da confrontare a una soglia mobile: una variazione del ritmo a metà progresso potrebbe perdere il resto o rendere impossibile raggiungere esattamente la nuova soglia. Non sono introdotti contatori separati per edificio o calcoli floating point.

## Verifica e limiti

La suite v24 testa +15%, +30%, tetto 2×, costruzione a metà periodo, pause, carenze, replay con batch diversi, conversione con backup, SQLite v2 senza la tabella nuova e lettura della velocità nella UI. I benchmark storici M4 sono preservati mediante `--rules legacy`; il nuovo confronto usa `--benchmark-housing-accelerated`. **La verifica .NET/Windows non è stata eseguita in questa preparazione**.

Restano invariati costi, razioni, ricette e capacità. Il ritmo effettivo in gioco dipende dalle scorte, dalle abitazioni costruite e dal momento degli acquisti; la capienza non garantisce crescita costante.
