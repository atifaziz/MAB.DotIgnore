﻿using System;

namespace MAB.DotIgnore
{
    [Flags]
    public enum MatchFlags
    {
        NONE = 0,
        IGNORE_CASE = 1,    // If set, matches are case-insensitive
        PATHNAME = 2        // If set, a single asterisk should not match slashes
    }
}
