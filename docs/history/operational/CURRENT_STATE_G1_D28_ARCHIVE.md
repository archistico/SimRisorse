# Stato operativo — SimRisorse (G1-D28)

**AGGIORNAMENTO G1-D28 / RC12 (03/11/2026):** RC11 dichiarata verde dall'utente (**466 test attesi**, numero esatto non comunicato). RC12: ricerca a **due decimali**, pulsante «Acquista ricerca» disabilitato e grigio se saldo o prerequisiti insufficienti, verde con hover ocra solo quando acquistabile. Controllo prezzi/saldi sul valore **non arrotondato** in millipunti. **468 test attesi, non eseguiti** in questo ambiente privo di SDK .NET 10. GUI v13, core e SQLite invariati; audit visivo P0 ancora aperto. [Report RC12](../../design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano](../../validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](../../adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).


**AGGIORNAMENTO G1-D27 / RC11 (03/11/2026):** l’utente conferma **test RC10 passati (464 attesi)**, senza comunicare un conteggio dettagliato. RC11 riordina il dettaglio Edifici della sola v14 in due righe: **Costruisci | +1 | +5 | +10** e **Lavoratore | +1 | +5 | +10**; il pulsante **−1** rimane compatto accanto alla label Lavoratore per non perdere la funzione di rilascio. Aggiunti comandi atomici per assegnare 5/10 persone, con limiti di popolazione libera e posti di lavoro, stati disabled espliciti e hover già previsto. **466 test attesi** (+2 casi), non eseguiti qui per assenza dell’SDK .NET 10; audit GUI P0 ancora aperto. [Rapporto RC11](../../design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Piano RC11](../../validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](../../adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).


**AGGIORNAMENTO G1-D26 / RC10 (03/11/2026):** la compilazione Windows della RC9 si è interrotta con **CS0246** in `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs` (riga 15): `GameScenario` è definito in `SimRisorse.Core.Content`, ma mancava il relativo `using`. **RC10 aggiunge esclusivamente tale import nel codice dei test.** Tutti gli altri progetti sono stati compilati nel log dell’utente; la suite xUnit RC9 non è partita. **Ultima suite verde: RC8, 456/456 test confermati. RC10: 464 test attesi, non ancora eseguiti; audit GUI P0 ancora aperto.** [Diagnosi RC10](../../design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano RC10](../../validation/G1_D26_RC10_TEST_PLAN.md).


**AGGIORNAMENTO G1-D25 / RC9 (03/11/2026):** **RC8 456/456 test superati, comunicato dall'utente.** RC9 rimuove «Esci senza salvare» in entrambe le UI; la tab Scuola segue l'Esercito con coda, formazione/progresso, ricercatori; introduce la risorsa **Matite** solo nella v14 (1 Legno lavorato + 1 Carbone → 4 Matite, fabbricazione manuale, 2 Matite + 10 Cibo per studente), persistenza v3 retrocompatibile e rimborsi non fittizi. **464 test attesi, NON eseguiti**; confronto visivo ancora aperto. [Rapporto RC9](../../design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Piano RC9](../../validation/G1_D25_RC9_TEST_PLAN.md) · [ADR 0035](../../adr/0035-scuola-formazione-matite-v14.md).


**Codice corrente:** `v60-E3b4-RC12 / G1-D28` · **ultima suite Windows verde:** RC10 (test dichiarati passati dall’utente; 464 attesi) · **RC11:** 466 attesi, non eseguiti, audit grafico P0 aperto. [RC9](../../design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Piano](../../validation/G1_D25_RC9_TEST_PLAN.md).

**Cosa cambia in RC8:** la v14 mostra in grigio gli edifici bloccati (senza impedirne la selezione); «Costruisci +1» è un pulsante a tutta riga, esplicitamente disabilitato senza risorse; i quattro pulsanti lavoratori/+5/+10 sono nella riga successiva; la scheda Scuola non propone la costruzione duplicata. [Rapporto D24](../../design/G1_D24_E3B4_RC8_STATI_EDIFICI_E_COMANDI.md). **RC7 (storico):** ha corretto nel test Avalonia il controllo `IsEnabled` → `IsEffectivelyEnabled`; non ha cambiato runtime. [Analisi D23](../../design/G1_D23_E3B4_RC7_AVALONIA_TEST_ENABLED.md).

**RC11 — layout operativo:** due righe `Costruisci | +1 | +5 | +10` e `Lavoratore | +1 | +5 | +10` con `−1` compatto nella label. Ordini di personale +5/+10 atomici, con controllo di abitanti e posti. Nessuna modifica al motore. Le precedenti descrizioni RC8–RC10 restano per storia.

## Semaforo delle evidenze

| Oggetto | Stato | Evidenza |
| --- | --- | --- |
| Gioco normale v13 / SQLite v2 | **Da preservare**, funzionalità già validate storicamente | `./run-game.cmd` |
| Core, sessione e SQLite v14 fino a E3b/3e | **Validati automaticamente** | E3b/3e RC5: 417 test confermati dall'utente |
| Desktop E3b/4a RC1 | **Test xUnit confermati**, grafica non approvata | RC1: 427/427 comunicati dall'utente |
| Desktop E3b/4b RC2 | **Regressioni visive/funzionali riferite dall'utente** | Risorse, costruzione bloccata, Esercito e altre lacune |
| **Desktop E3b/4c RC3** | **Candidato correttivo non ancora validato** | **439 test attesi**, confronto manuale v13/v14 aperto |
| **v60-E3b4-RC4 / G1-D20** | **Suite automatica confermata verde dall’utente** | 443 attesi (conteggio effettivo non comunicato), audit visivo ancora aperto |
| **v60-E3b4-RC5 / G1-D21** | **Non validata separatamente, inclusa nel run RC6** | Costi cumulativi ordini +5/+10 e upgrade sbloccati visibili se scorte insufficienti; +4 test, **inclusi nei 451 casi del run RC6**, audit GUI pendente |
| **v60-E3b4-RC6 / G1-D22** | **Suite fallita (test di controllo)** | **451 eseguiti, 450 passati, 1 fallito** per `IsEnabled` anziché `IsEffectivelyEnabled`; la validazione di tutta la RC6 è incompleta |
| **v60-E3b4-RC7 / G1-D23** | **Patch test P0 candidata** | Verifica `IsEffectivelyEnabled` e binding dei comandi; **451 test da ripetere**, GUI visiva ancora pendente |
| **v60-E3b4-RC9 / G1-D25** | **Build Windows fallita con 1 CS0246, suite non partita** | Test Application senza `using SimRisorse.Core.Content;`; altre compilazioni elencate nel log riuscite |
| **v60-E3b4-RC11 / G1-D27** | **Candidata: 466 test attesi** | Righe separate, batch lavoratori +5/+10, v13 e SQLite immutati |
| **v60-E3b4-RC10 / G1-D26** | **Suite Windows dichiarata verde dall’utente** | 464 attesi, numero effettivo non specificato; GUI P0 da confermare |
| **v60-E3b4-RC8 / G1-D24** | **Suite Windows superata, audit visivo aperto** | Bloccati grigi/selezionabili; Costruisci +1 a tutta riga e disabilitazione esplicita; seconda riga −1/+1 lav., +5/+10; Scuola senza costruzione duplicata; **456 attesi** |

## Implementato in RC3–RC8, da confermare nella GUI Windows

- Sidebar classica con Edifici / Risorse / Esercito / Scuola / Albero tecnologico.
- Risorse a sette colonne con `ResourceRow`, flussi, capienza, tooltip e tempo di riempimento.
- Costruzione +1/+5/+10, livelli come **edifici distinti**, upgrade separati, cause di blocco; nessuna ComboBox.
- Popolazione, Pane e comandi militari, con salvataggio sperimentale isolato.
- Correzione per chiusura della finestra su DB incompatibile.

**In RC8 (suite automatica confermata)** il colore grigio si applica solo ai bloccati per prerequisiti; il pulsante Costruisci grigio per materiali è un caso diverso. Non presentare questi punti come visivamente approvati:** RC4 aggiunge a livello codice la diagnosi di Pane (prima abitazione avanzata, capacità Forni, input, saturazione e riserva), ma la **verifica Windows è pendente**. Le due viste XAML rimangono distinte; il resto della matrice di parità non è chiuso.

## Invarianti attivi

- v13: `civil-growth-13`, vecchio `GameSession`, SQLite v2 e `MainWindow`; v14: `--experimental-v14`, `V14CampaignSession`, SQLite v3 e DB dedicato.
- Ricerca: punti accumulabili liberamente, spesi solo a richiesta.
- Prezzi a incremento, 1/3/10 lavoratori e edificio per livello sono requisiti di direzione; coefficienti, costi, tempi, capacità e ricerca quantitativa **non ratificati**.
- Municipio unico, 16 risorse e capacità condivisa, Pane gestito come ogni risorsa; nessun abitante duplicato tra lavoratori, soldati, studenti e ricercatori.

## Prossimo passo concreto

**P0:** validare **RC11** (include RC5–RC10) con `./validate.cmd` (**466 attesi**; RC10 dichiarata verde) e audit manuale v13/v14 secondo la [matrice](../../validation/G1_D19_MATRICE_PARITA_GUI.md). RC4 è stata confermata verde dall’utente, non è stato dichiarato un gate grafico. Correggere ulteriori regressioni **prima** di E4. [Piano RC8](../../validation/G1_D24_RC8_TEST_PLAN.md) · [Piano RC7](../../validation/G1_D23_RC7_TEST_PLAN.md) · [Piano RC6](../../validation/G1_D22_RC6_TEST_PLAN.md) · [Rapporto D22](../../design/G1_D22_E3B4_RC6_DIAGNOSTICA_PRODUZIONE.md) · [Rapporto RC5](../../design/G1_D21_E3B4_RC5_LOTTI_UPGRADE_GUI.md) · [Piano RC5](../../validation/G1_D21_RC5_TEST_PLAN.md) · [Roadmap](../../ROADMAP.md) · [Handoff](../../HANDOFF.md).
