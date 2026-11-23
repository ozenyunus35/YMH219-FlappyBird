using System.ComponentModel;
using System.Drawing.Drawing2D;
using FlappyBird.Oyun;

namespace FlappyBird.Kontroller;

/// <summary>
/// Kolay / Normal / Zor için yan yana üç seçenekli seçici (ComboBox yerine).
/// Seçim değişince SecimDegisti olayı tetikleniyor.
/// </summary>
public class ZorlukSecici : Control
{
    private static readonly ZorlukSeviyesi[] Seviyeler = Enum.GetValues<ZorlukSeviyesi>();

    private ZorlukSeviyesi secili = ZorlukSeviyesi.Normal;
    private int uzerindeki = -1;
    private readonly Font font = new("Segoe UI", 8.25F, FontStyle.Bold);

    public event EventHandler? SecimDegisti;

    public ZorlukSecici()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        SetStyle(ControlStyles.Selectable, false);
        TabStop = false;
        Cursor = Cursors.Hand;
    }

    [Category("Davranış")]
    [DefaultValue(ZorlukSeviyesi.Normal)]
    public ZorlukSeviyesi Secili
    {
        get => secili;
        set
        {
            if (secili == value)
                return;
            secili = value;
            Invalidate();
            SecimDegisti?.Invoke(this, EventArgs.Empty);
        }
    }

    private RectangleF Bolum(int i)
    {
        float w = (Width - 4) / (float)Seviyeler.Length;
        return new RectangleF(2 + i * w, 2, w, Height - 4);
    }

    private int IndeksBul(Point p)
    {
        for (int i = 0; i < Seviyeler.Length; i++)
            if (Bolum(i).Contains(p))
                return i;
        return -1;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int i = IndeksBul(e.Location);
        if (i != uzerindeki) { uzerindeki = i; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        uzerindeki = -1;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        int i = IndeksBul(e.Location);
        if (i >= 0)
            Secili = Seviyeler[i];
        base.OnMouseDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Color.Black);

        using (var yol = Cizim.YuvarlakDikdortgen(new RectangleF(0, 0, Width - 1, Height - 1), (Height - 1) / 2f))
        using (var arka = new SolidBrush(Color.FromArgb(46, 38, 82)))
            g.FillPath(arka, yol);

        for (int i = 0; i < Seviyeler.Length; i++)
        {
            RectangleF r = Bolum(i);
            bool seciliMi = Seviyeler[i] == secili;

            if (seciliMi || i == uzerindeki)
            {
                using var yol = Cizim.YuvarlakDikdortgen(r, r.Height / 2);
                using var b = seciliMi
                    ? (Brush)new LinearGradientBrush(r, Color.FromArgb(255, 186, 100), Color.FromArgb(247, 130, 70), LinearGradientMode.Vertical)
                    : new SolidBrush(Color.FromArgb(70, 60, 120));
                g.FillPath(b, yol);
            }

            Color yazi = seciliMi ? Color.FromArgb(40, 24, 60) : Color.FromArgb(200, 196, 230);
            TextRenderer.DrawText(g, Seviyeler[i].ToString(), font, Rectangle.Round(r), yazi,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            font.Dispose();
        base.Dispose(disposing);
    }
}
