# ADR 0105 — Municipio: 100 Carbone e 2 Matite all'avvio v15

**Stato:** accettata (richiesta esplicita dell'utente) · **07/11/2026** · **RC94.1**

## Problema

Carbone necessario alla produzione di Matite e `Estrazione I` che precede la Miniera di carbone possono creare una dipendenza circolare nelle prime fasi del gioco. Le 2 Matite introdotte con RC86 aiutano il primo studente, ma non forniscono una riserva materiale per la successiva produzione di Matite.

## Decisione

- Una **nuova partita v15** inizia con il normale stock civico originario (120 Legno lavorato, 80 Pietra, 30 Cibo), **2 Matite** e **100 Carbone**.
- È **un accredito unico**, eseguito solo nella factory `V15FoodCampaignCheckpoints.NewCampaign`. I salvataggi esistenti, le importazioni v14, il caricamento SQLite e il clock **non** concedono risorse aggiuntive.
- Il Carbone appartiene all'inventario condiviso della città, non a un deposito occulto o a una ricetta alternativa. La capacità fisica iniziale del Municipio è 1.000 Carbone; 100 vi rientrano.
- Al laboratorio della Scuola, una lavorazione standard richiede **1 Legno lavorato + 1 Carbone** e produce **4 Matite**. Non richiede `Estrazione I` né una Miniera di carbone già costruita, ma la Scuola deve esistere. Le ricerche e i costi relativi non cambiano.

## Compatibilità / limitazioni

- Solo per il nuovo avvio v15. Non alterare lo scenario storico `CivilGrowthScenario.Create()` né generare Carbone retroattivamente nei database già salvati.
- SQLite generation 6 invariato; nessun nuovo campo o migrazione. I benchmark naturali partono dalle medesime scorte della GUI.
- RC94 finanziamento progressivo e curve A/B/C rimangono sperimentali. GUI canonica e scheda Risorse immutate.

## Verifica

Casi di regressione: risorse e capacità di avvio, laboratorio prima di Estrazione I, inventario comune dei benchmark, nessun refill nei salvataggi e migrazioni, controllo ricevute RC87. Gate previsto **1233** test, da confermare su Windows.

## Evidenza RC94.1 e hotfix RC94.2

Il run Windows RC94.1 ha registrato 1228 test superati e 5 falliti: tutti i casi della teoria Zecca RC81 attendevano scorte iniziali senza i 100 Carbone introdotti qui. La RC94.2 corregge esclusivamente quelle asserzioni tramite differenza fra stock iniziale e finale. **La decisione di conferire 100 Carbone resta invariata**. [Piano hotfix](../validation/RC94_2_MINT_CREW_STARTER_COAL_HOTFIX.md).
