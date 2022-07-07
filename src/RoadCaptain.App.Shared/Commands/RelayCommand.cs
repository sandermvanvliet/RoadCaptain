using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Input;
using Avalonia.Threading;

namespace RoadCaptain.App.Shared.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _canExecute;
        private readonly Func<object?, CommandResult> _execute;
        private Action<CommandResultWithMessage>? _onFailure;
        private Action<CommandResult>? _onSuccess;
        private Action<CommandResultWithMessage>? _onSuccessWithMessage;
        private Action<CommandResult>? _onNotExecuted;

        public RelayCommand(Func<object?, CommandResult> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public object? CommandParameter { get; set; }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            var result = _execute(parameter);

            if (result.Result == Result.Success)
            {
                _onSuccess?.Invoke(result);
            }
            else if (result.Result == Result.SuccessWithMessage)
            {
                if (result is CommandResultWithMessage resultWithMessage)
                {
                    _onSuccessWithMessage?.Invoke(resultWithMessage);
                }
                else
                {
                    throw new ArgumentException("Expected a CommandResultWithMessage but did not receive one");
                }
            }
            else if (result.Result == Result.Failure)
            {
                if (result is CommandResultWithMessage resultWithMessage)
                {
                    _onFailure?.Invoke(resultWithMessage);
                }
                else
                {
                    throw new ArgumentException("Expected a CommandResultWithMessage but did not receive one");
                }
            }
            else if (result.Result == Result.NotExecuted)
            {
                _onNotExecuted?.Invoke(result);
            }
        }

        public RelayCommand OnSuccess(Action<CommandResult> action)
        {
            _onSuccess = action;
            return this;
        }

        public RelayCommand OnSuccessWithMessage(Action<CommandResultWithMessage> action)
        {
            _onSuccessWithMessage = action;
            return this;
        }

        public RelayCommand OnFailure(Action<CommandResultWithMessage> action)
        {
            _onFailure = action;
            return this;
        }

        public RelayCommand OnNotExecuted(Action<CommandResult> action)
        {
            _onNotExecuted = action;
            return this;
        }

        public RelayCommand SubscribeTo<T>(INotifyPropertyChanged notifyPropertyChanged, Expression<Func<T>> expr)
        {
            var memberExpr = expr.Body as MemberExpression;
            if (memberExpr == null)
            {
                throw new InvalidOperationException("Can only subscribe to a property");
            }

            var memberName = memberExpr.Member.Name;

            if (memberExpr.Expression is MemberExpression m2)
            {
                var propChangedName = m2.Member.Name;

                var propertyInfo = notifyPropertyChanged.GetType().GetProperty(propChangedName);
                if(propertyInfo == null)
                {
                    return this;
                }

                var propertyChanged = propertyInfo.GetValue(notifyPropertyChanged) as INotifyPropertyChanged;
                if (propertyChanged == null)
                {
                    return this;
                }

                notifyPropertyChanged = propertyChanged;
            }

            notifyPropertyChanged.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == memberName)
                {
                    OnCanExecuteChanged();
                }
            };

            return this;
        }

        private void OnCanExecuteChanged()
        {
            var eventHandler = CanExecuteChanged;

            if (eventHandler == null)
            {
                return;
            }

            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() => eventHandler.Invoke(this, EventArgs.Empty));
            }
            else
            {
                eventHandler.Invoke(this, EventArgs.Empty);
            }
        }
    }
}