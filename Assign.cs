using System;
using Rhino;
using Rhino.Commands;
using UnitsNet;

namespace EC3CarbonCalculator
{
    public class Assign : Command
    {
        EC3MaterialFilter mf = new EC3MaterialFilter();
        int dimension = 3;
        IQuantity unit;
        string category;

        public Assign()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static Assign Instance { get; private set; }

        public override string EnglishName => "Assign";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: complete command.
            return Result.Success;
        }

        private Result SetCategory()
        {
            UserText userText = new UserText();
            userText.SetPrompt("Set material category");
            userText.UserInputText();
            this.category = userText.GetInputText();

            EC3CategoryTree categoryTree = EC3CategoryTree.Instance;
            int catIdx = categoryTree.GetCategoryIdx(category);
            if (catIdx == -1)
            {
                RhinoApp.WriteLine("Entered category is not a valid EC3 category.");
                return Result.Failure;
            }
            mf.SetCategory(categoryTree.names[catIdx]);

            dimension = categoryTree.dimensions[catIdx];

            return Result.Success;
        }

        private Result SetJurisdiction()
        {
            UserText userText = new UserText();
            userText.SetPrompt("Set material source jurisdiction");
            userText.UserInputText();
            string jurisdiction = userText.GetInputText();

            string[] splitJurisdiction = jurisdiction.Split('-');
            if (splitJurisdiction[0] == "US")
            {
                mf.SetState(splitJurisdiction[1]);
            }
            if (!mf.SetCountry(splitJurisdiction[0]))
            {
                RhinoApp.WriteLine("Not a valid jurisdiction, will proceed with Global" +
                    "calculation.");
                return Result.Failure;
            }
            return Result.Success;
        }

        private Result SetExpireDate()
        {
            UserText userText = new UserText();
            userText.SetPrompt("Set minimum expiration date of EPD in format yyyy-MM-dd");
            userText.UserInputText();
            string date = userText.GetInputText();

            if (!mf.SetExpirationDate(date))
            {
                RhinoApp.WriteLine("Not a valid date, will proceed with current date.");
                return Result.Failure;
            }
            return Result.Success;
        }
    }
}