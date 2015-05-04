// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Framework.Runtime.Json
{
    internal struct JsonToken
    {
        public JsonTokenType Type;
        public string Value;
        public int Line;
        public int Column;

        public JsonPosition GetPosition()
        {
            return new JsonPosition(Line, Column);
        }
    }

    public enum JsonTokenType
    {
        LeftCurlyBracket,   // [
        LeftSquareBracket,  // {
        RightCurlyBracket,  // ]
        RightSquareBracket, // }
        Colon,              // :
        Comma,              // ,
        Null,
        True,
        False,
        Number,
        String,
        EOL
    }
}
