using FlappyBird.Oyun;
using FlappyBird.Veri;

namespace FlappyBird.Tests;

public class RekorDeposuTestleri : IDisposable
{
    private readonly string klasor;

    public RekorDeposuTestleri()
    {
        klasor = Path.Combine(Path.GetTempPath(), "flappy-test-" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(klasor))
            Directory.Delete(klasor, recursive: true);
    }

    [Fact]
    public void DosyaYoksa_RekorSifirdir()
    {
        var depo = new RekorDeposu(klasor);

        Assert.Equal(0, depo.RekorGetir(ZorlukSeviyesi.Normal));
        Assert.Null(depo.SonHata);
    }

    [Fact]
    public void YeniRekor_DosyayaYazilir_VeTekrarOkunur()
    {
        var depo = new RekorDeposu(klasor);

        bool yeniRekor = depo.YeniSkoruDene(ZorlukSeviyesi.Normal, 12);

        Assert.True(yeniRekor);
        Assert.True(File.Exists(depo.DosyaYolu));

        var yeniden = new RekorDeposu(klasor);
        Assert.Equal(12, yeniden.RekorGetir(ZorlukSeviyesi.Normal));
    }

    [Fact]
    public void DahaDusukSkor_RekoruDegistirmez()
    {
        var depo = new RekorDeposu(klasor);
        depo.YeniSkoruDene(ZorlukSeviyesi.Zor, 20);

        bool sonuc = depo.YeniSkoruDene(ZorlukSeviyesi.Zor, 15);

        Assert.False(sonuc);
        Assert.Equal(20, depo.RekorGetir(ZorlukSeviyesi.Zor));
    }

    [Fact]
    public void HerZorlugunRekoruAyriTutulur()
    {
        var depo = new RekorDeposu(klasor);

        depo.YeniSkoruDene(ZorlukSeviyesi.Kolay, 30);
        depo.YeniSkoruDene(ZorlukSeviyesi.Zor, 4);

        var yeniden = new RekorDeposu(klasor);
        Assert.Equal(30, yeniden.RekorGetir(ZorlukSeviyesi.Kolay));
        Assert.Equal(4, yeniden.RekorGetir(ZorlukSeviyesi.Zor));
        Assert.Equal(0, yeniden.RekorGetir(ZorlukSeviyesi.Normal));
    }

    [Fact]
    public void BozukSatirlar_AtlanirVeProgramCokmez()
    {
        Directory.CreateDirectory(klasor);
        File.WriteAllLines(Path.Combine(klasor, "rekorlar.txt"), new[]
        {
            "Normal=8",
            "bu satır bozuk",
            "Zor=abc",
            "Kolay=-5",
            "Bilinmeyen=3",
            ""
        });

        var depo = new RekorDeposu(klasor);

        Assert.Equal(8, depo.RekorGetir(ZorlukSeviyesi.Normal));
        Assert.Equal(0, depo.RekorGetir(ZorlukSeviyesi.Zor));
        Assert.Equal(0, depo.RekorGetir(ZorlukSeviyesi.Kolay));
    }

    [Fact]
    public void YazilamayanKlasor_HataMesajiVerir_AmaExceptionFirlatmaz()
    {
        // Klasör yerine aynı isimde bir dosya var -> klasör oluşturulamaz
        Directory.CreateDirectory(klasor);
        string engel = Path.Combine(klasor, "engel");
        File.WriteAllText(engel, "x");
        var depo = new RekorDeposu(Path.Combine(engel, "alt"));

        bool sonuc = depo.YeniSkoruDene(ZorlukSeviyesi.Normal, 5);

        Assert.True(sonuc);
        Assert.NotNull(depo.SonHata);
        Assert.Equal(5, depo.RekorGetir(ZorlukSeviyesi.Normal)); // bu oturumda yine geçerli
    }
}
