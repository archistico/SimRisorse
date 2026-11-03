# SimRisorse — RC16 / G1-D31

**Ultimo checkpoint Windows confermato:** RC15, 473 test attesi dichiarati passati. **Candidata RC16:** 476 test attesi, da eseguire su Windows. D31 blocca durante la conferma «Nuova partita?» tutte le azioni che possono modificare la colonia (Edifici, Scuola/Matite, Esercito e ricerca), oltre al pulsante Pausa/Riprendi già corretto in RC15. Gate grafico P0 ancora aperto, senza E4 o navale. [Rapporto RC16](docs/design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](docs/validation/G1_D31_RC16_TEST_PLAN.md).

**Storico RC14 / G1-D29-F1 (03/11/2026):** RC12 era già stata dichiarata verde (**468 test attesi**). Avviata la roadmap in [sei fasi](docs/ROADMAP.md); RC13 ha introdotto la correzione del tooltip della capacità **Matite** in base agli edifici realmente posseduti, e aggiunge 2 test (**470 attesi**); RC13 non compilava per 2 errori CS0136, **corretti in RC14** rinominando due variabili locali. RC14 testata: 470 dichiarati passati; RC15 ora con test dichiarati passati; RC16 da compilare e testare. La **parità grafica P0 v13/v14 non è approvata**; E4 e navale restano sospesi. [Audit D29](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Piano test RC14](docs/validation/G1_D29_RC14_TEST_PLAN.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).

Gioco gestionale di risorse e popolazione in **C# / .NET 10**, interfaccia desktop **Avalonia 12** in stile pixel art retro 8-bit, persistenza **SQLite**, test **xUnit**.

## Quale versione utilizzare

| Modalità | Comando | Stato | Salvataggi |
| --- | --- | --- | --- |
| **Gioco normale (`civil-growth-13`)** | `./run-game.cmd` | Campagna storica funzionante, da preservare | SQLite v2, profilo normale |
| **Anteprima economia v14** | `./run-game.cmd --experimental-v14` | **RC16 candidata** (azioni di gioco bloccate durante conferma nuova partita; RC15 validata), GUI complessiva da validare visivamente | SQLite v3, profilo sperimentale separato |

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

[RC14: diagnosi e fix CS0136](docs/design/G1_D29_RC14_FIX_CS0136.md) · [Validazione RC14](docs/validation/G1_D29_RC14_TEST_PLAN.md).
