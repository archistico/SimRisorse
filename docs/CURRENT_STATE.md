# Stato corrente — SimRisorse RC94.2

**07/11/2026 · CANDIDATA.** Ultima baseline verde confermata: **RC93.1, 1220 test**. RC94.1 ha eseguito in Windows **1233 test (1228 passati, 5 falliti)**: i cinque casi della teoria RC81 della Zecca assumevano 0 Carbone iniziali. **RC94.2** corregge l'asserzione sul consumo del tick, con **1233 test previsti**, ancora da eseguire. [Piano hotfix RC94.2](validation/RC94_2_MINT_CREW_STARTER_COAL_HOTFIX.md). Nessuna modifica a runtime/GUI/SQLite rispetto a RC94.1.

## Incremento implementato

- `E4bV15FundingShadow`: nuovo **checkpoint composito** contenente il `V15FoodCampaignCheckpoint` completo e l'escrow Curve A. `Open`, `TryBeginMint`, `TryDeposit`, `TryCancel`, `Restore`/`ExportCheckpoint` e `Readiness` hanno contratti separati e non alterano la partita vera.
- Tutti i versamenti diminuiscono anche le risorse dell'inventario della **copia shadow** della città; vengono validati con il validatore v15 e con controllo della capacità effettiva. Nessuna consegna ipotetica è accettata.
- Le revisioni impediscono richieste locali su snapshot obsoleti. Prezzo e copia della Zecca restano congelati; `Readiness` verifica prerequisiti e ricerca ma **non** costruisce alcun edificio.
- Nuova CLI `--study-e4b-v15-shadow`, ora parte del gate F1. [ADR 0104](adr/0104-checkpoint-composito-v15-escrow-curva-a-sandbox.md) e [piano RC94](validation/RC94_V15_FUNDING_SHADOW_TEST_PLAN.md).

## Cosa resta attivo e invariato

- Campagna v15, GUI pixel art v14 canonica, pannello Risorse a sette colonne e tooltip per fonte; filtri settoriali in Edifici; icone tecnologie e sblocchi edifici; pulsanti e disposizione già approvati.
- Municipio: 2 Matite + **100 Carbone iniziali**, accreditati **solo alla creazione** della nuova partita v15; il Carbone rimane nel normale inventario e può alimentare il laboratorio Matite anche prima di Estrazione I. Ricerca, tecnologie, deposito alimentare Base/Grande/Avanzato; nessun refill al caricamento.
- `MintTen` (10/30/100 lavoratori per Zecca completa per Base/Grande/Avanzato) ancora **opt-in**. Le nuove partite predefinite usano il regime `Legacy`.
- Prezzi attivi invariati. Le curve A/B/C e i pagamenti rateali **non sono ancora giocabili**. Salvataggi della campagna SQLite generation 6 immutati, e database RC93 separato; il nuovo composito RC94 non è persistito in nessuno di essi.
- Gli studi pregressi usano benchmark controfattuali e non dimostrano durata naturale con i nuovi prezzi.

## Stato test e limiti

| Versione | Evidenza |
| --- | --- |
| RC90.3 | 1167/1167 superati, comunicati dall'utente |
| RC93.1 | **1220/1220 superati**, comunicati dall'utente (include incremento RC91.3 e RC92) |
| RC94.1 | **1233 eseguiti: 1228 passati, 5 falliti** per attesa assoluta Carbone obsoleta nel test RC81 |
| RC94.2 | **1233 attesi**, hotfix test RC81; gate da ripetere su Windows |

**Limite di progettazione fondamentale:** un saldo escrow pagato non equivale a un edificio; collegarlo direttamente a `BuildCityProducer()` provocherebbe un doppio addebito. Prima del gameplay servono una transazione unica di commit v15, sincronizzazione col clock durante i versamenti, persistenza/replay idempotente nello stesso file di partita e prove di progressione naturale.

Aprire [ROADMAP](ROADMAP.md), [AUTHORITY_REGISTRY](AUTHORITY_REGISTRY.md), [NEW_CHAT_HANDOFF](NEW_CHAT_HANDOFF.md) e [ADR 0104](adr/0104-checkpoint-composito-v15-escrow-curva-a-sandbox.md).
