using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Input.Custom;
using Rhino.Commands;
using Rhino.Input;
using Rhino;

namespace EC3CarbonCalculator
{
    internal class UserText
    {
        string prompt = "Input text: ";
        string inputText;

        public Result UserInputText()
        {
            var getText = new Rhino.Input.Custom.GetString();
            getText.SetCommandPrompt(prompt);
            getText.AcceptNothing(true); // Allow empty input

            while (true)
            {
                var getResult = getText.Get();
                if (getResult == GetResult.Cancel)
                    return Result.Cancel;

                if (getResult == GetResult.String)
                {
                    inputText = getText.StringResult();
                    break;
                }

                if (getResult == GetResult.Nothing)
                {
                    inputText = null; break;
                }
            }

            return Result.Success;
        }

        public void SetPrompt(string prompt)
        {
            this.prompt = prompt;
        }

        public string GetInputText()
        {
            return this.inputText;
        }
    }
}
