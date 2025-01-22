// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;

namespace RoadCaptain.App.Shared.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _canExecute;
        private readonly Func<object?, Task<CommandResult>> _execute;
        private Action<CommandResultWithMessage>? _onFailure;
        private Func<CommandResultWithMessage, Task>? _onFailureAsync;
        private Action<CommandResult>? _onSuccess;
        private Func<CommandResult, Task>? _onSuccessAsync;
        private Action<CommandResultWithMessage>? _onSuccessWithMessage;
        private Func<CommandResultWithMessage, Task>? _onSuccessWithMessageAsync;
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
            CommandResult result;
            try
            {
                result = await _execute(parameter);
            }
            catch (Exception e)
            {
                result = CommandResult.Failure(e.Message);
            }

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
            else if (result.Result == Result.SuccessWithMessage)
            {
                if (result is CommandResultWithMessage resultWithMessage)
                {
                    if (_onSuccessWithMessage != null)
                    {
                        _onSuccessWithMessage(resultWithMessage);
                    }
                    else if (_onSuccessWithMessageAsync != null)
                    {
                        await _onSuccessWithMessageAsync(resultWithMessage);
                    }
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
                    if (_onFailure != null)
                    {
                        _onFailure(resultWithMessage);
                    }
                    else if (_onFailureAsync != null)
                    {
                        await _onFailureAsync(resultWithMessage);
                    }
                }
                else
                {
                    throw new ArgumentException("Expected a CommandResultWithMessage but did not receive one");
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

        public AsyncRelayCommand OnSuccessWithMessage(Func<CommandResultWithMessage, Task> action)
        {
            _onSuccessWithMessageAsync = action;
            return this;
        }

        public AsyncRelayCommand OnSuccessWithMessage(Action<CommandResultWithMessage> action)
        {
            _onSuccessWithMessage = action;
            return this;
        }

        public AsyncRelayCommand OnFailure(Func<CommandResultWithMessage, Task> action)
        {
            _onFailureAsync = action;
            return this;
        }

        public AsyncRelayCommand OnFailure(Action<CommandResultWithMessage> action)
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
