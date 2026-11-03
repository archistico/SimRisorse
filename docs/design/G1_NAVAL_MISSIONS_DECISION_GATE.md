# G1 — Contratto navale e spedizioni: proposta da approvare

**Checkpoint documentale:** v59-G1-D1, 3 novembre 2026. **Baseline eseguibile:** v59-RC1, `civil-growth-13`, `validate.cmd` interamente superato su Windows secondo la conferma dell'utente. **Stato:** proposta; nessuna nuova regola implementata. Le ipotesi numeriche non sono decisioni approvate.

## 1. Vincoli già consolidati

- La prima campagna richiede il possesso di **22 tipi di edifici**, escluso il Municipio iniziale. Oggi esistono **20 tipi-obiettivo + Municipio = 21 strutture**. Mancano Cantiere navale e Castello. Le navi **non** sono una nuova risorsa economica: restano 16 risorse.
- La conquista di **un'isola obbligatoria** sblocca il Castello; non è richiesta una mappa di posizionamento. La UI rimane ad elenchi e schede, stile retro 8-bit.
- Tempo **simulato**, pausa e chiusura non producono avanzamento offline; salvataggi SQLite v2 con copie di sicurezza, transazioni atomiche e regole versionate.
- Sistema civile/militare v13 invariato: Caserma 260 Legno lavorato e 200 Pietra, 5 Spade/recluta, 1 posto per Caserma, 30 s per formazione individuale. Reclute e soldati rimangono residenti e consumano razioni ordinarie.
- **Orientamenti espressi come base di discussione dall'utente:** esplorazione distinta dalla conquista; conquista deterministica con prerequisiti; assenza iniziale di perdite irreversibili. Questi orientamenti non equivalgono all'approvazione dei prezzi, delle soglie e dei comportamenti dettagliati.
- Validazione tecnica M5.3c **superata**, ma i due JSON prodotti dal validatore non sono in questo archivio; i numeri effettivi dell'economia dopo otto ore sono ancora da leggere prima di scegliere la soglia militare dell'isola.

## 2. Grafo del ciclo di gioco previsto

```text
Colonia civile (Legno lavorato, Pietra, abitanti, alimentazione)
                  |                           
            Zecca -> Monete                 Forgia -> Spade -> Caserma -> Soldati
                  |                                                 |
              Cantiere -> navi acquistate (ID persistente)         |
                  |                                                 |
                 esplorazione (nessun soldato)                       |
                  |                                                 |
              isola scoperta ---------------------------------------+
                  |
       controlli logistici (navi disponibili, capacità, soldati disponibili)
                  |
            conquista deterministica
                  |
        isola conquistata -> Castello -> possesso 22 edifici -> vittoria
```

Le filiere per la **prima nave** devono essere accessibili senza risorse coloniali; nessuna ricompensa di conquista è necessaria per pagarne il costo. La conquista non deve consentire di perdere irreversibilmente tutti i mezzi che permettono di terminarla.

## 3. N-01…N-07: contratto candidato per M5.4

| ID | Proposta A candidata (da approvare singolarmente o come pacchetto) | Effetto e rinvii |
| --- | --- | --- |
| N-01 | **Una nave = un'entità individuale con ID univoco e permanente** | Non è stock di risorsa; stato disponibile/prenotato/in viaggio in fasi future |
| N-02 | **Cantiere navale**: 320 **Legno lavorato** (`planks`) + 220 Pietra, prerequisito Zecca, nessun addetto fisso | 21º tipo-obiettivo; verificare costruibilità e capacità condivisa |
| N-03 | **Acquisto nave**: 160 Legno lavorato + 120 Monete, immediato, indivisibile | Controllare Cantiere e stock prima di spendere; niente acquisto gratuito dopo load |
| N-04 | **Capacità:** 5 soldati già addestrati per nave; nessun equipaggio civile aggiuntivo | Non fissa i soldati richiesti dall'isola obbligatoria |
| N-05 | **Navi riutilizzabili**, nessuna perdita irreversibile per la prima campagna | Danni, riparazioni e consumi di navigazione rinviati |
| N-06 | Almeno **un Cantiere posseduto** per acquistare navi; nessun limite di flotta per Cantiere | Demolizione rinviata; non eliminare navi se l'edificio sparisce nei dati storici |
| N-07 | **Commercio navale rinviato** | Niente navi mercantili o nuove risorse in M5.4 |

**Costo minimo proposto del primo Cantiere + una nave:** 480 Legno lavorato + 220 Pietra + 120 Monete; non comprende le filiere, la capacità di stoccaggio, la popolazione, le Spade o il tempo di produzione. Sono numeri da validare economicamente, non un risultato già misurato.

## 4. Contratto candidato delle missioni (M5.5–M5.6)

| ID | Decisione candidata | Requisito verificabile |
| --- | --- | --- |
| S-01 | Esplorazione separata, senza soldati, con almeno 1 nave libera | L'isola passa da ignota a scoperta; navi rientrano |
| S-02 | Isola possiede stato persistente `sconosciuta`, `scoperta`, `conquistata`; la missione ha uno stato *separato* | Nessuna doppia verità dopo reload |
| S-03 | Missione in **andata**, applicazione esito all'arrivo, **ritorno**, poi rilascio di navi e truppe | Successo territoriale non libera anticipatamente le unità |
| S-04 | La conquista usa requisiti **noti e deterministici**, verificati prima di partire; partenza bloccata se insufficienti | Non esistono fallimenti casuali o spese inutili per missioni impossibili |
| S-05 | **Nessuna perdita irreversibile** di navi/soldati nella campagna iniziale | Recupero completo al termine della missione; manutenzione fuori perimetro |
| S-06 | Più missioni simultanee **solo con assegnazioni disgiunte** | Nessuna nave/soldato contato in due missioni; iniziare con test monomissione |
| S-07 | Solo il **tempo simulato** muove missioni; salvataggio/ripresa esatta e risultato applicato una sola volta | Equivalenza `Advance(N)` vs `N` passi unitari, pausa/offline invarianti |
| S-08 | Soldati lontani restano nella **popolazione residente**, non lavorano e consumano le normali razioni della colonia | Nessun doppio conteggio e nessun nuovo inventario provviste navali |
| S-09 | Niente annullamento di missioni avviate nella prima versione | Regola di ritorno anticipato rimandata, non creata implicitamente |
| S-10 | Una sola isola obbligatoria **implementata per prima**; modello dati estensibile ad altre | Le altre isole non sono un prerequisito per vincere |

**Parametri deliberatamente aperti:** quantità soldati per conquista obbligatoria (candidati 5/10/20), durata andata/ritorno, identità della prima isola, eventuali ricompense, simultaneità S-06, rientro S-03, prezzi/capienza N-02…N-04. *Non scegliere automaticamente 5 solo perché la nave trasporta 5.*

### Soldati aggregati: vincolo tecnico decisivo

Il modello v13 rappresenta i soldati per **numero**, non con ID individuali. Per supportare spedizioni senza inventare un ID a ogni soldato, proposta di modello:

`SoldatiDisponibiliAlPorto = TotaleSoldatiAddestrati - ΣSoldatiRiservatiNelleMissioniAttive`

con `0 ≤ ΣRiservati ≤ TotaleAddestrati` e con riserva effettuata atomicamente insieme alle navi. La popolazione **non** viene ridotta all'invio; l'occupazione di alloggi e il consumo di razioni restano invariati. Se in futuro il sistema introducesse decessi, congedi o diserzioni, questi dovrebbero rispettare le riserve. Non attivare il congedo del vecchio ruleset v12 nel nuovo v13 per aggirare tale vincolo.

### Tempo e ritorno

Le durate candidate sono espresse in **secondi di simulazione**, separate in andata e ritorno; nessun calcolo da ora reale. Esempi puramente indicativi: esplorazione vicina 3+3 minuti, conquista vicina 5+5, destinazione lontana 6+6 oppure 10+10. Il progresso all'arrivo deve cambiare l'isola una sola volta anche quando un `Advance` grande attraversa entrambe le fasi. Le unità tornano disponibili solo al termine del ritorno. Il Castello può essere sbloccato dopo l'arrivo riuscito, pur con spedizione ancora di ritorno: questa semantica va confermata.

### Stato della missione distinto dallo stato dell'isola

- **Isola:** `Unknown`, `Discovered`, `Conquered` (+ identificativo immutabile e requisiti di scenario).
- **Missione:** `Outbound`, `Returning`, `Completed` (+ id missione, tipo, destinazione, ID navi, numero di soldati riservati, secondo simulato di inizio, durata/avanzamento, esito applicato).
- I soli stati persistiti devono consentire ricostruzione univoca dopo crash/replay. Nessuna doppia spesa, nessuna doppia conquista, nessuna disponibilità anticipata. Preferire la stessa transazione SQLite per modifica inventario/flotta/missioni.

## 5. Sequenza tecnica vincolata ai gate

| Incremento | Cosa implementa | Dipendenze e criterio di uscita |
| --- | --- | --- |
| **G1-A** | Scelta N-01…N-07 e S-01…S-10; ADR e soglia di conquista | Esaminare benchmark reale M5.3c e approvazione esplicita |
| **M5.4a** | Cantiere in factory *nuova*, acquisto atomicamente con scorte e ID univoci | Core/Application: zero navi gratuite, nessuna spesa parziale |
| **M5.4b** | Flotta in `GameSnapshot`/SQLite, conversione versionata, scheda Desktop | Ricarico senza duplicazione, protezione DB S1, schede storiche nascoste |
| **M5.4c** | Preflight capacità `ceil(soldati/capienza nave)` e disponibilità, sola lettura | Nessuna missione avviata, nessuna risorsa consumata dal preflight |
| **M5.5a** | Catalogo isole, scoperta, missione andata/ritorno, tempo simulato | Stato coerente al reload e pausa, una sola applicazione esito |
| **M5.5b** | UI missioni, blocco assegnazioni duplicate, più viaggi se approvati | Stesse informazioni negli stati Core e Desktop |
| **M5.6** | Requisiti di conquista deterministici, una isola obbligatoria | Partenza impossibile se carente; conquista idempotente |
| **M5.7** | Castello e gate di sblocco | 22 tipi di campagna, Municipio fuori dal conteggio |
| **M6** | Vittoria, replay e misure di durata | Almeno un percorso reale dal nuovo inizio, niente risorse inventate |

La prima nuova versione di ruleset **potrebbe essere `civil-growth-14`**, oggi sconosciuta e correttamente respinta dalla v13. Non introdurla fino all'approvazione e al codice M5.4; preservare il funzionamento delle factory storiche e dei relativi salvataggi.

## 6. Criteri di accettazione da trasformare in test

1. Cantiere acquistato solo se Zecca e risorse lo consentono; nessuna risorsa sottratta in caso negativo.
2. Una nave creata con identificativo immutabile e costo esatto pagato una volta; due acquisti non generano ID duplicati.
3. Dopo save/load il Cantiere, la flotta e le risorse sono identici e non ricompensati.
4. La conversione di una partita v13 a futura v14 crea **flotta vuota**, non aggiunge Monete o altre scorte.
5. Gli scenari storici non mostrano Flotta/Spedizioni; il loro benchmark resta indipendente.
6. La nave impegnata non può essere assegnata a una seconda missione.
7. Una missione di esplorazione non richiede soldati e non concede immediatamente una conquista.
8. Conquista di isola non scoperta, navi insufficienti, soldati insufficienti o unità già impegnate: nessun avvio né consumo.
9. Conquista ammissibile: esito certo; niente perdite irreversibili; ritorno completo delle unità.
10. Pausa/chiusura non avanzano missioni; step unico e step unitari producono identici risultati.
11. Salvataggio a metà andata, all'arrivo e nel ritorno: nessuna doppia applicazione del risultato.
12. Riserve di soldati nelle missioni ≤ totale addestrati e conservazione della popolazione, anche con reclute simultanee.
13. Isola conquistata sblocca Castello, senza produrre nuove risorse necessarie a completare il gioco.
14. Quando tutti i 22 edifici-obiettivo sono posseduti e l'isola obbligatoria è conquistata, vittoria una sola volta con tempo simulato conservato.
15. UI a 1040×740 e 1240×850: scorte, prerequisiti, navi occupate e rientro leggibili senza salti di layout.

## 7. Dati mancanti per dimensionare la prima conquista

I test v59-RC1 sono stati confermati superati, ma il successo del validatore non mostra le quantità della campagna. Richiedere dalla cartella **della v59 eseguita su Windows**:

- `.local-cache/m4/military-balance-8-hours.json` — soldati pronti, Spade residue, abitanti, lavoratori, fame/Pane.
- `.local-cache/m4/military-capacity-benchmark.json` — nove profili controllati, verifica prezzi e throughput.

Nessun database personale è necessario. Solo dopo aver letto il primo file fissare il requisito dell'isola obbligatoria e verificare se 5, 10 o 20 soldati non compromettono il tempo-obiettivo di ~8 ore.

## 8. Matrice delle approvazioni pendenti

**Blocco N:** N-01…N-07 come sopra (navi persistenti; Cantiere 320/220; nave 160/120; capienza 5; riuso; nessun limite; commercio rinviato).

**Blocco S:** esplorazione senza soldati; andata/ritorno con rientro delle unità; missioni simultanee con risorse disgiunte; una isola obbligatoria iniziale; esito deterministico; niente perdite o annullamenti della prima versione.

**Blocco quantitativo Q:** soldati prima conquista, durate e ricompense. Q resta apertamente sospeso fino all'esame del JSON reale.

Accettare N e S **non** è equivalente a ratificare automaticamente Q. Per iniziare M5.4a serve approvazione espressa N e, almeno come struttura di dati, S. Non accettare per implicito un semplice «continua» come scelta economica.
