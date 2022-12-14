namespace app.Extensions;

public static class NumericExtensions
{
    //
    //  kucoin wants 4 sig digits in fixed point notation

    public static decimal RoundToKuCoinSignificantDigits(this decimal value)
    {
        // return decimal.Parse(value.ToString("G" + numberOfSignificantDigits));
        return decimal.Parse(value.ToString("#.####"));
    }
}