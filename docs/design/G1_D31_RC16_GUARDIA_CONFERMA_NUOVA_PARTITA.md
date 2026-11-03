# G1-D31 / RC16 — Protezione delle azioni durante la conferma di nuova partita

**Data:** 03/11/2026 · **Base:** RC15, test dichiarati tutti superati dall'utente (473 previsti). **Gate P0:** ancora aperto.

## Evidenza del difetto

Nel `V14PreviewViewModel` RC15, `NewGameCommand` sospendeva la sessione e apriva `IsNewGameConfirmationVisible`. Solo `TogglePauseCommand` verificava `_newGameConfirmation`, mentre `BuildSelectedCommand`, i comandi di costruzione precedenti, personale, arruolamento/annullamento, iscrizione, Matite e acquisto tecnologia mantenevano i propri `CanExecute` originari. Era possibile continuare a modificare la colonia nella fase in cui si stava scegliendo se distruggere o conservare lo stato corrente. Il `MainViewModel` della GUI v13 protegge già le azioni di gioco durante la conferma.

## Correzione

- Guard unico `CanMutateColony => !_newGameConfirmation` condiviso dalle proprietà e dai predicati `CanExecute` delle azioni che mutano lo stato.
- I pulsanti **Costruisci** e lavoratori cambiano stato senza scomparire dall'elenco o dal dettaglio: la visibilità resta governata dai prerequisiti/possesso, non dallo stato della conferma.
- Upgrade, comandi Scuola, laboratorio Matite, ricerca, Esercito e comandi legacy interni rispettano il medesimo blocco.
- Il metodo `Perform` rifiuta inoltre l'esecuzione di azioni mutanti durante la conferma, a difesa dei futuri percorsi d'interfaccia.
- Dopo **Annulla**, i comandi originariamente disponibili tornano abilitati; **Avvia nuova** crea la sessione v14 iniziale in pausa.
- `Salva` e `Carica` non sono stati alterati, per rimanere coerenti con la v13 attuale. La loro interazione con la conferma è registrata come **verifica futura UI-20/UI-21**.

## Verifiche aggiunte

1. Inizio da Segheria Base acquistabile, controllo più comandi, esecuzione diretta di comandi mentre la conferma è visibile, confronto risorse/edifici senza variazioni, `Annulla` e successivo acquisto consentito.
2. Checkpoint con **100,000** millipunti di ricerca: acquisto realmente possibile prima, disabilitato durante, ripristinato dopo l'annullamento, acquistabile una volta sola.
3. `AvaloniaFact`: `V14BuildSelected` resta **visibile ma effettivamente disabilitato** durante la conferma e torna abilitato dopo `Annulla`, assieme al controllo Pausa/Riprendi.

## Ambito e rischi

Modificati solo due file di ViewModel v14 e il file dei test Desktop. Nessuna modifica a XAML, v13, Core, Application, Infrastructure, SQLite o valori economici. **Compilazione e test non eseguiti** per mancanza di .NET 10 nell'ambiente; eseguire `./validate.cmd` su Windows, **476 test attesi**. Il confronto grafico alle risoluzioni 1040×740 e 1240×850 resta obbligatorio per chiudere P0.

[Piano RC16](../validation/G1_D31_RC16_TEST_PLAN.md) · [Matrice audit](../validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Roadmap](../ROADMAP.md).
