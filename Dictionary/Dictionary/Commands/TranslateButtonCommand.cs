using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dictionary.Commands
{
    public class TranslateButtonCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        // TranslateButtonCommand konstruktor inicializálása
        public TranslateButtonCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        // Meghatározza, hogy a parancs végrehajtható-e
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        // A parancs végrehajtása
        public void Execute(object parameter)
        {
            execute();
        }

        // Jelzi, hogy a parancs végrehajthatósága megváltozott
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
