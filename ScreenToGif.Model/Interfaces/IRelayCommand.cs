using System.Windows.Input;

namespace ScreenToGif.Domain.Interfaces;

public interface IRelayCommand : ICommand
{
    public Predicate<object> CanExecutePredicate { get; set; }

    public Action<object> ExecuteAction { get; set; }
}

public interface IRelayCommand<T, TR>
{
    event EventHandler CanExecuteChanged;

    bool CanExecute(object parameter);

    void Execute(T parameter, TR secondParameter = default);

    public Predicate<object> CanExecutePredicate { get; set; }

    public Action<T, TR> ExecuteAction { get; set; }
}