# ADR 0080 — Gate esplicito e riproducibile per la chiusura di F1-D05e

**Data:** 05/11/2026 · **Stato:** accettato come procedura, **esecuzione pendente** · **Incremento:** RC72.

## Contesto

Fra RC64 e RC71 sono stati introdotti refactoring cumulativi senza poterli compilare in questo ambiente. RC63 è l'ultima suite confermata (771/771). F1 non può essere dichiarata conclusa per la sola esistenza di nuovi test o per un audit storico su un profilo diverso.

## Decisione

1. Conservare `validate.cmd` invariato. Fornire `validate-f1.cmd` come gate ulteriore, che esegue la suite originale e richiede 826/826 test correnti, zero fallimenti e zero skipped, con verifica TRX e smoke Desktop Windows.
2. Far eseguire due audit v15 sintetici di 120 s (base e combinato). Tutti gli stock devono riconciliarsi, nessun Cibo deve provenire dal vecchio produttore; il profilo combinato deve esercitare tutte le fasi alimentari.
3. Conservare JSON e log della validazione in una cartella identificabile. Produrre `status=failed` e un errore esplicito quando manca un elemento; nessuna certificazione implicita in assenza di ambiente Windows.
4. La parità GUI v14/v15 (ADR 0071) richiede anche controllo umano, oltre ai test automatici. L'esito automatico da solo **non** autorizza a chiudere F1.
5. Lo stress combinato 24h è opzionale e separato, per non allungare eccessivamente il gate ordinario. Nessuna modifica a logiche di gioco o persistenza in questa RC.

## Conseguenze

La RC72 è una **candidata di gate**, non una release già confermata. Solo dopo il superamento e la revisione grafica si può segnare completata F1-D05e e iniziare la misurazione economica naturale v15 (E4a). Il profilo sintetico combinato non fornisce benchmark di progressione giocata.

[Piano di test](../validation/F1_D05E3_RC72_GATE_PLAN.md) · [Roadmap](../ROADMAP.md) · [ADR grafico](0071-gui-v14-contratto-canonico-v15.md).
