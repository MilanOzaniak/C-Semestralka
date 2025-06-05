using CryptoTracker.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CryptoTracker.View
{
    /// <summary>
    /// Interaction logic for AddCrypto.xaml
    /// </summary>
    public partial class AddCrypto : Window
    {
        private string? amount;
        public Coin Coin { get; private set; }

        public string? Amount
        {
            get => amount;
            set
            {
                amount = value;

                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                // -> toto som našiel na internete ide o bezpečny spôsob ako previesť string na double
                if (double.TryParse(value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsed))
                {
                    Coin.AmountOwned = parsed;
                }

                UpdateTotal();
            }
        }


        public AddCrypto(Coin coin)
        {
            InitializeComponent();
            Coin = coin;
            DataContext = this;
            Coin.PropertyChanged += Coin_PropertyChanged;

            UpdateTotal();
        }

        private void Coin_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Coin.AmountOwned) || e.PropertyName == nameof(Coin.Price))
                UpdateTotal();
        }

        private void UpdateTotal()
        {
            var total = Coin.AmountOwned * Coin.Price;
            TotalBox.Text = total.ToString("F2");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void PreviewTextInput_(object sender, TextCompositionEventArgs e)
        {
            // -> toto mi vygeneroval ChatGPT ide o check aby v TextBoxe boli iba čísla a desatinné čiarky
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(((TextBox)sender).Text.Insert(((TextBox)sender).CaretIndex, e.Text),@"^\d*([.,]?\d*)?$");
        }

    }
}
