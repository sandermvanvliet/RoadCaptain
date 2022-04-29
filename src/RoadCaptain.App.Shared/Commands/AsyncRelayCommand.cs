using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RoadCaptain.App.Shared.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _canExecute;
        private readonly Func<object?, Task<CommandResult>> _execute;
        private Action<CommandResult>? _onFailure;
        private Action<CommandResult>? _onSuccess;
        private Action<CommandResult>? _onSuccessWithWarnings;
        private Action<CommandResult>? _onNotExecuted;

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
                _onSuccess?.Invoke(result);
            }
            else if (result.Result == Result.SuccessWithWarnings)
            {
                _onSuccessWithWarnings?.Invoke(result);
            }
            else if (result.Result == Result.Failure)
            {
                _onFailure?.Invoke(result);
            }
            else if (result.Result == Result.NotExecuted)
            {
                _onNotExecuted?.Invoke(result);
            }
        }

        public AsyncRelayCommand OnSuccess(Action<CommandResult> action)
        {
            _onSuccess = action;
            return this;
        }

        public AsyncRelayCommand OnSuccessWithWarnings(Action<CommandResult> action)
        {
            _onSuccessWithWarnings = action;
            return this;
        }

        public AsyncRelayCommand OnFailure(Action<CommandResult> action)
        {
            _onFailure = action;
            return this;
        }

        public AsyncRelayCommand OnNotExecuted(Action<CommandResult> action)
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