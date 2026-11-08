# Punteggio e classifiche — pianificazione (non implementato)

**Decisione di roadmap del 07/11/2026.** Il gioco dovrà poter calcolare il punteggio ottenuto in una partita e mostrare una **classifica locale** delle partite giocate. L'**estensione online via API sarà una delle ultimissime attività**, quando gameplay, punteggio, salvataggi e interfaccia saranno stabili.

## Quando affrontarlo

1. **Dopo il bilanciamento economico e il completamento dei principali sistemi di gioco:** definire gli obiettivi finali e i parametri confrontabili.
2. **Fase punteggio:** specificare una formula deterministica, versionata e testabile. Possibili dimensioni da valutare (non ancora pesate o approvate): popolazione, edifici e livelli, ricerca, filiere attive, risorse, esplorazione e sviluppo; considerare anche durata e difficoltà della partita.
3. **Fase classifica locale:** registrare risultati di partite concluse con ID della campagna, data, versione delle regole di punteggio, scenario/difficoltà e statistiche verificabili; supportare ordinamento e confronti senza alterare i salvataggi giocabili. Una partita in corso può mostrare un punteggio provvisorio, ma non deve essere classificata come conclusa.
4. **Ultima fase — classifica online via API:** definire identità pubblica facoltativa, privacy, autenticazione, sincronizzazione e convalida server delle partite. Non accettare come attendibile un punteggio arbitrario inviato dal client; un ID o un hash del salvataggio non bastano a prevenire manipolazioni.

## Regole da fissare prima della formula

- Che cosa costituisce la fine di una partita e se esistono scenari diversi.
- Se le classifiche sono globali o separate per difficoltà, scenario e versione delle regole.
- Criteri contro punteggi ottenuti con scorte artificiali, partite modificate o versioni sperimentali.
- Quali dati minimi possono essere inviati al server, soltanto con consenso esplicito dell'utente.

**Fuori ambito RC105:** formule numeriche, punti realmente assegnati, nuova GUI, database classifiche, API e servizi online.
