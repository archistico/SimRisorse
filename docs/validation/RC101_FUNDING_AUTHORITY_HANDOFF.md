# Piano di validazione RC101 — Autorità del finanziamento composito

**Baseline confermata:** RC95, 1245 test. RC96–RC100 candidate: 1294 test complessivi attesi. RC101 aggiunge 12 casi Infrastructure: **1306 test cumulativi attesi**, non ancora eseguiti con .NET 10.

## Controlli automatici

1. Registrare un composito già esistente e coerente; verificare persistenza di percorso, epoca e token dopo riapertura del registro.
2. Rifiutare secondo `RegisterInitial`, database native v15 e file non presenti senza creare un composito vuoto.
3. Applicare pagamento e clock **attraverso il registro**; dopo riavvio verificare idempotenza delle richieste e conservazione dei materiali.
4. Ripristinare una copia identica via RC100, promuoverla, verificare epoca incrementata e blocco di vecchie sessioni, poi effettuare una scrittura solo sul ramo nuovo.
5. Rifiutare copia precedente a un pagamento committato, ricevuta manomessa, stessa fonte come destinazione e token/epoca non validi. Non modificare il registro in caso di rifiuto.
6. Verificare che il salvataggio originale, la GUI e il formato giocabile restino invariati.

## Gate completo Windows

```powershell
.\validate.cmd
```

**Risultato atteso:** 1306 test superati, 0 falliti, screenshot, smoke nativo e audit F1 completati. Sono previsti controlli statici in questa consegna; compilazione ed esecuzione restano da confermare. Gli errori devono conservare le evidenze dell'ultimo run in `.local-cache/m4/`, non distribuita nello ZIP. Nessun `validate-f1.cmd`, nessun file manifest SHA-256.

## Limiti

La protezione vale soltanto per le operazioni compiute attraverso **lo stesso registro condiviso**. Gli accessi diretti all'attuale store e le copie indipendenti del registro non sono bloccati. Non dichiarare come acquisita una protezione di sessione universale, multi-host o contro manipolazione intenzionale. La curva A rimane sperimentale e i cantieri non entrano ancora nel normale salvataggio giocabile.
