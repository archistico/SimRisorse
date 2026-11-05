# RC73 / E4a.1 — Strumento di misurazione della progressione v15

**Stato:** implementato, compilazione e test .NET 10 **da eseguire**. Ultimo checkpoint confermato: RC63 (771 test); F1-D05e è aperto, gate RC72 non eseguito. Nessun tempo di completamento è stato misurato in questa consegna.

## Scopo e confini

- Non sostituire i benchmark v14 `--benchmark-e4a-natural` e `--benchmark-e4a-school`: la nuova CLI è `--benchmark-e4a-v15-natural`.
- L'avvio deriva dallo stesso scenario e dalla preparazione in memoria della **nuova partita v15 della GUI**, senza finanziamenti sintetici. I vecchi audit `--audit-v15 --profile combined` restano distinti, perché partono con edifici e scorte artificialmente configurati.
- Costruzioni e assegnazioni attraversano i comandi v15, con controlli di costi, prerequisiti, capacità e budget di popolazione. Nessun comando altera gli inventari se rifiutato.
- Ogni tick avanza tramite `V15UnifiedCampaignClock.Plan`; dopo il completamento dei passi previsti, le prime produzioni sono rilevate dal contatore **lordo effettivo** del clock, non dalla scorta finale (che può essere consumata durante lo stesso secondo).
- Non modifica la GUI, SQLite, prezzi, ricette, capienze né la funzione di crescita.

## Scenari RC73

| ID | Obiettivo | Orizzonte dichiarato | Azioni di riferimento |
| --- | --- | ---: | --- |
| V15-N01 | Primo Cibo prodotto dal Deposito v15 | 14.400 s | Acquisto Pescatore, addetto, Deposito e addetto |
| V15-N02 | Primo Pane prodotto | 28.800 s | Avvio filiera Cibo, casa aggiuntiva, Fattoria, Pozzo, Mulino e Forno con addetti |
| V15-N03 | Prima Selvaggina prodotta | 14.400 s | Cacciatore e addetto |

I passi sono **seriali** e la politica prova a eseguirli in successione. A scorte insufficienti attende il clock: nessun acquisto viene finanziato. L'assenza di personale può causare `timeout` e deve rimanere visibile, non risolta con abitanti regalati.

## Raccolta ed esiti

- `pass`: prima produzione positiva effettivamente osservata, con `CompletionSeconds` valorizzato.
- `timeout`: obiettivo non osservato entro l'orizzonte, con completamento nullo e ultima fase in `StopReason`.
- `blocked`: prerequisito invalido o azione non recuperabile nella politica, con completamento nullo e motivo.
- `Diagnostic=true`: l'utente ha ridotto l'orizzonte tramite `--max-seconds`, quindi **il risultato non è un benchmark a orizzonte pieno**.
- L'uscita della CLI è 0 per un **risultato di misura valido** (anche timeout/blocked) e 1 per errori di parametri, I/O o simulazione. L'esito è nel JSON/CSV, non nel solo exit code.

**Campionamento:** una decisione ogni 60 s mentre restano azioni da compiere, poi un secondo per individuare esattamente la prima produzione. Il tempo di costruzione è quantizzato alla frequenza delle decisioni della strategia, non corrisponde necessariamente al momento in cui un giocatore manuale avrebbe potuto acquistare.

**Precisione:** i valori monetari delle scorte e dei costi sono mantenuti in milliunità nei report. Le somme delle scorte sono fisiche, quindi non duplicano gli input trasferiti al Deposito. Per tick da 1 secondo viene eseguita anche la riconciliazione con `V15StockAuditor`.

## Limiti e passaggio a RC74

Questa RC è uno **strumento di rilevazione**, non un risultato economico. È ancora necessario validarlo su Windows e completare il gate F1. Solo dopo vanno eseguiti gli orizzonti completi e raccolti i dati con commit/build, configurazione e file prodotti.

RC74 amplierà i percorsi verso Scuola, ricerca, tier Grande, Forgia e Zecca, con strategie alternative e classificazione distinta fra impossibilità strutturale, softlock economico e insufficienza della politica scripted. Nessun coefficiente E4b è stato cambiato.
