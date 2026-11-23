using FlappyBird.Oyun;

namespace FlappyBird.Tests;

public class KusTestleri
{
    [Fact]
    public void KanatCirpinca_DikeyHiz_YukariDogruOlur()
    {
        var kus = new Kus(100, 300);

        kus.KanatCirp(7.5f);

        Assert.Equal(-7.5f, kus.DikeyHiz);
    }

    [Fact]
    public void YerCekimi_HerKaredeHiziArtirir_VeKusAsagiIner()
    {
        var kus = new Kus(100, 300);

        kus.Guncelle(0.5f);
        kus.Guncelle(0.5f);

        Assert.Equal(1.0f, kus.DikeyHiz, 3);
        Assert.True(kus.Y > 300);
    }

    [Fact]
    public void DususHizi_SinirinUstuneCikmaz()
    {
        var kus = new Kus(100, 0);

        for (int i = 0; i < 200; i++)
            kus.Guncelle(1f);

        Assert.True(kus.DikeyHiz <= 11f);
    }

    [Fact]
    public void Kus_EkranUstundenDisariCikmaz()
    {
        var kus = new Kus(100, 5);
        kus.KanatCirp(20);

        kus.Guncelle(0.1f);

        Assert.Equal(0, kus.Y);
        Assert.Equal(0, kus.DikeyHiz);
    }

    [Fact]
    public void Sifirla_BaslangicKonumunaDondurur()
    {
        var kus = new Kus(100, 250);
        kus.KanatCirp(7);
        kus.Guncelle(0.4f);

        kus.Sifirla();

        Assert.Equal(250, kus.Y);
        Assert.Equal(0, kus.DikeyHiz);
    }

    [Fact]
    public void CarpismaKutusu_GorunenKuştanKucuktur()
    {
        var kus = new Kus(100, 200);

        Assert.True(kus.Sinirlar.Contains(kus.CarpismaKutusu));
        Assert.True(kus.CarpismaKutusu.Width < kus.Sinirlar.Width);
    }
}

public class BoruTestleri
{
    [Fact]
    public void Kaydir_BoruyuSolaTasir()
    {
        var boru = new Boru(300, 150, 140);

        boru.Kaydir(4);

        Assert.Equal(296, boru.X);
    }

    [Fact]
    public void EkrandanCikti_BoruTamamenSoldaykenTrueOlur()
    {
        var boru = new Boru(-Boru.Genislik + 1, 150, 140);
        Assert.False(boru.EkrandanCikti);

        boru.Kaydir(2);
        Assert.True(boru.EkrandanCikti);
    }

    [Fact]
    public void YenidenKonumla_GecildiBilgisiniSifirlar()
    {
        var boru = new Boru(10, 150, 140) { Gecildi = true };

        boru.YenidenKonumla(600, 200);

        Assert.Equal(600, boru.X);
        Assert.Equal(200, boru.BoslukY);
        Assert.False(boru.Gecildi);
    }

    [Fact]
    public void UstVeAltParca_BoslugunIkiTarafindadir()
    {
        var boru = new Boru(100, 150, 140);

        RectangleF ust = boru.UstParca;
        RectangleF alt = boru.AltParca(560);

        Assert.Equal(150, ust.Bottom);
        Assert.Equal(290, alt.Top);
        Assert.Equal(560, alt.Bottom);
    }
}
