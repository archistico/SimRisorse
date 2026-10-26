using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gestionale
{
    public partial class ElementoRisorse : UserControl
    {
        public ElementoRisorse()
        {
            InitializeComponent();
        }

        public string Testo
        {
            get
            {
                return lblTesto.Text;
            }
            set
            {
                lblTesto.Text = value;
            }
        }

        public string Valore
        {
            get
            {
                return lblValore.Text;
            }
            set
            {
                lblValore.Text = value;
            }
        }

        private int valore = 0;

        private void btnMeno_Click(object sender, EventArgs e)
        {
            this.valore--;
            this.Valore = this.valore.ToString();
        }

        private void btnPiu_Click(object sender, EventArgs e)
        {
            this.valore++;
            this.Valore = this.valore.ToString();
        }
    }
}
