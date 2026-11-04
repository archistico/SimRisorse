# G1-D39 / RC24 — Piano di validazione Windows

## Preparazione

1. Estrarre lo ZIP RC24 in una **nuova** cartella; non sovrapporlo a RC20–RC23.
2. Eseguire un backup di `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3` se presente.
3. Da PowerShell nella root estratta eseguire `./validate.cmd` (o `./validate.ps1` se previsto dalle istruzioni della root): **501 test attesi** (RC23 499 + 2). Registrare TRX e errori esatti, senza attribuire test passati in assenza di log.
4. Avviare `./run-game.cmd --experimental-v14` e aprire Scuola, a 1040×740 e 1240×850.

## Regressione RC24

- Senza Scuola, il laboratorio Matite indica **«Costruisci prima una Scuola»** prima della conferma.
- Premere **Nuova partita**, non **Avvia nuova**. I tre comandi Scuola (iscrivere, annullare, produrre Matite) restano disabilitati e i rispettivi testi descrivono l'operazione sospesa.
- **Annulla** deve ripristinare gli stessi suggerimenti precedenti senza modificare le scorte.
- Con Scuola e materiali, verificare analogamente il blocco temporaneo in una **copia di una partita**; non usare un checkpoint personale senza backup.
- Verificare anche la suite cumulativa RC21 (upgrade), RC22 (scroll e addestramento), RC23 (navigazione e selezioni), oggi ancora senza conferma Windows.

## P0 generale

Seguire [matrice D39](G1_D39_P0_UI_MATRIX.md), registrando un esito per tutte le 22 voci a 1040×740 e 1240×850. **xUnit verde non chiude il gate grafico**.

## Stato

- Formati e integrità archivio: verificabili nella consegna.
- Build .NET 10, xUnit, rendering Windows, test manuali: **da eseguire**.
- Nessuna modifica a GUI v13, Core, Application, Infrastructure, SQLite o coefficienti economici.
