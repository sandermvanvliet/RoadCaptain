using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RoadCaptain.App.Shared.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _canExecute;
        private readonly Func<object?, Task<CommandResult>> _execute;
        private Func<CommandResult, Task>? _onFailure;
        private Func<CommandResult, Task>? _onSuccess;
        private Func<CommandResult, Task>? _onSuccessWithWarnings;
        private Func<CommandResult, Task>? _onNotExecuted;

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
                if (_onSuccess != null)
                {
                    await _onSuccess(result);
                }
            }
            else if (result.Result == Result.SuccessWithWarnings)
            {
                if (_onSuccessWithWarnings != null)
                {
                    await _onSuccessWithWarnings(result);
                }
            }
            else if (result.Result == Result.Failure)
            {
                if (_onFailure != null)
                {
                    await _onFailure(result);
                }
            }
            else if (result.Result == Result.NotExecuted)
            {
                if (_onNotExecuted != null)
                {
                    await _onNotExecuted(result);
                }
            }
        }

        public AsyncRelayCommand OnSuccess(Func<CommandResult, Task> action)
        {
            _onSuccess = action;
            return this;
        }

        public AsyncRelayCommand OnSuccessWithWarnings(Func<CommandResult, Task> action)
        {
            _onSuccessWithWarnings = action;
            return this;
        }

        public AsyncRelayCommand OnFailure(Func<CommandResult, Task> action)
        {
            _onFailure = action;
            return this;
        }

        public AsyncRelayCommand OnNotExecuted(Func<CommandResult, Task> action)
        {
            _onNotExecuted = action;
            return this;
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}