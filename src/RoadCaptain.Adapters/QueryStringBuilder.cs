// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;

namespace RoadCaptain.Adapters
{
    internal class QueryStringBuilder
    {
        private readonly List<KeyValuePair<string, string>> _parameters = new();
        private readonly UrlEncoder _urlEncoder;

        public QueryStringBuilder()
        {
            _urlEncoder = UrlEncoder.Create();
        }

        public void Add(string name, string value)
        {
            _parameters.Add(new KeyValuePair<string, string>(name, value));
        }

        public override string ToString()
        {
            if (!_parameters.Any())
            {
                return string.Empty;
            }

            return "?" + string.Join(
                "&",
                _parameters
                    .OrderBy(kv => kv.Key)
                    .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        }

        public void AddIfNotDefault<TValue>(string name, TValue? value)
        {
            if (value == null)
            {
                return;
            }

            if (value.Equals(default(TValue)))
            {
                return;
            }

            Add(name, value.ToString()!);
        }
    }
}
