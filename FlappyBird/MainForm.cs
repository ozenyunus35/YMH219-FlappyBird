using System.Diagnostics;
using System.Media;
using FlappyBird.Kontroller;
using FlappyBird.Oyun;
using FlappyBird.Veri;

namespace FlappyBird;

public partial class MainForm : Form
{
    private readonly OyunMotoru motor;
    private readonly RekorDeposu rekorDeposu = new();

    // Tuşa basılı tutunca sürekli zıplamasın diye (KeyDown tekrar tekrar geliyor)
    private bool ziplamaTusuBasili;
    private bool sesAcik = true;

    // Sabit zaman adımı: oyun saniyede 60 adım ilerliyor.
    // WinForms Timer'ı Windows'ta tam 16 ms'de tetiklenmiyor (bazen ~31 ms),
    // bu yüzden geçen gerçek süreyi ölçüp o kadar adım çalıştırıyoruz.
    private const double AdimSuresiMs = 1000.0 / 60;
    private readonly Stopwatch saat = new();
    private double birikenSureMs;

    public MainForm()
    {
        InitializeComponent();

        motor = new OyunMotoru(ZorlukSeviyesi.Normal);
        motor.SkorDegisti += Motor_SkorDegisti;
        motor.DurumDegisti += Motor_DurumDegisti;
        motor.OyunBitti += Motor_OyunBitti;

        zorlukSecici.Secili = ZorlukSeviyesi.Normal;

        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch (Exception)
        {
            // ikon okunamazsa varsayılan ikon kalsın
        }

        components ??= new System.ComponentModel.Container();
        var ipucu = new ToolTip(components);
        ipucu.SetToolTip(btnYeniOyun, "Yeni oyun (R)");
        ipucu.SetToolTip(btnDuraklat, "Duraklat / devam (P)");
        ipucu.SetToolTip(zorlukSecici, "Zorluk seviyesi");

        oyunAlani.Motor = motor;

        BilgileriGuncelle();
        DurumuGoster();
        saat.Start();
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

    private ZorlukSeviyesi SeciliZorluk => zorlukSecici.Secili;

    private void YeniOyunBaslat()
    {
        motor.YeniOyun(SeciliZorluk);
        oyunAlani.YeniRekor = false;
        // Boşluk basılı tutulurken R'ye basılırsa yeni oyun tuş tekrarıyla kendiliğinden başlamasın:
        // bayrak gerçek tuş durumuna göre ayarlanıyor
        ziplamaTusuBasili = ZiplamaTusuBasiliMi();
        oyunAlani.Focus();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static bool ZiplamaTusuBasiliMi() =>
        new[] { Keys.Space, Keys.Up, Keys.W }.Any(k => (GetAsyncKeyState((int)k) & 0x8000) != 0);

    // ---------- Oyun döngüsü ----------

    private void oyunZamanlayici_Tick(object? sender, EventArgs e)
    {
        double gecen = saat.Elapsed.TotalMilliseconds;
        saat.Restart();

        // Pencere sürüklenirken vs. uzun süre tick gelmezse oyun bir anda ileri sarıp oyuncuyu
        // haksız yere öldürmesin: oyun duraklatılıyor, devam edince 3-2-1 geri sayım var
        if (gecen > 400 && motor.Durum == OyunDurumu.Oynaniyor)
        {
            motor.Duraklat();
            birikenSureMs = 0;
            oyunAlani.Invalidate();
            return;
        }

        birikenSureMs += gecen;
        if (birikenSureMs > 250)
            birikenSureMs = 250;

        while (birikenSureMs >= AdimSuresiMs)
        {
            motor.Ilerle();
            birikenSureMs -= AdimSuresiMs;
        }

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

            case Keys.M:
                sesAcik = !sesAcik;
                lblSes.Text = sesAcik ? "Ses: Açık (M)" : "Ses: Kapalı (M)";
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
        motor.Duraklat();
    }

    // Geri dönünce: başka pencerede bırakılan tuşun KeyUp'ı bize gelmemiş olabilir
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        ziplamaTusuBasili = ZiplamaTusuBasiliMi();
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

    private void zorlukSecici_SecimDegisti(object? sender, EventArgs e)
    {
        // Motor henüz oluşturulmadıysa (constructor sırasında) bir şey yapma
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

        if (sesAcik)
        {
            if (yeniRekor)
                SystemSounds.Asterisk.Play();
            else
                SystemSounds.Hand.Play();
        }

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
            OyunDurumu.GeriSayim => "Oyun birazdan devam edecek...",
            OyunDurumu.Bitti => "Oyun bitti - R / Enter ile yeniden başla",
            _ => ""
        };

        btnDuraklat.Enabled = motor.Durum is OyunDurumu.Oynaniyor or OyunDurumu.Duraklatildi or OyunDurumu.GeriSayim;
        btnDuraklat.Text = motor.Durum == OyunDurumu.Duraklatildi ? "Devam" : "Duraklat";
        btnDuraklat.Ikon = motor.Durum == OyunDurumu.Duraklatildi ? ButonIkonu.Oynat : ButonIkonu.Duraklat;
    }
}
