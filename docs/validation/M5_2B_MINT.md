# M5.2b — Zecca e Monete

**Verificato su Windows il 2026-11-02**, checkpoint **v39**, regole **`civil-growth-9`**. **19 tipi di edifici / 16 risorse**. Contratto A approvato dall'utente il 28/10; Forgia/Spade già verificate in [M5.2a](M5_2A_FORGE.md).

## Regole implementate

| Proprietà | Valore |
| --- | --- |
| Edificio / risorsa | `mint` Zecca / `coins` Monete |
| Costo | 210 Legno lavorato +160 Pietra |
| Prerequisito | Possesso di una Fonderia d'oro |
| Addetti | 1 per edificio |
| Ricetta per addetto/s | 1 Oro +1 Carbone → 3 Monete |
| Priorità | 101, dopo Ferro 90, Oro 91 e Forgia 100 |
| Scorta iniziale | Zero |
| Capacità | 1.000 +500 per Magazzino |

Si usa il pianificatore esistente. Nessuna disponibilità degli output nel medesimo secondo della loro produzione. In una filiera nuova senza scorte, il primo passo estrae il minerale, il secondo produce Oro e il terzo le Monete.

I lotti sono indivisibili: da 997/1.000 entra un lotto esatto; a 998 o 999 la Zecca non produce e non consuma Oro/Carbone. Da zero, la capienza iniziale consente **999 Monete**, lasciando un posto libero. Un Magazzino aumenta la capacità a 1.500 e permette la ripresa.

Le Monete sono scorte per futuri pagamenti espliciti. Non sono valuta generale, non cambiano i costi degli altri edifici e non introducono acquisti di navi o reclutamento.

## Gate e artefatti

`validate.cmd` completo, restore online con lock, SDK **10.0.201**, Avalonia **12.1.4**, Microsoft.Data.Sqlite **10.0.14**. Build Release con **zero avvisi / zero errori**.

Run **`b478adfdae134725b7610d6acefee7e8`**, **258/258 test**, nessun test fallito o ignorato:

| Suite | Superati |
| --- | ---: |
| Core | 67 |
| Application | 96 |
| Infrastructure | 34 |
| Desktop | 61 |
| **Totale** | **258** |

Il gate verifica raggiungibilità dei due cataloghi, **20 cronologie più crescita accelerata**, cinque benchmark, pubblicazione e smoke nativi Windows dei due scenari: salvataggio/caricamento, ripresa della crescita, sospensione e chiusura. Salvataggi di prova separati dalle partite personali.

Artefatti locali ignorati da Git:

- `.local-cache/m4/validation.json`, `mint-validation.log`, `mint-start.json`, `mint-8-hours.json` e i JSON dei cinque benchmark.
- `.local-cache/m4/runs/b478adfdae134725b7610d6acefee7e8/test-results/`: TRX.
- Nella stessa run, `screenshots/`: **33 PNG**, di cui **31 richiesti dal manifest** e due render del dettaglio. Il manifest include anche i sei render degli effetti case/Magazzino introdotti in v38.

## Cronologia dalla nuova partita

`--scenario civil-growth --strategy mint` segue il percorso civile/ferro e costruisce Miniera/Fonderia d'oro a 1.500 s, poi la Zecca con un addetto a **1.800 s**. Acquisti e assegnazioni usano i comandi reali. Il primo lotto arriva a **1.801 s**.

| Misura | A 1.801 s | A 8 ore |
| --- | ---: | ---: |
| Monete | **3** | **999** |
| Capacità Monete | 1.000 | 1.000 |
| Oro | 299 | 1.000 |
| Ferro | 700 | 1.000 |
| Carbone | 401 | 1.000 |
| Legno lavorato | 100 | 1.000 |
| Pietra | 661 | 1.000 |
| Popolazione | 41 | 48 |

A otto ore la Zecca è bloccata per spazio insufficiente a un lotto intero, come confermato dal validatore. Il percorso non attiva la Forgia e non è il confronto completo M5.2c, né una misura della durata della campagna o una dimostrazione di ottimalità.

## Compatibilità e test

- Conversioni **v1–v8→v9**, con Zecca a quantità/personale zero e Monete zero. Capacità dei Magazzini posseduti applicata alle Monete.
- SQLite reale v8→v9: conservazione di Spade frazionarie, Forgia e relativo lavoratore, scorte industriali/civili, popolazione, edifici, tempo e frazione della crescita. Caricamento in pausa senza scrittura; backup v8 prima del primo salvataggio v9, non duplicato dai successivi salvataggi.
- Verifica conservata anche su v7, con conversione concatenata attraverso v8. Round trip SQLite di Monete frazionarie, addetto Zecca e residuo di mezzo secondo: ripresa esatta dopo altri 500 ms e nessun avanzamento in pausa.
- Vecchi checkpoint con Monete positive o righe Zecca incompatibili rifiutati senza sostituire la partita corrente. Test di tutte le versioni precedenti v1–v8, compreso il progresso frazionario.
- Primo lotto, costi effettivi, capienza residua 0/1/2 e riempimento esatto, ripresa dopo Magazzino, input mancanti, assenza di addetto, pause e replay interi/frazionari.
- Contesa fra i quattro trasformatori: con 2 Carbone lavorano solo le fonderie; con 3 lavora anche Forgia; con 4 anche Zecca. Rilasciare il lavoratore della Forgia libera il Carbone scarso per la Zecca.

Il caso SQLite v8 acquista realmente anche la Forgia: la dotazione sintetica dei materiali è 1.300/1.300, entro i 1.500 del Magazzino presente. I prezzi del gioco non sono stati adattati per finanziare la fixture.

SQLite/snapshot restano **v2**, slot civile e S1 invariati. [ADR 0016](../adr/0016-mint-coins-and-v8-save-upgrade.md). Cataloghi v4/v5/v6/v7 conservati; `CreateForgeForBenchmarks()` fissa v8 per le verifiche di compatibilità. M5.1c e l'esperimento isolato rimangono su v7.

## Interfaccia

Icone originali 8×8 di Zecca/Monete. Costi, output e consumi usano il template condiviso, con diagnosi per prerequisito, Oro/Carbone, addetti e capienza. Gli effetti delle abitazioni e del Magazzino conservano font/elenco/icone richiesti in v38.

Quattro render nuovi ispezionati a **1040×740 e 1240×850**: `mint-buildings[-compact].png` e `mint-resources[-compact].png`. Controllati ricetta, icone, pulsanti accessibili, scrollbar, colonne e inserimento alfabetico di Monete; nello stato sintetico a 3/1.000 la tabella mostra +3/s e stima **00:05:33**. I test verificano anche costruzione, assegnazione e prima produzione attraverso i comandi Desktop.

## Prossimo incremento e limiti

**M5.2c resta aperta:** confronto a fasi/parallelo nel catalogo corrente, acquisti reali, contabilità per secondo, capienze, replay e cronologia integrata dalla nuova partita. L'esperimento storico v7, eseguito in precedenza, conserva i 241/122 s nel checkpoint sintetico; non è stato rieseguito in questo incremento e non sostituisce il nuovo gate di M5.2c.

Quantità per recluta e nave da scegliere in M5.3/M5.4. Altre piattaforme, DPI fisici, accessibilità e divertimento non sono coperti dal gate Windows; gli obiettivi di circa otto ore fino alla vittoria restano da misurare in M6.
