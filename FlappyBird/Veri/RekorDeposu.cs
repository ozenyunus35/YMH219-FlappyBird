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
            foreach (var (seviye, skor) in DosyadanOku())
                rekorlar[seviye] = skor;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SonHata = "Rekor dosyası okunamadı: " + ex.Message;
        }
    }

    /// <summary>Dosyadaki geçerli satırlar. Aynı seviye birden çok kez geçerse büyük olan alınır.</summary>
    private Dictionary<ZorlukSeviyesi, int> DosyadanOku()
    {
        var sonuc = new Dictionary<ZorlukSeviyesi, int>();
        if (!File.Exists(dosyaYolu))
            return sonuc;

        foreach (string satir in File.ReadAllLines(dosyaYolu))
        {
            string[] parcalar = satir.Split('=');
            if (parcalar.Length != 2)
                continue;

            // Dosya elle bozulduysa o satırı atla, program çökmesin.
            // "9=5" gibi sayısal ya da "Kolay, Zor" gibi birleşik değerler de geçersiz sayılıyor.
            string ad = parcalar[0].Trim();
            if (ad.Length > 0 && char.IsLetter(ad[0]) && !ad.Contains(',') &&
                Enum.TryParse(ad, ignoreCase: true, out ZorlukSeviyesi seviye) && Enum.IsDefined(seviye) &&
                int.TryParse(parcalar[1].Trim(), System.Globalization.NumberStyles.Integer,
                             System.Globalization.CultureInfo.InvariantCulture, out int skor) && skor >= 0)
            {
                sonuc[seviye] = Math.Max(skor, sonuc.GetValueOrDefault(seviye));
            }
        }
        return sonuc;
    }

    private void Kaydet()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dosyaYolu)!);

            // Aynı anda açık başka bir oyun penceresi de rekor yazmış olabilir:
            // dosyadakiyle birleştirip her seviye için büyük olanı yazıyoruz.
            foreach (var (seviye, skor) in DosyadanOku())
                if (skor > RekorGetir(seviye))
                    rekorlar[seviye] = skor;

            // Önce geçici dosyaya yazılıyor; yazarken bir şey olursa eski rekorlar kaybolmasın
            string gecici = dosyaYolu + ".tmp";
            File.WriteAllLines(gecici, rekorlar.OrderBy(r => r.Key).Select(r => $"{r.Key}={r.Value}"));
            File.Move(gecici, dosyaYolu, overwrite: true);

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
