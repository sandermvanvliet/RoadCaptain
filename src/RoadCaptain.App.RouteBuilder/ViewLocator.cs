using System;
using System.Linq;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using RoadCaptain.App.RouteBuilder.ViewModels;
using RoadCaptain.App.Shared.ViewModels;

namespace RoadCaptain.App.RouteBuilder
{
    public class ViewLocator(IContainer container, MonitoringEvents monitoringEvents) : IDataTemplate
    {
        public Control Build(object? data)
        {
            var name = data!.GetType().FullName!.Replace("ViewModel", "").Split('.').Last();
            var type = Type.GetType($"RoadCaptain.App.RouteBuilder.Views.{name}");

            if (type != null)
            {
                try
                {
                    var instance = (Control)container.Resolve(type);
                    instance.DataContext = data;
                    return instance;
                }
                catch (Exception ex)
                {
                    monitoringEvents.Error(ex, "Unable to locate view");
                }
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}