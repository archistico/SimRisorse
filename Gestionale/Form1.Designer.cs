namespace Gestionale
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblSecondiPassati = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.orologio = new System.Windows.Forms.Timer(this.components);
            this.lblMagazzino = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSecondiPassati
            // 
            this.lblSecondiPassati.AutoSize = true;
            this.lblSecondiPassati.Location = new System.Drawing.Point(67, 9);
            this.lblSecondiPassati.Name = "lblSecondiPassati";
            this.lblSecondiPassati.Size = new System.Drawing.Size(12, 15);
            this.lblSecondiPassati.TabIndex = 5;
            this.lblSecondiPassati.Text = "-";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Secondi";
            // 
            // orologio
            // 
            this.orologio.Interval = 1000;
            this.orologio.Tick += new System.EventHandler(this.orologio_Tick);
            // 
            // lblMagazzino
            // 
            this.lblMagazzino.Location = new System.Drawing.Point(366, 26);
            this.lblMagazzino.Multiline = true;
            this.lblMagazzino.Name = "lblMagazzino";
            this.lblMagazzino.Size = new System.Drawing.Size(310, 521);
            this.lblMagazzino.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(366, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "Magazzino";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 559);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblMagazzino);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblSecondiPassati);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Label lblSecondiPassati;
        private Label label1;
        private System.Windows.Forms.Timer orologio;
        private TextBox lblMagazzino;
        private Label label2;
    }
}