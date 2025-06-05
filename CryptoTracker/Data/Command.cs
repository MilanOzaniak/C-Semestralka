using System.Windows.Input;

namespace CryptoTracker.Data
{
    // -> toto som našiel na internete ide o jednoduchu implemetáciu ICommand aby mi fungoval command property na Buttone WPF
    public class Command<T> : ICommand
    {
        private readonly Action<T> _execute;

        public Command(Action<T> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is T t)
                _execute(t);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
