namespace FlappyBird;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        ustPanel = new Panel();
        btnDuraklat = new Button();
        btnYeniOyun = new Button();
        cmbZorluk = new ComboBox();
        lblSeviye = new Label();
        lblHiz = new Label();
        lblRekor = new Label();
        lblSkor = new Label();
        oyunAlani = new Kontroller.OyunAlani();
        durumCubugu = new StatusStrip();
        lblDurum = new ToolStripStatusLabel();
        lblSes = new ToolStripStatusLabel();
        oyunZamanlayici = new System.Windows.Forms.Timer(components);
        ustPanel.SuspendLayout();
        durumCubugu.SuspendLayout();
        SuspendLayout();
        //
        // ustPanel
        //
        ustPanel.BackColor = Color.FromArgb(28, 24, 52);
        ustPanel.Controls.Add(btnDuraklat);
        ustPanel.Controls.Add(btnYeniOyun);
        ustPanel.Controls.Add(cmbZorluk);
        ustPanel.Controls.Add(lblSeviye);
        ustPanel.Controls.Add(lblHiz);
        ustPanel.Controls.Add(lblRekor);
        ustPanel.Controls.Add(lblSkor);
        ustPanel.Dock = DockStyle.Top;
        ustPanel.Location = new Point(0, 0);
        ustPanel.Name = "ustPanel";
        ustPanel.Size = new Size(420, 52);
        ustPanel.TabIndex = 0;
        //
        // btnDuraklat
        //
        btnDuraklat.BackColor = Color.FromArgb(60, 52, 104);
        btnDuraklat.Enabled = false;
        btnDuraklat.FlatAppearance.BorderColor = Color.FromArgb(247, 146, 86);
        btnDuraklat.FlatStyle = FlatStyle.Flat;
        btnDuraklat.ForeColor = Color.White;
        btnDuraklat.Location = new Point(340, 12);
        btnDuraklat.Name = "btnDuraklat";
        btnDuraklat.Size = new Size(72, 28);
        btnDuraklat.TabIndex = 6;
        btnDuraklat.TabStop = false;
        btnDuraklat.Text = "Duraklat";
        btnDuraklat.UseVisualStyleBackColor = false;
        btnDuraklat.Click += btnDuraklat_Click;
        //
        // btnYeniOyun
        //
        btnYeniOyun.BackColor = Color.FromArgb(247, 146, 86);
        btnYeniOyun.FlatAppearance.BorderSize = 0;
        btnYeniOyun.FlatStyle = FlatStyle.Flat;
        btnYeniOyun.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnYeniOyun.ForeColor = Color.FromArgb(28, 24, 52);
        btnYeniOyun.Location = new Point(258, 12);
        btnYeniOyun.Name = "btnYeniOyun";
        btnYeniOyun.Size = new Size(78, 28);
        btnYeniOyun.TabIndex = 5;
        btnYeniOyun.TabStop = false;
        btnYeniOyun.Text = "Yeni Oyun";
        btnYeniOyun.UseVisualStyleBackColor = false;
        btnYeniOyun.Click += btnYeniOyun_Click;
        //
        // cmbZorluk
        //
        cmbZorluk.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbZorluk.FormattingEnabled = true;
        cmbZorluk.Location = new Point(180, 15);
        cmbZorluk.Name = "cmbZorluk";
        cmbZorluk.Size = new Size(72, 23);
        cmbZorluk.TabIndex = 4;
        cmbZorluk.TabStop = false;
        cmbZorluk.SelectedIndexChanged += cmbZorluk_SelectedIndexChanged;
        //
        // lblSeviye
        //
        lblSeviye.AutoSize = true;
        lblSeviye.ForeColor = Color.FromArgb(200, 196, 230);
        lblSeviye.Location = new Point(95, 28);
        lblSeviye.Name = "lblSeviye";
        lblSeviye.Size = new Size(52, 15);
        lblSeviye.TabIndex = 3;
        lblSeviye.Text = "Seviye: 1";
        //
        // lblHiz
        //
        lblHiz.AutoSize = true;
        lblHiz.ForeColor = Color.FromArgb(200, 196, 230);
        lblHiz.Location = new Point(95, 8);
        lblHiz.Name = "lblHiz";
        lblHiz.Size = new Size(46, 15);
        lblHiz.TabIndex = 2;
        lblHiz.Text = "Hız: 0.0";
        //
        // lblRekor
        //
        lblRekor.AutoSize = true;
        lblRekor.ForeColor = Color.FromArgb(255, 214, 120);
        lblRekor.Location = new Point(10, 28);
        lblRekor.Name = "lblRekor";
        lblRekor.Size = new Size(50, 15);
        lblRekor.TabIndex = 1;
        lblRekor.Text = "Rekor: 0";
        //
        // lblSkor
        //
        lblSkor.AutoSize = true;
        lblSkor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSkor.ForeColor = Color.White;
        lblSkor.Location = new Point(10, 8);
        lblSkor.Name = "lblSkor";
        lblSkor.Size = new Size(50, 15);
        lblSkor.TabIndex = 0;
        lblSkor.Text = "Skor: 0";
        //
        // oyunAlani
        //
        oyunAlani.Dock = DockStyle.Fill;
        oyunAlani.Location = new Point(0, 52);
        oyunAlani.Name = "oyunAlani";
        oyunAlani.Size = new Size(420, 560);
        oyunAlani.TabIndex = 1;
        oyunAlani.MouseDown += oyunAlani_MouseDown;
        //
        // durumCubugu
        //
        durumCubugu.Items.AddRange(new ToolStripItem[] { lblDurum, lblSes });
        durumCubugu.Location = new Point(0, 612);
        durumCubugu.Name = "durumCubugu";
        durumCubugu.Size = new Size(420, 22);
        durumCubugu.SizingGrip = false;
        durumCubugu.TabIndex = 2;
        //
        // lblDurum
        //
        lblDurum.Name = "lblDurum";
        lblDurum.Size = new Size(328, 17);
        lblDurum.Spring = true;
        lblDurum.Text = "Hazır";
        lblDurum.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblSes
        // 
        lblSes.ForeColor = Color.DimGray;
        lblSes.Name = "lblSes";
        lblSes.Size = new Size(77, 17);
        lblSes.Text = "Ses: Açık (M)";
        //
        // oyunZamanlayici
        //
        oyunZamanlayici.Interval = 15;
        oyunZamanlayici.Tick += oyunZamanlayici_Tick;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(420, 634);
        Controls.Add(oyunAlani);
        Controls.Add(durumCubugu);
        Controls.Add(ustPanel);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        KeyPreview = true;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Flappy Bird - YMH219";
        Deactivate += MainForm_Deactivate;
        KeyDown += MainForm_KeyDown;
        KeyUp += MainForm_KeyUp;
        ustPanel.ResumeLayout(false);
        ustPanel.PerformLayout();
        durumCubugu.ResumeLayout(false);
        durumCubugu.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Panel ustPanel;
    private Label lblSkor;
    private Label lblRekor;
    private Label lblHiz;
    private Label lblSeviye;
    private ComboBox cmbZorluk;
    private Button btnYeniOyun;
    private Button btnDuraklat;
    private Kontroller.OyunAlani oyunAlani;
    private StatusStrip durumCubugu;
    private ToolStripStatusLabel lblDurum;
    private ToolStripStatusLabel lblSes;
    private System.Windows.Forms.Timer oyunZamanlayici;
}
