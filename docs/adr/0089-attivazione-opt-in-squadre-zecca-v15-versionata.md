# ADR 0089 — Zecca con squadre minime nel runtime v15, opt-in versionato (RC81)

- Data: 06/11/2026
- Stato: **implementazione candidata; build/test .NET non ancora eseguiti**.
- Decisione: introdurre una modalità di partita **esplicita** `V15CrewMode.MintTen` che richiede 10 lavoratori per modulo di Zecca. Non applicare la modalità automaticamente alle campagne preesistenti né allo storico v14.

## Motivazione

RC80 ha implementato il modello teorico ma il gioco attivo continuava a limitare la Zecca a 1/3/10 posti e a produrre per ogni addetto. Una semplice somma del personale a livello di famiglia avrebbe anche consentito di riunire indebitamente una squadra incompleta della Zecca Base con una della Zecca Grande. RC81 integra il contratto senza imporre una regressione ai benchmark naturali RC77 e alle partite precedenti non convalidate di nuovo.

## Contratto introdotto

- `Legacy` continua a usare 1 posto e 1 ricetta per lavoratore; nessuna scelta automatica al caricamento.
- `MintTen`: ogni edificio Zecca Base/Grande/Avanzato possiede rispettivamente 1/3/10 **moduli**, ciascuno da 10 lavoratori. Per occupare tutti i moduli sono richiesti 10/30/100 lavoratori. Tutte le altre famiglie restano invarianti.
- Le assegnazioni sono registrate in **persone reali** (non in moduli); l'occupazione e il budget demografico sono validati sulla campagna completa. Il pianificatore riceve sia le persone assegnate sia il numero di moduli con squadra completa **calcolato separatamente per tier**. Non si prestano persone tra tier o altre filiere.
- Una squadra incompleta non produce né spende risorse. Le squadre complete continuano a rispettare priorità, input e capacità del `ProductionPlanner` esistente.
- Il giocatore può selezionare il nuovo regime solo con un comando esplicito dell'API di sessione `EnableMintCrews()`, senza alterare l'inventario. **La GUI non espone ancora una scelta iniziale**. Non viene attivata una nuova curva economica.
- Il nuovo regime sopravvive a `Snapshot()`/`with`, al clock e al round-trip SQLite. `game_save.rules_version` è `civil-growth-15-mint-crew10`; per le partite ordinarie rimane `civil-growth-15`. L'intera generation 6 (`PRAGMA user_version`, tabelle, tipi e percorso) non cambia. Valori di regole sconosciuti sono rifiutati, non interpretati come legacy.
- La proiezione GUI usa `CrewSize`, `CompleteModules` e la capienza lavoratori reale nelle stesse righe e nella stessa gerarchia di layout approvate; nessuna alterazione delle sette colonne risorse o dei pulsanti storici.

## Fuori ambito

Non si attiva la curva A/B/C, non si introduce deposito rateale in partita, non si modifica il costo storico Zecca di 210 Legno lavorato / 160 Pietra, non si estendono i requisiti ad altre fabbriche, non si reinterpretano i vecchi benchmark come nuova economia, non si effettuano migrazioni automatiche dei DB v14.

## Gate di promozione

Eseguire `validate-f1.cmd` su Windows con .NET 10 e **985 test cumulativi attesi** (926 RC78 verificati, 17 RC79, 22 RC80, 20 RC81 candidati). Verificare carenza squadre 0/9/10/19/20, tier, pause e riallocazioni, output e consumo, round-trip con modalità non legacy, modello RC80 senza pooling, benchmark storici in `Legacy` e screenshot GUI. Una verifica statica o SQL limitata non sostituisce questi test.

[Specifiche RC81](../design/E4B_RC81_RUNTIME_ZECCA_OPT_IN.md) · [Piano test](../validation/E4B_RC81_RUNTIME_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
