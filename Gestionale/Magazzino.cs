using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gestionale
{
    public class Magazzino
    {
        // Risorsa nome diff qt max
        public Risorse massimo = new Risorse();
        public Risorse deposito = new Risorse();

        public Magazzino() 
        {
            for (int i = 0; i < this.massimo.ListaPersone.Count; i++)
            {
                this.massimo.ListaPersone[i].quantita = 0;
            }

            for (int i = 0; i < this.deposito.ListaRisorse.Count; i++)
            {
                this.deposito.ListaRisorse[i].quantita = 0;
            }
        }

        public void CalcoloMassimoMagazzino(List<Costruzione> costruzioni)
        {
            Magazzino magazzinoAttuale = new Magazzino();

            foreach (Costruzione costruzione in costruzioni)
            {
                foreach (Risorsa risorsa in costruzione.magazzino.Lista)
                {
                    magazzinoAttuale.massimo.getRisorsa(risorsa.nome).quantita += risorsa.quantita;
                }
            }

            massimo = magazzinoAttuale.massimo;
        }

        public void CalcoloDepositoCiclo(List<Costruzione> costruzioni)
        {
            Magazzino magazzinoAttuale = new Magazzino();

            foreach (Costruzione costruzione in costruzioni)
            {
                foreach (Risorsa risorsa in costruzione.produzione.Lista)
                {
                    magazzinoAttuale.deposito.getRisorsa(risorsa.nome).quantita += risorsa.quantita;
                }

                foreach (Risorsa risorsa in costruzione.consumo.Lista)
                {
                    magazzinoAttuale.deposito.getRisorsa(risorsa.nome).quantita -= risorsa.quantita;
                }
            }

            foreach (Risorsa risorsa in deposito.Lista)
            {
                deposito.getRisorsa(risorsa.nome).quantita += magazzinoAttuale.deposito.getRisorsa(risorsa.nome).quantita;
            }
        }

        
    }
}
