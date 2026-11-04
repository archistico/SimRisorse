# G1-D37 / RC22 — Piano di validazione

**Ultima versione dichiarata verde:** RC20 (490). RC21 (493) non testata; **RC22** include RC21 e aggiunge **3 casi** per **496 test attesi**. In questo ambiente non è disponibile l'SDK .NET 10.

## Gate automatico cumulativo

1. Estrarre lo ZIP integrale RC22 in una cartella nuova e preservare una copia di sicurezza dei salvataggi v14.
2. Eseguire `./validate.cmd` (PowerShell Windows: `.\validate.cmd`) e conservare output e file TRX. Non serve testare RC21 separatamente.
3. Controllare `V14BuildingDetailScrollAuditTests` (due viewport) e `V14MilitaryUiParityTests.TrainingProgressAndCountdownRefreshOnTicksAndCompletion` (barra, testo e fine addestramento).
4. Se compaiono errori di compilazione o test, registrare file, riga ed esito effettivo: l'integrità ZIP e il parsing XAML non equivalgono a un gate .NET verde.

## Prova visiva P0, obbligatoria dopo i test

- A **1040×740** e **1240×850**, selezionare Edifici e provocare una diagnosi con più cause (scorte insufficienti e produttore inattivo, se disponibile). Il testo deve restare leggibile nella propria area scorrevole, senza spostare fuori vista le righe `Costruisci` e `Lavoratore`, né i controlli upgrade.
- Scorrere il riquadro descrittivo principale e il riquadro diagnostica separatamente; verificare che la selezione dell'edificio e i comandi non cambino e che il pulsante disabilitato rimanga grigio anche in hover.
- Aprire Esercito con una Caserma e Spade sufficienti, arruolare una recluta, avviare la simulazione e osservare avanzamento percentuale, tempo residuo e passaggio a soldato pronto. Verificare pausa e ripresa, mantenendo la scheda e il testo leggibili.
- Rivedere il caso RC17 con **copia** del DB: nuova partita senza Salva manuale non deve sovrascrivere il checkpoint precedente.
- Proseguire la matrice completa UI-01…UI-22. I 496 test verdi non costituiscono approvazione grafica automatica.

## Condizioni di chiusura

Nessuna modifica al database, alla GUI classica, ai costi o alla simulazione. La fase P0 resta aperta fino alla conferma Windows e al completamento della revisione manuale delle 22 voci. Dopo P0: P1 Scuola/Albero, E4a analisi, E4b bilanciamento, E4c consolidamento, M5.4–M7.

[Rapporto D37](../design/G1_D37_RC22_SCORRIMENTO_DETTAGLI_ESERCITO.md) · [Roadmap](../ROADMAP.md).
