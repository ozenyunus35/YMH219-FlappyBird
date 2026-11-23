using System.ComponentModel;
using System.Drawing.Drawing2D;
using FlappyBird.Oyun;

namespace FlappyBird.Kontroller;

/// <summary>
/// Oyunun çizildiği alan. PictureBox kullanmak yerine her şeyi OnPaint içinde
/// GDI+ ile çiziyorum, böylece projede hiç resim dosyası gerekmiyor.
/// </summary>
public class OyunAlani : Control
{
    // Gün batımı renkleri
    private static readonly Color GokyuzuUst = Color.FromArgb(44, 32, 84);
    private static readonly Color GokyuzuAlt = Color.FromArgb(247, 146, 86);
    private static readonly Color BoruRengi = Color.FromArgb(31, 122, 107);
    private static readonly Color BoruKenar = Color.FromArgb(18, 74, 66);
    private static readonly Color ZeminRengi = Color.FromArgb(222, 196, 140);
    private static readonly Color CimenRengi = Color.FromArgb(96, 160, 72);

    private static readonly StringFormat ortala = new() { Alignment = StringAlignment.Center };

    private readonly Font skorFontu = new("Segoe UI", 40, FontStyle.Bold);
    private readonly Font baslikFontu = new("Segoe UI", 26, FontStyle.Bold);
    private readonly Font bilgiFontu = new("Segoe UI", 12, FontStyle.Regular);
    private readonly Font geriSayimFontu = new("Segoe UI", 72, FontStyle.Bold);

    // Bulutların yeri sabit, sadece kayma miktarı değişiyor
    private readonly PointF[] bulutlar =
    {
        new(40, 90), new(210, 150), new(360, 70), new(520, 130)
    };

    public OyunAlani()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.Selectable, true);
        TabStop = true;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public OyunMotoru? Motor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Rekor { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool YeniRekor { get; set; }

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
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Oyun 480x640 mantıksal alanda çalışıyor, kontrol boyutuna ölçekliyoruz
        g.ScaleTransform(Width / (float)OyunMotoru.AlanGenisligi,
                         Height / (float)OyunMotoru.AlanYuksekligi);

        GokyuzunuCiz(g);

        if (Motor is null)
            return;

        BulutlariCiz(g, Motor.TickSayisi);

        foreach (var boru in Motor.Borular)
            BoruyuCiz(g, boru, Motor.ZeminY);

        ZeminiCiz(g, Motor);
        KusuCiz(g, Motor);
        UstBilgiyiCiz(g, Motor);
    }

    private static void GokyuzunuCiz(Graphics g)
    {
        var alan = new Rectangle(0, 0, OyunMotoru.AlanGenisligi, OyunMotoru.AlanYuksekligi);
        using var firca = new LinearGradientBrush(alan, GokyuzuUst, GokyuzuAlt, LinearGradientMode.Vertical);
        g.FillRectangle(firca, alan);

        // Güneş
        using var gunes = new SolidBrush(Color.FromArgb(120, 255, 214, 120));
        g.FillEllipse(gunes, 300, 380, 150, 150);
    }

    private void BulutlariCiz(Graphics g, long tick)
    {
        using var firca = new SolidBrush(Color.FromArgb(70, 255, 255, 255));
        float kayma = tick * 0.4f;
        int genislik = OyunMotoru.AlanGenisligi + 160;

        foreach (var bulut in bulutlar)
        {
            // Bulutlar borulardan yavaş kayıyor (paralaks etkisi)
            float x = ((bulut.X - kayma) % genislik + genislik) % genislik - 120;
            g.FillEllipse(firca, x, bulut.Y, 90, 34);
            g.FillEllipse(firca, x + 25, bulut.Y - 16, 60, 40);
            g.FillEllipse(firca, x + 50, bulut.Y - 4, 70, 32);
        }
    }

    private static void BoruyuCiz(Graphics g, Boru boru, float zeminY)
    {
        using var govde = new SolidBrush(BoruRengi);
        using var kenar = new Pen(BoruKenar, 3);
        using var parlama = new SolidBrush(Color.FromArgb(60, 255, 255, 255));

        foreach (var parca in new[] { boru.UstParca, boru.AltParca(zeminY) })
        {
            if (parca.Height <= 0)
                continue;

            g.FillRectangle(govde, parca);
            g.FillRectangle(parlama, parca.X + 8, parca.Y, 8, parca.Height);
            g.DrawRectangle(kenar, parca.X, parca.Y, parca.Width, parca.Height);
        }

        // Boşluğun kenarlarındaki "ağız" kısımları
        const int agizYuksekligi = 22;
        var ustAgiz = new RectangleF(boru.X - 5, boru.BoslukY - agizYuksekligi, Boru.Genislik + 10, agizYuksekligi);
        var altAgiz = new RectangleF(boru.X - 5, boru.BoslukY + boru.BoslukBoyu, Boru.Genislik + 10, agizYuksekligi);

        foreach (var agiz in new[] { ustAgiz, altAgiz })
        {
            g.FillRectangle(govde, agiz);
            g.DrawRectangle(kenar, agiz.X, agiz.Y, agiz.Width, agiz.Height);
        }
    }

    private static void ZeminiCiz(Graphics g, OyunMotoru motor)
    {
        float y = motor.ZeminY;
        using var zemin = new SolidBrush(ZeminRengi);
        using var cimen = new SolidBrush(CimenRengi);
        using var cizgi = new Pen(Color.FromArgb(70, 120, 90, 40), 3);

        g.FillRectangle(zemin, 0, y, OyunMotoru.AlanGenisligi, OyunMotoru.ZeminYuksekligi);
        g.FillRectangle(cimen, 0, y, OyunMotoru.AlanGenisligi, 12);

        // Oyun bitince zemin de dursun
        float kayma = motor.Durum == OyunDurumu.Bitti ? 0 : motor.TickSayisi * motor.Hiz % 24;
        for (float x = -kayma; x < OyunMotoru.AlanGenisligi; x += 24)
            g.DrawLine(cizgi, x, y + 20, x + 12, y + 32);
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
        g.RotateTransform(motor.Durum == OyunDurumu.Hazir ? 0 : kus.Aci);

        float w = Kus.Genislik, h = Kus.Yukseklik;
        using var govde = new SolidBrush(Color.FromArgb(255, 200, 40));
        using var karin = new SolidBrush(Color.FromArgb(255, 236, 160));
        using var kanat = new SolidBrush(Color.FromArgb(240, 150, 30));
        using var gaga = new SolidBrush(Color.FromArgb(235, 90, 50));
        using var kontur = new Pen(Color.FromArgb(80, 50, 20), 2);

        g.FillEllipse(govde, -w / 2, -h / 2, w, h);
        g.FillEllipse(karin, -w / 2 + 6, 0, w - 14, h / 2 - 2);
        g.DrawEllipse(kontur, -w / 2, -h / 2, w, h);

        // Kanat her 6 tick'te bir yukarı/aşağı
        bool kanatYukarida = motor.Durum != OyunDurumu.Bitti && motor.TickSayisi / 6 % 2 == 0;
        float kanatY = kanatYukarida ? -h / 2 + 2 : -2;
        g.FillEllipse(kanat, -w / 2 + 2, kanatY, 16, 11);
        g.DrawEllipse(kontur, -w / 2 + 2, kanatY, 16, 11);

        // Göz
        g.FillEllipse(Brushes.White, w / 2 - 14, -h / 2 + 3, 11, 11);
        g.FillEllipse(Brushes.Black, w / 2 - 8, -h / 2 + 6, 4, 5);

        // Gaga
        PointF[] gagaNoktalari = { new(w / 2 - 4, -1), new(w / 2 + 9, 3), new(w / 2 - 4, 8) };
        g.FillPolygon(gaga, gagaNoktalari);

        g.Restore(onceki);
    }

    private void UstBilgiyiCiz(Graphics g, OyunMotoru motor)
    {
        if (motor.Durum is OyunDurumu.Oynaniyor or OyunDurumu.Duraklatildi or OyunDurumu.GeriSayim)
            GolgeliYazi(g, motor.Skor.ToString(), skorFontu, new RectangleF(0, 30, OyunMotoru.AlanGenisligi, 70), ortala);

        switch (motor.Durum)
        {
            case OyunDurumu.Hazir:
                Panel(g, new RectangleF(70, 150, 340, 130));
                GolgeliYazi(g, "HAZIR MISIN?", baslikFontu, new RectangleF(70, 165, 340, 50), ortala);
                GolgeliYazi(g, "Boşluk / ↑ / fare tıkı: zıpla\nP: duraklat   R: yeni oyun   M: ses",
                            bilgiFontu, new RectangleF(70, 215, 340, 60), ortala);
                break;

            case OyunDurumu.Duraklatildi:
                Panel(g, new RectangleF(70, 250, 340, 95));
                GolgeliYazi(g, "DURAKLATILDI", baslikFontu, new RectangleF(70, 262, 340, 45), ortala);
                GolgeliYazi(g, "Devam etmek için P", bilgiFontu, new RectangleF(70, 307, 340, 30), ortala);
                break;

            case OyunDurumu.GeriSayim:
                GolgeliYazi(g, motor.GeriSayimSaniye.ToString(), geriSayimFontu,
                            new RectangleF(0, 220, OyunMotoru.AlanGenisligi, 130), ortala);
                GolgeliYazi(g, "Hazırlan!", bilgiFontu, new RectangleF(0, 350, OyunMotoru.AlanGenisligi, 30), ortala);
                break;

            case OyunDurumu.Bitti:
                Panel(g, new RectangleF(60, 160, 360, 230));
                GolgeliYazi(g, "GAME OVER", baslikFontu, new RectangleF(60, 175, 360, 50), ortala);
                if (YeniRekor)
                    GolgeliYazi(g, "YENİ REKOR!", bilgiFontu, new RectangleF(60, 225, 360, 25), ortala);
                string sonuc = $"Skor: {motor.Skor}      Rekor: {Math.Max(Rekor, motor.Skor)}\n" +
                               $"Seviye: {motor.Seviye}   Zorluk: {motor.Zorluk}";
                GolgeliYazi(g, sonuc, bilgiFontu, new RectangleF(60, 258, 360, 50), ortala);
                MadalyaCiz(g, motor.Madalya, new PointF(240, 318));
                GolgeliYazi(g, "Yeniden oynamak için R veya Enter", bilgiFontu, new RectangleF(60, 340, 360, 30), ortala);
                break;
        }
    }

    private void MadalyaCiz(Graphics g, Madalya madalya, PointF merkez)
    {
        Color renk = madalya switch
        {
            Madalya.Bronz => Color.FromArgb(205, 127, 50),
            Madalya.Gumus => Color.FromArgb(200, 205, 215),
            Madalya.Altin => Color.FromArgb(255, 205, 60),
            _ => Color.FromArgb(90, 255, 255, 255)
        };

        string yazi = madalya == Madalya.Yok
            ? $"Madalya için {MadalyaHesaplayici.BronzSiniri} puan gerekli"
            : $"{MadalyaHesaplayici.Ad(madalya)} madalya";

        SizeF boyut = g.MeasureString(yazi, bilgiFontu);
        float toplam = 24 + 8 + boyut.Width;
        float x = merkez.X - toplam / 2;

        using var firca = new SolidBrush(renk);
        using var kenar = new Pen(Color.FromArgb(120, 0, 0, 0), 2);
        g.FillEllipse(firca, x, merkez.Y - 12, 24, 24);
        g.DrawEllipse(kenar, x, merkez.Y - 12, 24, 24);
        GolgeliYazi(g, yazi, bilgiFontu, new RectangleF(x + 32, merkez.Y - boyut.Height / 2, boyut.Width + 4, boyut.Height), StringFormat.GenericDefault);
    }

    private static void Panel(Graphics g, RectangleF alan)
    {
        using var arkaPlan = new SolidBrush(Color.FromArgb(150, 20, 16, 40));
        using var kenar = new Pen(Color.FromArgb(200, 255, 220, 150), 2);
        using GraphicsPath yol = YuvarlakDikdortgen(alan, 16);
        g.FillPath(arkaPlan, yol);
        g.DrawPath(kenar, yol);
    }

    private static void GolgeliYazi(Graphics g, string yazi, Font font, RectangleF alan, StringFormat bicim)
    {
        var golgeAlani = alan;
        golgeAlani.Offset(2, 2);
        using var golge = new SolidBrush(Color.FromArgb(140, 0, 0, 0));
        g.DrawString(yazi, font, golge, golgeAlani, bicim);
        g.DrawString(yazi, font, Brushes.White, alan, bicim);
    }

    private static GraphicsPath YuvarlakDikdortgen(RectangleF r, float yaricap)
    {
        float d = yaricap * 2;
        var yol = new GraphicsPath();
        yol.AddArc(r.X, r.Y, d, d, 180, 90);
        yol.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        yol.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        yol.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        yol.CloseFigure();
        return yol;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            skorFontu.Dispose();
            baslikFontu.Dispose();
            bilgiFontu.Dispose();
            geriSayimFontu.Dispose();
        }
        base.Dispose(disposing);
    }
}
