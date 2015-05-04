// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Framework.Runtime.Json
{
    internal class JsonDeserializer
    {
        private JsonBuffer _buffer;

        public JsonValue Deserialize(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            _buffer = new JsonBuffer(reader);
            return Deserialize();
        }

        private JsonValue Deserialize()
        {
            var result = DeserializeInternal(_buffer.Read());

            // There are still unprocessed char. The parsing is not finished. Error happened.
            var nextToken = _buffer.Read();
            if (nextToken.Type != JsonTokenType.EOL)
            {
                throw new JsonDeserializerException("Failed to continue deserializing. Additional token " + nextToken.Value, nextToken);
            }

            return result;
        }

        private JsonValue DeserializeInternal(JsonToken next)
        {
            if (next.Type == JsonTokenType.EOL)
            {
                return null;
            }

            if (next.Type == JsonTokenType.LeftSquareBracket)
            {
                return DeserializeArray(next);
            }

            if (next.Type == JsonTokenType.LeftCurlyBracket)
            {
                return DeserializeObject(next);
            }

            if (next.Type == JsonTokenType.String)
            {
                return new JsonString(next.Value, next.GetPosition());
            }

            if (next.Type == JsonTokenType.True || next.Type == JsonTokenType.False)
            {
                return new JsonBoolean(next);
            }

            if (next.Type == JsonTokenType.Null)
            {
                return new JsonNull(next.GetPosition());
            }

            if (next.Type == JsonTokenType.Number)
            {
                return new JsonNumber(next);
            }

            throw new JsonDeserializerException(JsonDeserializerResource.Format_UnexpectedToken(next.Value, next.Type), next);
        }

        private JsonArray DeserializeArray(JsonToken head)
        {
            var list = new List<JsonValue>();
            while (true)
            {
                var next = _buffer.Read();
                if (next.Type == JsonTokenType.RightSquareBracket)
                {
                    break;
                }

                list.Add(DeserializeInternal(next));

                next = _buffer.Read();
                if (next.Type == JsonTokenType.EOL)
                {
                    throw new JsonDeserializerException(JsonDeserializerResource.JSON_InvalidArrayEnd, next);
                }
                else if (next.Type == JsonTokenType.RightSquareBracket)
                {
                    break;
                }
                else if (next.Type != JsonTokenType.Comma)
                {
                    throw new JsonDeserializerException(JsonDeserializerResource.JSON_InvalidArrayExpectComma, next);
                }
            }

            return new JsonArray(list.ToArray(), head.GetPosition());
        }

        private JsonObject DeserializeObject(JsonToken head)
        {
            var dictionary = new Dictionary<string, JsonValue>();

            // Loop through each JSON entry in the input object
            while (true)
            {
                var next = _buffer.Read();
                if (next.Type == JsonTokenType.EOL)
                {
                    throw new JsonDeserializerException(JsonDeserializerResource.JSON_InvalidObject, next);
                }

                if (next.Type == JsonTokenType.Colon)
                {
                    throw new JsonDeserializerException(JsonDeserializerResource.JSON_InvalidMemberName, next);
                }
                else if (next.Type == JsonTokenType.RightCurlyBracket)
                {
                    break;
                }
                else
                {
                    if (next.Type != JsonTokenType.String)
                    {
                        throw new JsonDeserializerException(JsonDeserializerResource.JSON_InvalidMemberName, next);
                    }

                    var memberName = next.Value;
                    if (dictionary.ContainsKey(memberName))
                    {
                        throw new JsonDeserializerException(JsonDeserializerResource.Format_DuplicateObjectMemberName(memberName), next);
                    }

                    next = _buffer.Read();
                    if (next.Type != JsonTokenType.Colon)
                    {
                        throw new JsonDeserializerException(JsonDeserializerResource.JSON_InvalidObject, next);
                    }

                    dictionary[memberName] = DeserializeInternal(_buffer.Read());

                    next = _buffer.Read();
                    if (next.Type == JsonTokenType.RightCurlyBracket)
                    {
                        break;
                    }
                    else if (next.Type != JsonTokenType.Comma)
                    {
                        throw new JsonDeserializerException(JsonDeserializerResource.JSON_InvalidObject, next);
                    }
                }
            }

            return new JsonObject(dictionary, head.GetPosition());
        }
    }
}
