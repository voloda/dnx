// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Framework.Runtime.Json
{
    internal class JsonConstants
    {
        public const int CR = 0x0D;
        public const int LF = 0x0A;
        public const int SP = 0x20;
        public const int HT = 0x09;
        public const int FF = 0x0C;
        public const int BACKSPACE = 0x08;

        public const int LeftSquareBracket  = '['; // 0x5B
        public const int RightSquareBracket = ']'; // 0x5D
        public const int LeftCurlyBracket   = '{'; // 0x7B
        public const int RightCurlyBracket  = '}'; // 0x7D

        public const int Colon = ':';           // 0x3A
        public const int Comma = ',';           // 0x2C
        public const int Quotation = '"';       // 0x22
        public const int ReverseSolidus = '\\'; // 0x5C
        public const int Solidus = '/';         // 0x2F
        public const int FullStop = '.';        // 0x2E

        public const int UpperE = 'E';
        public const int LowerE = 'e';

        public const string ValueNull = "null";
        public const string ValueTrue = "true";
        public const string ValueFalse = "false";
    }
}
