using CryptoTracker.Service;
using CryptoTracker.ViewModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace CryptoTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainViewModel vm = new MainViewModel();

            _ = Task.Run(StartPriceUpdater);
            _ = Task.Run(StartPortfolioUpdater);
        }

        private async Task StartPriceUpdater()
        {
            while (true)
            {
                var updater = new CoinGeckoService();
                await updater.UpdatePricesInDatabaseAsync();
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private async void StartPortfolioUpdater()
        {
            while (true)
            {
                var updater = new MainViewModel();
                await updater.UpdatePortfolioPricesAsync();
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

    }
}
