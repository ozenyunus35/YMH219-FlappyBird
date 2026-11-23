using System.ComponentModel;
using System.Drawing.Drawing2D;
using FlappyBird.Oyun;

namespace FlappyBird.Kontroller;

/// <summary>
/// Oyunun çizildiği alan. PictureBox kullanmak yerine her şeyi OnPaint içinde
/// GDI+ ile çiziyorum, böylece projede hiç resim dosyası gerekmiyor.
/// Katmanlar (arkadan öne): gökyüzü, yıldızlar, güneş, tepeler, şehir, bulutlar,
/// borular, zemin, kuş, efektler, ekran yazıları.
/// </summary>
public class OyunAlani : Control
{
    private const int W = OyunMotoru.AlanGenisligi;
    private const int H = OyunMotoru.AlanYuksekligi;

    private static readonly Color BoruAcik = Color.FromArgb(72, 196, 160);
    private static readonly Color BoruKoyu = Color.FromArgb(20, 104, 92);
    private static readonly Color BoruKenar = Color.FromArgb(14, 60, 54);

    private readonly Font bilgiFontu = new("Segoe UI", 12, FontStyle.Regular);
    private readonly Font kucukFont = new("Segoe UI", 10, FontStyle.Bold);
    private readonly Font soruFontu = new("Segoe UI", 22, FontStyle.Bold);

    private readonly PointF[] bulutlar = { new(40, 110), new(210, 170), new(360, 90), new(520, 150) };
    private readonly PointF[] yildizlar;
    private readonly (float X, float Genislik, float Yukseklik)[] binalar;

    // Efektler
    private readonly List<(float X, float Y, long Baslangic)> skorBalonlari = new();
    private long bitisTick = -1;
    private OyunMotoru? motor;
    private int sonSkor;

    public OyunAlani()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.Selectable, true);
        TabStop = true;

        // Yıldız ve bina yerleri sabit tohumla üretiliyor, her açılışta aynı manzara
        var r = new Random(219);
        yildizlar = Enumerable.Range(0, 40).Select(_ => new PointF(r.Next(0, W), r.Next(0, 260))).ToArray();

        var liste = new List<(float, float, float)>();
        for (float x = 0; x < W + 200;)
        {
            float gen = r.Next(28, 60);
            liste.Add((x, gen, r.Next(50, 150)));
            x += gen + r.Next(2, 10);
        }
        binalar = liste.ToArray();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public OyunMotoru? Motor
    {
        get => motor;
        set
        {
            if (motor != null)
            {
                motor.SkorDegisti -= Motor_SkorDegisti;
                motor.OyunBitti -= Motor_OyunBitti;
                motor.DurumDegisti -= Motor_DurumDegisti;
            }
            motor = value;
            if (motor != null)
            {
                motor.SkorDegisti += Motor_SkorDegisti;
                motor.OyunBitti += Motor_OyunBitti;
                motor.DurumDegisti += Motor_DurumDegisti;
                sonSkor = motor.Skor;
            }
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Rekor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool YeniRekor { get; set; }

    private void Motor_SkorDegisti(object? sender, EventArgs e)
    {
        if (motor is null)
            return;
        if (motor.Skor > sonSkor)
            skorBalonlari.Add((motor.Kus.X + 10, motor.Kus.Y - 10, motor.TickSayisi));
        sonSkor = motor.Skor;
    }

    private void Motor_OyunBitti(object? sender, EventArgs e)
    {
        bitisTick = motor?.TickSayisi ?? 0;
    }

    private void Motor_DurumDegisti(object? sender, EventArgs e)
    {
        if (motor?.Durum == OyunDurumu.Hazir)
        {
            bitisTick = -1;
            skorBalonlari.Clear();
        }
    }

    // Ok tuşları ve boşluk normalde form tarafından "gezinme" için yutuluyor
    protected override bool IsInputKey(Keys keyData)
    {
        return keyData is Keys.Space or Keys.Up or Keys.Down or Keys.Left or Keys.Right
               || base.IsInputKey(keyData);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        base.OnMouseDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // Pencere küçültülünce boyut 0 olabiliyor; ölçekleme ve fırçalar hata vermesin
        if (Width < 1 || Height < 1)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        // Oyun 480x640 mantıksal alanda çalışıyor, kontrol boyutuna ölçekliyoruz
        g.ScaleTransform(Width / (float)W, Height / (float)H);

        long tick = motor?.TickSayisi ?? 0;

        // Çarpışma anında kısa bir ekran sarsıntısı
        long bitistenBeri = bitisTick < 0 ? long.MaxValue : tick - bitisTick;
        if (bitistenBeri < 14)
        {
            float guc = (14 - bitistenBeri) * 0.6f;
            g.TranslateTransform((float)Math.Sin(tick * 2.1) * guc, (float)Math.Cos(tick * 1.7) * guc);
        }

        GokyuzunuCiz(g, tick);
        YildizlariCiz(g, tick);
        float kayma = motor?.AlinanYol ?? tick;
        TepeleriCiz(g, kayma * 0.15f);
        SehriCiz(g, kayma * 0.35f);
        BulutlariCiz(g, tick);

        if (motor is null)
            return;

        foreach (var boru in motor.Borular)
            BoruyuCiz(g, boru, motor.ZeminY);

        ZeminiCiz(g, motor, kayma);
        KusuCiz(g, motor);
        SkorBalonlariniCiz(g, tick);

        if (bitistenBeri < 12)
        {
            using var flas = new SolidBrush(Color.FromArgb((int)(200 * (1 - bitistenBeri / 12f)), Color.White));
            g.FillRectangle(flas, -20, -20, W + 40, H + 40);
        }

        EkranYazilariniCiz(g, motor, tick, bitistenBeri);
    }

    // ---------------- Arka plan ----------------

    private static void GokyuzunuCiz(Graphics g, long tick)
    {
        var alan = new Rectangle(0, 0, W, H);
        using (var firca = new LinearGradientBrush(alan, Color.Black, Color.Black, LinearGradientMode.Vertical))
        {
            firca.InterpolationColors = new ColorBlend
            {
                Colors = new[]
                {
                    Color.FromArgb(28, 22, 66),
                    Color.FromArgb(84, 52, 128),
                    Color.FromArgb(214, 112, 120),
                    Color.FromArgb(252, 176, 100)
                },
                Positions = new[] { 0f, 0.38f, 0.7f, 1f }
            };
            g.FillRectangle(firca, alan);
        }

        // Güneş ve etrafındaki parıltı
        var gunes = new RectangleF(285, 330, 150, 150);
        using (var yol = new GraphicsPath())
        {
            var parilti = RectangleF.Inflate(gunes, 70, 70);
            yol.AddEllipse(parilti);
            using var pf = new PathGradientBrush(yol)
            {
                CenterColor = Color.FromArgb(110, 255, 220, 140),
                SurroundColors = new[] { Color.FromArgb(0, 255, 200, 120) }
            };
            g.FillEllipse(pf, parilti);
        }
        using (var gf = new LinearGradientBrush(gunes, Color.FromArgb(255, 240, 170), Color.FromArgb(255, 170, 90), LinearGradientMode.Vertical))
            g.FillEllipse(gf, gunes);
    }

    private void YildizlariCiz(Graphics g, long tick)
    {
        for (int i = 0; i < yildizlar.Length; i++)
        {
            int alfa = (int)(110 + 100 * Math.Sin(tick / 20.0 + i * 1.7));
            using var b = new SolidBrush(Color.FromArgb(Math.Clamp(alfa, 20, 220), 255, 255, 255));
            float boy = i % 5 == 0 ? 2.6f : 1.6f;
            g.FillEllipse(b, yildizlar[i].X, yildizlar[i].Y, boy, boy);
        }
    }

    private static void TepeleriCiz(Graphics g, float kayma)
    {
        float zemin = OyunMotoru.AlanYuksekligi - OyunMotoru.ZeminYuksekligi;
        const float periyot = 260f;
        float baslangic = -(kayma % periyot) - periyot;

        using var yol = new GraphicsPath();
        var noktalar = new List<PointF> { new(baslangic, zemin) };
        for (float x = baslangic; x <= W + periyot; x += 20)
        {
            float y = zemin - 120 - 35 * (float)Math.Sin((x + kayma) / periyot * Math.PI * 2) - 15 * (float)Math.Sin((x + kayma) / 70.0);
            noktalar.Add(new PointF(x, y));
        }
        noktalar.Add(new PointF(W + periyot, zemin));
        yol.AddPolygon(noktalar.ToArray());

        using var firca = new LinearGradientBrush(new RectangleF(0, zemin - 200, W, 200),
            Color.FromArgb(150, 110, 70, 140), Color.FromArgb(200, 70, 40, 100), LinearGradientMode.Vertical);
        g.FillPath(firca, yol);
    }

    private void SehriCiz(Graphics g, float kayma)
    {
        float zemin = OyunMotoru.AlanYuksekligi - OyunMotoru.ZeminYuksekligi;
        float toplam = binalar[^1].X + binalar[^1].Genislik + 8;
        float ofset = kayma % toplam;

        using var govde = new SolidBrush(Color.FromArgb(235, 48, 30, 72));
        using var pencere = new SolidBrush(Color.FromArgb(200, 255, 214, 130));

        for (int tekrar = 0; tekrar < 2; tekrar++)
        {
            foreach (var (bx, gen, yuk) in binalar)
            {
                float x = bx - ofset + tekrar * toplam;
                if (x > W || x + gen < 0)
                    continue;

                g.FillRectangle(govde, x, zemin - yuk, gen, yuk);

                // Pencereler: bina konumuna göre sabit desen
                int seed = (int)bx;
                for (float py = zemin - yuk + 8; py < zemin - 10; py += 14)
                    for (float px = x + 5; px < x + gen - 6; px += 10)
                        if (((int)(px - x) * 7 + (int)py * 3 + seed) % 5 == 0)
                            g.FillRectangle(pencere, px, py, 4, 6);
            }
        }
    }

    private void BulutlariCiz(Graphics g, long tick)
    {
        using var firca = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
        float kayma = tick * 0.4f;
        int genislik = W + 160;

        foreach (var bulut in bulutlar)
        {
            // Bulutlar borulardan yavaş kayıyor (paralaks etkisi)
            float x = ((bulut.X - kayma) % genislik + genislik) % genislik - 120;
            g.FillEllipse(firca, x, bulut.Y, 90, 34);
            g.FillEllipse(firca, x + 25, bulut.Y - 16, 60, 40);
            g.FillEllipse(firca, x + 50, bulut.Y - 4, 70, 32);
        }
    }

    // ---------------- Oyun nesneleri ----------------

    private static void BoruyuCiz(Graphics g, Boru boru, float zeminY)
    {
        using var kenar = new Pen(BoruKenar, 3);
        using var parlama = new SolidBrush(Color.FromArgb(70, 255, 255, 255));

        foreach (var parca in new[] { boru.UstParca, boru.AltParca(zeminY) })
        {
            if (parca.Height <= 0)
                continue;

            using var govde = new LinearGradientBrush(new RectangleF(parca.X - 1, 0, parca.Width + 2, 10), BoruAcik, BoruKoyu, 0f);
            g.FillRectangle(govde, parca);
            g.FillRectangle(parlama, parca.X + 10, parca.Y, 6, parca.Height);
            g.DrawRectangle(kenar, parca.X, parca.Y, parca.Width, parca.Height);
        }

        // Boşluğun kenarlarındaki "ağız" kısımları
        const int agizYuksekligi = 24;
        var ustAgiz = new RectangleF(boru.X - 6, boru.BoslukY - agizYuksekligi, Boru.Genislik + 12, agizYuksekligi);
        var altAgiz = new RectangleF(boru.X - 6, boru.BoslukY + boru.BoslukBoyu, Boru.Genislik + 12, agizYuksekligi);

        foreach (var agiz in new[] { ustAgiz, altAgiz })
        {
            using var yol = Cizim.YuvarlakDikdortgen(agiz, 4);
            using var govde = new LinearGradientBrush(new RectangleF(agiz.X - 1, 0, agiz.Width + 2, 10), BoruAcik, BoruKoyu, 0f);
            g.FillPath(govde, yol);
            g.FillRectangle(parlama, agiz.X + 10, agiz.Y + 3, 6, agiz.Height - 6);
            g.DrawPath(kenar, yol);
        }
    }

    private static void ZeminiCiz(Graphics g, OyunMotoru motor, float kayma)
    {
        float y = motor.ZeminY;
        var alan = new RectangleF(0, y, W, OyunMotoru.ZeminYuksekligi);
        using (var zemin = new LinearGradientBrush(alan, Color.FromArgb(234, 206, 150), Color.FromArgb(205, 168, 108), LinearGradientMode.Vertical))
            g.FillRectangle(zemin, alan);

        using (var cimen = new LinearGradientBrush(new RectangleF(0, y, W, 16), Color.FromArgb(132, 204, 84), Color.FromArgb(76, 150, 56), LinearGradientMode.Vertical))
            g.FillRectangle(cimen, 0, y, W, 14);
        using (var cimenKenar = new Pen(Color.FromArgb(52, 110, 40), 3))
            g.DrawLine(cimenKenar, 0, y + 14, W, y + 14);

        // Kayan çizgi deseni hareket hissi veriyor
        using var cizgi = new Pen(Color.FromArgb(70, 140, 100, 50), 4);
        float ofset = kayma % 28;
        for (float x = -ofset - 28; x < W + 28; x += 28)
            g.DrawLine(cizgi, x, y + 24, x + 14, y + 40);

        using var ust = new Pen(Color.FromArgb(40, 255, 255, 255), 2);
        g.DrawLine(ust, 0, y + 1, W, y + 1);
    }

    private static void KusuCiz(Graphics g, OyunMotoru motor)
    {
        Kus kus = motor.Kus;
        float y = kus.Y;

        // Başlamadan önce kuş havada hafifçe salınıyor
        if (motor.Durum == OyunDurumu.Hazir)
            y += (float)Math.Sin(motor.TickSayisi / 8.0) * 6;

        GraphicsState onceki = g.Save();
        g.TranslateTransform(kus.X + Kus.Genislik / 2f, y + Kus.Yukseklik / 2f);
        g.RotateTransform(motor.Durum is OyunDurumu.Hazir or OyunDurumu.GeriSayim ? 0 : kus.Aci);

        float w = Kus.Genislik, h = Kus.Yukseklik;
        var govdeAlan = new RectangleF(-w / 2, -h / 2, w, h);
        using var govde = new LinearGradientBrush(govdeAlan, Color.FromArgb(255, 226, 80), Color.FromArgb(250, 160, 30), LinearGradientMode.Vertical);
        using var karin = new SolidBrush(Color.FromArgb(255, 244, 190));
        using var kanat = new LinearGradientBrush(new RectangleF(-w / 2, -h / 2, 18, h), Color.FromArgb(255, 250, 230), Color.FromArgb(240, 170, 60), LinearGradientMode.Vertical);
        using var gaga = new SolidBrush(Color.FromArgb(240, 96, 48));
        using var kontur = new Pen(Color.FromArgb(70, 40, 20), 2);

        g.FillEllipse(govde, govdeAlan);
        g.FillEllipse(karin, -w / 2 + 6, 1, w - 14, h / 2 - 3);
        g.DrawEllipse(kontur, govdeAlan);

        // Kanat her 6 tick'te bir yukarı/aşağı
        bool kanatYukarida = motor.Durum != OyunDurumu.Bitti && motor.TickSayisi / 6 % 2 == 0;
        float kanatY = kanatYukarida ? -h / 2 + 2 : -1;
        g.FillEllipse(kanat, -w / 2 + 1, kanatY, 17, 11);
        g.DrawEllipse(kontur, -w / 2 + 1, kanatY, 17, 11);

        // Göz
        g.FillEllipse(Brushes.White, w / 2 - 15, -h / 2 + 2, 12, 12);
        g.DrawEllipse(kontur, w / 2 - 15, -h / 2 + 2, 12, 12);
        g.FillEllipse(Brushes.Black, w / 2 - 8, -h / 2 + 5, 4, 6);

        // Gaga
        PointF[] gagaNoktalari = { new(w / 2 - 5, -1), new(w / 2 + 10, 3), new(w / 2 - 5, 9) };
        g.FillPolygon(gaga, gagaNoktalari);
        g.DrawPolygon(kontur, gagaNoktalari);

        g.Restore(onceki);
    }

    private void SkorBalonlariniCiz(Graphics g, long tick)
    {
        skorBalonlari.RemoveAll(b => tick - b.Baslangic > 40);
        foreach (var (x, y, baslangic) in skorBalonlari)
        {
            float t = (tick - baslangic) / 40f;
            int alfa = (int)(255 * (1 - t));
            Cizim.KenarliYazi(g, "+1", 26, new PointF(x + 20, y - 30 * t), Color.White, null,
                Color.FromArgb(60, 30, 80), 5, FontStyle.Bold, Math.Clamp(alfa, 0, 255));
        }
    }

    // ---------------- Ekran yazıları ----------------

    private void EkranYazilariniCiz(Graphics g, OyunMotoru motor, long tick, long bitistenBeri)
    {
        if (motor.Durum is OyunDurumu.Oynaniyor or OyunDurumu.Duraklatildi or OyunDurumu.GeriSayim)
            Cizim.KenarliYazi(g, motor.Skor.ToString(), 54, new PointF(W / 2f, 66), Color.White, Color.FromArgb(255, 236, 200));

        switch (motor.Durum)
        {
            case OyunDurumu.Hazir:
                BaslangicEkrani(g, motor, tick);
                break;

            case OyunDurumu.Duraklatildi:
                Karart(g, 110);
                Cizim.Kart(g, new RectangleF(80, 240, 320, 120));
                Cizim.KenarliYazi(g, "DURAKLATILDI", 32, new PointF(W / 2f, 282), Color.FromArgb(255, 200, 80), Color.FromArgb(250, 140, 40));
                using (var f = new SolidBrush(Color.FromArgb(110, 70, 50)))
                    g.DrawString("Devam etmek için P", bilgiFontu, f, new RectangleF(80, 312, 320, 30), Ortala);
                break;

            case OyunDurumu.GeriSayim:
                Karart(g, 60);
                float oran = 1 + (motor.GeriSayimKalan % 60) / 60f * 0.6f;   // her saniye büyükten küçülüyor
                Cizim.KenarliYazi(g, motor.GeriSayimSaniye.ToString(), 90 * oran, new PointF(W / 2f, 290),
                    Color.White, Color.FromArgb(255, 220, 150), null, 9);
                Cizim.KenarliYazi(g, "Hazırlan!", 24, new PointF(W / 2f, 370), Color.White, null, null, 5);
                break;

            case OyunDurumu.Bitti:
                OyunSonuEkrani(g, motor, bitistenBeri);
                break;
        }
    }

    private static readonly StringFormat Ortala = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

    private static void Karart(Graphics g, int alfa)
    {
        using var b = new SolidBrush(Color.FromArgb(alfa, 15, 8, 30));
        g.FillRectangle(b, -20, -20, W + 40, H + 40);
    }

    private void BaslangicEkrani(Graphics g, OyunMotoru motor, long tick)
    {
        // Logo hafifçe yukarı aşağı süzülüyor
        float sal = (float)Math.Sin(tick / 15.0) * 4;
        Cizim.KenarliYazi(g, "FLAPPY", 64, new PointF(W / 2f, 120 + sal), Color.FromArgb(255, 232, 110), Color.FromArgb(255, 150, 40), Color.FromArgb(60, 30, 70), 10);
        Cizim.KenarliYazi(g, "BIRD", 64, new PointF(W / 2f, 182 + sal), Color.FromArgb(255, 232, 110), Color.FromArgb(255, 150, 40), Color.FromArgb(60, 30, 70), 10);

        // "YMH219 edition" etiketi
        var etiket = new RectangleF(W / 2f - 70, 222 + sal, 140, 26);
        using (var yol = Cizim.YuvarlakDikdortgen(etiket, 13))
        using (var b = new SolidBrush(Color.FromArgb(230, 236, 72, 110)))
        {
            g.FillPath(b, yol);
            g.DrawString("YMH219 EDITION", kucukFont, Brushes.White, etiket, Ortala);
        }

        // Alt bilgi kartı
        var kart = new RectangleF(70, 392, 340, 112);
        Cizim.Kart(g, kart, 16);
        using (var koyu = new SolidBrush(Color.FromArgb(110, 70, 50)))
        using (var vurgu = new SolidBrush(Color.FromArgb(236, 120, 40)))
        {
            g.DrawString("EN İYİ", kucukFont, koyu, new RectangleF(kart.X, kart.Y + 12, kart.Width / 2, 20), Ortala);
            g.DrawString("ZORLUK", kucukFont, koyu, new RectangleF(kart.X + kart.Width / 2, kart.Y + 12, kart.Width / 2, 20), Ortala);
            using var buyuk = new Font("Segoe UI", 20, FontStyle.Bold);
            g.DrawString(Rekor.ToString(), buyuk, vurgu, new RectangleF(kart.X, kart.Y + 30, kart.Width / 2, 38), Ortala);
            g.DrawString(motor.Zorluk.ToString().ToUpperInvariant(), buyuk, vurgu, new RectangleF(kart.X + kart.Width / 2, kart.Y + 30, kart.Width / 2, 38), Ortala);
        }

        // Tuş ipuçları
        float ty = kart.Y + 76;
        float tx = kart.X + 22;
        foreach (var (tus, aciklama, genislik) in new[] { ("Boşluk", "zıpla", 62f), ("P", "duraklat", 24f), ("M", "ses", 24f) })
        {
            Cizim.Tus(g, tus, new RectangleF(tx, ty, genislik, 22), kucukFont);
            using var b = new SolidBrush(Color.FromArgb(110, 70, 50));
            g.DrawString(aciklama, kucukFont, b, tx + genislik + 4, ty + 2);
            tx += genislik + 4 + g.MeasureString(aciklama, kucukFont).Width + 12;
        }

        // Yanıp sönen başlama yazısı
        int alfa = (int)(215 + 40 * Math.Sin(tick / 9.0));
        Cizim.KenarliYazi(g, "Başlamak için BOŞLUK tuşuna bas", 19, new PointF(W / 2f, 352), Color.White, null, null, 5, FontStyle.Bold, alfa);
    }

    private void OyunSonuEkrani(Graphics g, OyunMotoru motor, long bitistenBeri)
    {
        Karart(g, (int)Math.Min(130, bitistenBeri * 8));

        // Başlık yukarıdan kayarak geliyor
        float giris = Math.Min(1f, bitistenBeri / 20f);
        float baslikY = 150 - (1 - giris) * 80;
        Cizim.KenarliYazi(g, "GAME OVER", 56, new PointF(W / 2f, baslikY), Color.FromArgb(255, 120, 90), Color.FromArgb(220, 50, 60),
            Color.FromArgb(60, 20, 40), 9, FontStyle.Bold, (int)(255 * giris));

        if (bitistenBeri < 10)
            return;

        var kart = new RectangleF(60, 210, 360, 190);
        Cizim.Kart(g, kart);

        using var koyu = new SolidBrush(Color.FromArgb(110, 70, 50));
        using var vurgu = new SolidBrush(Color.FromArgb(236, 120, 40));
        using var buyuk = new Font("Segoe UI", 26, FontStyle.Bold);

        // Sol: madalya
        g.DrawString("MADALYA", kucukFont, koyu, new RectangleF(kart.X + 16, kart.Y + 16, 130, 20), Ortala);
        MadalyaCiz(g, motor.Madalya, new PointF(kart.X + 81, kart.Y + 92));
        g.DrawString(motor.Madalya == Madalya.Yok ? $"{MadalyaHesaplayici.BronzSiniri} puanda bronz" : MadalyaHesaplayici.Ad(motor.Madalya),
            kucukFont, koyu, new RectangleF(kart.X + 6, kart.Y + 140, 150, 22), Ortala);

        // Sağ: skor ve en iyi
        float sx = kart.X + 170;
        g.DrawString("SKOR", kucukFont, koyu, new RectangleF(sx, kart.Y + 16, 170, 20), Ortala);
        g.DrawString(motor.Skor.ToString(), buyuk, vurgu, new RectangleF(sx, kart.Y + 34, 170, 46), Ortala);
        g.DrawString("EN İYİ", kucukFont, koyu, new RectangleF(sx, kart.Y + 88, 170, 20), Ortala);
        g.DrawString(Math.Max(Rekor, motor.Skor).ToString(), buyuk, vurgu, new RectangleF(sx, kart.Y + 106, 170, 46), Ortala);

        if (YeniRekor)
        {
            var etiket = new RectangleF(sx + 112, kart.Y + 112, 52, 22);
            using var yol = Cizim.YuvarlakDikdortgen(etiket, 11);
            using var b = new SolidBrush(Color.FromArgb(236, 72, 110));
            g.FillPath(b, yol);
            g.DrawString("YENİ", kucukFont, Brushes.White, etiket, Ortala);
        }

        g.DrawString($"Seviye {motor.Seviye}  ·  {motor.Zorluk}", kucukFont, koyu, new RectangleF(sx, kart.Y + 156, 170, 20), Ortala);

        int alfa = (int)(215 + 40 * Math.Sin(bitistenBeri / 9.0));
        Cizim.KenarliYazi(g, "Tekrar oynamak için R veya Enter", 19, new PointF(W / 2f, 440), Color.White, null, null, 5, FontStyle.Bold, alfa);
    }

    private void MadalyaCiz(Graphics g, Madalya madalya, PointF merkez)
    {
        float r = 34;
        if (madalya == Madalya.Yok)
        {
            using var bos = new Pen(Color.FromArgb(120, 150, 110, 80), 3) { DashStyle = DashStyle.Dash };
            g.DrawEllipse(bos, merkez.X - r, merkez.Y - r, r * 2, r * 2);
            using var b = new SolidBrush(Color.FromArgb(150, 150, 110, 80));
            g.DrawString("?", soruFontu, b, new RectangleF(merkez.X - r, merkez.Y - r, r * 2, r * 2), Ortala);
            return;
        }

        (Color acik, Color koyu) = madalya switch
        {
            Madalya.Altin => (Color.FromArgb(255, 236, 130), Color.FromArgb(214, 158, 20)),
            Madalya.Gumus => (Color.FromArgb(245, 247, 252), Color.FromArgb(150, 160, 178)),
            _ => (Color.FromArgb(244, 180, 120), Color.FromArgb(170, 96, 40))
        };

        // Kurdele
        using (var kurdele = new SolidBrush(Color.FromArgb(220, 60, 90)))
        {
            g.FillPolygon(kurdele, new[] { new PointF(merkez.X - 22, merkez.Y - 56), new PointF(merkez.X - 6, merkez.Y - 56), new PointF(merkez.X + 4, merkez.Y - 20), new PointF(merkez.X - 12, merkez.Y - 20) });
            g.FillPolygon(kurdele, new[] { new PointF(merkez.X + 22, merkez.Y - 56), new PointF(merkez.X + 6, merkez.Y - 56), new PointF(merkez.X - 4, merkez.Y - 20), new PointF(merkez.X + 12, merkez.Y - 20) });
        }

        var daire = new RectangleF(merkez.X - r, merkez.Y - r, r * 2, r * 2);
        using (var dolgu = new LinearGradientBrush(daire, acik, koyu, 45f))
            g.FillEllipse(dolgu, daire);
        using (var kenar = new Pen(Color.FromArgb(120, 70, 40, 20), 3))
            g.DrawEllipse(kenar, daire);

        // Ortada yıldız
        var yildiz = new PointF[10];
        for (int i = 0; i < 10; i++)
        {
            double aci = -Math.PI / 2 + i * Math.PI / 5;
            float yr = i % 2 == 0 ? 18 : 8;
            yildiz[i] = new PointF(merkez.X + (float)Math.Cos(aci) * yr, merkez.Y + (float)Math.Sin(aci) * yr);
        }
        using (var yb = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            g.FillPolygon(yb, yildiz);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Motor = null;
            bilgiFontu.Dispose();
            kucukFont.Dispose();
            soruFontu.Dispose();
        }
        base.Dispose(disposing);
    }
}
