using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gestionale
{
    public class Risorse
    {
        public Risorsa persone = new Risorsa() { nome = RisorsaTipo.Persone, quantita = 0 };
        public Risorsa soldati = new Risorsa() { nome = RisorsaTipo.Soldati, quantita = 0 };

        public Risorsa alberi = new Risorsa() { nome = RisorsaTipo.Alberi, quantita = 0 };
        public Risorsa legno = new Risorsa() { nome = RisorsaTipo.Legno, quantita = 0 };
        public Risorsa pietra = new Risorsa() { nome = RisorsaTipo.Pietra, quantita = 0 };

        public List<Risorsa> ListaRisorse
        {
            get {
                return new List<Risorsa> { alberi, legno, pietra };                
            }
        }

        public List<Risorsa> ListaPersone
        {
            get
            {
                return new List<Risorsa> { persone, soldati };
            }
        }

        public List<Risorsa> Lista
        {
            get
            {
                return new List<Risorsa> { persone, soldati, alberi, legno, pietra };
            }
        }

        public Risorsa getRisorsa(String tipo)
        {
            switch(tipo)
            {
                case var _ when tipo == RisorsaTipo.Persone: return persone;
                case var _ when tipo == RisorsaTipo.Soldati: return soldati;

                case var _ when tipo == RisorsaTipo.Alberi: return alberi;
                case var _ when tipo == RisorsaTipo.Legno: return legno;
                case var _ when tipo == RisorsaTipo.Pietra: return pietra;
                default: throw new Exception();
            }
        }

        public void setRisorsa(String tipo, float quantita)
        {
            this.getRisorsa(tipo).quantita = quantita;
        }
    }
}
