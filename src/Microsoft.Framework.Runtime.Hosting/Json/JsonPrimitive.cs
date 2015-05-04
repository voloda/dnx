// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.Framework.Runtime.Json
{
    internal class JsonPrimitive : JsonValue
    {
        public JsonPrimitive(JsonPosition position) : base(position) { }
    }

    internal class JsonNull : JsonPrimitive
    {
        public JsonNull(JsonPosition position) : base(position) { }
    }

    internal class JsonBoolean : JsonPrimitive
    {
        public JsonBoolean(JsonToken token)
            : base(token.GetPosition())
        {
            if (token.Type == JsonTokenType.True)
            {
                Value = true;
            }
            else if (token.Type == JsonTokenType.False)
            {
                Value = false;
            }
            else
            {
                throw new ArgumentException("Token value should be either True or False.", nameof(token));
            }
        }

        public bool Value { get; private set; }

        public static implicit operator bool (JsonBoolean jsonBoolean)
        {
            return jsonBoolean.Value;
        }
    }

    internal class JsonNumber : JsonPrimitive
    {
        private readonly string _raw;
        private readonly double _double;

        public JsonNumber(JsonToken token)
            : base(token.GetPosition())
        {
            try
            {
                _raw = token.Value;
                _double = double.Parse(_raw, NumberStyles.Float);
            }
            catch (FormatException ex)
            {
                throw new JsonDeserializerException(
                    JsonDeserializerResource.Format_InvalidFloatNumberFormat(_raw),
                    ex,
                    token.Line,
                    token.Column);
            }
            catch (OverflowException ex)
            {
                throw new JsonDeserializerException(
                    JsonDeserializerResource.Format_FloatNumberOverflow(_raw),
                    ex,
                    token.Line,
                    token.Column);
            }
        }

        public double Double
        {
            get { return _double; }
        }

        public int Int
        {
            get { return (int)_double; }
        }
    }
}
