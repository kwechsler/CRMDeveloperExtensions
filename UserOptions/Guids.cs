// Guids.cs
// MUST match guids.h
using System;

namespace UserOptions
{
    static class GuidList
    {
        public const string guidUserOptionsPkgString = "8f9d97a5-8fc7-461c-9a4f-0c4d2e03210f";
        public const string guidUserOptionsCmdSetString = "044d3d3a-3d4c-42db-a7b0-ce4d04a5b002";
        public static readonly Guid guidUserOptionsCmdSet = new Guid(guidUserOptionsCmdSetString);
    };
}