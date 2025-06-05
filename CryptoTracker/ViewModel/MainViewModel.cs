using CryptoTracker.Data;
using CryptoTracker.Model;
using CryptoTracker.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using CryptoTracker.View;
using System.Windows.Data;

namespace CryptoTracker.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public Coin? Coin { get; set; }
        public Transaction? Transaction { get; set; }

        public ObservableCollection<Coin> AllCoins { get; set; } = new();
        public ObservableCollection<Coin> PortfolioCoins { get; set; } = new();
        public ObservableCollection<Coin> TopCoins { get; set; } = new();
        public ObservableCollection<Transaction> Transactionss { get; set; } = new();
        public CoinGeckoService Service { get; set; } = new CoinGeckoService();
        public ICollectionView FilteredCoins { get; }

        public Command<Coin> AddToPortfolioCommand { get; }

        private bool _coinsLoaded = false;

        private Coin? _selectedCoin;

        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilteredCoins.Refresh();
            }
        }

        public Coin? SelectedCoin
        {
            get => _selectedCoin;
            set
            {
                _selectedCoin = value;
                OnPropertyChanged(nameof(SelectedCoin));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // -> toto mi vygeneroval ChatGPT
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainViewModel()
        {
            AddToPortfolioCommand = new Command<Coin>(AddToPortfolio);

            FilteredCoins = CollectionViewSource.GetDefaultView(AllCoins);
            FilteredCoins.Filter = obj =>
            {
                if (obj is Coin c)
                {
                    return string.IsNullOrWhiteSpace(SearchText) || (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                return false;
            };

            LoadPortfolioFromDatabase();
            LoadTransactionsFromDatabase();

            _ = LoadTopCoinsAsync();

        }

        public async Task LoadAllCoinsAsync()
        {
            if (_coinsLoaded)
            { 
                return; 
            }

            var coins = await Service.GetAllCoinsAsync();
            AllCoins.Clear();
            foreach (var coin in coins)
            {
                AllCoins.Add(coin);
            }

            _coinsLoaded = true;
        }
        private async void AddToPortfolio(Coin _coin)
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            if (string.IsNullOrEmpty(_coin.Id))
            {
                return;
            }

            var freshData = await Service.GetMarketPricesAsync(new[] { _coin.Id });
            var updated = freshData.FirstOrDefault();
            if (updated == null)
            {
                MessageBox.Show("Nepodarilo sa načítať aktuálnu cenu.");
                return;
            }

            var coin = new Coin
            {
                Id = updated.id,
                Name = _coin.Name,
                Symbol = _coin.Symbol,
                Price = updated.current_price ?? 0.0,
                Image = _coin.Image
            };

            var window = new AddCrypto(coin);
            if (window.ShowDialog() == true)
            {
                coin.BoughtSum = coin.Price * coin.AmountOwned;

                var existing = db.Coins.FirstOrDefault(c => c.Id == coin.Id);

                var transaction = new Transaction
                {
                    TypeOfTransaction = "BUY",
                    Created = DateTime.Now,
                    Note = $"Kúpené {coin.Name} ({coin.Symbol}) za {coin.BoughtSum} EUR"
                };

                db.Transactions.Add(transaction);

                if (existing != null)
                {
                    existing.AmountOwned += coin.AmountOwned;
                    existing.BoughtSum += coin.AmountOwned * coin.Price;
                    LoadPortfolioFromDatabase();
                    Transactionss.Add(transaction);
                    db.Transactions.Add(transaction);
                    db.SaveChanges();

                    MessageBox.Show($"{coin.Name} bol aktualizovaný v portfóliu.");
                    return;
                }

                db.Coins.Add(coin);
                db.Transactions.Add(transaction);
                db.SaveChanges();

                PortfolioCoins.Add(coin);
                Transactionss.Add(transaction);

                MessageBox.Show($"{coin.Name} bol pridaný do portfólia.");
            }

        }
        public void LoadPortfolioFromDatabase()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            PortfolioCoins.Clear();
            foreach (var coin in db.Coins)
            {
                PortfolioCoins.Add(coin);
            }
        }

        public void LoadTransactionsFromDatabase()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
            Transactionss.Clear();

            var sortedTransactions = db.Transactions.OrderByDescending(t => t.Id).ToList();

            foreach (var transact in sortedTransactions)
            {
                Transactionss.Add(transact);
            }
        }

        public async Task UpdatePortfolioPricesAsync()
        {
            if (!PortfolioCoins.Any())
            {
                return;
            }

            var ids = PortfolioCoins.Select(c => c.Id).Where(id => id != null).Cast<string>().ToArray();
            var data = await Service.GetMarketPricesAsync(ids);

            if(data == null)
            {
                return;
            }

            foreach (var coin in PortfolioCoins)
            {
                var updated = data.FirstOrDefault(d => d.id == coin.Id);
                if (updated != null)
                {
                    coin.Price = updated.current_price ?? 0.0;
                }
            }
        }


        public async Task LoadTopCoinsAsync()
        {
            var top = await Service.GetTopCoinsAsync();
            TopCoins.Clear();
            foreach (var coin in top)
            {
                TopCoins.Add(coin);
            }
        }


    }
}
