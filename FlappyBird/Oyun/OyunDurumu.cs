namespace FlappyBird.Oyun;

public enum OyunDurumu
{
    Hazir,          // ilk tuşa basılmasını bekliyor
    Oynaniyor,
    Duraklatildi,
    GeriSayim,      // duraklatmadan sonra 3-2-1
    Bitti
}
