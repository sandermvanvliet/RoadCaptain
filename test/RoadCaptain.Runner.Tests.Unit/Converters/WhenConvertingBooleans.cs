using System.Globalization;
using System.Windows;
using FluentAssertions;
using RoadCaptain.UserInterface.Shared.Converters;
using Xunit;

namespace RoadCaptain.Runner.Tests.Unit.Converters
{
    public class WhenConvertingBooleans
    {
        [Fact]
        public void GivenInputIsNotABoolean_UnsetValueIsReturned()
        {
            ResultOfConvert("not a boolean")
                .Should()
                .Be(DependencyProperty.UnsetValue);
        }

        [Fact]
        public void GivenInputIsFalse_OutputIsBooleanWithValueFalse()
        {
            ResultOfConvert(false)
                .Should()
                .Be(false);
        }

        [Fact]
        public void GivenInputIsFalseAndValueShouldBeInverted_OutputIsBooleanWithValueTrue()
        {
            ResultOfConvert(false, "invert")
                .Should()
                .Be(true);
        }

        [Fact]
        public void GivenInputIsTrueAndValueShouldBeInverted_OutputIsBooleanWithValueFalse()
        {
            ResultOfConvert(true, "invert")
                .Should()
                .Be(false);
        }

        [Fact]
        public void GivenInputIsFalseAndParameterIsNotInvert_OutputIsBooleanWithValueFalse()
        {
            ResultOfConvert(false, 1234)
                .Should()
                .Be(false);
        }

        private static object ResultOfConvert(object input, object parameter = null)
        {
            return new BooleanConverter()
                .Convert(
                    input, 
                    typeof(bool), 
                    parameter, 
                    CultureInfo.CurrentCulture);
        }
    }
}
