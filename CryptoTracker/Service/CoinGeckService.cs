using CryptoTracker.Data;
using CryptoTracker.Model;
using Newtonsoft.Json;
using System.Net.Http;


namespace CryptoTracker.Service
{
    public class CoinGeckoService
    {
        public Coin? Coin { get; set; }
        public Transaction? Transaction { get; set; }

        private readonly HttpClient _client = new();

        public CoinGeckoService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (CryptoTrackerApp)");
            _client.DefaultRequestHeaders.Add("x-cg-demo-api-key", "CG-PRH4teRLiMPyxars9WLN1jyW");
        }

        public async Task<List<Coin>> GetTopCoinsAsync()
        {
            var url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=eur&per_page=10&page=1&sparkline=false&price_change_percentage=24h";
            var response = await _client.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<dynamic>>(response);
            Console.WriteLine("Call GetTopCoinsAsync");

            if (data == null)
            {
                return new List<Coin>();
            }


            return data.Select(c => new Coin
            {
                Id = c.id,
                Name = c.name,
                Image = c.image,
                Price = c.current_price,
                PriceChange24h = c.price_change_percentage_24h,
            }).ToList();
        }

        public async Task<List<Coin>> GetAllCoinsAsync()
        {
            var url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=eur&per_page=100&page=1";
            var response = await _client.GetStringAsync(url);

            var data = JsonConvert.DeserializeObject<List<Coin>>(response);

            Console.WriteLine("Call GetAllCoins");

            if (data == null)
            {
                return new List<Coin>();
            }



            return data.Select(c => new Coin
            {
                Id = c.Id,
                Name = c.Name,
                Symbol = c.Symbol,
                Price = c.Price,
                Image = c.Image
            }).ToList();
        }

        public async Task UpdatePricesInDatabaseAsync()
        {
            using var db = new AppDbContext();
            var coins = db.Coins.ToList();

            Console.WriteLine("Call UpdatePricesInDatabase");


            if (!coins.Any())
                return;

            var ids = string.Join(",", coins.Select(c => c.Id));
            var url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=eur&ids={ids}";

            var response = await _client.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<CoinMarketDto>>(response);

            if (data == null)
            {
                Console.WriteLine("Nepodarilo sa nacitat data ");
                return;
            }

            Console.WriteLine($"Stiahnuté dáta: {data.Count} coinov");

            foreach (var coin in coins)
            {
                var updated = data.FirstOrDefault(m => m.id == coin.Id);
                if (updated != null)
                {
                    var oldPrice = coin.Price;
                    coin.Price = (double)(updated.current_price ?? 0.0);

                    Console.WriteLine($"{coin.Name}: {oldPrice} -> {coin.Price}");
                }
                db.SaveChanges();
            }
        }

        public async Task<List<CoinMarketDto>> GetMarketPricesAsync(IEnumerable<string> ids)
        {
            var idList = string.Join(",", ids);
            var url = $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=eur&ids={idList}&order=market_cap_desc&sparkline=false";

            Console.WriteLine("Call GetMarketPricesASync");

            var response = await _client.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<List<CoinMarketDto>>(response);

            if (data == null)
            {
                return new List<CoinMarketDto>();
            }

            return data;
        }

        public class CoinMarketDto
        {
            public string? id { get; set; }
            public double? current_price { get; set; }
        }
    }

}
