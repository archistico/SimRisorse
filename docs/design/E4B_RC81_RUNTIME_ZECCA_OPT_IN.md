# RC81 / E4b.2c — Squadra minima Zecca nel clock effettivo, opt-in

## Dove entra nel gioco

1. Campagna carica un `V15FoodCampaignCheckpoint` con `CrewMode=Legacy` (impostazione preesistente).
2. `V15FoodIndustryCampaignSession.EnableMintCrews()` attiva una sola volta la modalità `MintTen` con validazione atomica dell'intero checkpoint. È un punto di integrazione per una futura selezione nell'interfaccia; **oggi non compare alcun nuovo comando visuale**.
3. `NativePortfolio("mint")` fornisce capacità di assunzione `1/3/10 × 10` per livello; le altre famiglie restano a `1/3/10 × 1`. Upgrade conserva le persone effettivamente assegnate (non crea lavoratori).
4. `V15UnifiedCampaignClock.Plan` costruisce il roster con i **moduli** della Zecca, passa `crewSizes` e `staffedModules` al `ProductionPlanner`, raccoglie flussi effettivi dalla ricetta vera e valida il checkpoint successivo.
5. Nella GUI, `V15BuildingEntry` espone dimensione squadra e moduli completi; `V15BuildingRow` usa le persone disponibili per la capienza e aggiunge informazioni nel `Summary` esistente, senza cambiare il template.
6. SQLite generation 6 continua a usare la medesima struttura fisica: `game_save.rules_version` vale `civil-growth-15` per `Legacy` e `civil-growth-15-mint-crew10` per `MintTen`. Il loader applica la validazione appropriata e rifiuta i valori sconosciuti.

## Regola essenziale del personale

Per ciascun tier `t`, `moduli_t = edifici_t × fattore_t` con fattori `1/3/10`, `completi_t = floor(assegnati_t / 10)`. Il limite di produzione Zecca è `sum(completi_t)` (soggetto anche agli input e all'output), non `floor(sum(assegnati_t)/10)`. Esempio: 9 addetti Base + 9 Grande = **0** moduli completi. Dieci Base + nove Grande = **1**, benché ci siano 19 persone impegnate.

## Cosa non è stato dimostrato

I test sviluppati sono fixture controllate, NON percorrenze naturali. Restano necessari verifica con .NET, validazione desktop reale, benchmark naturale da zero con una politica che sappia trovare 10 addetti, scelta curva economica e analisi anti-softlock capacità/prezzo. I 9 risultati naturali RC77 misurano il regime precedente e non sono riferibili a `MintTen`.

## Scelte sospese

La semantica dei tier da 1/3/10 moduli rimane sperimentale e potrebbe essere ritarata prima dell'attivazione **predefinita**. La scelta esplicita mantiene la semantica precedente conservata per confronti e verifica. Nessun aumento economico e nessun pagamento rateale vengono anticipati.

[ADR 0089](../adr/0089-attivazione-opt-in-squadre-zecca-v15-versionata.md) · [Piano test](../validation/E4B_RC81_RUNTIME_TEST_PLAN.md).
