# RC78 / E4b.1 — Verifiche eseguite

Esecuzione Windows iniziata il 05/11/2026, conclusa il 06/11/2026; SDK **10.0.201**. `validate-f1.cmd`, incluso `validate.cmd`, uscita **0**. Build Release senza errori o avvisi; **926/926 test superati**, zero saltati o falliti.

| Progetto | Test superati |
| --- | ---: |
| Core | 212 |
| Application | 391 |
| Infrastructure | 129 |
| Desktop | 194 |

## Nuove verifiche

27 casi Application verificano prezzi dichiarati delle tre proposte, tutti i tier e lotti 5/10 contro gli acquisti sequenziali, crescita monotona con storico, upgrade telescopici e confronto senza sconto, squadre 0/9/10/19/20, input mancanti e spazio insufficiente, personale di supporto alimentato/alloggiato, ramo Ferro deduplicato, differenza di liquidità fra lotti/acquisti/upgrade, lavoro ricorsivo dei materiali, input invalidi e overflow, determinismo, catalogo corrente invariato e rifiuto di misure diagnostiche come baseline completa.

Durante lo sviluppo un'aspettativa del test sul lavoro dei metalli era errata (4,5 invece di 5 secondi-lavoratore per 1 Ferro + 1 Oro). Corretta considerando i due minerali per unità di Ferro; nessuna ricetta modificata. Tutti i casi risultano verdi nel gate finale.

Il gate esegue anche la CLI di studio senza baseline, verifica modalità `analytical-proposal/not-applied`, tre curve, 81 preventivi, 18 investimenti, zero run misurati inventati e cinque file prodotti. Conserva audit alimentari Base/Combinato da 120s, sei esperimenti di recupero controllati, TRX e catture desktop precedenti. Smoke nativo esistente superato su entrambi gli scenari storici (salvataggio, caricamento, crescita/ripresa, sospensione e chiusura).

Il confronto interattivo Windows della GUI v15 resta pendente e distinto; questa consegna non cambia Desktop e non dichiara conclusa formalmente F1. Stress facoltativo 24h non eseguito.

## Studio con baseline misurata e replay

Eseguiti due studi indipendenti importando i sei run naturali RC77-M01 da `.local-cache/e4a-v15/rc77-natural-1793917768540/industry/e4a-v15-industry-results.json`. Il validatore E4a accetta sei obiettivi osservati, zero diagnostiche e i tre confronti di politica; nessuna simulazione aggiuntiva o grant. Il file di provenienza registra il SHA-256 del JSON importato; verificato contro i byte sorgente.

**5/5 output identici byte per byte** al replay: JSON, preventivi CSV, investimenti CSV, Markdown, provenienza baseline. Cinque invocazioni errate rifiutate con uscita 1: sovrascrittura, opzione sconosciuta, duplicazione, valore mancante, JSON malformato. Gli hash dei risultati esistenti restano identici dopo i rifiuti. Le prove CLI non sono conteggiate come ulteriori test xUnit.

Gli esiti economici sono nel [rapporto comparativo](E4B_RC78_CURVE_COMPARISON.md). Non sono stati misurati tempi di campagna o ROI delle proposte e non sono implementati nuovi prezzi, squadre, migrazione o salvataggi.

## Percorsi delle evidenze locali

- Gate: `.local-cache/m4/f1-gates/b4b4bd11ba494989bc99ca636df02da3/f1-gate.json`.
- TRX: `.local-cache/m4/runs/7253b178269c4b028b8b6970dc459876/test-results/`.
- Studio nel gate, senza baseline importata: `economic-curves/` accanto al JSON del gate.
- Studio con dati RC77-M01: `.local-cache/e4b-v15/rc78/measured/`.
- Replay: `.local-cache/e4b-v15/rc78/replay/`.
- Riconciliazione replay/rifiuti: `.local-cache/e4b-v15/rc78/study-validation.json`.
- Hash baseline, sei sorgenti/configurazioni operative, tre file di configurazione e tre assembly: `.local-cache/e4b-v15/rc78/provenance.json`.

Gli archivi di consegna sono in `.local-cache/releases/rc78-*/` e `.local-cache/releases/rc78-evidence-*/`, con manifest SHA-256 e verifica dei byte di ogni entry ZIP. I pacchetti sorgente includono il progetto integrale senza cache, output di compilazione o database personali; rimane la fixture SQLite intenzionale dei test Infrastructure. I pacchetti evidenze mantengono input naturale, report, replay, provenienza, script di controllo e risultati del gate separati dai sorgenti.

[Specifica](../design/E4B_RC78_THREE_CURVES_STUDY.md) · [ADR 0086](../adr/0086-confronto-curve-economiche-isolato.md).
