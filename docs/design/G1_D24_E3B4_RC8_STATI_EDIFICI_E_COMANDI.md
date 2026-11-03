# G1-D24 — RC8: stati Edifici, disposizione comandi e Scuola

**Data:** 03/11/2026 · **Pacchetto:** `v60-E3b4-RC8` · **Stato:** implementato nei sorgenti, **da compilare e provare su Windows**.

## Segnalazione originale

L'utente segnala che nell'elenco v14 gli edifici bloccati appaiono troppo simili a quelli disponibili; i dettagli devono restare consultabili. In dettaglio il pulsante «Costruisci» è tagliato, verde/cliccabile anche senza materiali e non ha uno stato hover sufficientemente chiaro. «−1 lavoratore», «+1 lavoratore», «+5», «+10» devono stare su un'altra singola riga. Nella scheda Scuola la costruzione è duplicata: si deve costruire soltanto da Edifici.

## Correzione realizzata

1. **Elenco:** `V14BuildingEntry.RowOpacity = 0.48` quando `IsUnlocked == false`, altrimenti `1`; XAML applica l'opacità al **contenuto** della riga e non al `ListBoxItem`, che rimane abilitato/selezionabile. Stato «Bloccato» e prerequisiti sono sempre consultabili nel dettaglio. Edifici sbloccati senza materie prime **non** vengono grigiati in elenco, perché il blocco è diverso.
2. **Dettaglio:** `V14BuildSelected` è un singolo pulsante a larghezza piena, non più inserito nelle tre colonne insieme a +5/+10. `IsVisible=CanBuildSelected` (prerequisiti) e `IsEnabled=CanAffordSelected` (risorse + costo progressivo). L'abilitazione esplicita fa applicare le classi Avalonia `primary build:disabled` già usate per il grigio; con comando disponibile si usa il tema attivo e hover ocra già definito in `App.axaml` senza cambiare spessori o geometria.
3. **Seconda riga:** `V14BuildingSecondaryActions` a quattro colonne, in ordine «−1 lavoratore», «+1 lavoratore», «+5», «+10». Sono comandi distinti e i pulsanti lavoratori sono visibili solo per produttori posseduti. +5/+10 mantengono la quotazione cumulativa del lotto e ora espongono `IsEnabled=CanAffordFive/Ten`. Il pannello dettaglio v14 passa da **310 a 410 px** per mostrare le etichette complete anche a finestra minima; non cambia la GUI v13.
4. **Scuola:** rimosso esclusivamente il pulsante «Costruisci +1 Scuola» dalla scheda Scuola; restano iscrizione, annullamento formazione, assegnazione/sospensione/ritorno ricercatori e conteggi. La Scuola resta una riga nell'elenco **Edifici** e lì è costruibile se sbloccata e pagabile. Il comando interno `BuildSchoolCommand` resta per la compatibilità dei test/VM, senza un secondo pulsante nella scheda.
5. **Aggiornamento:** `CanAffordFive/Ten` notificati insieme a `CanAffordSelected` alla selezione e a ogni `Refresh()`, così i pulsanti cambiano stato dopo acquisti e tick; comando `CanExecute` continua a controllare il motore.

## Protezione da regressioni

- Tre nuovi metodi di test: grigiatura e selezione con prerequisiti, geometria e ordine dei quattro pulsanti, costruzione disabilitata con materiali assenti; il caso hover è un nuovo `AvaloniaTheory` a **due risoluzioni**, per **5 casi nuovi**.
- Aggiornato un test esistente che si aspettava il pulsante di costruzione nella scheda Scuola; ora verifica la sua assenza e la presenza dei cinque comandi operativi.
- Totale previsto **456 casi** (451 RC7 + 5), **NON eseguiti** nell'ambiente di packaging. Il run RC6 su Windows aveva 450/451, RC7 correggeva l'asserzione errata ma non ha ancora un risultato confermato.
- Nessun cambiamento a `MainWindow.axaml`, `App.axaml`, `GameSession`, SQLite, salvataggi o regole economiche. Il cambiamento riguarda **solo la presentazione v14**.

## Controlli obbligatori

Eseguire `./validate.cmd`, poi UI v14 a 1040×740 e 1240×850. Verificare hover, costo senza risorse, prerequisiti e selezione, larghezza del dettaglio, scorciatoie dopo acquisto, Scuola e tabella Risorse a sette colonne. La sola esecuzione xUnit non certifica il risultato visivo. [Piano D24](../validation/G1_D24_RC8_TEST_PLAN.md) · [ADR 0034](../adr/0034-v14-stati-edifici-e-layout-azioni.md).
