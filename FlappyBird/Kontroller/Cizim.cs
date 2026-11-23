using System.Drawing.Drawing2D;

namespace FlappyBird.Kontroller;

/// <summary>Birden fazla yerde kullanılan çizim yardımcıları.</summary>
internal static class Cizim
{
    public static GraphicsPath YuvarlakDikdortgen(RectangleF r, float yaricap)
    {
        float d = Math.Min(yaricap * 2, Math.Min(r.Width, r.Height));
        var yol = new GraphicsPath();
        yol.AddArc(r.X, r.Y, d, d, 180, 90);
        yol.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        yol.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        yol.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        yol.CloseFigure();
        return yol;
    }

    /// <summary>
    /// Kalın kenarlı "oyun yazısı". Yazı GraphicsPath'e çevrilip önce kalın kenar,
    /// sonra içi (gerekirse gradyanla) dolduruluyor.
    /// </summary>
    public static void KenarliYazi(Graphics g, string yazi, float boyut, PointF merkez,
                                   Color ic, Color? ic2 = null, Color? kenar = null, float kenarKalinligi = 6,
                                   FontStyle stil = FontStyle.Bold, int alfa = 255)
    {
        using var aile = new FontFamily("Segoe UI");
        using var yol = new GraphicsPath();
        using var bicim = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        yol.AddString(yazi, aile, (int)stil, boyut, merkez, bicim);

        Color kenarRengi = kenar ?? Color.FromArgb(40, 24, 60);
        using (var kalem = new Pen(Color.FromArgb(alfa, kenarRengi), kenarKalinligi) { LineJoin = LineJoin.Round })
            g.DrawPath(kalem, yol);

        RectangleF sinir = yol.GetBounds();
        if (ic2 is Color ikinci && sinir.Height > 0)
        {
            using var firca = new LinearGradientBrush(new RectangleF(sinir.X, sinir.Y - 1, sinir.Width, sinir.Height + 2),
                Color.FromArgb(alfa, ic), Color.FromArgb(alfa, ikinci), LinearGradientMode.Vertical);
            g.FillPath(firca, yol);
        }
        else
        {
            using var firca = new SolidBrush(Color.FromArgb(alfa, ic));
            g.FillPath(firca, yol);
        }
    }

    public static void Kart(Graphics g, RectangleF alan, float yaricap = 18)
    {
        using var yol = YuvarlakDikdortgen(alan, yaricap);
        using var golgeYol = YuvarlakDikdortgen(new RectangleF(alan.X, alan.Y + 5, alan.Width, alan.Height), yaricap);
        using var golge = new SolidBrush(Color.FromArgb(70, 20, 10, 30));
        using var arka = new LinearGradientBrush(alan, Color.FromArgb(255, 251, 240), Color.FromArgb(255, 236, 205), LinearGradientMode.Vertical);
        using var kenar = new Pen(Color.FromArgb(120, 70, 40), 3);
        g.FillPath(golge, golgeYol);
        g.FillPath(arka, yol);
        g.DrawPath(kenar, yol);
    }

    /// <summary>Klavye tuşu görünümünde küçük kutu (ipuçları için).</summary>
    public static void Tus(Graphics g, string yazi, RectangleF alan, Font font)
    {
        using var yol = YuvarlakDikdortgen(alan, 5);
        using var alt = YuvarlakDikdortgen(new RectangleF(alan.X, alan.Y + 3, alan.Width, alan.Height), 5);
        using var golge = new SolidBrush(Color.FromArgb(150, 30, 20, 50));
        using var yuz = new SolidBrush(Color.FromArgb(245, 245, 250));
        g.FillPath(golge, alt);
        g.FillPath(yuz, yol);
        // TextRenderer ScaleTransform'u dikkate almadığı için burada DrawString kullanılıyor
        using var bicim = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var yaziFircasi = new SolidBrush(Color.FromArgb(40, 30, 60));
        g.DrawString(yazi, font, yaziFircasi, alan, bicim);
    }
}
