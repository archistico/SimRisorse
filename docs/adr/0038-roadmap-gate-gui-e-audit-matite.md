# ADR 0038 — Gate della nuova roadmap e trasparenza dei tooltip di capienza

**Stato:** **Accettato** per la sequenza di lavoro (richiesta esplicita dell'utente di scrivere la roadmap e riprendere lo sviluppo). Patch tooltip RC13 implementata, non ancora validata su Windows. **Data:** 03/11/2026.

## Contesto

Molte RC correggevano un problema per volta senza chiudere la matrice UI-01…UI-22, con il rischio di ripetere regressioni. Inoltre la nuova risorsa Matite esibiva una capienza statica non corrispondente agli edifici effettivi.

## Decisioni

1. La successione è vincolante: **parità GUI P0 → rifinitura civili/tecnologie P1 → audit economico E4a → bilanciamento approvato E4b → consolidamento/migrazione E4c → navale M5.4–M7**.
2. P0 non è chiuso con soli test verdi: servono una matrice completa e confronti manuali a 1040×740 e 1240×850.
3. I tooltip della capacità di qualsiasi risorsa devono descrivere **contributi effettivi degli edifici posseduti**, quantità e spazio libero; non valori potenziali presentati come attuali.
4. La RC13 implementa solo la presentazione delle Matite, usando il calcolo Core già esistente: **nessun cambiamento economico**.
5. La v13 e gli store SQLite rimangono invariati. Test non eseguiti non sono dichiarati superati.

## Conseguenze

La prima milestone produce audit, due test, una correzione puntuale e ZIP completo, ma lascia aperta la certificazione grafica. Valori economici e nuove meccaniche non vengono introdotti finché i rispettivi gate non lo consentono.

[Roadmap](../ROADMAP.md) · [Audit](../validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Rapporto RC13](../design/G1_D29_RC13_AUDIT_E_TOOLTIP_MATITE.md).
