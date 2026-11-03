# Roadmap operativa e dettagliata — SimRisorse

**AGGIORNAMENTO G1-D28 / RC12 (03/11/2026):** RC11 dichiarata verde dall'utente (**466 test attesi**, numero esatto non comunicato). RC12: ricerca a **due decimali**, pulsante «Acquista ricerca» disabilitato e grigio se saldo o prerequisiti insufficienti, verde con hover ocra solo quando acquistabile. Controllo prezzi/saldi sul valore **non arrotondato** in millipunti. **468 test attesi, non eseguiti** in questo ambiente privo di SDK .NET 10. GUI v13, core e SQLite invariati; audit visivo P0 ancora aperto. [Report RC12](design/G1_D28_RC12_ALBERO_TECNOLOGICO_PUNTI_E_ACQUISTO.md) · [Piano](validation/G1_D28_RC12_TEST_PLAN.md) · [ADR 0037](adr/0037-v14-ricerca-precisione-e-stati-acquisto.md).


**AGGIORNAMENTO G1-D27 / RC11 (03/11/2026):** l’utente conferma **test RC10 passati (464 attesi)**, senza comunicare un conteggio dettagliato. RC11 riordina il dettaglio Edifici della sola v14 in due righe: **Costruisci | +1 | +5 | +10** e **Lavoratore | +1 | +5 | +10**; il pulsante **−1** rimane compatto accanto alla label Lavoratore per non perdere la funzione di rilascio. Aggiunti comandi atomici per assegnare 5/10 persone, con limiti di popolazione libera e posti di lavoro, stati disabled espliciti e hover già previsto. **466 test attesi** (+2 casi), non eseguiti qui per assenza dell’SDK .NET 10; audit GUI P0 ancora aperto. [Rapporto RC11](design/G1_D27_RC11_LAYOUT_COSTRUZIONE_LAVORATORI.md) · [Piano RC11](validation/G1_D27_RC11_TEST_PLAN.md) · [ADR 0036](adr/0036-v14-layout-costruzione-e-lotti-lavoratori.md).


**AGGIORNAMENTO G1-D26 / RC10 (03/11/2026):** la compilazione Windows della RC9 si è interrotta con **CS0246** in `tests/SimRisorse.Application.Tests/V14SchoolSuppliesTests.cs` (riga 15): `GameScenario` è definito in `SimRisorse.Core.Content`, ma mancava il relativo `using`. **RC10 aggiunge esclusivamente tale import nel codice dei test.** Tutti gli altri progetti sono stati compilati nel log dell’utente; la suite xUnit RC9 non è partita. **Ultima suite verde: RC8, 456/456 test confermati. RC10: 464 test attesi, non ancora eseguiti; audit GUI P0 ancora aperto.** [Diagnosi RC10](design/G1_D26_RC10_FIX_IMPORT_GAMESCENARIO.md) · [Piano RC10](validation/G1_D26_RC10_TEST_PLAN.md).


**AGGIORNAMENTO G1-D25 / RC9 (03/11/2026):** **RC8 456/456 test superati, comunicato dall'utente.** RC9 rimuove «Esci senza salvare» in entrambe le UI; la tab Scuola segue l'Esercito con coda, formazione/progresso, ricercatori; introduce la risorsa **Matite** solo nella v14 (1 Legno lavorato + 1 Carbone → 4 Matite, fabbricazione manuale, 2 Matite + 10 Cibo per studente), persistenza v3 retrocompatibile e rimborsi non fittizi. **464 test attesi, NON eseguiti**; confronto visivo ancora aperto. [Rapporto RC9](design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Piano RC9](validation/G1_D25_RC9_TEST_PLAN.md) · [ADR 0035](adr/0035-scuola-formazione-matite-v14.md).

**Regola di priorità:** nessuna nuova meccanica E4 o navale deve avere precedenza su regressioni e parità funzionale della GUI v14. I gate richiedono evidenze: test automatici, comparazione manuale e assenza di regressioni, non soltanto codice scritto.

## 1. Stato consolidato

| Traguardo | Stato verificabile | Evidenza / vincolo |
| --- | --- | --- |
| Gioco normale `civil-growth-13`, SQLite v2 | Base funzionante da preservare | Ultime versioni Windows v59+ validate; nessuna modifica al profilo normale |
| E1 prezzi progressivi, E2 tre livelli, E3 Scuola e ricerca | Implementati nel ramo v14 | Componenti esercitati dai successivi test, **numeri non ratificati** |
| E3b/1 snapshot e migrazione in memoria | Integrato nel prototipo | Convalida successiva della sessione v14 |
| E3b/2 store SQLite v3 | Validato | RC3 di persistenza: 380 test confermati |
| E3b/3 ciclo v14, tier, civili, replay | Validato | RC5: 417 test confermati |
| E3b/4a anteprima desktop isolata | Test verdi, UX non approvata | RC1: **427 test confermati** |
| E3b/4b ricostruzione GUI classica | **Respinta visivamente** | RC2 regressioni Risorse, comandi e parti Esercito |
| E3b/4c RC3: ripristino parità | Candidata da verificare | 439 test attesi, non confermati; audit Windows aperto |
| **E3b/4d RC4: diagnosi Pane v13/v14** | **Suite confermata passata dall’utente, UX aperta** | **4 test aggiunti, 443 attesi** (numero esatto non comunicato); stessa diagnosi/read model |
| **E3b/4e RC5: preventivi lotti e upgrade** | **Inclusi nella suite RC6, gate automatico non ancora verde** | **4 test Desktop aggiunti**; solo GUI v14; confronto visivo pendente |
| **E3b/4f RC6: diagnosi produttiva Edifici** | **451 test eseguiti; 450 passati e 1 fallito** | Errore nell'asserzione UI `IsEnabled` (riga 191), non nella verifica `CanExecute` o nei costi; gate non superato |
| **E3b/4f RC7: correzione test Avalonia** | **Da validare** | `IsEffectivelyEnabled` e corrispondenza comandi di upgrade; **451 test invariati**; codice prodotto v13/v14 intatto |
| **E3b/4g RC8 / G1-D24: stati Edifici/Scuola** | **456 test passati su Windows, GUI non certificata** | Righe prerequisiti grigie/selezionabili, Costruisci piena riga disabilitato senza materiali, 4 pulsanti in seconda riga, Scuola senza secondo Costruisci; **456 test attesi** |
| **E3b/4i RC9: Scuola e Matite** | **Build Windows fallita: 1 CS0246; test non partiti** | Import `SimRisorse.Core.Content` mancante in `V14SchoolSuppliesTests.cs`; 464 test non eseguiti |
| **E3b/4j RC10: correzione compilazione test** | **Test confermati passati dall’utente** | Import aggiunto nel solo test Application; 464 test attesi; runtime, UI e salvataggi immutati |
| **E3b/4k RC11 / G1-D27: due righe Edifici e lotti lavoratori** | **Test confermati passati dall’utente (466 attesi)** | RC10 verde confermata dall’utente, +2 casi RC11; layout e comandi +5/+10 lavoratori; audit grafico P0 aperto |
| **E3b/4l RC12 / G1-D28: albero tecnologico** | **Candidata: 468 test attesi** | RC11 test confermati passati (466 attesi); N2 per punti e prezzi, Acquista grigio se non pagabile, verifica exact milli-points; audit grafico P0 aperto |
| **E3b/4h: parità definitiva** | Da realizzare dopo validazione RC8 e confronto manuale | Conservare integralmente v13 e integrare solo funzioni nuove |
| E4a audit economico anti-blocco | Non iniziato | Benchmark e definizione del contratto economico |
| E4b bilanciamento approvato | Non iniziato | Decisione esplicita dell'utente sui numeri |
| E4c attivazione v14 e migrazione controllata | Non iniziata | Backup, rollback, reale compatibilità salvataggi |
| M5.4, M5.5–M5.7, M6/M7 | Futuro | Solo dopo economica/GUI stabile e gates |

## 2. P0 — Validare RC12 (RC11 verde, parità visiva ancora aperta)

**Input:** v60-E3b4-RC12 (include RC5–RC10 e novità Scuola/Matite); baseline confronto `./run-game.cmd` v13; v14 opt-in `./run-game.cmd --experimental-v14`.

**Attività:**

1. Da cartella RC12 estratta nuova eseguire `./validate.cmd`; conservare riepilogo e `.trx`. Obiettivo **468 test** (+2 su RC11 dichiarata verde); confronto: RC6 = **450/451**, un errore di asserzione Avalonia. Se numero diverso, analizzare discovery.
2. Avviare entrambe le modalità a **1040×740** e **1240×850**; verificare la GUI anche sotto cambi di dimensione/DPI quando possibile.
3. Confrontare scheda Risorse **voce per voce** con la v13: sette colonne, quantità arrotondate, capacità con tooltip e scomposizione, produzione/consumo/saldo, tempo di riempimento, ordinamento alfabetico, selezione ocra, allineamento, scrollbar, Pane e risorse a saldo negativo.
4. Confrontare **Edifici**: stesso elenco/pannello dettagli, nomi e icone, costi/produzione/consumi/capacità con stile uniforme; blocchi da prerequisiti ⇒ nessun Costruisci visibile; materiale insufficiente ⇒ pulsante disabilitato e causa spiegata; ordini +1/+5/+10, upgrade separato, nessuna ComboBox.
5. Confrontare **Esercito**: Caserme, Spade, numero soldati/reclute, coda e progressi individuali, arruola/annulla, congedo non ammesso; stessa interazione e struttura, non semplice pannello informativo.
6. Confrontare **Popolazione/Cibo/Pane**: crescita, razioni, posti, blocchi della produzione, testo di diagnosi e indicazione di quale edificio/capacità manchi. Controllare in particolare le diagnosi prodotte da `BreadSupplyProjector` e `Storage`.
7. Testare **selezione, focus, hover, disabilitazione, scrolling e footer**: la selezione rimane dopo aggiornamenti; hover non muove pulsanti; colonne non coprono scrollbar; testi e controlli rimangono visibili.
8. Riprovare nuova partita con conferma, pausa, riprendi, salvataggio manuale/automatico, chiusura, ripristino e apertura con DB incompatibile **senza alterarlo e senza bloccare la chiusura**.

**Verificare per prima la RC12:** [piano RC12](validation/G1_D28_RC12_TEST_PLAN.md). Poi rieseguire i controlli precedenti. **RC11:** [piano RC11](validation/G1_D27_RC11_TEST_PLAN.md). **Fix già confermato in RC10:** [piano RC10](validation/G1_D26_RC10_TEST_PLAN.md). **Quindi verificare le novità Scuola/Matite:** [piano RC9](validation/G1_D25_RC9_TEST_PLAN.md). **Verifiche storiche specifiche RC8 (superate nei test, layout sostituito in RC11):** [piano D24](validation/G1_D24_RC8_TEST_PLAN.md): elenco grigio senza disabilitare selezione, Costruisci unico a larghezza piena, disabled grigio senza risorse, hover ocra senza scostamenti, una seconda riga a quattro pulsanti con testi completi; tab Scuola senza costruzione. Confronto a 1040×740 e 1240×850. **RC7 non ha un run confermato.**

**Verifiche specifiche RC7:** il test `UpgradeWithResearchButWithoutMaterialsRemainsVisibleAndDisabled` deve usare `IsEffectivelyEnabled`, dopo aver verificato il `Command` legato al pulsante. Nessuna modifica funzionale è stata eseguita sul ViewModel o sullo XAML. [Piano D23](validation/G1_D23_RC7_TEST_PLAN.md). **Verifiche specifiche RC5:** quote +5/+10 visibili e cumulative con arrotondamenti per edificio; comandi disabilitati con mancanti; upgrade visibili ma disabilitati se la ricerca è sbloccata e mancano solo materiali. Confrontare report/piano [G1-D21](validation/G1_D21_RC5_TEST_PLAN.md). **RC6:** Segheria senza Tronchi, filiera condivisa Base/Grande/Avanzato, capacità Pane piena e doppio blocco costruzione/produzione, secondo [piano D22](validation/G1_D22_RC6_TEST_PLAN.md).

**Criterio di chiusura P0:** log Windows verde + checklist manuale registrata, **nessuna perdita rispetto alla v13**. Se emergono altri problemi, aprire una patch successiva di sola correzione, niente E4. Vedere [matrice dettagliata](validation/G1_D19_MATRICE_PARITA_GUI.md) e [piano RC5](validation/G1_D21_RC5_TEST_PLAN.md), oltre al [piano RC4](validation/G1_D20_RC4_TEST_PLAN.md); [piano RC3](validation/V60_E3B4_RC3_PARITY_TEST_PLAN.md) ancora applicabile.

## 3. P1 — Completare l'architettura GUI senza duplicazione funzionale

**Motivo:** la RC2 è regredita perché `V14PreviewWindow`/`V14PreviewViewModel` ricostruivano viste e presentazioni originariamente disponibili nella `MainWindow` v13.

**Attività tecniche:**

1. Inventario di righe, tooltip, command, stati e template della finestra v13 e relativo mapping nella v14; ogni elemento esistente avrà un corrispettivo documentato.
2. Estrarre **solo dove garantita equivalenza** componenti riutilizzabili: risorse, header/footer, `ResourceDetailTemplate`, dettagli blocchi, area Crescita e comandi Esercito. Favorire un ViewModel adapter per differenze di stato, non una seconda logica di produzione.
3. Testare i componenti estratti nella **v13** e nella **v14** in parallelo; introdurre controlli automatici che falliscano se scompaiono le sette colonne, i tooltip, i pulsanti corretti o gli stati dei blocchi.
4. Evitare un restyling, cambio di palette o grid redesign: piccole modifiche visibili solo per Scuola, ricerca e tier.
5. Garantire che gli ordini multipli indicano sempre **prezzo cumulativo crescente**, non solo prezzo unitario e moltiplicatore.

**Criterio di chiusura:** nessun cambiamento visivo/funzionale non richiesto nella modalità normale; v14 senza le lacune inventariate; test headless + audit Windows. La rifattorizzazione è **subordinata** al gate P0, non un prerequisito per rilasciare una patch mirata.

## 4. P2 — Rifinitura delle nuove funzioni v14

**Scuola:** costruzione nel catalogo edifici; pagina dedicata alla coda, tempi in secondi simulati, costi e requisiti, posti studenti e ricercatori; ritorno civile e annullamento con regole esplicite, non implicite.

**Ricerca:** albero consultabile, saldo liberamente accumulato e acquisto **solo su clic**; prerequisiti, costi e benefici comprensibili; tecnologia non implementata non acquistabile; pagina coerente con sidebar originale.

**Produttori:** per ogni famiglia righe **Base, Grande, Avanzato** come edifici autonomi, con 1/3/10 posti; pulsanti Costruisci direttamente per la riga selezionata, Upgrade distinto con sorgente/destinazione esplicite. Informare su capacità propria (candidata +50/+150/+500 per output), produzione per lavoratore e manodopera effettiva. Niente ComboBox di livello o lotto.

**Criterio di chiusura:** Esercito e Risorse della v13 rimangono identici; nessuna nuova pagina oscura dati già presenti; errori spiegati e operazioni atomicamente rifiutate.

## 5. P3 — E4a audit di progressione economica e anti-blocco

**Prima di fissare costi e tecnologie:**

- Verificare per ogni risorsa che la capacità disponibile permetta i lotti indivisibili; il Magazzino amplia **tutte** le risorse; Pane non ha eccezioni invisibili.
- Simulare costi crescenti e upgrade 1→3→10, acquisti multipli, domanda/produzione di Cibo e Pane, esigenze militari, riserve per ricercatori.
- Cercare stalli: prezzi superiori alla capacità raggiungibile, filiere senza materiali rinnovabili, prerequisiti circolari, ricercatori/formazione impossibili per mancanza abitanti, risorse in deadlock.
- Usare benchmark riproducibili **dalla partita reale** e replay: simulazione a 1 s e frazioni, salva/carica a metà, overflow, edifici demoliti, errori senza addebiti e punteggi coerenti.
- Completare metriche (tempi per obiettivi, produzione netta e saturazione, prezzi per unità, tempo al primo ricercatore e sblocchi, scorte di sicurezza). I benchmark M5.3c a 8 ore, quando mancanti, devono essere forniti/rigenerati; **non inventare dati**.

**Criterio di chiusura:** report misurato, confronti di configurazioni e nessuna situazione inevitabile di blocco nella progressione prevista.

## 6. P4 — E4b gate delle decisioni economiche

**Da sottoporre all'utente in forma compatta e comprensibile**, senza confondere prototipo e regola approvata:

- Tasso di incremento dei costi (candidato **+15%** per acquisto), formula e arrotondamenti; conteggio storico per famiglia.
- Moltiplicatori di costo per Base/Grande/Avanzato (candidati **1 / 2,6 / 6,5**) e prezzi di upgrade.
- Capacità per edificio per risorsa prodotta (candidate **+50 / +150 / +500**); personale **1 / 3 / 10** è un requisito espresso dall'utente.
- Scuola: prezzo, prerequisiti, posti, tempo e costo della formazione; produzione punti per ricercatore, politica di annullamento/ritorno civile.
- Tecnologie: otto settoriali implementate sperimentalmente, livelli e costi; Logistica/Navigazione non acquistabili finché senza effetto; scelta delle regole di sblocco definitive.
- Relazione con case avanzate, Pane, stoccaggio, popolazione e condizioni di vittoria.

**Criterio:** approvazione **esplicita** dei valori e ADR aggiornato. Il superamento dei test non approva numeri, gameplay o UX.

## 7. P5 — E4c attivazione e migrazione controllata v13→v14

1. Inventario dei salvataggi v13, inclusi formati storici/fork militari previsti; fixture reali **anonimizzate**, non database personali incorporati negli ZIP.
2. Backup coerente e verificabile prima della prima operazione; conversione v13→v14 a **percorso separato**, mai overwrite dell'originale; versioni/snapshot espliciti.
3. Round-trip SQLite v3, protezioni contro file malformati o schema incompatibile, prova di rollback e recupero dopo crash/spazio esaurito.
4. Integrare v14 nel gioco normale **solo dopo** approvazione GUI e bilanciamento; fornire migrazione opt-in con diagnosi e possibilità di tornare al backup.
5. Smoke test Windows end-to-end, compatibilità vecchi salvataggi e controllo scrittura del profilo corretto.

**Criterio:** nessuna perdita di partite o dati; rollback dimostrato; il profilo v13 non viene modificato senza scelta esplicita.

## 8. P6 — Ripresa del gameplay navale e delle fasi successive

- **M5.4:** Cantiere navale, Navi, equipaggi/risorse, produzione, costi, UI e regole di raggiungibilità; sblocchi tecnologici navali solo quando producono effetti reali.
- **M5.5:** esplorazione separata dalla conquista; requisiti espliciti, esiti deterministici dove approvati, inizialmente senza perdite irreversibili.
- **M5.6–M5.7:** conquista/territori, Castello e obiettivo finale; decisioni numeriche e condizioni di vittoria da validare prima dell'implementazione.
- **M6–M7:** rifinitura, bilanciamento conclusivo, UX/accessibilità e distribuzione, da ripianificare alla luce del codice effettivo.

Documenti di base: [G1 navale](design/G1_NAVAL_MISSIONS_DECISION_GATE.md), [ADR 0017 proposto](adr/0017-naval-mission-boundaries-proposta.md).

## 9. Regole di consegna e stop

- Un incremento alla volta; checkpoint, ADR se cambia una decisione, test, diff, ZIP **integrale**, roadmap e handoff aggiornati.
- Preservare **GUI v13**, database personali, stoccaggio condiviso e tempo simulato; niente cancellazioni retroattive.
- Separare **test dichiarati passati dall'utente**, verifiche statiche e test non eseguiti; archiviare le evidenze.
- Se un gate fallisce, **non** sviluppare la fase successiva: risolvere prima la regressione.
- Directory principale essenziale; documenti storici in `docs/history/`, non accumulare checkpoint/versioni obsolete alla radice.
