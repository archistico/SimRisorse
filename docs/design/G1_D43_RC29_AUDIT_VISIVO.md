# G1-D43 — Confronto v13/v14: evidenze statiche e limiti

**Data:** 04/11/2026. **Versione esaminata:** RC28, sorgenti integrali (RC29 ne conserva integralmente il codice dell’applicazione). **Baseline confermata:** RC27 (507 test). **RC28/RC29:** 511 test attesi, non eseguiti qui. **Esiti visivi realmente verificati: 0/44**, nessuna screenshot Windows acquisita: questa relazione **non chiude P0**.

## Verifiche direttamente riscontrate nei sorgenti

| Area / UI-ID | Fatto verificato confrontando `MainWindow.axaml` e `V14PreviewWindow.axaml` | Limite / prossimo controllo |
| --- | --- | --- |
| UI-01 / UI-18 | Entrambe le finestre hanno dimensione iniziale **1240×850**, minima **1040×740** e sidebar larga **240 px**; v13 ha tre pagine Edifici/Risorse/Esercito, v14 ne ha cinque aggiungendo Scuola e Albero tecnologico. | Leggibilità e TAB non verificabili solo da XAML; RC28 introduce bordo focus. |
| UI-02 / UI-03 | Le intestazioni Risorse usano in entrambe `ColumnDefinitions="*,80,80,108,96,85,108"` con sei colonne numeriche allineate a destra. | Va verificato visivamente che ultima colonna e scrollbar non si sovrappongano alle due dimensioni. |
| UI-07 / UI-12 | L'area dettaglio Edifici è larga **310** in v13 e **410** in v14; differenza funzionale deliberata per costi, tier, diagnosi e pulsanti. La v14 contiene un secondo ScrollViewer diagnostico con massimo 104 px. | Il confronto non può pretendere pixel identici in questa zona; verificare testo non troncato, comandi fissi, scroll e focus. |
| UI-09 / UI-10 | La v14 dichiara esplicitamente `IsEnabled` per Costruisci +1/+5/+10 e i due upgrade; gli stati sono calcolati dal ViewModel. | Solo interazione reale permette di approvare grigio/verde e hover senza salti. |
| UI-15 | Entrambe le GUI hanno la pagina Esercito con `MilitaryProgressBar`; v14 conserva la diagnostica di formazione. | Necessarie prove a 10/30 secondi e controllo interattivo dei comandi. |
| UI-16 / UI-17 | Scuola e Albero tecnologico esistono soltanto nella v14: **nessun confronto v13 omologo**. | Screenshot v14 obbligatori, non inventare uno stato v13 né dichiarare l'intera voce NA. |
| UI-20 / UI-21 | La v14 dispone dei comandi Nuova partita, Salva v14 e Carica v14; il comportamento di protezione del checkpoint è coperto da test precedenti. | Il salvataggio deve essere provato solo su copia, compresa la chiusura senza primo Salva. |

Lettura delle fonti: `src/SimRisorse.Desktop/Views/MainWindow.axaml`, `src/SimRisorse.Desktop/Views/V14PreviewWindow.axaml`, test Desktop esistenti e [matrice canonica](../validation/G1_D39_P0_UI_MATRIX.md). Nessuna riga non citata nella tabella è per questo motivo dichiarata conforme: **tutte UI-01…UI-22 sono ancora da verificare in finestra**.

## Distinzione necessaria per il gate

1. **Parità strutturale:** alcune proprietà si confrontano da XAML; le differenze funzionali autorizzate (5 sezioni, tecnologie, Matite, tier) non sono regressioni.
2. **Test automatici:** RC27 507 passati, RC28/RC29 511 attesi non ancora verificati; test headless non equivalgono a validazione visuale Windows reale.
3. **Evidenza manuale:** 22 voci × due viewport = **44 scenari**. Tutti partono da `NON_ESEGUITA`. Per Scuola/Albero v13 non applicabile all'interno dello scenario, ma deve esserci un esito della v14.
4. **Gate:** D44 corregge solo FAIL riprodotti; D45 richiede 44/44 evidenze valide (PASS o NA motivata), suite cumulativa verde, protezione dei salvataggi e conferma esplicita dell'utente.

## Materiale consegnato per il prossimo ciclo

- [Piano d'uso e acquisizione screenshot](../validation/G1_D43_RC29_PIANO_EVIDENZE.md).
- `scripts/p0-ui-evidence.ps1`: iniziatore CSV, cattura facoltativa della clipboard, rapporto HTML locale che segnala documentazione e screenshot mancanti.
- [Roadmap](../ROADMAP.md), [runbook precedente](../validation/G1_D42_P0_RUNBOOK.md), [matrice P0](../validation/G1_D39_P0_UI_MATRIX.md).

**Nessuna modifica alla GUI v13/v14, al motore, ai test .NET o ai salvataggi.** Nessun bug grafico nuovo dichiarato senza prova riproducibile.
