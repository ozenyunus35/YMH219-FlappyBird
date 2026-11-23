using FlappyBird.Oyun;

namespace FlappyBird.Veri;

/// <summary>
/// Her zorluk seviyesi için en yüksek skoru bir metin dosyasında tutar.
/// Dosya biçimi basit: her satırda "Seviye=Skor" (ör. Normal=14)
/// </summary>
public class RekorDeposu
{
    private readonly string dosyaYolu;
    private readonly Dictionary<ZorlukSeviyesi, int> rekorlar = new();

    public RekorDeposu() : this(VarsayilanKlasor()) { }

    public RekorDeposu(string klasor)
    {
        dosyaYolu = Path.Combine(klasor, "rekorlar.txt");
        Yukle();
    }

    /// <summary>Son okuma/yazma hatası, yoksa null.</summary>
    public string? SonHata { get; private set; }

    public string DosyaYolu => dosyaYolu;

    public int RekorGetir(ZorlukSeviyesi seviye)
    {
        return rekorlar.TryGetValue(seviye, out int rekor) ? rekor : 0;
    }

    /// <summary>
    /// Skor rekoru geçiyorsa kaydeder ve true döner.
    /// </summary>
    public bool YeniSkoruDene(ZorlukSeviyesi seviye, int skor)
    {
        if (skor <= RekorGetir(seviye))
            return false;

        rekorlar[seviye] = skor;
        Kaydet();
        return true;
    }

    private void Yukle()
    {
        try
        {
            if (!File.Exists(dosyaYolu))
                return;

            foreach (string satir in File.ReadAllLines(dosyaYolu))
            {
                string[] parcalar = satir.Split('=');
                if (parcalar.Length != 2)
                    continue;

                // Dosya elle bozulduysa o satırı atla, program çökmesin
                if (Enum.TryParse(parcalar[0].Trim(), out ZorlukSeviyesi seviye) &&
                    int.TryParse(parcalar[1].Trim(), out int skor) && skor >= 0)
                {
                    rekorlar[seviye] = skor;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SonHata = "Rekor dosyası okunamadı: " + ex.Message;
        }
    }

    private void Kaydet()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dosyaYolu)!);

            using var yazici = new StreamWriter(dosyaYolu, append: false);
            foreach (var (seviye, skor) in rekorlar)
                yazici.WriteLine($"{seviye}={skor}");

            SonHata = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SonHata = "Rekor kaydedilemedi: " + ex.Message;
        }
    }

    private static string VarsayilanKlasor()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "YMH219-FlappyBird");
    }
}
