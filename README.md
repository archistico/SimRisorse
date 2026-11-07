# SimRisorse — RC94.2 / Hotfix test Zecca dopo bootstrap Carbone (candidata)

**Baseline verde Windows:** RC93.1 con **1220 test superati**. RC94.1 ha eseguito **1233 test: 1228 superati, 5 falliti** per un'asserzione rimasta ancorata al vecchio stock Carbone. **RC94.2 corregge il solo test RC81**, conservando **1233 test attesi**; gate F1 da riconfermare su Windows. [Piano hotfix RC94.2](docs/validation/RC94_2_MINT_CREW_STARTER_COAL_HOTFIX.md).

**RC94:** `E4bV15FundingShadow` associa il checkpoint completo della campagna v15 a un deposito di materiali Curve A. I versamenti sono sottratti dall'inventario della copia di campagna, con controllo capacità, conservazione, costo congelato, contatore storico delle Zecche, revisioni, annullamento e round-trip. Si tratta di una **shadow in memoria non giocabile**: non inserisce edifici, non produce risorse, non modifica salvataggi, gameplay, GUI e prezzi attivi. Non accetta consegne ipotetiche dall'esperimento RC93.

**CLI di controllo:** `dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-v15-shadow`. È integrata nel gate ` .\validate-f1.cmd `.

**Riferimenti:** [ADR 0104](docs/adr/0104-checkpoint-composito-v15-escrow-curva-a-sandbox.md), [piano di test RC94](docs/validation/RC94_V15_FUNDING_SHADOW_TEST_PLAN.md), [stato](docs/CURRENT_STATE.md), [roadmap](docs/ROADMAP.md), [handoff](docs/NEW_CHAT_HANDOFF.md), [registro decisioni](docs/AUTHORITY_REGISTRY.md).

---

## Contenuti e cronologia funzionale RC90–RC90.2

## RC90 — Tecnologie e Depositi alimentari Grande/Avanzato

- Il dettaglio di **ogni tecnologia** nella GUI v15 mostra l'elenco degli edifici che sblocca, distinto per livello **Grande** o **Avanzato**, più gli eventuali prerequisiti; le tecnologie future non pubblicizzano edifici non disponibili. La v14 conserva la descrizione già presente.
- **Alimentazione I** sblocca anche **Deposito alimentare grande**, con 3 posti lavoro e capienza equivalente a 3 depositi Base; **Alimentazione II** sblocca **Deposito alimentare avanzato**, con 10 posti e capienza equivalente a 10 Base. Base, Grande e Avanzato sono voci autonome in Edifici, senza ComboBox e con icona del deposito già approvata.
- Acquisto verificato sul catalogo tecnologico, materiali addebitati normalmente, costo progressivo per famiglia, lavoratori separati per livello, produzione alimentare aggregata nel *medesimo* planner e capacità fisica contabilizzata una sola volta. Non viene reintrodotto il vecchio `food-store` v14.
- SQLite v15 **rimane generation 6**: `food_industry_site` memorizza due righe aggiuntive (`food-depot-large`, `food-depot-advanced`), mentre i file precedenti a sei righe continuano a caricarsi con livelli aggiuntivi a zero. Nessun refill o migrazione automatica, salvataggi v14 invariati.
- **Invariati:** scheda Risorse a sette colonne e tooltip RC89.1, layout pixel art, prezzi delle altre famiglie, curva economica A/B/C ancora inattiva e Zecca MintTen ancora opt-in.
- [ADR 0099](docs/adr/0099-tecnologie-e-depositi-alimentari-per-tier.md) · [Piano test RC90](docs/validation/RC90_DEPOSITI_E_DESCRIZIONI_TECNOLOGIE.md).

## Novità RC89.1 — dettaglio al passaggio del mouse su Produzione e Consumo

- Nella scheda **Risorse**, sorvolando le celle **Produzione /s** e **Consumo /s** si vedono i contributi dei singoli edifici/attività, con numero di edifici, unità attive, quantità al secondo e totale.
- La v15 registra tali contributi nello **stesso tick del clock** che produce i tassi lordi: sono inclusi industrie tradizionali, Stalla/Cacciatore/Macellaio, Orto/Frutteto, Deposito alimentare e pasti degli abitanti, senza contare le consegne come produzione o consumo.
- Compatibilità della stessa interazione con le finestre v14 e storica, senza cambiare le **sette colonne**, il loro allineamento, lo stile pixel art, i salvataggi o l'economia. Un produttore fermo non viene conteggiato come attivo.
- **12 nuovi test**, [ADR 0098](docs/adr/0098-tooltip-flussi-risorse-per-fonte.md) e [piano RC89.1](docs/validation/RC89_1_RESOURCE_FLOW_TOOLTIPS_TEST_PLAN.md); gate F1 aggiornato a **1135**.

## RC89 ereditata — verifica prima di cambiare i costi

- Matrice delle curve **A/B/C × Base/Grande/Avanzato**: costo prima/seconda Zecca e lotto x2, necessità personale 10/20, 30/60, 100/200, capacità iniziale, vincoli per risorsa, Magazzini e rate minime teoriche.
- CLI `--study-e4b-price-readiness [--input RC88_RESULTS_JSON] [--output NEW_DIRECTORY]`: importa facoltativamente ricevute RC88 con controlli di coerenza, esporta JSON/CSV/Markdown e provenienza SHA-256. Le curve sono controfattuali e i loro tempi di gioco **non sono misurati**.
- L'obiettivo di circa **100–1000 Tagliapietre** per la prima Zecca Base resta qualitativo; nessuna curva o finanziamento rateale è stata attivata. Vedi [ADR 0097](docs/adr/0097-verifica-pre-attivazione-costi-zecca-e-capienza.md).

## Gameplay attivo e invarianti

- Nuova partita v15: il Municipio assegna **2 Matite e 100 Carbone**, accreditati una sola volta per la nuova campagna, **senza** refill ai caricamenti o alle migrazioni. Insieme al Legno lavorato il Carbone permette di creare ulteriori Matite alla Scuola prima di sbloccare Estrazione I. Ricerca e sblocco `Estrazione I` → industria; scuola e punti ricerca accumulabili.
- Cibo pronto nel Deposito dedicato, filiera Pane, abitazioni che accelerano la crescita fino a ×20; risorse e capacità con regole uniformi.
- Zecca con squadre da 10 per modulo (`MintTen`) già disponibile in modalità **opt-in**; nuove partite predefinite `Legacy`. Costi attivi **immutati**.
- GUI v14 canonica mantenuta sulla v15: risorse a sette colonne, edifici bloccati ma consultabili, tier come edifici distinti, costi in **un solo elenco con icone**, pulsanti storici e sidebar Scuola/Tecnologie. Nessuna modifica grafica RC89.
- SQLite v15 generation 6 e salvataggi v14 separati, non modificati in RC89.

## Avvio e verifica (Windows, SDK .NET 10)

```powershell
.\run-game.cmd        # partita v15, interfaccia predefinita
.\run-game.cmd --v14 # modalità storica separata
.\validate-f1.cmd     # suite completa e gate F1
```

**Importante:** il programma non modifica retroattivamente i salvataggi v15 esistenti: la dotazione si applica solo scegliendo una nuova partita. Prima di sostituire il checkpoint attuale, il gioco continua a richiedere salvataggio esplicito.

## Documenti da aprire

1. **[Indice e comandi](docs/README.md)** — unico punto di ingresso alla documentazione.
2. **[Stato del progetto](docs/CURRENT_STATE.md)** — implementato, test eseguiti e test mancanti.
3. **[Roadmap](docs/ROADMAP.md)** — lavori futuri in ordine e criteri di uscita.
4. **[Handoff per nuova chat](docs/NEW_CHAT_HANDOFF.md)** — tutto il contesto necessario per riprendere.
5. **[Registro decisioni](docs/AUTHORITY_REGISTRY.md)** — regole approvate contro ipotesi.

Le release precedenti sono documentate nel [`CHANGELOG.md`](CHANGELOG.md); specifiche e decisioni puntuali rimangono in `docs/adr/`, `docs/design/` e `docs/validation/`.
