using System;
using Rhino;
using Rhino.Commands;

namespace EC3CarbonCalculator
{
    public class GetGWP : Command
    {
        public GetGWP()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static GetGWP Instance { get; private set; }

        public override string EnglishName => "GetGWP";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: complete command.
            return Result.Success;
        }
    }
}