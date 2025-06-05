using CryptoTracker.Data;
using CryptoTracker.Model;
using CryptoTracker.Service;
using CryptoTracker.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CryptoTracker.View
{
    /// <summary>
    /// Interaction logic for SwapCrypto.xaml
    /// </summary>
    public partial class SwapCrypto : Window, INotifyPropertyChanged
    {
        private Coin? _toCoin;


        public Coin FromCoin { get; set; }
        public Coin? ToCoin
        {
            get => _toCoin;
            set
            {
                _toCoin = value;
                OnPropertyChanged(nameof(ToCoin));
                OnPropertyChanged(nameof(ToCoinPrice));
                OnPropertyChanged(nameof(ToCoinTotal));
                OnPropertyChanged(nameof(ToAmount));

                _ = UpdateToCoinPrice();
            }
        }
        private string _fromAmount = "0";
        public string FromAmount
        {
            get => _fromAmount;
            set
            {
                _fromAmount = value;
                OnPropertyChanged(nameof(FromAmount));
                OnPropertyChanged(nameof(FromAmountDouble));
                OnPropertyChanged(nameof(FromCoinTotal));
                OnPropertyChanged(nameof(ToAmount));
                OnPropertyChanged(nameof(ToCoinTotal));
            }
        }

        public double FromAmountDouble
        {
            get
            {
                // -> toto som našiel na internete ide o bezpečny spôsob ako previesť string na double
                if (double.TryParse(_fromAmount.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
                    return result;
                return 0;
            }
        }

        public double? ToCoinPrice => ToCoin?.Price;

        public double ToAmount => ToCoinPrice is > 0 ? FromCoinTotal.GetValueOrDefault() / ToCoinPrice.Value : 0;

        public double? ToCoinTotal => (ToCoinPrice * ToAmount);

        public double? FromCoinPrice => FromCoin?.Price;
        public double? FromCoinTotal => FromCoin?.Price * FromAmountDouble;

        // -> toto mi vygeneroval ChatGPT 
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICollectionView FilteredCoins { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;
        public SwapCrypto(Coin fromCoin, MainViewModel vm)
        {
            InitializeComponent();


            FromCoin = fromCoin;
            FilteredCoins = vm.FilteredCoins;
            DataContext = this;
        }

        private async Task UpdateToCoinPrice()
        {
            if (ToCoin == null)
            {
                return;
            }

            var service = new CoinGeckoService();

            if (string.IsNullOrEmpty(ToCoin.Id))
            {
                return;
            }
            var freshData = await service.GetMarketPricesAsync(new[] { ToCoin.Id });
            var updated = freshData.FirstOrDefault();
            if (updated != null)
            {
                ToCoin.Price = (double)(updated.current_price ?? 0.0);
                OnPropertyChanged(nameof(ToCoin));
                OnPropertyChanged(nameof(ToCoinPrice));
            }
        }


        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (FromCoin == null || ToCoin == null)
            {
                MessageBox.Show("Nie je vybraná kryptomena.");
                return;
            }

            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            var fromInDb = db.Coins.FirstOrDefault(c => c.Id == FromCoin.Id);
            var toInDb = db.Coins.FirstOrDefault(c => c.Id == ToCoin.Id);

            if (fromInDb == null)
            {
                MessageBox.Show("Kryptomena nie je v databáze.");
                return;
            }

            if (FromAmountDouble > fromInDb.AmountOwned)
            {
                MessageBox.Show("Nemáš dostatočné množstvo na výmenu.");
                return;
            }

            fromInDb.AmountOwned -= FromAmountDouble;
            fromInDb.BoughtSum = fromInDb.Price * fromInDb.AmountOwned;

            if (fromInDb.AmountOwned <= 0)
                db.Coins.Remove(fromInDb);

            if (toInDb != null)
            {
                toInDb.AmountOwned += ToAmount;
                toInDb.BoughtSum += ToCoin.Price * ToAmount;
            }
            else
            {
                db.Coins.Add(new Coin
                {
                    Id = ToCoin.Id,
                    Name = ToCoin.Name,
                    Symbol = ToCoin.Symbol,
                    Image = ToCoin.Image,
                    Price = ToCoin.Price,
                    AmountOwned = ToAmount,
                    BoughtSum = ToCoin.Price * ToAmount
                });
            }

            db.SaveChanges();

            DialogResult = true;
            Close();
        }

        private void PreviewTextInput_(object sender, TextCompositionEventArgs e)
        {
            // -> toto mi vygeneroval ChatGPT ide o check aby v TextBoxe boli iba čísla a desatinné čiarky
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(((TextBox)sender).Text.Insert(((TextBox)sender).CaretIndex, e.Text), @"^\d*([.,]?\d*)?$");
        }


    }


}
