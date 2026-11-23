namespace FlappyBird.Oyun;

public enum ZorlukSeviyesi
{
    Kolay,
    Normal,
    Zor
}

/// <summary>
/// Her zorluk seviyesinin fizik ayarları. Değerler deneme yanılma ile bulundu.
/// </summary>
public sealed record ZorlukAyari(float BaslangicHizi, float BoslukBoyu, float YerCekimi, float ZiplamaGucu)
{
    public static ZorlukAyari Getir(ZorlukSeviyesi seviye) => seviye switch
    {
        ZorlukSeviyesi.Kolay => new ZorlukAyari(2.6f, 175, 0.40f, 7.0f),
        ZorlukSeviyesi.Normal => new ZorlukAyari(3.2f, 150, 0.45f, 7.5f),
        ZorlukSeviyesi.Zor => new ZorlukAyari(4.0f, 130, 0.50f, 8.0f),
        _ => throw new ArgumentOutOfRangeException(nameof(seviye))
    };
}
