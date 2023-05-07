using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestionale
{
    public class Segheria : Costruzione
    {
        public Segheria() : base()
        {
            this.quantita = 1;
            this.nome = "Segheria";
            this.descrizione = "Taglia gli alberi e produce legno";
            this.costo.legno.quantita = 200;
            this.costo.pietra.quantita = 100;
            this.costo.persone.quantita = 2;

            this.produzione.legno.quantita = 50;

            this.consumo.alberi.quantita = 100;
        }
    }
}
