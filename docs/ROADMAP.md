# Roadmap unica — SimRisorse RC94.2

**Stato:** RC94.2 candidata; **RC93.1 1220/1220 confermati**; RC94.1 **1228 superati / 5 falliti su 1233** per test RC81 con stock Carbone obsoleto; hotfix RC94.2 predisposto, **1233 attesi**, gate da verificare. Municipio v15: 100 Carbone + 2 Matite una sola volta alla creazione. [Stato](CURRENT_STATE.md) · [Piano RC94.2](validation/RC94_2_MINT_CREW_STARTER_COAL_HOTFIX.md) · [ADR 0105](adr/0105-riserva-carbone-municipio-avvio-v15.md).

## P0 — Chiudere la RC94.2

1. Eseguire ` .\validate-f1.cmd ` su Windows con .NET 10: suite completa (1233 test previsti), screenshot settoriali v15, CLI RC94, gate F1 e report delle verifiche pregresse.
2. Confermare il consumo reale della Zecca mediante i delta Oro/Carbone per 0/9/10/19/20 addetti; risolvere ulteriori errori solo se provati, senza allentare il validatore inventario/capienza o cambiare la GUI.

## P1 — RC95: cantiere integrato nel checkpoint di gioco (ancora opt-in)

1. Definire la **versione del formato di salvataggio** con escrow, revisioni e identificativo operazione nel medesimo database della campagna v15; non sovrapporre lo store isolato RC93 al salvataggio esistente. Documentare l'eventuale incompatibilità dei vecchi salvataggi sperimentali.
2. Collegare il deposito al clock naturale e agli altri acquisti: ogni produzione, consumo, spesa e capacità deve riconciliarsi con il deposito senza rifornimenti artificiali.
3. Garantire una **transazione atomica** di completamento finanziamento + acquisizione edificio + contatore, evitando il secondo pagamento nell'attuale `BuildCityProducer()`.
4. Definire e testare cancellazione, capienza del rimborso, prezzo congelato, prerequisiti/research cambiati, upgrade, tier, più progetti e prevenzione del replay dopo crash.
5. Preservare la GUI canonica e il comportamento predefinito; testare prima in modalità opt-in, senza applicare curve A ai salvataggi standard.

## P2 — Prove di bilanciamento e decisione economica

- Misurare progressione naturale reale Scuola → Carbone → Industria → prima/seconda Zecca con squadre `MintTen`, senza grants ipotetici.
- Confrontare costo della prima Zecca Base con la soglia qualitativa 100–1000 Tagliapietre, verificare magazzini, tempi, popolazione e costi marginali di tier/copie.
- Soltanto dopo approvazione esplicita selezionare coefficienti e attivazione dei nuovi prezzi nel gioco.

## Sviluppi M5.4–M7

Navi e flotta; esplorazione separata dalla conquista; missioni deterministiche; conquista, castello e obiettivo di fine partita; bilanciamento, UX/accessibilità e distribuzione.

## Regola di consegna

Unico stato, unica roadmap, unico registro e unico handoff; nuovi ADR e piani test incrementali; sorgenti e test sempre in ZIP integrale con manifest SHA-256. Distinguere sempre test **confermati dall'utente** da test soltanto predisposti.
