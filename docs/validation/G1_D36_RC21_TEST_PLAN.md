# G1-D36 / RC21 — Piano di validazione Windows

**Baseline verde comunicata:** RC20 con 490 test attesi e tutti dichiarati superati. **Candidata RC21: 493 test attesi (+3)**. Non sono stati eseguiti in questo ambiente, privo dell'SDK .NET 10.

## 1. Suite automatica

1. Estrarre lo ZIP **RC21** in una nuova cartella, senza cancellare salvataggi personali.
2. Lanciare `.\validate.cmd` dalla root (comando PowerShell: `.\validate.cmd`, senza spazi superflui) e registrare compilazione, conteggio xUnit e percorso `.trx`.
3. Verificare la classe `V14BuildingUpgradeAuditTests` (2 casi per viewport + 1 caso upgrade pagabile con costo preciso). Se fallisce un'asserzione Avalonia, conservare log e dimensione finestra, senza cambiare prezzi a tentativi.
4. Includere i gate cumulativi RC17–RC20. Su **copia dei propri salvataggi** riprovare il caso RC17: Nuova partita → Avvia nuova → chiusura senza Salva manuale preserva il checkpoint precedente.

## 2. Verifica visiva (P0, non sostituita da xUnit)

- Aprire `.\run-game.cmd --experimental-v14` e selezionare **Segheria** nell'elenco: Costruisci +1/+5/+10 su una riga, Lavoratore −1/+1/+5/+10 sulla seconda, stesso bordo in hover, senza movimenti.
- Selezionare una **Segheria grande/avanzata bloccata**: riga attenuata ma cliccabile, dettagli e requisiti leggibili; il pulsante Costruisci non deve comparire.
- Selezionare un edificio con **ricerca upgrade sbloccata ma materiali insufficienti**: preventivo visibile, pulsanti potenziamento **grigi non cliccabili**, nessun colore verde al passaggio del mouse.
- Se le risorse bastano, pulsante upgrade verde e hover ocra senza salto di posizione. Un acquisto deve trasferire **un solo** edificio, senza creare personale dal nulla e senza incorrere in lotti parziali.
- Controllare entrambe le risoluzioni **1040×740** e **1240×850**, inclusi box dettagli, scroll, testi lunghi, riapertura scheda e selezione dopo tick.
- Confrontare con `.\run-game.cmd` (GUI v13), verificando che nessuna funzione della vecchia scheda Edifici sia scomparsa.

## 3. Limiti e criteri d'uscita

Nessuna modifica a motore, costi, capienze, JSON economici, SQLite, salvataggi o XAML v13. Un pass dei 493 test autorizza soltanto il gate automatico **RC21**. La chiusura P0 delle 22 voci richiede ancora la conferma visiva manuale e il trattamento di ogni difetto individuato.

[Report D36](../design/G1_D36_RC21_AUDIT_EDIFICI_UPGRADE.md) · [Roadmap](../ROADMAP.md).
