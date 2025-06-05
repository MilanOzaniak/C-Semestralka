using CryptoTracker.Data;
using CryptoTracker.Model;
using CryptoTracker.View;
using CryptoTracker.ViewModel;
using System.Windows;
using System.Windows.Input;


namespace CryptoTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Coin? Coin { get; set; }
        public Transaction? Transaction { get; set; }

        private readonly MainViewModel _vm = new();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            Loaded += async (_, __) => await _vm.LoadAllCoinsAsync();
        }
        

        private void CryptoComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is MainViewModel vm && CryptoComboBox.SelectedItem is Coin coin)
                {
                    if (vm.AddToPortfolioCommand.CanExecute(coin))
                        vm.AddToPortfolioCommand.Execute(coin);
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (Portfolio.SelectedItem is Coin selectedCoin)
            {
                var editableCoin = new Coin
                {
                    Id = selectedCoin.Id,
                    Name = selectedCoin.Name,
                    Symbol = selectedCoin.Symbol,
                    Image = selectedCoin.Image,
                    Price = selectedCoin.Price,
                    AmountOwned = selectedCoin.AmountOwned,
                    BoughtSum = selectedCoin.BoughtSum
                };

                var window = new AddCrypto(editableCoin);
                if (window.ShowDialog() == true)
                {
                    using var db = new AppDbContext();
                    var coinInDb = db.Coins.FirstOrDefault(c => c.Id == editableCoin.Id);

                    if (coinInDb != null)
                    {
                        var transaction = new Transaction
                        {
                            TypeOfTransaction = "MOD",
                            Created = DateTime.Now,
                            Note = $"Upravené {coinInDb.Name} ({coinInDb.Symbol}) | {coinInDb.Price} -> {editableCoin.Price} | {coinInDb.AmountOwned} -> {editableCoin.AmountOwned} EUR"
                        };

                        coinInDb.AmountOwned = editableCoin.AmountOwned;
                        coinInDb.Price = editableCoin.Price;
                        coinInDb.BoughtSum = editableCoin.Price * editableCoin.AmountOwned;
                        db.Transactions.Add(transaction);
                        db.SaveChanges();
                        _vm.LoadTransactionsFromDatabase();

                        _vm.LoadPortfolioFromDatabase();
                    }


                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Portfolio.SelectedItem is Coin selectedCoin)
            {
                var result = MessageBox.Show(
                    $"Naozaj chceš predať {selectedCoin.Name}?",
                    "Potvrdenie predaju",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using var db = new AppDbContext();
                    var coinInDb = db.Coins.FirstOrDefault(c => c.Id == selectedCoin.Id);
                    if (coinInDb != null)
                    {
                        db.Coins.Remove(coinInDb);
                        db.SaveChanges();
                        _vm.LoadPortfolioFromDatabase();

                        var transaction = new Transaction
                        {
                            TypeOfTransaction = "SELL",
                            Created = DateTime.Now,
                            Note = $"Predané {selectedCoin.Name} ({selectedCoin.Symbol}) za {selectedCoin.Price} EUR"
                        };

                        db.Transactions.Add(transaction);
                        db.SaveChanges();
                        _vm.LoadTransactionsFromDatabase();
                        MessageBox.Show($"{selectedCoin.Name} bol predaný.");
                    }
                    else
                    {
                        MessageBox.Show("Crypto sa nenaslo v databáze.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Nie je vybrate žiadne crypto.");
            }
        }
        private void Swap_Click(object sender, RoutedEventArgs e)
        {
            if (Portfolio.SelectedItem is Coin selectedCoin)
            {
                var swapWindow = new SwapCrypto(selectedCoin, _vm);
                swapWindow.Owner = this;
                if (swapWindow.ShowDialog() == true)
                {
                    _vm.LoadPortfolioFromDatabase();
                }
            }
        }
    }
}