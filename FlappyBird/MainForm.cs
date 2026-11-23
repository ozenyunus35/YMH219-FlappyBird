using System.Media;
using FlappyBird.Oyun;
using FlappyBird.Veri;

namespace FlappyBird;

public partial class MainForm : Form
{
    private readonly OyunMotoru motor;
    private readonly RekorDeposu rekorDeposu = new();

    // Tuşa basılı tutunca sürekli zıplamasın diye (KeyDown tekrar tekrar geliyor)
    private bool ziplamaTusuBasili;

    public MainForm()
    {
        InitializeComponent();

        motor = new OyunMotoru(ZorlukSeviyesi.Normal);
        motor.SkorDegisti += Motor_SkorDegisti;
        motor.DurumDegisti += Motor_DurumDegisti;
        motor.OyunBitti += Motor_OyunBitti;

        foreach (ZorlukSeviyesi seviye in Enum.GetValues<ZorlukSeviyesi>())
            cmbZorluk.Items.Add(seviye);
        cmbZorluk.SelectedItem = ZorlukSeviyesi.Normal;

        oyunAlani.Motor = motor;

        BilgileriGuncelle();
        DurumuGoster();
        oyunZamanlayici.Start();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (rekorDeposu.SonHata is not null)
        {
            MessageBox.Show(rekorDeposu.SonHata + "\nRekorlar bu oturumda sıfırdan başlayacak.",
                "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private ZorlukSeviyesi SeciliZorluk =>
        cmbZorluk.SelectedItem is ZorlukSeviyesi seviye ? seviye : ZorlukSeviyesi.Normal;

    private void YeniOyunBaslat()
    {
        motor.YeniOyun(SeciliZorluk);
        oyunAlani.YeniRekor = false;
        ziplamaTusuBasili = false;
        oyunAlani.Focus();
    }

    // ---------- Oyun döngüsü ----------

    private void oyunZamanlayici_Tick(object? sender, EventArgs e)
    {
        motor.Ilerle();
        oyunAlani.Invalidate();
    }

    // ---------- Klavye / fare ----------

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Space:
            case Keys.Up:
            case Keys.W:
                if (!ziplamaTusuBasili)
                {
                    ziplamaTusuBasili = true;
                    motor.KanatCirp();
                }
                e.Handled = e.SuppressKeyPress = true;
                break;

            case Keys.P:
            case Keys.Escape:
                motor.DuraklatVeyaDevamEt();
                e.Handled = true;
                break;

            case Keys.R:
                YeniOyunBaslat();
                e.Handled = true;
                break;

            case Keys.Enter:
                if (motor.Durum == OyunDurumu.Bitti)
                    YeniOyunBaslat();
                e.Handled = e.SuppressKeyPress = true;
                break;
        }
    }

    private void MainForm_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Space or Keys.Up or Keys.W)
            ziplamaTusuBasili = false;
    }

    private void oyunAlani_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            motor.KanatCirp();
    }

    // Başka pencereye geçilince oyun kendiliğinden dursun
    private void MainForm_Deactivate(object? sender, EventArgs e)
    {
        ziplamaTusuBasili = false;
        if (motor.Durum == OyunDurumu.Oynaniyor)
            motor.DuraklatVeyaDevamEt();
    }

    // ---------- Butonlar ----------

    private void btnYeniOyun_Click(object? sender, EventArgs e)
    {
        YeniOyunBaslat();
    }

    private void btnDuraklat_Click(object? sender, EventArgs e)
    {
        motor.DuraklatVeyaDevamEt();
        oyunAlani.Focus();
    }

    private void cmbZorluk_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Constructor içinde Items doldurulurken de tetikleniyor, motor henüz yoksa geç
        if (motor is null)
            return;

        if (SeciliZorluk != motor.Zorluk)
            YeniOyunBaslat();

        oyunAlani.Focus();
    }

    // ---------- Motor olayları ----------

    private void Motor_SkorDegisti(object? sender, EventArgs e)
    {
        BilgileriGuncelle();
    }

    private void Motor_DurumDegisti(object? sender, EventArgs e)
    {
        DurumuGoster();
    }

    private void Motor_OyunBitti(object? sender, EventArgs e)
    {
        bool yeniRekor = rekorDeposu.YeniSkoruDene(motor.Zorluk, motor.Skor);
        oyunAlani.YeniRekor = yeniRekor;
        BilgileriGuncelle();

        if (yeniRekor)
            SystemSounds.Asterisk.Play();
        else
            SystemSounds.Hand.Play();

        if (rekorDeposu.SonHata is not null)
            lblDurum.Text = rekorDeposu.SonHata;
    }

    private void BilgileriGuncelle()
    {
        int rekor = rekorDeposu.RekorGetir(motor.Zorluk);
        oyunAlani.Rekor = rekor;
        lblRekor.Text = $"Rekor: {rekor}";
        lblSkor.Text = $"Skor: {motor.Skor}";
        lblHiz.Text = $"Hız: {motor.Hiz:0.0}";
        lblSeviye.Text = $"Seviye: {motor.Seviye}";
    }

    private void DurumuGoster()
    {
        lblDurum.Text = motor.Durum switch
        {
            OyunDurumu.Hazir => "Hazır - başlamak için Boşluk tuşuna bas",
            OyunDurumu.Oynaniyor => "Oyun devam ediyor",
            OyunDurumu.Duraklatildi => "Duraklatıldı (P ile devam)",
            OyunDurumu.Bitti => "Oyun bitti - R / Enter ile yeniden başla",
            _ => ""
        };

        btnDuraklat.Enabled = motor.Durum is OyunDurumu.Oynaniyor or OyunDurumu.Duraklatildi;
        btnDuraklat.Text = motor.Durum == OyunDurumu.Duraklatildi ? "Devam" : "Duraklat";
    }
}
