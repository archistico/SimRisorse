# RC75 — Piano di verifica E4a.2b

**Stato:** implementato, non eseguito; RC63 (771 test) ultimo checkpoint validato. RC74 843 test attesi; RC75 **857 cumulativi attesi** (+14).

1. `./validate.cmd`: compilazione, tutti i progetti e 857/857 test attesi, nessun fallito/ignorato. In alternativa `./validate-f1.cmd` controlla TRX + audit (e non sostituisce la verifica visiva).
2. Validare i sei abbinamenti scenario/politica a orizzonte nullo: `Diagnostic=true`, `Outcome=timeout`, nessun `CompletionSeconds` né evento fittizio.
3. Testare che il runner inizi esattamente con la stessa popolazione, catalogo e scorte fisiche del benchmark RC73, senza contributi sintetici; `SchoolResearchRules.PencilsPerStudent == 2` come nella GUI v15.
4. Testare che nessun percorso iscriva studenti prima di aver costruito Scuola, Pane/Casa grande e Miniera di carbone, prodotto le Matite (1 plank + 1 coal → 4 Matite) e pagato 2 Matite + 10 Cibo per studente. Testare che il percorso Forgia richieda Ferro, Carbone e Fonderia; il percorso Zecca richieda Oro, Carbone e Fonderia d'oro, e che entrambi acquistino `estrazione-1` e `industria-1` prima della milestone.
5. Verificare ordine delle politiche e determinismo a 61 secondi, senza confrontare collezioni di dizionari per sola identità.
6. Verificare registrazione completa delle risorse effettivamente pagate (non solo planks/stone), spese ricerca in millipunti e assenza di costi per tentativi falliti.
7. Provare analisi con report non ordinati, duplicati, costi negativi, milestone non giustificate; tutti devono essere respinti.
8. Il confronto fra politiche deve rimanere `null` se manca una misura a orizzonte completo per entrambe. Niente tempi imputati a timeout/blocked.
9. Dopo il gate, provare una diagnosi `--max-seconds 120`, verificare JSON/CSV/Markdown, poi eseguire orizzonti completi su cartelle nuove. Registrare eventuali arresti e tempi effettivi **solo dopo** l'esecuzione.
10. Verificare manualmente grafica v15 vs GUI canonica RC63. Il confronto SHA256 dei file Desktop/Core è una protezione statica, non sostituisce ispezione visiva.

Non trattare i dati nel manifest `industrial-scenarios.csv` come risultati. Nessun cambiamento a costi o requisiti della Zecca è autorizzato in RC75.
