# ADR 0109 — Completamento atomico di Zecca finanziata nel laboratorio RC98

**Stato:** sperimentale, in attesa del gate Windows. **Data:** 07/11/2026.

## Contesto

RC92–RC97 hanno introdotto la curva A, un deposito progressivo, uno stato v15 *shadow*, uno store composito SQLite e tick reali del clock. La fase finale richiede di convertire un finanziamento completamente pagato in **una** Zecca senza rieseguire `BuildCityProducer`, che scalerebbe nuovamente le risorse.

## Decisione

1. `E4bV15FundingShadow.TryCompleteMint` accetta solo un preventivo interamente pagato, la revisione attesa e prerequisiti/ricerca ancora validi. Usa il `TieredBuildingPortfolio` e incrementa di uno **il tier** e **il contatore di acquisti**, senza addebitare il preventivo una seconda volta.
2. `E4bCurveAFundingSandbox.TryCompleteWithCapacity` sposta il deposito pagato in **Committed**, registra una ricevuta verificabile con tier/epoca/prezzo Curve A e aggiorna la capacità fisica dopo la costruzione. I registri distinguono pagamenti pendenti e già consumati.
3. Il ripristino esige coerenza tra **receipts**, prezzi congelati, acquisti, edifici per tier, inventario e magazzini. Le quote non vengono ricalcolate durante il pagamento, ma solo validate alla riapertura.
4. `E4bV15FundingCompositeSqliteStore.Apply` ammette `Complete` nello **stesso commit SQLite** dell'immagine v15, checkpoint escrow, nuova revisione e ricevuta idempotente. Richieste duplicate fanno replay; revisioni obsolete e richieste discordanti non costruiscono.
5. Il comando `--study-e4b-v15-mint-commit` e il fixture `E4bV15MintCompletionFixture` utilizzano **una città sintetica già dotata di magazzini e materiali**: sono test di transazione, **non** prove di progressione naturale. Il gate deve indicarlo.

## Esclusioni

- Non si attiva la curva A nei salvataggi giocabili; non si modifica `BuildCityProducer` o la GUI.
- Non si concede un cantiere nella partita normale, non si introducono rifornimenti nella campagna reale.
- Non si cambia il formato SQLite generation 6 dei giocatori né la squadra Zecca predefinita.
- La struttura JSON **sperimentale** del finanziamento include nuove informazioni su origine e ricevute. I vecchi record sperimentali già completati senza tali informazioni possono essere rifiutati; **non** riguarda i salvataggi del giocatore.

## Verifica richiesta

`./validate.cmd` su Windows: 1270 test attesi, zero errori, render/stress/smoke F1 compresi, nuovo report `rc98-v15-mint-commit.json` coerente. Un test verde locale non è disponibile nell'ambiente di preparazione.

## Passi successivi

RC99: consolidare i contratti di promozione dal laboratorio al salvataggio giocabile, replay e recupero dopo interruzione. Prima della promozione: prove di progressione naturale e decisione esplicita sui prezzi.
