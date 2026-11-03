# M5.2b — Zecca e Monete (v40, validazione Windows pendente)

Data: **28 ottobre 2026**. Questo incremento prepara il secondo trasformatore del contratto A già approvato. I test .NET, il validatore completo e il controllo grafico non sono eseguibili nell'ambiente di preparazione.

## Modifiche introdotte

| Caratteristica | Valore in `civil-growth-9` |
| --- | --- |
| Struttura | **Zecca**, ID `mint` |
| Costruzione | **210 Tavole** e **160 Pietra** |
| Prerequisito | Una **Fonderia d'oro** costruita |
| Personale | **1 addetto** per Zecca |
| Consumo ogni secondo per addetto | **1 Oro + 1 Carbone** |
| Produzione ogni secondo per addetto | **3 Monete**, ID `coins` |
| Priorità del Carbone | Ferro **90**, Oro **91**, Forgia **100**, **Zecca 101** |
| Capacità Monete | **1.000 iniziale + 500 per Magazzino** |
| Ricetta | **Lotto indivisibile**: occorrono almeno 3 posti liberi; non produce monete parziali |
| Destinazione Monete | Scorta per futuri **pagamenti espliciti**; **nessun costo monetario generalizzato** |

Il catalogo attivo comprende **19 tipi di edifici e 16 risorse**. Sono disponibili icone pixel originali per Zecca e Monete, descrizioni dei prerequisiti e delle cause di blocco e informazioni sul lotto/capacità nella pagina Risorse. Non vengono aggiunti soldati, navi, pagamenti, bonus, produzione offline o nuove modalità di controllo del Carbone.

## Compatibilità e isolamento

- Il catalogo nuovo è **`civil-growth-9`**; la migrazione esplicita **v8→v9** aggiunge Zecca con `Count=0`, `AssignedWorkers=0`. La risorsa Monete parte da zero.
- Restano gli upgrade concatenati **v1→…→v7→v8→v9** e lo slot civile preesistente.
- I checkpoint v8 che dichiarano **Monete positive** prima della loro introduzione sono rifiutati, senza sostituzione silenziosa dello stato.
- Salvataggi SQLite e snapshot nel formato **v2**, backup del file originale al primo salvataggio successivo alla conversione; tempo, crescita frazionale, scorte, edifici e lavoratori esistenti preservati.
- La factory `CreateGoldForBenchmarks()` resta **v7**; la nuova `CreateForgeForBenchmarks()` congela il **catalogo v8** per le prove storiche di Forgia e salvataggi.
- Anche il validatore PowerShell ora attende `civil-growth-9` nelle verifiche industriali e di crescita accelerata, mentre i benchmark storici continuano a usare le proprie factory.

## Nuovi controlli xUnit preparati (non eseguiti)

**Application:** contratto A e integrità v8; primo lotto dopo la disponibilità dell'Oro del secondo precedente; capacità Monete a 997/998/999; precedenza del Carbone 90/91/100/101; assenza di pagamento monetario implicito negli acquisti ordinari; replay con salvataggio dopo 250 ms; rifiuto di checkpoint v8 con Monete preesistenti (**9 casi**).

**Infrastructure:** migrazione v8→v9 tramite SQLite con scorta Spade non nulla, popolazione, addetti, tick/frazioni, lettura senza alterare il file e backup dell'originale alla prima scrittura (**1 caso**).

**Desktop:** nome, prezzi, consumi, produzione, prerequisiti, pittogrammi e messaggio di stoccaggio Monete/lotto Zecca (**1 caso**).

**Regressione conversioni:** esteso ai checkpoint v8 un dato del test parametrizzato di conversione S1 (**1 caso**).

La v39 prevedeva **222 casi xUnit**. L'incremento aggiunge **12 casi**: **234 casi xUnit attesi nella v40**, senza conferma di compilazione o esecuzione. Il log della v38 aveva mostrato **33/53 DesktopTests falliti**, tutti per la stessa violazione del thread grafico; la correzione introdotta in v39 resta da verificare.

## Gate operativo

Prima la suite che ha prodotto gli errori sul PC Windows:

```powershell
dotnet test tests/SimRisorse.Desktop.Tests/SimRisorse.Desktop.Tests.csproj -c Release --logger "console;verbosity=normal"
```

Poi, quando tutti i test Desktop passano, la validazione completa:

```powershell
.\validate.cmd
```

Controlli manuali suggeriti: entrambe le risoluzioni **1040×740 e 1240×850**; selezione ocra e scrollbar degli elenchi; Zecca visibile in Edifici; Monete in Risorse e incremento di capacità dopo un Magazzino; Zecca senza Oro/Carbone e con 1–2 soli posti liberi; salvataggio e ripresa di una partita civile v8 con Spade già accumulate.

Finché non viene acquisito questo gate, M5.2b è **implementata ma non validata**. Nella successiva **v41** è stato preparato un [benchmark diagnostico M5.2c](M5_2C_COAL_BENCHMARK.md) sul catalogo già implementato, senza modifiche al gameplay; **non** rappresenta una validazione o la chiusura di M5.2c. Non introdurre ancora reclutamento o pagamento di navi; i numeri **241/122 secondi** restano stime teoriche finché il benchmark non viene eseguito.
