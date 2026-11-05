# RC63 — Audit di parità GUI v15 / v14

| Contratto v14 | Regressione RC62 | Intervento RC63 |
|---|---|---|
| Edifici bloccati grigi, selezionabili | lista non attenuata | `V15BuildingRow.RowOpacity`, `CanBuildSelected`, dettaglio prerequisiti |
| Elenco STRUTTURA/EDIFICI/PERSONALE | lista compatta incompleta | ripreso template v14 con glyph 24px, contatori e staff |
| Costi + ricette + capacità con PixelGlyph | 3 frasi senza icone | righe tipizzate `ResourceDetailLine` basate sui ruleset v15 |
| Costruisci +1/+5/+10 e lavoratori −1/+1/+5/+10 | pulsanti ridotti e spostati | righe e azioni v14, quote atomiche, limiti persone/posti |
| Diagnosi in box scrollabile | assente | pannello scorrevole max 104px |
| Sidebar progresso nuova nascita | assente | `V15CampaignGrowthProjection` con `PopulationPlanner` e regole v15 |
| Cards popolazione / risorse originali | semplificate | struttura e progress bar v14 |
| Risorse 7 colonne / quantità arrotondate / hover | resa tipografica diversa | template v14, `V15ResourceRow` mantiene economia vera |
| Esercito: addestramento, spade, posti, batch | vista ridotta | layout v14 e batch 1/5/10 sul motore v15 |
| Scuola: studenti individuali, materiali, batch | vista ridotta | layout v14 e batch 1/5/10 sul motore v15 |
| Albero tecnologico e menu | già presenti | mantenuti; nessuna ComboBox nei dettagli edificio |

### Note sulle ricette

Le ricette di Macellaio (Bovini/ Selvaggina) e Deposito (Pesce/Carne/Verdura/Frutta/Pane) sono **alternative** nella stessa lavorazione e vengono presentate come tali. Le produzioni e i consumi sono nominali per addetto/secondo; le quantità effettive dipendono da materiali, dipendenti, priorità e spazio, e sono misurate nella tabella Risorse dal vero clock.

### Salvataggi e regole

Nessun cambiamento allo store SQLite generation 6, alle filiere o ai dati di partite v14. La v15 resta predefinita, `--v14` apre la GUI precedente. La copia visiva di template non è una rifondazione di UX: l'ADR 0071 vieta regressioni.
