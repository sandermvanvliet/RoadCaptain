using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RoadCaptain.App.Shared.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _canExecute;
        private readonly Func<object?, Task<CommandResult>> _execute;
        private Action<CommandResult>? _onFailure;
        private Func<CommandResult, Task>? _onFailureAsync;
        private Action<CommandResult>? _onSuccess;
        private Func<CommandResult, Task>? _onSuccessAsync;
        private Action<CommandResult>? _onSuccessWithWarnings;
        private Func<CommandResult, Task>? _onSuccessWithWarningsAsync;
        private Action<CommandResult>? _onNotExecuted;
        private Func<CommandResult, Task>? _onNotExecutedAsync;

        public AsyncRelayCommand(Func<object?, Task<CommandResult>> execute, Func<object?, bool>? canExecute = null)
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

        public async void Execute(object? parameter)
        {
            var result = await _execute(parameter);

            if (result.Result == Result.Success)
            {
                if (_onSuccessAsync != null)
                {
                    await _onSuccessAsync(result);
                }
                else if (_onSuccess != null)
                {
                    _onSuccess(result);
                }
            }
            else if (result.Result == Result.SuccessWithWarnings)
            {
                if (_onSuccessWithWarnings != null)
                {
                    _onSuccessWithWarnings(result);
                }
                else if (_onSuccessWithWarningsAsync != null)
                {
                    await _onSuccessWithWarningsAsync(result);
                }
            }
            else if (result.Result == Result.Failure)
            {
                if (_onFailure != null)
                {
                    _onFailure(result);
                }
                else if (_onFailureAsync != null)
                {
                    await _onFailureAsync(result);
                }
            }
            else if (result.Result == Result.NotExecuted)
            {
                if (_onNotExecuted != null)
                {
                    _onNotExecuted(result);
                }
                else if (_onNotExecutedAsync != null)
                {
                    await _onNotExecutedAsync(result);
                }
            }
        }

        public AsyncRelayCommand OnSuccess(Func<CommandResult, Task> action)
        {
            _onSuccessAsync = action;
            return this;
        }

        public AsyncRelayCommand OnSuccess(Action<CommandResult> action)
        {
            _onSuccess = action;
            return this;
        }

        public AsyncRelayCommand OnSuccessWithWarnings(Func<CommandResult, Task> action)
        {
            _onSuccessWithWarningsAsync = action;
            return this;
        }

        public AsyncRelayCommand OnSuccessWithWarnings(Action<CommandResult> action)
        {
            _onSuccessWithWarnings = action;
            return this;
        }

        public AsyncRelayCommand OnFailure(Func<CommandResult, Task> action)
        {
            _onFailureAsync = action;
            return this;
        }

        public AsyncRelayCommand OnFailure(Action<CommandResult> action)
        {
            _onFailure = action;
            return this;
        }

        public AsyncRelayCommand OnNotExecuted(Func<CommandResult, Task> action)
        {
            _onNotExecutedAsync = action;
            return this;
        }

        public AsyncRelayCommand OnNotExecuted(Action<CommandResult> action)
        {
            _onNotExecuted = action;
            return this;
        }

        public AsyncRelayCommand SubscribeTo<T>(INotifyPropertyChanged notifyPropertyChanged, Expression<Func<T>> expr)
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

            eventHandler?.Invoke(this, EventArgs.Empty);
        }
    }
}