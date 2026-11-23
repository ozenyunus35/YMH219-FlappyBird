namespace FlappyBird.Oyun;

public enum Madalya
{
    Yok,
    Bronz,
    Gumus,
    Altin
}

public static class MadalyaHesaplayici
{
    public const int BronzSiniri = 10;
    public const int GumusSiniri = 20;
    public const int AltinSiniri = 40;

    public static Madalya Hesapla(int skor)
    {
        if (skor >= AltinSiniri) return Madalya.Altin;
        if (skor >= GumusSiniri) return Madalya.Gumus;
        if (skor >= BronzSiniri) return Madalya.Bronz;
        return Madalya.Yok;
    }

    public static string Ad(Madalya madalya) => madalya switch
    {
        Madalya.Bronz => "Bronz",
        Madalya.Gumus => "Gümüş",
        Madalya.Altin => "Altın",
        _ => "-"
    };
}
