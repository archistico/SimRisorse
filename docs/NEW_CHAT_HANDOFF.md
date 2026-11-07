# Handoff unico — SimRisorse RC94.2

## Ripresa immediata

**Baseline verde:** RC93.1, **1220 test superati**. In Windows RC94.1 ha eseguito **1233 test (1228 superati, 5 falliti)** tutti per l'asserzione storica RC81 che prevedeva 0 Carbone iniziale. **RC94.2** cambia soltanto il test `Rc81LiveMintCrewTests`: confronta il consumo reale (`prima - dopo`) di Oro e Carbone con i moduli Zecca attivi. **1233 test attesi**, gate ancora da confermare con ` .\validate-f1.cmd `. [Piano hotfix](validation/RC94_2_MINT_CREW_STARTER_COAL_HOTFIX.md).

**RC94 — `E4bV15FundingShadow`:** un modello applicativo opt-in, in memoria, che vincola il checkpoint completo v15 all'escrow Curve A. `Open` usa il contatore storico `mint`, la capacità calcolata dalla città e il vero inventario. `TryBeginMint` congela il prezzo, `TryDeposit` trasferisce dalla città shadow all'escrow, `TryCancel` restituisce i materiali; `ExportCheckpoint`/`Restore` rifiuta stock discordanti, capacità o contatori falsi, consegne ipotetiche e operazioni completate soltanto nel vecchio sandbox. `Readiness` controlla pagamento, edificio prerequisito (`gold-smelter`) e tecnologie di tier; è **solo diagnostica**, nessun nuovo edificio viene costruito.

**Dettaglio hotfix RC94.2:** l'incremento di +100 Carbone dal Municipio nella RC94.1 è corretto e intenzionale; non rimuoverlo per far passare il test. Nessuna modifica a produzione, salvataggi o GUI.

**CLI RC94:** `dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-v15-shadow`. Test xUnit: `tests/SimRisorse.Application.Tests/Rc94V15FundingShadowTests.cs`. Gate atteso 1233. [ADR 0105](adr/0105-riserva-carbone-municipio-avvio-v15.md) · [Piano RC94.1](validation/RC94_1_MUNICIPIO_CARBONE_TEST_PLAN.md) · [ADR 0104](adr/0104-checkpoint-composito-v15-escrow-curva-a-sandbox.md) · [Piano RC94](validation/RC94_V15_FUNDING_SHADOW_TEST_PLAN.md).

## Principale limite aperto

RC94 **NON** è un salvataggio giocabile. RC93 usa un SQLite sperimentale indipendente per il vecchio sandbox con consegne ipotetiche; RC94 lega inventario e deposito solo in una copia di checkpoint v15. La partita vera non è modificata. Prima di integrare nel gameplay occorrono: stock sincronizzato col clock naturale, cantiere e ricevute nello stesso salvataggio, idempotenza e costruzione finale in un unico commit senza secondo addebito. Il completamento RC93 non può essere invocato direttamente come `BuildCityProducer()` perché quel comando ripagherebbe integralmente il prezzo.

## Requisiti approvati da preservare

- **GUI pixel art v14 canonica**: preservare tabella Risorse a sette colonne e tooltip Produzione/Consumo per fonte, elenco edifici con filtri `Tutti`, `Alimentazione`, `Estrazione`, `Industria`, `Lavorazioni`, `Logistica`, `Abitazioni`, `Civici`, `Militari`; icone nel dettaglio e nelle tecnologie; edifici bloccati grigi ma selezionabili; nessuna ComboBox dei livelli.
- **Economia attuale** invariata. Prezzi quasi esponenziali Curve A/B/C solo proposte, obiettivo qualitativo prima Zecca Base 100–1000 Tagliapietre; finanziamento non ancora attivo.
- **Municipio**: 100 Carbone e 2 Matite solo a nuova partita v15 (no refill in caricamento/migrazione); 100 Carbone consentono fabbricare altre Matite alla Scuola con Legno lavorato anche prima di Estrazione I. Scuola → ricerca → estrazione senza loop artificiale; deposito Cibo pronto unico, Deposito Base/Grande/Avanzato sbloccato da Alimentazione I/II; pane, popolazione accelerata con abitazioni.
- `MintTen` 10/30/100 addetti per tier disponibile **opt-in**, nuove partite Legacy; inventari e lavoratori mai duplicati.
- Formato SQLite v15 generation 6 della campagna **non modificato** in RC94; format del laboratorio RC93 separato. Eventuali rotture future dei vecchi salvataggi vanno documentate esplicitamente.

## Dopo validazione

Seguire [ROADMAP](ROADMAP.md): RC95 per checkpoint persistente e commit transazionale; poi prova naturale della curva A con magazzini e filiere. Non dichiarare conclusa la progressione in assenza di eventi reali del clock. Conservare sempre ZIP integrale, ADR, piano test, changelog e i documenti operativi unici.

[Stato corrente](CURRENT_STATE.md) · [Decisioni](AUTHORITY_REGISTRY.md) · [ADR](adr/README.md) · [Changelog](../CHANGELOG.md).
