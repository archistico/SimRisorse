# F1-D05e.1 — RC58: confronto deterministico dei checkpoint SQLite

## Evidenza osservata

RC57 è stata eseguita su Windows/.NET 10: **743 test totali, 741 superati e 2 falliti**, entrambi nella suite Infrastructure. In entrambi il confronto `AssertSame` si arresta su `OtherBuildings`, pur riportando gli stessi cinque record con stessi conteggi e stessi lavoratori. La GUI Desktop ha superato i propri test. Il processo di validazione termina con exit code 1 **per gli xUnit falliti**, non per un errore CSxxxx.

## Causa precisa

`V15SqliteFoodSaveStore.ReadFile` legge `building_stock` con `ORDER BY building_id` (ordine alfabetico stabile); `V14CampaignSnapshot` generato dalla fixture conserva l'ordine del catalogo. `OtherBuildings` è un insieme logico identificato da `BuildingId`, non una sequenza con indice significativo; nessuna posizione è salvata in SQLite. La comparazione diretta di due `ToArray()` imponeva accidentalmente l'ordine e creava un falso negativo.

Anche `ProductiveFamilies`, `PurchaseCounters` e `UnlockedTechnologyIds` sono persistiti per chiave e ricaricati ordinati per ID. Normalizzare anche questi evita altri falsi negativi negli stessi due test quando superano la prima asserzione.

## Correzione RC58

- Ordinare entrambi i lati prima di `Assert.Equal`, rispettivamente per `BuildingId`, `FamilyId`, `FamilyId` e ID tecnologia, con `StringComparer.Ordinal`.
- Non confrontare soltanto gli ID: gli oggetti completi restano nelle asserzioni, quindi variazioni di conteggi, lavoratori e livelli restano rilevate. Differenze di elementi, duplicati o molteplicità restano visibili.
- Conservare l'ordine per progressi individuali di addestramento/scuola (indici con significato) e non alterare altri confronti di risorse, popolazione, ricerca, militari, Cibo e scorte intermedie.
- Aggiungere `KeyedCampaignCollectionsCanBeReorderedWithoutChangingCheckpointMeaning` come regressione autonoma dell'helper.

**Ambito modifiche:** solo un file di test, nessun `.cs` di produzione. Non cambia il round-trip SQLite, il catalogo, il clock, lo schema v15 generation 5, la GUI o il salvataggio v14.

## Criteri d'accettazione

1. `./validate.cmd` completa l'intera suite **744/744** su .NET 10 Windows; nessuna esecuzione è dichiarata nel pacchetto.
2. Entrambi i test SQLite RC57 superano `AssertSame` in tutti i campi, dopo il confronto per chiave.
3. Il nuovo caso con collezioni invertite supera `AssertSame` senza scambiare lavoratori o risorse.
4. Se emergono fallimenti in altri campi, trattarli separatamente e non allentare la validazione del gioco.

[Piano test](../validation/F1_D05E1_RC58_TEST_PLAN.md) · [Stato](../CURRENT_STATE.md) · [Roadmap](../ROADMAP.md) · [ADR 0067](../adr/0067-riconciliazione-fisica-v15-e-stress-separato.md).
