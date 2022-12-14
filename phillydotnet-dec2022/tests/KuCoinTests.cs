using app;
using app.Extensions;
using Kucoin.Net.Clients;
using Kucoin.Net.Enums;
using Kucoin.Net.Objects;
using Microsoft.Extensions.Configuration;
using Serilog;
using tests.Util;
using Xunit.Abstractions;

namespace tests;

#pragma warning disable xUnit1033
public class KuCoinTests : BaseTest, IClassFixture<ConfigurationFixture>
#pragma warning restore xUnit1033
{
    #region Instance Data

    private readonly IConfiguration _configuration;

    #endregion

    #region Constants

    private const string StableCoin = "USDT";
    private const string Crypto = "BTC";

    #endregion

    #region Tests

    [Fact]
    public async void test_account_balance()
    {
        using var client = new KucoinClient(new KucoinClientOptions
            { ApiCredentials = CreateApiCredentials(_configuration) });

        //  tether balance
        var response = await client.SpotApi.Account.GetAccountsAsync(StableCoin, AccountType.Trade);
        var tetherData = response.Data.FirstOrDefault(d => d.Available > 0);
        Assert.NotNull(tetherData);
        Log.Logger.Information("Stable coin {coin} has available balance {balance}", tetherData.Asset,
            tetherData.Available);

        //  bitcoin 
        response = await client.SpotApi.Account.GetAccountsAsync(Crypto, AccountType.Trade);
        var bitcoinData = response.Data.FirstOrDefault(d => d.Available > 0);
        Assert.NotNull(bitcoinData);
        Log.Logger.Information("Crypto {coin} has available balance {balance}", bitcoinData.Asset,
            bitcoinData.Available);

        //
        //  get current BTC price
        var statsAsync = await client.SpotApi.ExchangeData.Get24HourStatsAsync(TradingPair());
        Assert.NotNull(statsAsync);
        var btcStats = statsAsync.Data;
        Log.Logger.Information("Coin pair {pair} has current price {price}", TradingPair(), btcStats.LastPrice);

        //
        //  get BTC balance in USD
        var btcBalance = btcStats.LastPrice * bitcoinData.Available;
        Log.Logger.Information("Coin {coin} has USD balance available ${usd}", Crypto, btcBalance);
    }

    [Fact]
    public async void test_buy_crypto()
    {
        using var client = new KucoinClient(new KucoinClientOptions
            { ApiCredentials = CreateApiCredentials(_configuration) });

        //  tether balance
        var response = await client.SpotApi.Account.GetAccountsAsync(StableCoin, AccountType.Trade);
        var tetherData = response.Data.FirstOrDefault(d => d.Available > 0);
        Assert.NotNull(tetherData);
        Log.Logger.Information("Stable coin {coin} has available balance {balance}", tetherData.Asset,
            tetherData.Available);
        
        //
        //  btc market price
        var statsAsync = await client.SpotApi.ExchangeData.Get24HourStatsAsync(TradingPair());
        Assert.NotNull(statsAsync);
        var btcStats = statsAsync.Data;
        Log.Logger.Information("Coin pair {pair} has current price {price}", TradingPair(), btcStats.LastPrice);

        //
        //  place a buy order (market) using 20% of tether balance
        var buyOrderAmount = (tetherData.Available / btcStats.LastPrice!.Value) * (decimal)0.20;
        Log.Logger.Information("about to acquire {amount} worth of {coin}", buyOrderAmount, Crypto);
        var orderResponse = await client.SpotApi.Trading.PlaceOrderAsync(
            TradingPair(),
            OrderSide.Buy,
            NewOrderType.Market,
            buyOrderAmount.RoundToKuCoinSignificantDigits());
        Assert.NotNull(orderResponse);
        var kucoinOrderId = orderResponse.Data.Id;
        Log.Logger.Information("Created kucoin buy order with ID {Id}", kucoinOrderId);
    }

    [Fact]
    public async Task test_sell_crypto()
    {
        using var client = new KucoinClient(new KucoinClientOptions
            { ApiCredentials = CreateApiCredentials(_configuration) });

        //
        //  get current BTC balance
        var response = await client.SpotApi.Account.GetAccountsAsync(Crypto, AccountType.Trade);
        var bitcoinData = response.Data.FirstOrDefault(d => d.Available > 0);
        Assert.NotNull(bitcoinData);
        Log.Logger.Information("Crypto {coin} has available balance {balance}", bitcoinData.Asset,
            bitcoinData.Available);

        //
        //  sell 50%
        var amountToSell = bitcoinData.Available * (decimal)0.50;
        Log.Logger.Information("SELLING {amount} of {coin}", amountToSell, Crypto);
        var orderResponse = await client.SpotApi.Trading.PlaceOrderAsync(
            TradingPair(),
            OrderSide.Sell,
            NewOrderType.Market,
            amountToSell.RoundToKuCoinSignificantDigits());
        Assert.NotNull(orderResponse);
        var kucoinOrderId = orderResponse.Data.Id;
        Log.Logger.Information("Created kucoin sell order with ID {Id}", kucoinOrderId);
    }

    #endregion

    #region Private Methods

    private static string TradingPair() => $"{Crypto}-{StableCoin}";

    private static KucoinApiCredentials CreateApiCredentials(IConfiguration configuration)
    {
        var apiKey = configuration[AppSettings.KuCoinApiKey];
        var apiSecret = configuration[AppSettings.KuCoinApiSecret];
        var apiPassphrase = configuration[AppSettings.KuCoinApiPassphrase];

        if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentNullException(apiKey);
        if (string.IsNullOrWhiteSpace(apiSecret)) throw new ArgumentNullException(apiSecret);
        if (string.IsNullOrWhiteSpace(apiPassphrase)) throw new ArgumentNullException(apiPassphrase);
        return new KucoinApiCredentials(apiKey, apiSecret, apiPassphrase);
    }

    #endregion

    #region Constructor

    public KuCoinTests(ConfigurationFixture configuration, ITestOutputHelper output) : base(configuration, output)
    {
        _configuration = configuration.Config;
    }

    #endregion
}