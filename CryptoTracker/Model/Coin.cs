using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTracker.Model
{
    public class Coin : INotifyPropertyChanged
    {
        private double amountOwned;
        private double boughtSum;
        private double price;

        [Key]
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public double Price 
        { 
            get => price;
            set {
                price = value;
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(TotalValue));
                OnPropertyChanged(nameof(Profit));
                OnPropertyChanged(nameof(ProfitPercent));
            } 
        }
        public double AmountOwned
        {
            get => amountOwned;
            set
            {
                amountOwned = value;
                OnPropertyChanged(nameof(AmountOwned));
                OnPropertyChanged(nameof(TotalValue));
                OnPropertyChanged(nameof(Profit));
                OnPropertyChanged(nameof(ProfitPercent));
            }
        }

        public double BoughtSum
        {
            get => boughtSum;
            set
            {
                boughtSum = value;
                OnPropertyChanged(nameof(BoughtSum));
                OnPropertyChanged(nameof(Profit));
                OnPropertyChanged(nameof(ProfitPercent));
            }
        }

        [NotMapped]
        public double TotalValue => AmountOwned * Price;

        [NotMapped]
        public double Profit => TotalValue - BoughtSum;

        [NotMapped]
        public double ProfitPercent => BoughtSum == 0 ? 0 : (Profit / BoughtSum) * 100;

        public string? Image { get; set; }

        [NotMapped]
        public double PriceChange24h { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
