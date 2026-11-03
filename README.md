# SimRisorse

**AGGIORNAMENTO G1-D28 / RC12 (03/11/2026):** l’utente ha confermato i **test RC11 passati (466 attesi)**. RC12 corregge l'Albero tecnologico della sola v14: **punti e prezzi ricerca a due decimali**; pulsante **«Acquista ricerca» grigio e non azionabile** se manca il saldo effettivo o un prerequisito, verde/hover ocra solo se acquistabile. La precisione contabile in millipunti resta invariata. **468 test attesi**, non eseguiti qui per assenza SDK .NET 10; parità visiva P0 ancora aperta. [Report](docs/design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano](docs/validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](docs/adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).

**AGGIORNAMENTO G1-D27 / RC11 (03/11/2026):** l’utente conferma **test RC10 passati (464 attesi)**, senza comunicare un conteggio dettagliato. RC11 riordina il dettaglio Edifici della sola v14 in due righe: **Costruisci | +1 | +5 | +10** e **Lavoratore | +1 | +5 | +10**; il pulsante **−1** rimane compatto accanto alla label Lavoratore per non perdere la funzione di rilascio. Aggiunti comandi atomici per assegnare 5/10 persone, con limiti di popolazione libera e posti di lavoro, stati disabled espliciti e hover già previsto. **466 test attesi** (+2 casi), non eseguiti qui per assenza dell’SDK .NET 10; audit GUI P0 ancora aperto. [Rapporto RC11](docs/design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Piano RC11](docs/validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](docs/adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).

Gioco gestionale di risorse e popolazione in **C# / .NET 10**, interfaccia desktop **Avalonia 12** in stile pixel art retro 8-bit, persistenza **SQLite**, test **xUnit**.

**AGGIORNAMENTO G1-D26 / RC10 (03/11/2026):** la compilazione Windows della RC9 si è interrotta con **CS0246** in `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs` (riga 15): `GameScenario` è definito in `SimRisorse.Core.Content`, ma mancava il relativo `using`. **RC10 aggiunge esclusivamente tale import nel codice dei test.** Tutti gli altri progetti sono stati compilati nel log dell’utente; la suite xUnit RC9 non è partita. **Ultima suite verde: RC8, 456/456 test confermati. RC10: 464 test attesi, non ancora eseguiti; audit GUI P0 ancora aperto.** [Diagnosi RC10](docs/design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano RC10](docs/validation/G1_D26_RC10_TEST_PLAN.md).

## Quale versione utilizzare

| Modalità | Comando | Stato | Salvataggi |
| --- | --- | --- | --- |
| **Gioco normale (`civil-growth-13`)** | `./run-game.cmd` | Campagna storica funzionante, da preservare | SQLite v2, profilo normale |
| **Anteprima economia v14** | `./run-game.cmd --experimental-v14` | **RC12 candidata** (ricerca a due decimali e pulsante Acquista), GUI complessiva da validare visivamente | SQLite v3, profilo sperimentale separato |

**Stato storico e corrente (03/11/2026):** la **RC8 ha superato 456 test su Windows** (comunicato dall'utente). La **RC9 / G1-D25** rimuove «Esci senza salvare» e allinea la Scuola all'Esercito (coda, posti, progresso, ricercatori), introducendo le **Matite** nella sola v14. Fabbricazione manuale: 1 Legno lavorato + 1 Carbone → 4 Matite; ogni iscrizione richiede **10 Cibo e almeno 2 Matite**. Salvaguardati i vecchi salvataggi v14, con rimborsi delle sole Matite realmente pagate. **RC9: compilazione fallita per CS0246 in un test; RC10 ha corretto l’import ed è stata dichiarata verde dall’utente; RC11 ha 466 test attesi**; la parità grafica completa v13/v14 resta aperta.

## Come avviare e validare

Su Windows 11 con SDK .NET 10 coerente con `global.json` (versione minima di riferimento `10.0.200`) e dipendenze ripristinabili:

```powershell
.\validate.cmd
.\run-game.cmd
.\run-game.cmd --experimental-v14
```

Usare **cartelle nuove** per i nuovi ZIP. Prima di testare la v14, eseguire una copia di sicurezza dei propri salvataggi. La modalità normale e quella sperimentale usano percorsi differenti:

- v13: `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`
- v14: `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3`

La v14 **non deve migrare o sovrascrivere automaticamente** la partita v13. La protezione dei salvataggi deve essere verificata, non presunta.

## Contratto dell'interfaccia

La **GUI v13 è la base funzionale e visiva da conservare**. Nella barra laterale della v14 si aggiungono **Scuola** e **Albero tecnologico** senza eliminare Edifici, Risorse ed Esercito. La tabella Risorse resta a **sette colonne**, con ordinamento, capacità, tooltip e allineamenti storici; Esercito mantiene i comandi di addestramento. Ogni livello di produttore è un **edificio distinto** nell'elenco (es. Segheria / Segheria grande / Segheria avanzata): niente ComboBox di livello nei dettagli. Costruzioni +1/+5/+10, upgrade separati, prerequisiti e motivi di blocco chiari. Gli edifici bloccati sono grigi ma selezionabili; nel dettaglio v14 due righe:
`Costruisci | +1 | +5 | +10` (stato grigio se non pagabile, hover ocra se attivo) e
`Lavoratore | +1 | +5 | +10` (disabilitazione quando mancano abitanti/posti). Un `−1` compatto resta accanto alla label Lavoratore per liberare personale. La Scuola si costruisce esclusivamente da Edifici; il tab Scuola è riservato a formazione, ricercatori e laboratorio Matite, senza pulsante Costruisci. «Esci senza salvare» non compare più nelle due interfacce. **Non riprogettare le schede già funzionanti.**

La ricerca si **accumula liberamente** e si spende su decisione del giocatore. Nell’Albero tecnologico i punti sono mostrati con due decimali; «Acquista ricerca» è grigio se non acquistabile, con verifica esatta del saldo prima dell’acquisto. I coefficienti economici, prezzi e tempi della Scuola restano **candidati** finché non saranno espressamente approvati attraverso il bilanciamento.

## Documentazione di riferimento

- [Ripresa rapida](docs/START_HERE.md) e [handoff completo per nuova chat](docs/HANDOFF.md)
- [Prompt di passaggio a nuova chat](docs/NEW_CHAT_HANDOFF.md)
- [Stato validazione corrente](docs/CURRENT_STATE.md)
- [Roadmap dettagliata e gate](docs/ROADMAP.md)
- [Decisioni approvate / candidate](docs/AUTHORITY_REGISTRY.md)
- [Changelog consolidato](CHANGELOG.md) e [storico integrale precedente](docs/history/operational/CHANGELOG_PRIMA_DEL_RIORDINO_G1_D19.md)
- [Matrice di parità GUI](docs/validation/G1_D19_MATRICE_PARITA_GUI.md), [rapporto RC6 diagnostica produttiva](docs/design/G1_D22_E3B4_RC6_DIAGNOSTICA_PRODUZIONE.md), [piano RC6](docs/validation/G1_D22_RC6_TEST_PLAN.md), [piano RC5](docs/validation/G1_D21_RC5_TEST_PLAN.md) e [correzione test RC7](docs/validation/G1_D23_RC7_TEST_PLAN.md), [nuovo piano RC8](docs/validation/G1_D24_RC8_TEST_PLAN.md), [piano RC9](docs/validation/G1_D25_RC9_TEST_PLAN.md), [rapporto RC9](docs/design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) e [rapporto D24](docs/design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md)

[RC11: layout e batch lavoratori](docs/design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [piano test RC11](docs/validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](docs/adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).

**Ordine di sviluppo:** prima correggere e validare la **parità GUI**; poi audit del bilanciamento e migrazioni; infine riprendere il navale. Ogni incremento di codice richiede ZIP integrale, test, documentazione e stato esplicito.

[RC12: acquisto ricerca e due decimali](docs/design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano RC12](docs/validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](docs/adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).
