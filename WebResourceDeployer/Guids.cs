// Guids.cs
// MUST match guids.h
using System;

namespace JasonLattimer.WebResourceDeployer
{
    static class GuidList
    {
        public const string guidWebResourceDeployerPkgString = "e42f2686-2a9b-40c1-8d13-9ab90d3b951d";
        public const string guidWebResourceDeployerCmdSetString = "86128e89-8a56-4a17-9029-a75379a89b9f";
        public const string guidToolWindowPersistanceString = "96aa3696-8674-484f-a95e-08355d14a7fb";

        public static readonly Guid guidWebResourceDeployerCmdSet = new Guid(guidWebResourceDeployerCmdSetString);
    };
}