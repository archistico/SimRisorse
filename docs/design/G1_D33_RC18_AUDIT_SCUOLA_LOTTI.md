# G1-D33 — RC18: coerenza suggerimenti nella Scuola

## Baseline e causa

RC16 è l'ultimo gate Windows confermato (476 test). RC17 non è ancora stata testata (481 attesi). RC18 prosegue da RC17 (484 attesi, senza validazione .NET qui). In `V14PreviewViewModel.School.cs`, `SchoolResearchHint` controllava se i posti ricerca liberi fossero **zero**, non se fossero **almeno `SchoolQuantity`**. Con un ordine di 5 ricercatori e 2 posti, l'interfaccia diceva «Assegna» benché `AssignResearcherCommand.CanExecute` fosse `false`.

## Intervento

Il suggerimento distingue: (1) Scuola non costruita; (2) posti liberi inferiori alla quantità selezionata, con numero disponibile e suggerimento per costruire una Scuola o sospendere ricercatori; (3) qualificati liberi inferiori al lotto, con numero; (4) conferma di nuova partita pendente, che blocca temporaneamente i comandi; (5) lotto fattibile. Il comando preesistente, già corretto, non viene modificato. `RefreshSchoolPanel()` notificava già proprietà e comandi al cambio di quantità.

## Test nuovi

1. Una Scuola, 5 ricercatori qualificati e 2 posti: lotto 5 respinto con causa esplicita, lotto 1 ammesso e assegnato, dopo l'assegnazione lotto 5 ancora correttamente respinto (1 posto residuo).
2. Nessuna Scuola e lotto 5: indicazione di costruire una Scuola, comando disabilitato.
3. Tre Scuole (6 posti) ma 1 qualificato: lotto 5 disabilitato con carenza di qualificati esplicitata.

## Confini e gate

Solo presentazione v14, test Desktop e documentazione. Nessun file XAML, Core, Application, Infrastructure, salvataggio o GUI v13 modificato. **RC18 deve ancora superare build/test Windows: 484 attesi; P0 estetico aperto**. Una verifica manuale sul gioco deve confrontare i testi e gli stati dei pulsanti a 1040×740 e 1240×850. Non introdurre E4.

[Piano Windows](../validation/G1_D33_RC18_TEST_PLAN.md) · [Audit](../validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Roadmap](../ROADMAP.md).
