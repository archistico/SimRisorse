# SimRisorse — RC20 / G1-D35

**Ultimo gate Windows confermato:** RC16, **476 test superati**. RC17 (481), RC18 (484), RC19 (487) **non sono state testate**. **RC20** aggiunge tre casi di regressione alla tabella Risorse v14, per **490 test attesi**, senza cambiare l'applicazione. La GUI classica e la tabella a sette colonne restano immutate. La verifica visiva P0 è ancora aperta; E4 sospeso.

[Rapporto D35](docs/design/G1_D35_RC20_AUDIT_TABELLA_RISORSE.md) · [Piano Windows RC20](docs/validation/G1_D35_RC20_TEST_PLAN.md) · [Roadmap](docs/ROADMAP.md).

---

# SimRisorse — RC19 / G1-D34

**RC16:** 476 test confermati. **RC17:** 481 attesi, non testata. **RC18:** 484 attesi, non testata. **RC19:** **487 test attesi**, non eseguiti qui. La RC19 include RC17+RC18 e corregge i **suggerimenti Esercito**: stato durante «Nuova partita?», costi e carenze numeriche per i lotti, singolare «1 recluta». Nessuna modifica a comandi, GUI v13, motore, database o bilanciamento. **Audit grafico P0 ancora aperto.**

[Rapporto D34](docs/design/G1_D34_RC19_AUDIT_ESERCITO_SUGGERIMENTI.md) · [Piano test RC19](docs/validation/G1_D34_RC19_TEST_PLAN.md) · [Roadmap ufficiale](docs/ROADMAP.md).

---

# SimRisorse — RC18 / G1-D33

**Baseline confermata:** RC16, 476 test superati. **RC17:** 481 attesi, non ancora testati. **RC18:** 484 test attesi (+3), non ancora testati. La nuova versione include integralmente la RC17 e corregge un suggerimento nella Scuola: se scegli +5/+10 ricercatori ma i posti liberi non bastano, il messaggio mostra il limite effettivo invece di indicare erroneamente che puoi assegnarli. I comandi e le regole di simulazione non cambiano. **Gate grafico P0 aperto**, E4 sospeso. [Rapporto RC18](docs/design/G1_D33_RC18_AUDIT_SCUOLA_LOTTI.md) · [Piano test](docs/validation/G1_D33_RC18_TEST_PLAN.md).

---

# SimRisorse — RC17 / G1-D32

**Baseline Windows confermata:** RC16 (476 test superati dall'utente). **RC17 candidata:** 481 test attesi, non ancora compilati/eseguiti su Windows. Il flusso v14 **Nuova partita → Avvia nuova** non sovrascrive più automaticamente il checkpoint precedente: il nuovo gioco viene salvato soltanto premendo **Salva v14**; poi riprende l'autosalvataggio. Durante la conferma, **Salva/Carica** sono disabilitati; la chiusura della finestra conserva la colonia precedente. Nessun cambiamento a GUI v13, Core, SQLite o bilanciamento. **Gate grafico P0 aperto.** [Rapporto RC17](docs/design/G1_D32_RC17_PROTEZIONE_CHECKPOINT_NUOVA_PARTITA.md) · [Piano RC17](docs/validation/G1_D32_RC17_TEST_PLAN.md) · [ADR 0039](docs/adr/0039-protezione-checkpoint-e-autosalvataggio-v14.md).

## Contesto precedente — RC16 / G1-D31

**RC16 confermata successivamente dall’utente:** 476 test passati. D31 blocca durante la conferma «Nuova partita?» tutte le azioni che possono modificare la colonia (Edifici, Scuola/Matite, Esercito e ricerca), oltre al pulsante Pausa/Riprendi già corretto in RC15. Gate grafico P0 ancora aperto, senza E4 o navale. [Rapporto RC16](docs/design/G1_D31_RC16_GUARDIA_CONFERMA_NUOVA_PARTITA.md) · [Piano RC16](docs/validation/G1_D31_RC16_TEST_PLAN.md).

**Storico RC14 / G1-D29-F1 (03/11/2026):** RC12 era già stata dichiarata verde (**468 test attesi**). Avviata la roadmap in [sei fasi](docs/ROADMAP.md); RC13 ha introdotto la correzione del tooltip della capacità **Matite** in base agli edifici realmente posseduti, e aggiunge 2 test (**470 attesi**); RC13 non compilava per 2 errori CS0136, **corretti in RC14** rinominando due variabili locali. RC14 testata: 470 dichiarati passati; RC15 ora con test dichiarati passati; RC16 da compilare e testare. La **parità grafica P0 v13/v14 non è approvata**; E4 e navale restano sospesi. [Audit D29](docs/validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Piano test RC14](docs/validation/G1_D29_RC14_TEST_PLAN.md) · [Handoff](docs/NEW_CHAT_HANDOFF.md).

Gioco gestionale di risorse e popolazione in **C# / .NET 10**, interfaccia desktop **Avalonia 12** in stile pixel art retro 8-bit, persistenza **SQLite**, test **xUnit**.

## Quale versione utilizzare

| Modalità | Comando | Stato | Salvataggi |
| --- | --- | --- | --- |
| **Gioco normale (`civil-growth-13`)** | `./run-game.cmd` | Campagna storica funzionante, da preservare | SQLite v2, profilo normale |
| **Anteprima economia v14** | `./run-game.cmd --experimental-v14` | **RC20 candidata** (audit Risorse con 3 nuovi casi; salvaguardie RC17 ancora da validare; RC16 validata), GUI complessiva da validare visivamente | SQLite v3, profilo sperimentale separato |

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
