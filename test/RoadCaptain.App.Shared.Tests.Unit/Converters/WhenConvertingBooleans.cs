// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Globalization;
using FluentAssertions;
using RoadCaptain.App.Shared.Converters;
using Xunit;

namespace RoadCaptain.App.Shared.Tests.Unit.Converters
{
    public class WhenConvertingBooleans
    {
        [Fact]
        public void GivenInputIsNotABoolean_InputValueIsReturned()
        {
            var input = "not a boolean";

            ResultOfConvert(input)
                .Should()
                .BeSameAs(input); // Note: Using BeSameAs because the exact same reference to _input_ should be returned
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

        private static object? ResultOfConvert(object input, object? parameter = null)
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

