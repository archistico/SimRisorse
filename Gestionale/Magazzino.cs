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
    }
}
