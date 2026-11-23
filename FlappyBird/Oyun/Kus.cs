namespace FlappyBird.Oyun;

/// <summary>
/// Oyuncunun kontrol ettiği kuş. Sadece dikey eksende hareket eder,
/// yatay hareket hissi boruların sola kaymasıyla veriliyor.
/// </summary>
public class Kus
{
    public const int Genislik = 34;
    public const int Yukseklik = 26;
    private const float MaksimumDususHizi = 11f;

    private readonly float baslangicY;

    public float X { get; }
    public float Y { get; private set; }
    public float DikeyHiz { get; private set; }

    public Kus(float x, float y)
    {
        X = x;
        Y = y;
        baslangicY = y;
    }

    public RectangleF Sinirlar => new(X, Y, Genislik, Yukseklik);

    // Kanat ucu boruya değince hemen ölmesin diye çarpışma kutusu biraz küçük tutuldu
    public RectangleF CarpismaKutusu => RectangleF.Inflate(Sinirlar, -4, -3);

    // Çizim sırasında kuşun burnunu hıza göre yukarı/aşağı çevirmek için
    public float Aci => Math.Clamp(DikeyHiz * 4f, -25f, 70f);

    public void KanatCirp(float ziplamaGucu)
    {
        DikeyHiz = -ziplamaGucu;
    }

    public void Guncelle(float yerCekimi)
    {
        DikeyHiz = Math.Min(DikeyHiz + yerCekimi, MaksimumDususHizi);
        Y += DikeyHiz;

        // Ekranın üstünden dışarı çıkmasın
        if (Y < 0)
        {
            Y = 0;
            DikeyHiz = 0;
        }
    }

    public void ZemineOturt(float zeminY)
    {
        Y = zeminY - Yukseklik;
        DikeyHiz = 0;
    }

    public void Sifirla()
    {
        Y = baslangicY;
        DikeyHiz = 0;
    }
}
