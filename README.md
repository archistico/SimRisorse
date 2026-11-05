# SimRisorse — gestionale di risorse, popolazione e ricerca

Progetto desktop C# / .NET 10 / Avalonia / SQLite in sviluppo, con GUI pixel art e campagna v15 comprendente industria, nuove filiere alimentari, Scuola, ricerca e militari.

**Codice corrente:** RC76-HF04 / E4a.2c, **885/885 test verificati su Windows** con `validate-f1.cmd`, incluso `validate.cmd`; compilazione senza errori o avvisi, smoke nativo esistente e audit v15 superati. [Rapporto HF04](docs/validation/RC76_HF04_ICONS_AND_GROWTH.md). Render v15 ispezionati alle due risoluzioni; confronto interattivo Windows ancora da confermare.

**RC76-HF04:** icone aggiornate su richiesta, intestazione Abitanti/Cibo/Legno lavorato/Pietra e pulsanti Carica/Salva. La crescita v15 sale fino a **×20**, mantenendo i bonus per casa: al massimo un abitante ogni 3 secondi con alloggi e alimentazione sufficienti. Undici nuovi test di crescita e ripresa.

**RC76-HF03:** corrette intestazioni sovrapposte dal filtro filiere, nomi tecnici nelle quotazioni e testi dei materiali tagliati in Scuola/Esercito. Cinque regressioni aggiunte e otto catture v15 obbligatorie nel gate.

**RC76-HF02:** ripristinata la proprietà pubblica `V15FoodIndustryCampaignSession.CityCatalog` utilizzata da GUI e test dopo i refactoring; corretti gli 8 riferimenti CS1061 senza modificare le chiamate. [Piano di verifica](docs/validation/RC76_HF02_CITY_CATALOG_TEST_PLAN.md).

**RC76-HF01 (inclusa):** corretti due errori di compilazione CS0117 nei benchmark E4a dovuti all’accesso a `BusyOutsideFood` (`internal` nel Core). Nuova proiezione pubblica `AvailableWorkers` condivisa con il gioco e due test; [piano di verifica](docs/validation/RC76_HF01_COMPILATION_TEST_PLAN.md). Corretto anche il confronto sessione/benchmark nel test del personale: le stesse assegnazioni al secondo zero lasciano 3 lavoratori liberi.

## Comandi Windows

```powershell
.\validate.cmd          # suite e smoke; 885 test, comprese le catture v15
.\validate-f1.cmd       # gate F1: validate + verifica TRX + audit v15 120 s
.\run-game.cmd          # v15 predefinita
.\run-game.cmd --v14    # v14, partita/salvataggio separato
```

Il gate F1 produce un report in `.local-cache/m4/f1-gates/`: l'esecuzione del 05/11/2026 è superata. La verifica visiva della GUI canonica rimane necessaria per chiudere formalmente F1-D05e. Per uno stress combinato lungo e facoltativo: `./validate-f1.cmd -Combined24Hours` (non eseguito).

**Filiere v15:** Stalla, Cacciatore, Macellaio, Orto, Frutteto, Deposito alimentare; il Cibo pronto è conservato soltanto nel Deposito. SQLite v15 generation 6, separato dal salvataggio personale v14. I vecchi database v15 di sviluppo possono essere incompatibili.

**Documentazione operativa:** [avvio](docs/START_HERE.md) · [stato](docs/CURRENT_STATE.md) · [roadmap](docs/ROADMAP.md) · [handoff](docs/NEW_CHAT_HANDOFF.md) · [decisioni](docs/AUTHORITY_REGISTRY.md) · [salvataggi](docs/SAVE_FORMAT.md) · [contratto grafico](docs/adr/0071-gui-v14-contratto-canonico-v15.md).

La RC73 ha introdotto il **benchmark naturale v15**; la RC74 aggiunge l’**analizzatore dei report** (tempi e cause registrate, senza nuove simulazioni). I due strumenti rimangono separati da v14 e dagli audit tecnici con scorte prefinanziate. Non sono ancora stati eseguiti benchmark completi, né modificati i costi. Il gate automatico è superato; F1-D05e rimane aperta per la conferma visiva della GUI.

```powershell
# Prova diagnostica breve, non misura economica definitiva:
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-natural --scenario V15-N01 --max-seconds 120
```

[Scenari benchmark v15](benchmarks/e4a-v15/README.md) · [specifiche E4a.1](docs/design/E4A_V15_RC73_NATURAL_RUNNER.md) · [piano test RC73](docs/validation/E4A_V15_RC73_TEST_PLAN.md). La progettazione quasi esponenziale e le squadre minime della Zecca rimangono successive alla raccolta dei dati E4a.

```powershell
# Analizzare un JSON RC73 già prodotto (nessuna simulazione aggiuntiva):
dotnet run --project tools/SimRisorse.Simulation -c Release -- --analyze-e4a-v15-natural --input .local-cache/e4a-v15/misura-001 --output .local-cache/e4a-v15/analisi-001
```

[Specifiche analisi RC74](docs/design/E4A_V15_RC74_ANALISI_PROGRESSI.md) · [Piano test RC74](docs/validation/E4A_V15_RC74_TEST_PLAN.md) · [ADR 0082](docs/adr/0082-analisi-basata-su-evidenze-benchmark-v15.md). Nessuna misura a orizzonte pieno disponibile.

## RC75 — Ricerca, Forgia e Zecca, senza cambiare il gioco

Il benchmark v15 confronta `research-first` e `bread-first` per le prime tecnologie, Spade e Monete, registrando tutte le spese effettive delle azioni e i punti ricerca separatamente. Eseguita la diagnostica V15-I01 / research-first da 120 secondi e verificata la rianalisi dei report; le misure a orizzonte pieno restano `not-run` e il bilanciamento non cambia.

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-v15-industry --scenario V15-I01 --policy research-first --max-seconds 120
```

[Scenari](benchmarks/e4a-v15/industrial-scenarios.csv) · [Specifica](docs/design/E4A_V15_RC75_INDUSTRIA_RICERCA.md) · [Test](docs/validation/E4A_V15_RC75_TEST_PLAN.md) · [ADR 0083](docs/adr/0083-benchmark-industria-ricerca-v15-senza-grants.md).


## RC76 — Evidenza di ammanchi e capacità per i benchmark v15

Il benchmark industriale registra ora, quando misurabili, i materiali che mancano a un acquisto (al millesimo) e scorte/capacità cittadine di Legname lavorato, Pietra, Carbone, Ferro e Oro. L'analisi aggiunge tre file `e4a-v15-industry-constraints.{json,csv,md}`; non considera mai un timeout o una saturazione isolata come deadlock certo. I JSON RC75 privi dei nuovi campi rimangono leggibili, con telemetria marcata come non disponibile.

[Specifiche RC76](docs/design/E4A_V15_RC76_ANALISI_VINCOLI.md) · [Piano test RC76](docs/validation/E4A_V15_RC76_TEST_PLAN.md) · [ADR 0084](docs/adr/0084-telemetria-vincoli-benchmark-v15-senza-diagnosi-automatica-stallo.md) · [Esito diagnostico Windows](docs/validation/RC76_WINDOWS_VALIDATION.md).
