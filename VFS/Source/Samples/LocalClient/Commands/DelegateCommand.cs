using System;


namespace Vfs.Samples.LocalClient.Commands
{
  /// <summary>
  /// Provides a command implementation that relies on delegates.
  /// </summary>
  public class DelegateCommand : CommandExtension
  {
    /// <summary>
    /// Gets the Predicate to execute in order to determine whether the command
    /// can be executed or not.
    /// </summary>
    public Predicate<object> CanExecutePredicate { get; private set; }

    /// <summary>
    /// Gets the action to be called when <see cref="Execute"/> is being invoked.
    /// </summary>
    public Action<object> ExecuteAction { get; private set; }


    /// <summary>
    /// Initializes a new instance of a command that can always execute.
    /// </summary>
    /// <param name="executeDelegate">An action that is being invoked if the command
    /// executes (<see cref="Execute"/> is being invoked).</param>
    /// <exception cref="ArgumentNullException">If <paramref name="executeDelegate"/>
    /// is a null reference.</exception>
    public DelegateCommand(Action<object> executeDelegate) : this(null, executeDelegate)
    {
    }

    /// <summary>
    /// Initializes a new instance of the command.
    /// </summary>
    /// <param name="canExecuteDelegate">Checks whether the command can be executed
    /// or not. This delegate is being invoked if <see cref="CanExecute"/> is being
    /// called. Might be null in order to always enable the command.</param>
    /// <param name="executeDelegate">An action that is being invoked if the command
    /// executes (<see cref="Execute"/> is being invoked).</param>
    /// <exception cref="ArgumentNullException">If <paramref name="executeDelegate"/>
    /// is a null reference.</exception>
    public DelegateCommand(Predicate<object> canExecuteDelegate, Action<object> executeDelegate)
    {
      if (executeDelegate == null) throw new ArgumentNullException("executeDelegate");
      ExecuteAction = executeDelegate;

      CanExecutePredicate = canExecuteDelegate;
    }


    /// <summary>
    /// Checks if the command Execute method can run
    /// </summary>
    /// <param name="parameter">The command parameter to be passed</param>
    /// <returns>Checks whether the command can execute by evaluating
    /// the <see cref="CanExecutePredicate"/> function. If <see cref="CanExecutePredicate"/>
    /// is null, this method returns always true.
    /// </returns>
    public override bool CanExecute(object parameter)
    {
      //always return true if there is no predicate.
      if (CanExecutePredicate == null) return true;
      return CanExecutePredicate(parameter);
    }


    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command.  If the command
    /// does not require data to be passed, this object can be set to null.
    /// </param>
    public override void Execute(object parameter)
    {
      if (ExecuteAction != null) ExecuteAction(parameter);
    }

  }
}