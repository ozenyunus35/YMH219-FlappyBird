using FlappyBird.Oyun;

namespace FlappyBird.Tests;

public class OyunMotoruTestleri
{
    private const int Tohum = 42;

    /// <summary>
    /// Kuşu bir sonraki boşluğun ortasına doğru yönlendiren basit otomatik pilot.
    /// Skorla ilgili testlerde oyunu "oynatmak" için kullanılıyor.
    /// </summary>
    private static void OtomatikOyna(OyunMotoru motor, int hedefSkor, int maksimumTick = 60 * 120)
    {
        motor.KanatCirp();
        for (int i = 0; i < maksimumTick && motor.Skor < hedefSkor && motor.Durum == OyunDurumu.Oynaniyor; i++)
        {
            Boru hedef = motor.Borular.Where(b => b.X + Boru.Genislik > motor.Kus.X).MinBy(b => b.X)!;
            float merkez = hedef.BoslukY + hedef.BoslukBoyu * 0.62f;
            if (motor.Kus.Y + Kus.Yukseklik / 2f > merkez && motor.Kus.DikeyHiz > -1)
                motor.KanatCirp();
            motor.Ilerle();
        }
    }

    [Fact]
    public void YeniOyun_HazirDurumundaVeSifirSkorlaBaslar()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);

        Assert.Equal(OyunDurumu.Hazir, motor.Durum);
        Assert.Equal(0, motor.Skor);
        Assert.Equal(3, motor.Borular.Count);
        Assert.All(motor.Borular, b => Assert.True(b.X > OyunMotoru.AlanGenisligi));
    }

    [Fact]
    public void HazirDurumunda_KusVeBorularHareketEtmez()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);
        float kusY = motor.Kus.Y;
        float boruX = motor.Borular[0].X;

        for (int i = 0; i < 30; i++)
            motor.Ilerle();

        Assert.Equal(kusY, motor.Kus.Y);
        Assert.Equal(boruX, motor.Borular[0].X);
    }

    [Fact]
    public void IlkKanatCirpma_OyunuBaslatir()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);

        motor.KanatCirp();

        Assert.Equal(OyunDurumu.Oynaniyor, motor.Durum);
        Assert.True(motor.Kus.DikeyHiz < 0);
    }

    [Fact]
    public void Borular_SagdanSolaHareketEder()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);
        float onceki = motor.Borular[0].X;
        motor.KanatCirp();

        motor.Ilerle();

        Assert.Equal(onceki - motor.Hiz, motor.Borular[0].X, 3);
    }

    [Fact]
    public void HicZiplamazsa_ZemineCarpar_VeOyunBitti_BirKezTetiklenir()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);
        int bittiSayisi = 0;
        motor.OyunBitti += (_, _) => bittiSayisi++;

        motor.KanatCirp();
        for (int i = 0; i < 500; i++)
            motor.Ilerle();

        Assert.Equal(OyunDurumu.Bitti, motor.Durum);
        Assert.Equal(1, bittiSayisi);
        Assert.Equal(motor.ZeminY - Kus.Yukseklik, motor.Kus.Y);
    }

    [Fact]
    public void OyunBittiktenSonra_KanatCirpmaOyunuTekrarBaslatmaz()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);
        motor.KanatCirp();
        for (int i = 0; i < 500; i++)
            motor.Ilerle();

        motor.KanatCirp();

        Assert.Equal(OyunDurumu.Bitti, motor.Durum);
    }

    [Fact]
    public void BorudanGecince_SkorArtar_VeEventGelir()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Kolay, Tohum);
        int skorEventi = 0;
        motor.SkorDegisti += (_, _) => skorEventi++;

        OtomatikOyna(motor, hedefSkor: 3);

        Assert.True(motor.Skor >= 3, $"Skor: {motor.Skor}");
        Assert.True(skorEventi >= 3);
    }

    [Fact]
    public void HerBesPuanda_HizArtar()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Kolay, Tohum);
        float baslangicHizi = motor.Hiz;

        OtomatikOyna(motor, hedefSkor: 5);

        Assert.True(motor.Skor >= 5, $"Skor: {motor.Skor}");
        Assert.True(motor.Hiz > baslangicHizi);
        Assert.Equal(2, motor.Seviye);
    }

    [Fact]
    public void EkrandanCikanBoru_SagaGeriKonumlanir_VeBoslukFarkiSinirli()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Kolay, Tohum);
        motor.KanatCirp();
        int yenidenKonumlanan = 0;

        for (int i = 0; i < 2000 && motor.Durum == OyunDurumu.Oynaniyor; i++)
        {
            float[] oncekiX = motor.Borular.Select(b => b.X).ToArray();
            Boru hedef = motor.Borular.Where(b => b.X + Boru.Genislik > motor.Kus.X).MinBy(b => b.X)!;
            if (motor.Kus.Y + Kus.Yukseklik / 2f > hedef.BoslukY + hedef.BoslukBoyu * 0.62f && motor.Kus.DikeyHiz > -1)
                motor.KanatCirp();
            motor.Ilerle();

            for (int k = 0; k < motor.Borular.Count; k++)
                if (motor.Borular[k].X > oncekiX[k])
                    yenidenKonumlanan++;
        }

        Assert.True(yenidenKonumlanan > 0, "Hiç boru yeniden konumlanmadı");
        Assert.Equal(3, motor.Borular.Count);

        // Boru dizilimi soldan sağa sıralanınca komşu boşluklar arasındaki fark 150'yi geçmemeli
        var sirali = motor.Borular.OrderBy(b => b.X).ToList();
        for (int k = 1; k < sirali.Count; k++)
            Assert.InRange(Math.Abs(sirali[k].BoslukY - sirali[k - 1].BoslukY), 0, 150);
    }

    [Fact]
    public void Duraklatinca_HicBirSeyHareketEtmez()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);
        motor.KanatCirp();
        motor.Ilerle();

        motor.DuraklatVeyaDevamEt();
        float kusY = motor.Kus.Y;
        float boruX = motor.Borular[0].X;
        for (int i = 0; i < 100; i++)
            motor.Ilerle();

        Assert.Equal(OyunDurumu.Duraklatildi, motor.Durum);
        Assert.Equal(kusY, motor.Kus.Y);
        Assert.Equal(boruX, motor.Borular[0].X);
    }

    [Fact]
    public void DevamEdince_OnceGeriSayimYapar_SonraOyunDevamEder()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);
        motor.KanatCirp();
        motor.DuraklatVeyaDevamEt();

        motor.DuraklatVeyaDevamEt();
        Assert.Equal(OyunDurumu.GeriSayim, motor.Durum);
        Assert.Equal(3, motor.GeriSayimSaniye);

        float kusY = motor.Kus.Y;
        for (int i = 0; i < OyunMotoru.GeriSayimTickSayisi - 1; i++)
            motor.Ilerle();
        Assert.Equal(OyunDurumu.GeriSayim, motor.Durum);
        Assert.Equal(kusY, motor.Kus.Y);   // geri sayımda kuş düşmüyor

        motor.Ilerle();
        Assert.Equal(OyunDurumu.Oynaniyor, motor.Durum);
    }

    [Fact]
    public void GeriSayimSirasinda_KanatCirpmaEtkisizdir()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);
        motor.KanatCirp();
        motor.DuraklatVeyaDevamEt();
        motor.DuraklatVeyaDevamEt();

        motor.KanatCirp();

        Assert.Equal(OyunDurumu.GeriSayim, motor.Durum);
    }

    [Fact]
    public void Duraklat_HazirVeBittiDurumundaBirSeyYapmaz()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);

        motor.Duraklat();

        Assert.Equal(OyunDurumu.Hazir, motor.Durum);
    }

    [Fact]
    public void YeniOyun_HerSeyiSifirlar_VeZorlugaGoreHizVerir()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Kolay, Tohum);
        OtomatikOyna(motor, hedefSkor: 2);

        motor.YeniOyun(ZorlukSeviyesi.Zor);

        Assert.Equal(0, motor.Skor);
        Assert.Equal(OyunDurumu.Hazir, motor.Durum);
        Assert.Equal(ZorlukSeviyesi.Zor, motor.Zorluk);
        Assert.Equal(ZorlukAyari.Getir(ZorlukSeviyesi.Zor).BaslangicHizi, motor.Hiz);
    }

    [Fact]
    public void AlinanYol_OyundaHizKadarArtar_BitinceDurur()
    {
        var motor = new OyunMotoru(ZorlukSeviyesi.Normal, Tohum);
        motor.KanatCirp();
        float once = motor.AlinanYol;

        motor.Ilerle();
        Assert.Equal(once + motor.Hiz, motor.AlinanYol, 3);

        for (int i = 0; i < 500; i++)
            motor.Ilerle();
        float bitince = motor.AlinanYol;
        for (int i = 0; i < 50; i++)
            motor.Ilerle();

        Assert.Equal(OyunDurumu.Bitti, motor.Durum);
        Assert.Equal(bitince, motor.AlinanYol);
    }

    [Fact]
    public void AyniTohum_AyniBoruDiziliminiUretir()
    {
        var a = new OyunMotoru(ZorlukSeviyesi.Normal, 7);
        var b = new OyunMotoru(ZorlukSeviyesi.Normal, 7);

        Assert.Equal(a.Borular.Select(x => x.BoslukY), b.Borular.Select(x => x.BoslukY));
    }
}

public class ZorlukVeMadalyaTestleri
{
    [Fact]
    public void ZorSeviyede_BoslukDahaDar_HizDahaYuksek()
    {
        var kolay = ZorlukAyari.Getir(ZorlukSeviyesi.Kolay);
        var zor = ZorlukAyari.Getir(ZorlukSeviyesi.Zor);

        Assert.True(zor.BoslukBoyu < kolay.BoslukBoyu);
        Assert.True(zor.BaslangicHizi > kolay.BaslangicHizi);
    }

    [Fact]
    public void GecersizZorluk_HataFirlatir()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ZorlukAyari.Getir((ZorlukSeviyesi)99));
    }

    [Theory]
    [InlineData(0, Madalya.Yok)]
    [InlineData(9, Madalya.Yok)]
    [InlineData(10, Madalya.Bronz)]
    [InlineData(19, Madalya.Bronz)]
    [InlineData(20, Madalya.Gumus)]
    [InlineData(40, Madalya.Altin)]
    [InlineData(150, Madalya.Altin)]
    public void Madalya_SkoraGoreHesaplanir(int skor, Madalya beklenen)
    {
        Assert.Equal(beklenen, MadalyaHesaplayici.Hesapla(skor));
    }
}
