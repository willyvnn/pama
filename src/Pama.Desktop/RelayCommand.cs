using System.Windows.Input;

namespace Pama.Desktop;

/// <summary>Always-enabled command wrapping an action, for keyboard shortcuts.</summary>
public sealed class RelayCommand(Action execute) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute();
}
