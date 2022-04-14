using System.Globalization;
using System.Windows;
using FluentAssertions;
using RoadCaptain.UserInterface.Shared.Converters;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.Converters
{
    public class WhenConvertingVisibility
    {
        [Fact]
        public void GivenInputIsNotABoolean_UnsetValueIsReturned()
        {
            ResultOfConvert("not a boolean")
                .Should()
                .Be(DependencyProperty.UnsetValue);
        }

        [Fact]
        public void GivenInputIsFalse_OutputIsVisibilityCollapsed()
        {
            ResultOfConvert(false)
                .Should()
                .Be(Visibility.Collapsed);
        }

        [Fact]
        public void GivenInputIsTrue_OutputIsVisibilityVisible()
        {
            ResultOfConvert(true)
                .Should()
                .Be(Visibility.Visible);
        }

        [Fact]
        public void GivenInputIsFalseAndValueShouldBeInverted_OutputIsVisibilityVisible()
        {
            ResultOfConvert(false, "invert")
                .Should()
                .Be(Visibility.Visible);
        }

        [Fact]
        public void GivenInputIsTrueAndValueShouldBeInverted_OutputIsVisibilityCollapsed()
        {
            ResultOfConvert(true, "invert")
                .Should()
                .Be(Visibility.Collapsed);
        }

        [Fact]
        public void GivenInputIsFalseAndParameterIsNotInvert_OutputIsVisibilityCollapsed()
        {
            ResultOfConvert(false, 1234)
                .Should()
                .Be(Visibility.Collapsed);
        }

        private static object ResultOfConvert(object input, object parameter = null)
        {
            return new VisibilityConverter()
                .Convert(
                    input, 
                    typeof(bool), 
                    parameter, 
                    CultureInfo.CurrentCulture);
        }
    }
}
