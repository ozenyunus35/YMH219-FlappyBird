namespace FlappyBird.Oyun;

/// <summary>
/// Üst ve alt parçadan oluşan bir boru çifti. Aradaki boşluktan kuş geçiyor.
/// </summary>
public class Boru
{
    public const int Genislik = 64;

    public float X { get; private set; }
    public float BoslukY { get; private set; }   // boşluğun üst kenarı
    public float BoslukBoyu { get; }
    public bool Gecildi { get; set; }

    public Boru(float x, float boslukY, float boslukBoyu)
    {
        X = x;
        BoslukY = boslukY;
        BoslukBoyu = boslukBoyu;
    }

    public RectangleF UstParca => new(X, 0, Genislik, BoslukY);

    public RectangleF AltParca(float zeminY)
    {
        float altBaslangic = BoslukY + BoslukBoyu;
        return new RectangleF(X, altBaslangic, Genislik, zeminY - altBaslangic);
    }

    public bool EkrandanCikti => X + Genislik < 0;

    public void Kaydir(float hiz)
    {
        X -= hiz;
    }

    /// <summary>
    /// Ekranın solundan çıkan boruyu silmek yerine sağa geri taşıyoruz.
    /// </summary>
    public void YenidenKonumla(float yeniX, float yeniBoslukY)
    {
        X = yeniX;
        BoslukY = yeniBoslukY;
        Gecildi = false;
    }
}
