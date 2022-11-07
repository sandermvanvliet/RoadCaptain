using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;

namespace RoadCaptain.App.Shared.Commands
{
    public static class CommandExtensions
    {
        public static BindingBuilder Bind(this Window window, ICommand command)
        {
            return new BindingBuilder(window, command);
        }
    }

    public class BindingBuilder
    {
        private readonly Window _window;
        private readonly ICommand _command;
        private Key _key;
        private readonly KeyModifiers _platformModifier;

        public BindingBuilder(Window window, ICommand command)
        {
            _window = window;
            _command = command;
            _platformModifier = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? KeyModifiers.Meta
                : KeyModifiers.Control;
        }

        public BindingBuilder To(Key key)
        {
            _key = key;

            return this;
        }

        public void WithPlatformModifier()
        {
            _window.KeyBindings.Add(new KeyBinding { Command = _command, Gesture = new KeyGesture(_key, _platformModifier) });
        }
    }
}