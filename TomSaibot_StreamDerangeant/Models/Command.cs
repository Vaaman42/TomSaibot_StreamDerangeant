using System.Windows.Input;

namespace TomSaibot_StreamDerangeant.Models;

public class Command : ICommand
{
    public event EventHandler CanExecuteChanged;

    public Action<object> ExecuteAction { get; set; }

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        ExecuteAction.Invoke(parameter);
    }
}
