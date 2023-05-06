using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestionale
{
    public class Risorse
    {
        public Risorsa persone = new Risorsa() { nome = "Persone", quantita = 0 };
        public Risorsa soldati = new Risorsa() { nome = "Soldati", quantita = 0 };
        public Risorsa alberi = new Risorsa() { nome = "Alberi", quantita = 0 };
        public Risorsa legno = new Risorsa() { nome = "Legno", quantita = 0 };
        public Risorsa pietra = new Risorsa() { nome = "Pietra", quantita = 0 };
    }
}
