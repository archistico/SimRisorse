# Piano test RC100 — Backup e ripristino SQLite con cantiere aperto

**Baseline confermata:** RC95, 1245 test. RC96–RC99 candidate: 1282 test attesi. RC100 aggiunge **12 test Infrastructure**, totale cumulativo **1294 attesi**. Non dichiarare verde prima della verifica Windows.

## Casi da verificare

- Backup consistente del file composito (online SQLite backup) in destinazione nuova. Originale invariato, `integrity_check=ok`, caricamento dello snapshot completo prima della promozione.
- Ripristino in un altro file **nuovo**, lasciando invariato il backup; non usare i due file simultaneamente come autorità concorrenti.
- Rate pendenti preservate (città, escrow e revisione), con replay dello stesso identificativo pagamento **senza secondo addebito**.
- Zecca completata e contatore storico recuperati; richiesta `Complete` già ricevuta non deve incrementare di nuovo il contatore.
- Rifiuto di fonte=destinazione, file target già esistente, file di origine mancante, directory target assente e database nativo giocabile usato come backup composito.
- Ricevuta cancellata oppure revisione ricevuta alterata: errore prima di procedere e nessuna destinazione pubblicata; staging temporaneo eliminato.
- Il backup contiene uno snapshot consistente ma non blocca definitivamente la sorgente: altre operazioni successive non sono incorporate.

## Validazione completa

```powershell
.\validate.cmd
```

Il gate deve chiudersi con **1294 test verdi**, build Release, screenshot, smoke Windows e gli audit preesistenti. Le prove sono conservate nell'ultima `.local-cache` locale, esclusa dallo ZIP. Non esistono né `validate-f1.cmd` né file `MANIFEST_*_SHA256.txt`.

## Limiti

Questa release **non** cambia il formato del salvataggio giocabile, non consente cantieri pendenti nel salvataggio nativo, non attiva Curve A nel gameplay e non dimostra la progressione naturale verso la seconda Zecca. Questo è il contratto tecnico per il recupero dello **store sperimentale**, non un protocollo definitivo di migrazione in-place.
