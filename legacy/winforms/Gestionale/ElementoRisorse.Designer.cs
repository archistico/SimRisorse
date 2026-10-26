namespace Gestionale
{
    partial class ElementoRisorse
    {
        /// <summary> 
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione componenti

        /// <summary> 
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare 
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnMeno = new System.Windows.Forms.Button();
            this.btnPiu = new System.Windows.Forms.Button();
            this.lblTesto = new System.Windows.Forms.Label();
            this.lblValore = new System.Windows.Forms.Label();
            this.pbxImmagine = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxImmagine)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.btnMeno, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnPiu, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblTesto, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblValore, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.pbxImmagine, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(448, 48);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnMeno
            // 
            this.btnMeno.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMeno.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnMeno.Location = new System.Drawing.Point(248, 0);
            this.btnMeno.Margin = new System.Windows.Forms.Padding(0);
            this.btnMeno.Name = "btnMeno";
            this.btnMeno.Size = new System.Drawing.Size(50, 48);
            this.btnMeno.TabIndex = 0;
            this.btnMeno.Text = "-";
            this.btnMeno.UseVisualStyleBackColor = true;
            this.btnMeno.Click += new System.EventHandler(this.btnMeno_Click);
            // 
            // btnPiu
            // 
            this.btnPiu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPiu.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnPiu.Location = new System.Drawing.Point(398, 0);
            this.btnPiu.Margin = new System.Windows.Forms.Padding(0);
            this.btnPiu.Name = "btnPiu";
            this.btnPiu.Size = new System.Drawing.Size(50, 48);
            this.btnPiu.TabIndex = 1;
            this.btnPiu.Text = "+";
            this.btnPiu.UseVisualStyleBackColor = true;
            this.btnPiu.Click += new System.EventHandler(this.btnPiu_Click);
            // 
            // lblTesto
            // 
            this.lblTesto.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTesto.AutoSize = true;
            this.lblTesto.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTesto.Location = new System.Drawing.Point(55, 5);
            this.lblTesto.Margin = new System.Windows.Forms.Padding(5);
            this.lblTesto.Name = "lblTesto";
            this.lblTesto.Size = new System.Drawing.Size(188, 38);
            this.lblTesto.TabIndex = 2;
            this.lblTesto.Text = "-";
            this.lblTesto.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblValore
            // 
            this.lblValore.AutoSize = true;
            this.lblValore.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblValore.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblValore.Location = new System.Drawing.Point(303, 5);
            this.lblValore.Margin = new System.Windows.Forms.Padding(5);
            this.lblValore.Name = "lblValore";
            this.lblValore.Size = new System.Drawing.Size(90, 38);
            this.lblValore.TabIndex = 3;
            this.lblValore.Text = "-";
            this.lblValore.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pbxImmagine
            // 
            this.pbxImmagine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbxImmagine.Location = new System.Drawing.Point(0, 0);
            this.pbxImmagine.Margin = new System.Windows.Forms.Padding(0);
            this.pbxImmagine.Name = "pbxImmagine";
            this.pbxImmagine.Size = new System.Drawing.Size(50, 48);
            this.pbxImmagine.TabIndex = 4;
            this.pbxImmagine.TabStop = false;
            // 
            // ElementoRisorse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ElementoRisorse";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Size = new System.Drawing.Size(452, 52);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxImmagine)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button btnMeno;
        private Button btnPiu;
        private Label lblTesto;
        private Label lblValore;
        private PictureBox pbxImmagine;
    }
}
