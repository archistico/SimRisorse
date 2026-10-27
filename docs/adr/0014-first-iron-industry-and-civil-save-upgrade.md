# ADR 0014 — Prima industria del ferro e continuità dei salvataggi civili

- **Data:** 2026-10-27
- **Stato:** Accettato e verificato su Windows
- **Ambito:** M5.1a, catalogo Application, presentazione Desktop e compatibilità delle regole

## Contesto

Il catalogo della prima campagna comprende 22 tipi scelti dall'utente. M4 è chiusa dopo il gate tecnico, la lettura dei benchmark, il giudizio positivo sul prototipo monospace, Carne fuori dal primo catalogo e demolizione rinviata. L'esperimento industriale ha confrontato le ipotesi A/B senza applicarle al gioco.

L'utente ha scelto esplicitamente **S1**, continuare le partite civili con conversione e backup, e **configurazione A per la sola prima filiera ferro/carbone**. Oro e contenuti successivi restano fuori da questa approvazione.

## Decisione

- Estendere lo scenario `civil-growth` con le regole **`civil-growth-6`**: tre edifici e tre risorse nuovi, per un totale di **15 tipi di edifici e 12 risorse**.
- Applicare i costi e le ricette approvati: Miniera ferro 120/80 legno/pietra, 2 minerale/s; Miniera carbone 130/90, 2 carbone/s; Fonderia ferro 200/150, 2 minerale + 1 carbone → 1 ferro/s. Un lavoratore per edificio; miniere subordinate alla Casa grande e fonderia alle due miniere.
- Capacità base 1.000 per le nuove risorse, +500 per ogni Magazzino. Scorte, quantità e personale industriali iniziali a zero.
- Usare costruzione, assegnazione, pianificazione e limiti del Core esistente, senza introdurre un secondo motore industriale. Gli output rimangono disponibili soltanto dal passo successivo.
- Aggiungere la conversione esplicita **v5 → v6**, concatenabile alle conversioni precedenti. Conservare risorse civili, persone, edifici, tempo, progresso e frazione di crescita. Rifiutare un vecchio checkpoint che dichiara già scorte industriali positive o righe industriali incompatibili.
- Conservare formato snapshot/file **v2** e lo stesso slot civile. Le righe esistenti rappresentano già risorse ed edifici dinamici. Il caricamento converte in memoria; la prima scrittura preserva il checkpoint originale con il meccanismo di backup già adottato.
- Mantenere separati i cataloghi di benchmark storici **`civil-growth-4`** e **`civil-growth-5`**. Anche l'esperimento A/B resta basato sul catalogo civile v5, per preservare il confronto originario.

## Alternative

- **S2, nuova campagna con slot separato:** valutata e non scelta per questo incremento.
- **Prezzi B, 1,5×:** misurati nell'esperimento, non scelti per M5.1a.
- **Integrare anche Oro/Forgia/Zecca:** rinviato agli incrementi successivi e alle rispettive decisioni.

## Verifica e conseguenze

`validate.cmd` completo superato con **191/191 test**, zero avvisi/errori, quattordici cronologie di riferimento, quattro benchmark conservati e smoke nativi di entrambi gli scenari. Tredici render generati. Il [report M5.1a](../validation/M5_1A_IRON.md) precisa misure e limiti.

La vittoria scelta dall'utente — possesso dei 22 tipi, Castello conclusivo sbloccato da una conquista obbligatoria — guida la progressione futura; non è ancora una funzionalità eseguibile. Reclutamento, missioni e oro non vengono aggiunti da questa decisione.
