using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace FlappyBird.Kontroller;

public enum ButonIkonu
{
    Yok,
    Yenile,
    Duraklat,
    Oynat
}

/// <summary>
/// Hap şeklinde, ikonlu oyun butonu. Button'dan türetildiği için Click/Enabled aynen çalışıyor.
/// </summary>
public class OyunButonu : Button
{
    private ButonIkonu ikon = ButonIkonu.Yok;
    private bool vurgulu;
    private bool uzerinde;
    private bool basili;

    public OyunButonu()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        TabStop = false;
        Cursor = Cursors.Hand;
    }

    [Category("Görünüm")]
    [DefaultValue(ButonIkonu.Yok)]
    public ButonIkonu Ikon
    {
        get => ikon;
        set { ikon = value; Invalidate(); }
    }

    /// <summary>True ise turuncu (ana) buton, false ise koyu.</summary>
    [Category("Görünüm")]
    [DefaultValue(false)]
    public bool Vurgulu
    {
        get => vurgulu;
        set { vurgulu = value; Invalidate(); }
    }

    /// <summary>Sadece ikon gösterilsin mi (yazı tooltip/erişilebilirlik için kalıyor).</summary>
    [Category("Görünüm")]
    [DefaultValue(false)]
    public bool SadeceIkon { get; set; }

    protected override void OnMouseEnter(EventArgs e) { uzerinde = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { uzerinde = false; basili = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { basili = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { basili = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.Black);

        Color ust, alt, yazi;
        if (!Enabled)
        {
            ust = alt = Color.FromArgb(52, 46, 84);
            yazi = Color.FromArgb(110, 104, 150);
        }
        else if (vurgulu)
        {
            ust = Color.FromArgb(255, 190, 105);
            alt = Color.FromArgb(247, 128, 68);
            yazi = Color.FromArgb(40, 24, 60);
        }
        else
        {
            ust = Color.FromArgb(78, 68, 132);
            alt = Color.FromArgb(58, 50, 104);
            yazi = Color.White;
        }

        if (Enabled && basili) { ust = Koyulastir(ust); alt = Koyulastir(alt); }
        else if (Enabled && uzerinde) { ust = Acik(ust); alt = Acik(alt); }

        var r = new RectangleF(0, 0, Width - 1, Height - 1);
        using (var yol = Cizim.YuvarlakDikdortgen(r, r.Height / 2))
        using (var b = new LinearGradientBrush(r, ust, alt, LinearGradientMode.Vertical))
            g.FillPath(b, yol);

        float ikonBoyu = 10;
        bool ikonVar = ikon != ButonIkonu.Yok;

        if (SadeceIkon || string.IsNullOrEmpty(Text))
        {
            IkonCiz(g, new PointF(Width / 2f, Height / 2f), ikonBoyu, yazi);
            return;
        }

        Size yaziBoyu = TextRenderer.MeasureText(Text, Font);
        float toplam = yaziBoyu.Width + (ikonVar ? ikonBoyu + 6 : 0);
        float x = (Width - toplam) / 2f;

        if (ikonVar)
        {
            IkonCiz(g, new PointF(x + ikonBoyu / 2, Height / 2f), ikonBoyu, yazi);
            x += ikonBoyu + 6;
        }

        TextRenderer.DrawText(g, Text, Font, new Point((int)x, (Height - yaziBoyu.Height) / 2), yazi);
    }

    private void IkonCiz(Graphics g, PointF m, float s, Color renk)
    {
        using var firca = new SolidBrush(renk);
        using var kalem = new Pen(renk, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

        switch (ikon)
        {
            case ButonIkonu.Duraklat:
                g.FillRectangle(firca, m.X - s / 2, m.Y - s / 2, s / 3, s);
                g.FillRectangle(firca, m.X + s / 6, m.Y - s / 2, s / 3, s);
                break;

            case ButonIkonu.Oynat:
                g.FillPolygon(firca, new[] { new PointF(m.X - s / 2.5f, m.Y - s / 2), new PointF(m.X + s / 2, m.Y), new PointF(m.X - s / 2.5f, m.Y + s / 2) });
                break;

            case ButonIkonu.Yenile:
                var yay = new RectangleF(m.X - s / 2, m.Y - s / 2, s, s);
                g.DrawArc(kalem, yay, -60, 290);
                float aci = -60 * (float)Math.PI / 180;
                var uc = new PointF(m.X + (float)Math.Cos(aci) * s / 2, m.Y + (float)Math.Sin(aci) * s / 2);
                g.FillPolygon(firca, new[] { new PointF(uc.X - 4, uc.Y - 3), new PointF(uc.X + 3, uc.Y - 4), new PointF(uc.X + 1, uc.Y + 3) });
                break;
        }
    }

    private static Color Acik(Color c) => Color.FromArgb(c.A, Math.Min(255, c.R + 18), Math.Min(255, c.G + 18), Math.Min(255, c.B + 18));
    private static Color Koyulastir(Color c) => Color.FromArgb(c.A, (int)(c.R * 0.85), (int)(c.G * 0.85), (int)(c.B * 0.85));
}
