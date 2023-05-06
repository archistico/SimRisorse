using System.Timers;

namespace Gestionale
{
    public partial class Form1 : Form
    {
        public DateTime inizio;
        public Costruzione comune;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            inizio = DateTime.Now;
            orologio.Start();


        }

        private void orologio_Tick(object sender, EventArgs e)
        {
            DateTime ora = DateTime.Now;
            int secondi = Convert.ToInt32(System.Math.Round((ora - inizio).TotalSeconds));

            lblSecondiPassati.Text = secondi.ToString();            
        }
    }
}