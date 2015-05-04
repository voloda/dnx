// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Microsoft.Framework.Runtime.Json
{
    internal class JsonBuffer
    {
        private readonly TextReader _reader;

        private int _line = 1;
        private int _column = 0;

        public JsonBuffer(TextReader reader)
        {
            _reader = reader;
        }

        public JsonToken Read()
        {
            var result = new JsonToken();

            int first;
            while (true)
            {
                first = ReadNextChar();
                result.Line = _line;
                result.Column = _column;

                if (first == -1)
                {
                    result.Type = JsonTokenType.EOL;
                    return result;
                }
                else if (!IsWhitespace(first))
                {
                    break;
                }
            }

            if (first == JsonConstants.LeftCurlyBracket)
            {
                result.Type = JsonTokenType.LeftCurlyBracket;
            }
            else if (first == JsonConstants.RightCurlyBracket)
            {
                result.Type = JsonTokenType.RightCurlyBracket;
            }
            else if (first == JsonConstants.LeftSquareBracket)
            {
                result.Type = JsonTokenType.LeftSquareBracket;
            }
            else if (first == JsonConstants.RightSquareBracket)
            {
                result.Type = JsonTokenType.RightSquareBracket;
            }
            else if (first == JsonConstants.Colon)
            {
                result.Type = JsonTokenType.Colon;
            }
            else if (first == JsonConstants.Comma)
            {
                result.Type = JsonTokenType.Comma;
            }
            else if (first == JsonConstants.Quotation)
            {
                result.Type = JsonTokenType.String;
                result.Value = ReadString();
            }
            else if (first == (int)'t')
            {
                ReadLiteral(JsonConstants.ValueTrue);
                result.Type = JsonTokenType.True;
            }
            else if (first == (int)'f')
            {
                ReadLiteral(JsonConstants.ValueFalse);
                result.Type = JsonTokenType.False;
            }
            else if (first == (int)'n')
            {
                ReadLiteral(JsonConstants.ValueNull);
                result.Type = JsonTokenType.Null;
            }
            else if (char.IsDigit((char)first) || first == (int)'-')
            {
                result.Type = JsonTokenType.Number;
                result.Value = ReadNumber(first);
            }
            else
            {
                throw new JsonDeserializerException(string.Format("Illegal character {0:X4}.", first), result.GetPosition());
            }

            return result;
        }

        private int ReadNextChar()
        {
            while (true)
            {
                var value = _reader.Read();
                _column += 1;

                if (value == -1)
                {
                    // This is the end of file
                    return -1;
                }
                else if (value == JsonConstants.LF)
                {
                    // This is a new line. Let the next loop read the first charactor of the following line.
                    // Set position ahead of next line
                    _column = 0;
                    _line += 1;

                    continue;
                }
                else if (value == JsonConstants.CR)
                {
                    // Skip the carriage return.
                    // Let the next loop read the following char
                }
                else
                {
                    // Returns the normal value
                    return value;
                }
            }
        }

        private string ReadNumber(int firstRead)
        {
            var buf = new List<char> { (char)firstRead };
            while (true)
            {
                var next = _reader.Peek();

                if (char.IsDigit((char)next) ||
                    next == JsonConstants.FullStop ||
                    next == JsonConstants.UpperE ||
                    next == JsonConstants.LowerE)
                {
                    buf.Add((char)ReadNextChar());
                }
                else
                {
                    break;
                }
            }

            return new string(buf.ToArray());
        }

        private void ReadLiteral(string literal)
        {
            for (int i = 1; i < literal.Length; ++i)
            {
                var next = _reader.Peek();
                if (next != (int)literal[i])
                {
                    throw new JsonDeserializerException(
                        JsonDeserializerResource.Format_UnrecognizedLiteral(literal),
                        _line, _column);
                }
                else
                {
                    ReadNextChar();
                }
            }

            var tail = _reader.Peek();
            if (tail != JsonConstants.RightCurlyBracket &&
                tail != JsonConstants.RightSquareBracket &&
                tail != JsonConstants.CR &&
                tail != JsonConstants.LF &&
                tail != JsonConstants.Comma &&
                tail != -1 &&
                !IsWhitespace(tail))
            {
                throw new JsonDeserializerException(
                    JsonDeserializerResource.Format_IllegalTrailingCharacterAfterLiteral(tail, literal),
                    _line, _column);
            }
        }

        private string ReadString()
        {
            var buf = new List<char>();
            var escaped = false;

            while (true)
            {
                var next = ReadNextChar();

                if (next == -1 || next == JsonConstants.LF)
                {
                    throw new JsonDeserializerException(JsonDeserializerResource.JSON_OpenString, _line, _column);
                }
                else if (escaped)
                {
                    if (next == JsonConstants.Quotation)
                    {
                        buf.Add((char)JsonConstants.Quotation);
                    }
                    else if (next == JsonConstants.ReverseSolidus)
                    {
                        buf.Add((char)JsonConstants.ReverseSolidus);
                    }
                    else if (next == JsonConstants.Solidus)
                    {
                        buf.Add((char)JsonConstants.Solidus);
                    }
                    else if (next == 0x62)
                    {
                        // '\b' backspace
                        buf.Add((char)JsonConstants.BACKSPACE);
                    }
                    else if (next == 0x66)
                    {
                        // '\f' form feed
                        buf.Add((char)JsonConstants.FF);
                    }
                    else if (next == 0x6E)
                    {
                        // '\n' line feed
                        buf.Add((char)JsonConstants.LF);
                    }
                    else if (next == 0x72)
                    {
                        // '\r' carriage return
                        buf.Add((char)JsonConstants.CR);
                    }
                    else if (next == 0x74)
                    {
                        // '\t' tab
                        buf.Add((char)JsonConstants.HT);
                    }
                    else if (next == 0x75)
                    {
                        var unicodeLine = _line;
                        var unicodeColumn = _column;

                        var unicodes = new char[4];
                        for (int i = 0; i < 4; ++i)
                        {
                            next = ReadNextChar();
                            if (next == -1)
                            {
                                throw new JsonDeserializerException(JsonDeserializerResource.JSON_OpenString, unicodeLine, unicodeColumn);
                            }
                            else
                            {
                                unicodes[i] = (char)next;
                            }
                        }

                        try
                        {
                            var unicodeValue = int.Parse(new string(unicodes), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                            buf.Add((char)unicodeValue);
                        }
                        catch (FormatException ex)
                        {
                            throw new JsonDeserializerException("Invalid Unicode format [" + new string(unicodes) + "]", ex, unicodeLine, unicodeColumn);
                        }
                    }
                    else
                    {
                        throw new JsonDeserializerException(JsonDeserializerResource.JSON_BadEscape, _line, _column);
                    }

                    escaped = false;
                }
                else if (next == JsonConstants.ReverseSolidus)
                {
                    escaped = true;
                }
                else if (next == JsonConstants.Quotation)
                {
                    break;
                }
                else
                {
                    buf.Add((char)next);
                }
            }

            return new string(buf.ToArray());
        }

        private static bool IsWhitespace(int value)
        {
            return value == JsonConstants.SP || value == JsonConstants.HT || value == JsonConstants.CR;
        }
    }
}
