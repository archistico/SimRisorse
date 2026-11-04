# P1-D46 / RC32 — Ciclo di formazione nella Scuola

## Contesto e decisione

La GUI v14 e' stata approvata per l'uso ordinario nella RC31. La RC30 e' l'ultimo checkpoint confermato dai test dell'utente (511 previsti); **RC31 non e' ancora stata eseguita su Windows**. P1-D46 inaugura la fase di completamento Scuola/ricerca. E4 resta separata.

La formazione gia' lavora per posto occupato: una Scuola offre 1 posto di formazione e 2 posti di ricerca; studenti in coda e qualificati sono abitanti riservati, non lavoratori duplicati. Iscrivere richiede attualmente 10 Cibo e **2 Matite** a studente; i 120 secondi sono un parametro da esaminare in E4, non un valore modificato qui.

## Problema verificato nel codice

`V14PreviewViewModel.School.cs` esponeva `SchoolTrainingPercent` e `SchoolTrainingStatus` soltanto per il primo allievo. Con piu' Scuole, non era possibile leggere il progresso degli altri allievi in formazione, anche se `SchoolResearchState.TrainingProgressSeconds` conservava i valori individuali.

## Intervento RC32

- Aggiunta la proprieta' di sola lettura `SchoolTrainingDetails`, calcolata dall'ordine esistente della coda di formazione, con progressi, durata e tempo rimanente di ogni posto occupato.
- Nel pannello Scuola e' stato aggiunto `V14SchoolIndividualProgress` sotto la barra gia' esistente. La nuova area resta nel `ScrollViewer` principale; i comandi in basso restano invariati.
- Per evitare un pannello molto lungo, sono mostrati al massimo **6 progressi** e il conteggio degli ulteriori allievi in formazione.
- La proprieta' e' inclusa nelle notifiche di `RefreshSchoolPanel`, cosi' si aggiorna dopo ogni tick, salvataggio o caricamento, senza reset della selezione.

**Non cambiano:** formazione, ordine della coda, costi, filiera Matite, ricercatori attivi, bilanciamento, formato SQLite, GUI legacy v13 e percorsi dei salvataggi.

## Prove previste

1. Caricamento di due Scuole con due progressi differenti (90/120 e 30/120 secondi), un allievo in coda e visualizzazione individuale coerente.
2. Dopo 30 secondi il primo allievo si qualifica, quello successivo raggiunge 60/120 e l'allievo in coda entra in formazione a 0/120; nessun ricercatore viene assegnato automaticamente.
3. Controllo Avalonia headless che il testo sia collegato alla pagina Scuola e che iscrizioni/assegnazioni restino visibili.
4. Prova di regole Core su due posti, quattro iscritti e due cicli di formazione: risorse prepagate, popolazione sempre riservata, nessuna produzione automatica di punti ricerca.

## Gate e successivi

- Attesi **518 test** cumulativi (515 RC31 + 3 RC32). I test Windows RC31–RC32 non sono stati eseguiti in questo ambiente.
- Eseguire `./validate.cmd`, quindi prova visiva della Scuola a 1040x740 e 1240x850.
- D47: analisi Matite e limiti di stoccaggio. D48: prerequisiti/benefici delle tecnologie. D49: scenari completi. D50: gate P1. E4a solo dopo P1.
