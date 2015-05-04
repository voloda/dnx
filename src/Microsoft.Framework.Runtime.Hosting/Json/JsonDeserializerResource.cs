// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Framework.Runtime.Json
{
    internal class JsonDeserializerResource
    {
        internal static string Format_IllegalTrailingCharacterAfterLiteral(int value, string literal)
        {
            return string.Format("Illegal character {0} ({1:X4}) trails the literal name {2}", (char)value, value, literal);
        }

        internal static string Format_UnrecognizedLiteral(string literal)
        {
            return string.Format("Unrecognized json literal. {0} is expected.", literal);
        }

        internal static string Format_UnexpectedToken(string tokenValue, JsonTokenType type)
        {
            return string.Format("Unexpected token, type: {0} value: {1}.", type.ToString(), tokenValue);
        }

        internal static string Format_DuplicateObjectMemberName(string memberName)
        {
            return string.Format("Duplicate member name {0}", memberName);
        }

        internal static string Format_InvalidFloatNumberFormat(string raw)
        {
            return string.Format("Invalid float number format: {0}.", raw);
        }

        internal static string Format_FloatNumberOverflow(string raw)
        {
            return string.Format("Float number overflow: {0}.", raw);
        }

        internal static string JSON_BadEscape
        {
            get { return "Unrecognized escape sequence."; }
        }

        internal static string JSON_DepthLimitExceeded
        {
            get { return "RecursionLimit exceeded."; }
        }

        internal static string JSON_ExpectedOpenBrace
        {
            get { return "Invalid object passed in, '{' expected."; }
        }

        internal static string JSON_InvalidArrayEnd
        {
            get { return "Invalid array passed in, ']' expected."; }
        }

        internal static string JSON_InvalidArrayExpectComma
        {
            get { return "Invalid array passed in, ',' expected."; }
        }

        internal static string JSON_InvalidArrayExtraComma
        {
            get { return "Invalid array passed in, extra trailing ','."; }
        }

        internal static string JSON_InvalidArrayStart
        {
            get { return "Invalid array passed in, '[' expected."; }
        }

        internal static string JSON_InvalidMemberName
        {
            get { return "Invalid object passed in, member name expected."; }
        }

        internal static string JSON_InvalidObject
        {
            get { return "Invalid object passed in, ':' or '}' expected."; }
        }

        internal static string JSON_OpenString
        {
            get { return "Invalid open string, '\"' is expected."; }
        }
    }
}
