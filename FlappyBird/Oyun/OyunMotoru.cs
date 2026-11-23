namespace FlappyBird.Oyun;

/// <summary>
/// Oyunun bütün kuralları burada: hareket, skor, hız artışı ve çarpışma.
/// Form sadece bu sınıfı her tick'te ilerletip ekrana çiziyor.
/// </summary>
public class OyunMotoru
{
    public const int AlanGenisligi = 480;
    public const int AlanYuksekligi = 640;
    public const int ZeminYuksekligi = 80;

    private const int BoruSayisi = 3;
    private const int BorularArasiMesafe = 220;
    private const int HizArtisAraligi = 5;      // her 5 puanda bir hızlanır
    private const float HizArtisMiktari = 0.35f;
    private const int MaksimumBoslukFarki = 150;
    public const int GeriSayimTickSayisi = 180;  // 3 saniye (saniyede 60 adım)

    private readonly List<Boru> borular = new();
    private readonly Random rastgele;
    private ZorlukAyari ayar;

    public Kus Kus { get; }
    public IReadOnlyList<Boru> Borular => borular;
    public OyunDurumu Durum { get; private set; }
    public ZorlukSeviyesi Zorluk { get; private set; }
    public int Skor { get; private set; }
    public float Hiz { get; private set; }
    public int Seviye => Skor / HizArtisAraligi + 1;

    // Kanat animasyonu ve kayan zemin için tick sayacı
    public long TickSayisi { get; private set; }

    public float ZeminY => AlanYuksekligi - ZeminYuksekligi;

    // Duraklatmadan dönünce kalan geri sayım (tick cinsinden)
    public int GeriSayimKalan { get; private set; }
    public int GeriSayimSaniye => (GeriSayimKalan + 59) / 60;

    public Madalya Madalya => MadalyaHesaplayici.Hesapla(Skor);

    public event EventHandler? SkorDegisti;
    public event EventHandler? DurumDegisti;
    public event EventHandler? OyunBitti;

    public OyunMotoru(ZorlukSeviyesi zorluk) : this(zorluk, null) { }

    /// <param name="tohum">Testlerde aynı boru dizilimini elde etmek için sabit bir sayı verilebilir.</param>
    public OyunMotoru(ZorlukSeviyesi zorluk, int? tohum)
    {
        rastgele = tohum is int t ? new Random(t) : new Random();
        Kus = new Kus(110, 280);
        ayar = ZorlukAyari.Getir(zorluk);
        YeniOyun(zorluk);
    }

    public void YeniOyun(ZorlukSeviyesi zorluk)
    {
        Zorluk = zorluk;
        ayar = ZorlukAyari.Getir(zorluk);
        Skor = 0;
        GeriSayimKalan = 0;
        Hiz = ayar.BaslangicHizi;
        Kus.Sifirla();

        borular.Clear();
        float? oncekiBosluk = null;
        for (int i = 0; i < BoruSayisi; i++)
        {
            float x = AlanGenisligi + 80 + i * BorularArasiMesafe;
            float boslukY = RastgeleBoslukY(oncekiBosluk);
            borular.Add(new Boru(x, boslukY, ayar.BoslukBoyu));
            oncekiBosluk = boslukY;
        }

        DurumuDegistir(OyunDurumu.Hazir);
        SkorDegisti?.Invoke(this, EventArgs.Empty);
    }

    public void KanatCirp()
    {
        if (Durum == OyunDurumu.Hazir)
            DurumuDegistir(OyunDurumu.Oynaniyor);

        if (Durum == OyunDurumu.Oynaniyor)
            Kus.KanatCirp(ayar.ZiplamaGucu);
    }

    /// <summary>
    /// Oyun sırasında duraklatır. Duraklatılmışsa hemen başlatmıyor,
    /// oyuncu hazırlansın diye önce 3 saniyelik geri sayım yapıyor.
    /// </summary>
    public void DuraklatVeyaDevamEt()
    {
        switch (Durum)
        {
            case OyunDurumu.Oynaniyor:
            case OyunDurumu.GeriSayim:
                Duraklat();
                break;

            case OyunDurumu.Duraklatildi:
                GeriSayimKalan = GeriSayimTickSayisi;
                DurumuDegistir(OyunDurumu.GeriSayim);
                break;
        }
    }

    public void Duraklat()
    {
        if (Durum is OyunDurumu.Oynaniyor or OyunDurumu.GeriSayim)
        {
            GeriSayimKalan = 0;
            DurumuDegistir(OyunDurumu.Duraklatildi);
        }
    }

    /// <summary>Timer her tetiklendiğinde bir kare ilerletir.</summary>
    public void Ilerle()
    {
        if (Durum == OyunDurumu.Duraklatildi)
            return;

        TickSayisi++;

        if (Durum == OyunDurumu.GeriSayim)
        {
            GeriSayimKalan--;
            if (GeriSayimKalan <= 0)
                DurumuDegistir(OyunDurumu.Oynaniyor);
            return;
        }

        if (Durum != OyunDurumu.Oynaniyor)
            return;

        Kus.Guncelle(ayar.YerCekimi);

        foreach (var boru in borular)
            boru.Kaydir(Hiz);

        BorulariYenile();
        SkoruKontrolEt();
        CarpismayiKontrolEt();
    }

    private void BorulariYenile()
    {
        foreach (var boru in borular)
        {
            if (!boru.EkrandanCikti)
                continue;

            Boru enSagdaki = borular.MaxBy(b => b.X)!;
            boru.YenidenKonumla(enSagdaki.X + BorularArasiMesafe, RastgeleBoslukY(enSagdaki.BoslukY));
        }
    }

    private void SkoruKontrolEt()
    {
        foreach (var boru in borular)
        {
            if (boru.Gecildi || boru.X + Boru.Genislik >= Kus.X)
                continue;

            boru.Gecildi = true;
            Skor++;

            if (Skor % HizArtisAraligi == 0)
                Hiz += HizArtisMiktari;

            SkorDegisti?.Invoke(this, EventArgs.Empty);
        }
    }

    private void CarpismayiKontrolEt()
    {
        RectangleF kutu = Kus.CarpismaKutusu;

        if (kutu.Bottom >= ZeminY)
        {
            Kus.ZemineOturt(ZeminY);
            Bitir();
            return;
        }

        foreach (var boru in borular)
        {
            if (kutu.IntersectsWith(boru.UstParca) || kutu.IntersectsWith(boru.AltParca(ZeminY)))
            {
                Bitir();
                return;
            }
        }
    }

    private void Bitir()
    {
        DurumuDegistir(OyunDurumu.Bitti);
        OyunBitti?.Invoke(this, EventArgs.Empty);
    }

    private void DurumuDegistir(OyunDurumu yeniDurum)
    {
        if (Durum == yeniDurum)
            return;

        Durum = yeniDurum;
        DurumDegisti?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Yeni boşluğun yerini rastgele seçer. Bir önceki boruya göre çok uzak
    /// olursa yetişmek imkansızlaşıyordu, o yüzden fark en fazla 150 piksel.
    /// </summary>
    private float RastgeleBoslukY(float? oncekiBoslukY)
    {
        int enAz = 70;
        int enCok = (int)(ZeminY - ayar.BoslukBoyu - 70);

        if (oncekiBoslukY is float onceki)
        {
            enAz = Math.Max(enAz, (int)onceki - MaksimumBoslukFarki);
            enCok = Math.Min(enCok, (int)onceki + MaksimumBoslukFarki);
        }

        return rastgele.Next(enAz, enCok + 1);
    }
}
