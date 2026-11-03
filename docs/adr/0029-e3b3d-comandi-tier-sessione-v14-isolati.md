# ADR 0029 — Comandi atomici per edifici produttivi a livelli nella sessione v14

**Stato:** decisione tecnica applicata al prototipo isolato, in attesa di test Windows. **Non approva i coefficienti sperimentali.** 03/11/2026.

## Contesto

E1 definisce prezzi progressivi; E2 gestisce portafogli per livelli; E3a definisce ricerca. La sessione E3b/3c gestisce tempo e popolazione, ma non costruzione/upgrades dei tier. Collegare direttamente la UI sarebbe prematuro.

## Decisione

1. La sessione v14 mantiene la proprietà esclusiva di risorse, produttori, assegnazioni e contatori, usando i calcolatori Core E1/E2 senza duplicare formule.
2. I comandi pubblici restituiscono errori tipizzati, non mutano lo snapshot al fallimento e validano in un unico commit all'esito positivo.
3. Le nuove costruzioni rispettano i prerequisiti fisici e gli sblocchi tecnologici. Gli upgrade richiedono sblocchi e non incrementano K; il costo è il differenziale di prezzo allo stesso K.
4. Il personale aggiuntivo di un livello non appare automaticamente; può occupare un posto solo dopo assegnazione esplicita di abitanti disponibili. La capacità di stoccaggio è legata ai posti dell'edificio, non al personale assegnato.
5. La politica dei prezzi è iniettabile ma viene fornita una configurazione candidata 1,15/2,6/6,5, ancora da ratificare.
6. Non si modificano `civil-growth-13`, SQLite v2, la GUI, le ricette né il motore militare.

## Conseguenze

Servono test di sessione con stato persistito e ripresa del tempo simulato, e un successivo gate di performance/bilanciamento. Non usare questa milestone come autorizzazione ad attivare v14 nella campagna. [Contratto G1-D14](../design/G1_D14_E3B3D_COSTRUZIONE_UPGRADE_TIER_V14.md).
