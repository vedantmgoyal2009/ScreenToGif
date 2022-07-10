using ScreenToGif.Domain.Interfaces;
using System.Windows.Input;

namespace ScreenToGif.Util;

public class RelayCommand : IRelayCommand
{
    public Predicate<object> CanExecutePredicate { get; set; }

    public Action<object> ExecuteAction { get; set; }

    /// <summary>
    /// Raised when CanExecute should be requeried on commands.
    /// Since commands are often global, it will only hold onto the handler as a weak reference.
    /// Users of this event should keep a strong reference to their event handler to avoid
    /// it being garbage collected. This can be accomplished by having a private field
    /// and assigning the handler as the value before or after attaching to this event.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public RelayCommand() { }

    public RelayCommand(Action<object> execute) : this()
    {
        ExecuteAction = execute;
    }

    public RelayCommand(Predicate<object> canExecute, Action<object> execute) : this(execute)
    {
        CanExecutePredicate = canExecute;
    }

    /// <summary>
    /// Whether the command can be executed with the given parameter on the given target.
    /// </summary>
    /// <param name="parameter">Parameter to be passed to any command handlers.</param>
    /// <returns>true if the command can be executed, false otherwise.</returns>
    public bool CanExecute(object parameter)
    {
        return CanExecutePredicate == null || CanExecutePredicate(parameter);
    }

    /// <summary>
    /// Executes the command with the given parameter.
    /// </summary>
    /// <param name="parameter">Parameter to be passed to any command handlers.</param>
    public void Execute(object parameter)
    {
        ExecuteAction(parameter);
    }
}

public class RelayCommand<T, TR> : IRelayCommand<T, TR>
{
    public Predicate<object> CanExecutePredicate { get; set; }

    public Action<T, TR> ExecuteAction { get; set; }

    /// <summary>
    /// Raised when CanExecute should be requeried on commands.
    /// Since commands are often global, it will only hold onto the handler as a weak reference.
    /// Users of this event should keep a strong reference to their event handler to avoid
    /// it being garbage collected. This can be accomplished by having a private field
    /// and assigning the handler as the value before or after attaching to this event.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public RelayCommand() { }

    public RelayCommand(Action<T, TR> execute) : this()
    {
        ExecuteAction = execute;
    }

    public RelayCommand(Predicate<object> canExecute, Action<T, TR> execute) : this(execute)
    {
        CanExecutePredicate = canExecute;
    }

    /// <summary>
    /// Whether the command can be executed with the given parameter on the given target.
    /// </summary>
    /// <param name="parameter">Parameter to be passed to any command handlers.</param>
    /// <returns>true if the command can be executed, false otherwise.</returns>
    public bool CanExecute(object parameter)
    {
        return CanExecutePredicate == null || CanExecutePredicate(parameter);
    }

    /// <summary>
    /// Executes the command with the given parameter.
    /// </summary>
    /// <param name="parameter">Parameter to be passed to any command handlers.</param>
    /// <param name="secondParameter">Another parameter to be passed to any command handlers.</param>
    public void Execute(T parameter, TR secondParameter)
    {
        ExecuteAction(parameter, secondParameter);
    }
}