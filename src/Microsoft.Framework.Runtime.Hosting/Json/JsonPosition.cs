// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Framework.Runtime.Json
{
    internal class JsonPosition
    {
        public JsonPosition(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as JsonPosition;
            return other != null &&
                   other.Line == Line &&
                   other.Column == Column;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
