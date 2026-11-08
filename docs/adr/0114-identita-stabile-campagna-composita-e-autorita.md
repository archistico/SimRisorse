# ADR 0114 — Identità stabile della campagna nel laboratorio composito

**07/11/2026 · RC103 · Stato: candidata (gate .NET 10 da confermare).**

## Problema

RC101–RC102 riconoscono il file autorevole tramite percorso del registro, percorso del composito, epoca e token. Un file sostituito con un composito indipendente ma strutturalmente valido, o una copia appartenente a un'altra partita, non porta un identificatore persistente che permetta di distinguerlo prima di un nuovo versamento. Il solo digest di città/escrow/ricevute non rappresenta l'identità del ramo.

## Decisione

1. Ogni **nuovo contenitore sperimentale** RC103 riceve un `campaign_id` GUID canonico (32 caratteri esadecimali) nella tabella `composite_campaign_identity`. Non viene rigenerato a ogni apertura e non dipende dal nome del file.
2. `E4bV15FundingAuthorityRegistry` memorizza lo **stesso ID** in `authority_campaign_identity`. `Current`, `Apply` e `AdvanceClock` verificano che il composito indicato dal percorso autorevole conservi tale ID. Una sostituzione fuori banda con un'altra campagna è rifiutata prima di pagamenti o tick.
3. `Promote` accetta soltanto un backup che conserva lo **stesso** ID, con contenuti e ricevute equivalenti. Il digest include anche l'identità, oltre alla città, al deposito e alle ricevute; la promozione continua ad aggiornare epoca e token e a invalidare la lease precedente.
4. L'identità dei compositi **registrati** è protetta da tre ulteriori trigger SQLite (INSERT, UPDATE, DELETE), distinti dagli otto trigger RC102. Le scritture ordinarie non possono sostituirla senza passare dal percorso autorizzato.
5. File legacy RC95–RC102 **non registrati** restano leggibili senza ID. La prima registrazione attribuisce un ID al file, con migrazione limitata al laboratorio. Registri RC101/RC102 esistenti vengono completati al riavvio e associati all'ID dell'attuale file autorevole. Il formato `PRAGMA user_version=1` del registro e del composito resta invariato; il salvataggio normale v15 generation 6 non è coinvolto.

## Garanzie, compatibilità e limiti

- Backup o ripristino SQLite del **medesimo** composito conservano l'identità e non creano una nuova campagna. Una partita creata indipendentemente (anche con stock e revisioni uguali) riceve un ID diverso.
- Un backup legacy realizzato **prima** che la campagna ricevesse un ID non viene promosso automaticamente: deve essere esaminato manualmente perché manca una prova dell'appartenenza alla stessa linea.
- La migrazione sperimentale aggiunge tabelle e trigger. Prima di registrare un file legacy di sviluppo è consigliabile creare una copia verificata; tornare a una versione vecchia non rende automaticamente scrivibile un file recintato.
- **Non è ancora un protocollo di salvataggio giocabile unico.** Registro e composito restano due database; non esiste commit atomico tra essi. L'identità non evita copie del file sullo stesso percorso, rollback causati da sostituzioni deliberate di file appartenenti alla stessa campagna, manomissioni dello schema, accessi multi-host non coordinati o dischi non affidabili. Non usare questo meccanismo come autenticazione o controllo antimanomissione.
- Nessuna modifica a GUI, prezzi, ricette, Municipio, personale, clock predefinito o salvataggi SQLite v15 generation 6. La curva A rimane nel solo laboratorio.

## Verifiche

Dodici nuovi test `Rc103CampaignIdentityTests` coprono identità univoche, stabilità, backup/ripristino, riapertura, promozione, rifiuto di campagna indipendente, sostituzione del file attivo, trigger di protezione, modifica del registro e migrazione controllata di dati RC101/RC102. Totale cumulativo **1330 test attesi**, di cui **1245 confermati** con RC95. Eseguire unicamente ` .\validate.cmd ` su Windows prima di considerare convalidata la RC103. Nessun `MANIFEST_XXX_SHA256.txt` nella distribuzione.
