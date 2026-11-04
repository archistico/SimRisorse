# G1-D37 / RC22 — Audit scorrimento dettagli Edifici e progressi Esercito

**Checkpoint Windows confermato:** RC20, 490 test dichiarati superati. **RC21** non validata (493 attesi), **RC22** cumulativa candidata (496 attesi, +3 casi Desktop), compilazione/xUnit da eseguire su Windows. L'audit estetico P0 delle 22 voci resta aperto.

## Evidenza e intervento

- **UI-12 / UI-18 — Dettagli Edifici:** in RC21 il blocco `V14BuildingDiagnosisPanel` era figlio diretto della riga `Auto` inferiore del `Grid` del dettaglio. Se la diagnosi includeva molti motivi simultanei (materiali insufficienti, lavoratori, produzione o capacità), la sua altezza cresceva senza limite e poteva sottrarre spazio alla sezione superiore, comprimendo la consultazione dei dettagli e i comandi inferiori.
- **RC22:** la diagnosi viene mantenuta nello stesso punto e con identico testo, ma inserita in un `ScrollViewer` verticale con altezza massima **104 px**, conservando la stessa cornice, i colori, il padding e i pulsanti separati Costruisci/Lavoratore. Il box descrittivo principale rimane scorrevole indipendentemente.
- **UI-15 — Esercito:** nessuna regressione confermata nel calcolo di progresso; aggiunto un test headless che verifica l'avanzamento individuale dopo 10 s, il tempo residuo, la progress bar e la conclusione dopo 30 s. Nessuna modifica applicativa ai militari.

## Ambito del diff

Un solo XAML della v14, nuovo test `V14BuildingDetailScrollAuditTests.cs` con due casi (1040×740, 1240×850), un caso aggiunto a `V14MilitaryUiParityTests.cs` e documentazione. **Invariati** v13, ViewModel, Core, Application, Infrastructure, SQLite, cataloghi e salvataggi. Nessun valore economico modificato.

## Limiti

I controlli statici sono distinti dai test reali. Il test headless non dimostra da solo l'estetica finale: verificare su Windows con diagnostica lunga, scrollbar della diagnosi, pulsanti sempre leggibili, selezione stabile e progressi reali dell'Esercito. Non chiudere la fase P0 senza prova manuale completa delle 22 voci.

[Piano RC22](../validation/G1_D37_RC22_TEST_PLAN.md) · [Roadmap](../ROADMAP.md) · [Audit GUI](../validation/G1_D29_AUDIT_GUI_V13_V14.md).
