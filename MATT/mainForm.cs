using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Excel = Microsoft.Office.Interop.Excel;

namespace MATT
{
    public partial class mainForm : Form
    {

        private Dictionary<string, string> instructionDict;
        enum equationType { noVar, oneVar, twoVar, multEqn, unknown };

        public mainForm()
        {
            InitializeComponent();

            instructionDict = new Dictionary<string, string>();

            Excel.Application exApp;
            Excel.Workbooks workbooks;
            Excel.Workbook workbook;
            Excel.Sheets sheets;
            Excel.Worksheet worksheet;
            Excel.Range range;
            try
            {
                exApp = new Excel.Application();
                exApp.Visible = false;
                workbooks = exApp.Workbooks;
                workbook = workbooks.Open(@"C:\Users\Nicki\Desktop\MATT_Instructions.xlsx");
                sheets = workbook.Worksheets;
                worksheet = sheets[1];
                range = worksheet.UsedRange;

                object[,] valueArray = (object[,])range.get_Value();
                for (int i = 1; i <= valueArray.GetLength(0); i++)
                {
                    instructionDict.Add((string)valueArray[i, 1], (string)valueArray[i, 2]);
                }

                workbook.Close();
                exApp.Quit();

                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(exApp);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workbooks);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(worksheet);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(range);

                equationTB.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private equationType determineType(string enteredEqn)
        {
            //get rid of "abs" and "sqrt" so they're not mistaken as variables
            enteredEqn = enteredEqn.Replace("abs", "");
            enteredEqn = enteredEqn.Replace("sqrt", "");

            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYXabcdefghijklmnopqrstuvwxyz".ToCharArray();
            if (enteredEqn.IndexOfAny(alphabet) == -1)
            {
                //if the string has no letters
                return equationType.noVar;
            }

            //look for semicolon indicating multiple equations
            if (enteredEqn.Split(';').Length > 1)
            {
                bool isEqn = true;
                foreach (string s in enteredEqn.Split(';'))
                {
                    if (!s.Contains('='))
                    {
                        isEqn = false;
                    }
                }
                if (isEqn)
                    return equationType.multEqn;
            }

            //find the first variable and remove all instances
            char firstVar = enteredEqn[enteredEqn.IndexOfAny(alphabet)];
            string removedFirstVar = enteredEqn.Replace(firstVar, ' ');
            //if there aren't any more letters, it must be a one variable eqn
            if (removedFirstVar.IndexOfAny(alphabet) == -1)
            {
                return equationType.oneVar;
            }
            else
            {
                char secondVar = removedFirstVar[removedFirstVar.IndexOfAny(alphabet)];
                string removedSecondVar = removedFirstVar.Replace(secondVar, ' ');
                if (removedSecondVar.IndexOfAny(alphabet) == -1)
                {
                    return equationType.twoVar;
                }
            }

            return equationType.unknown;
        }

        private void insertPictures()
        {
            while (instructionsRTB.Text.Contains('<'))
            {
                int startIndex = instructionsRTB.Text.IndexOf('<');
                int endIndex = instructionsRTB.Text.IndexOf('>');
                int urlLength = endIndex - startIndex;
                string currentDir = Environment.CurrentDirectory;
                string url = currentDir + instructionsRTB.Text.Substring(startIndex + 1, urlLength - 1);

                instructionsRTB.Select(startIndex, urlLength + 1);
                Clipboard.SetImage(Image.FromFile(url));
                instructionsRTB.Paste();
            }
        }

        private LinkLabel addLinkLabel(string text, string tag, int index)
        {
            //if this link is already listed, don't add it
            foreach (LinkLabel ll in linkLabelPanel.Controls)
            {
                if (ll.Tag.ToString().Equals(tag))
                {
                    return null;
                }
            }

            //as long as the link isn't already there, make it and add it to the list
            LinkLabel newLinkLabel = new LinkLabel();
            newLinkLabel.Text = text;
            newLinkLabel.Tag = tag;
            newLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            newLinkLabel.AutoSize = true;
            int x = 0;
            int y = 17 * (index - 1);
            if (index > 4 && index < 9)
            {
                y = 17 * (index % 5);
                x = 250;
            }
            else if (index > 8)
            {
                x = 500;
                y = 17 * (index % 9);
            }
            newLinkLabel.Location = new Point(x, y);
            linkLabelPanel.Controls.Add(newLinkLabel);
            newLinkLabel.BringToFront();
            return newLinkLabel;
        }

        private void loadInstructions(object sender)
        {
            if (sender != null)
            {
                LinkLabel selected = null;
                if (sender is LinkLabel)
                {
                    selected = sender as LinkLabel;
                }

                //Look up and display instructions based on tag
                string text = "";
                instructionDict.TryGetValue(selected.Tag.ToString(), out text);
                instructionsRTB.Text += text;
                instructionsRTB.Text += "\n";
            }
        }

        private void linkLabel_LinkClicked(object sender, EventArgs e)
        {
            instructionsRTB.Text = "";
            loadInstructions(sender);
            insertPictures();
        }

        private bool separateMultTerms(string givenEqn, out string separatedEqn)
        {
            separatedEqn = givenEqn.Trim();

            //replace vertical bars with abs()
            while (separatedEqn.Contains('|'))
            {
                int openIndex = separatedEqn.IndexOf('|');
                separatedEqn = separatedEqn.Insert(openIndex, "abs(");
                separatedEqn = separatedEqn.Remove(openIndex + 4, 1);
                int closeIndex = separatedEqn.IndexOf('|');
                separatedEqn = separatedEqn.Insert(closeIndex, ")");
                separatedEqn = separatedEqn.Remove(closeIndex + 1, 1);
            }

            //remove outer parens if unnecessary (eg (-10) )
            if (separatedEqn.ElementAt(0).Equals('(') && separatedEqn.ElementAt(separatedEqn.Length - 1).Equals(')'))
            {
                string withoutParens = separatedEqn.Remove(0, 1);
                withoutParens = withoutParens.Remove(withoutParens.Length - 1, 1);
                if (!(withoutParens.Contains('(') || withoutParens.Contains(')')))
                {
                    separatedEqn = withoutParens;
                }
            }

            bool multipleTerms = false;
            int parens = 0;
            int index = 0;
            double result;
            while (index < separatedEqn.Length)
            {
                char cur = separatedEqn.ElementAt(index);
                int inserted = 0;
                if (cur.Equals('('))
                {
                    parens++;
                    if (index != 0)
                    {
                        //if a ( is preceded by a number, insert a * (ie 5(6) -> 5 * (6) )
                        if (double.TryParse(separatedEqn.ElementAt(index - 1) + "", out result))
                        {
                            separatedEqn = separatedEqn.Insert(index, " * ");
                            inserted += 3;
                            multipleTerms = true;
                        }
                        else if (separatedEqn.ElementAt(index - 1).Equals(')'))
                        {
                            //if ( is preceded by ) (eg (8)(9) ) then insert multiplication
                            separatedEqn = separatedEqn.Insert(index, " * ");
                            inserted += 3;
                            multipleTerms = true;
                        }
                    }
                }
                else if (cur.Equals(')'))
                    parens--;
                else if (parens > 0)
                {
                    //inside of a pair of parentheses remove spaces
                    if (cur.Equals(' '))
                    {
                        separatedEqn = separatedEqn.Remove(index, 1);
                        inserted--;
                    }
                }
                else
                {
                    //outside of parentheses add spaces around operators (except ^, !, ||)
                    char[] basicOperators = { '+', '-', '*', '/' };
                    if (basicOperators.Contains(cur))
                    {
                        if (cur.Equals('-'))
                        {
                            //need to differentiate between subtraction or negative number
                            if (index != 0)
                            {
                                bool hasSpace = false;
                                int prevChar = index - 1;
                                if (separatedEqn.ElementAt(index - 1).Equals(' '))
                                {
                                    prevChar = prevChar - 1;
                                    hasSpace = true;
                                }
                                if (prevChar >= 0)
                                {
                                    //check if previous character is a number (eg 8-9) or a paren (eg (8+1)-9)
                                    if (double.TryParse(separatedEqn.ElementAt(prevChar) + "", out result) || separatedEqn.ElementAt(prevChar).Equals(')'))
                                    {
                                        //subraction -> insert spaces
                                        multipleTerms = true;
                                        if (index != separatedEqn.Length - 1)
                                        {
                                            //check for space already after
                                            if (!separatedEqn.ElementAt(index + 1).Equals(' '))
                                            {
                                                separatedEqn = separatedEqn.Insert(index + 1, " ");
                                                inserted += 1;
                                            }
                                        }
                                        if (!hasSpace)
                                        {
                                            separatedEqn = separatedEqn.Insert(index, " ");
                                            inserted += 1;
                                        }
                                    }
                                    else if (separatedEqn.ElementAt(prevChar).Equals('('))
                                    {
                                        //- was preceded by a ( (eg 8(-9) ) therefore its a negative
                                        if (hasSpace)
                                        {
                                            separatedEqn = separatedEqn.Remove(index - 1, 1);
                                            inserted--;
                                        }
                                        if (index != separatedEqn.Length - 1)
                                        {
                                            if (separatedEqn.ElementAt(index + 1).Equals(' '))
                                            {
                                                separatedEqn = separatedEqn.Remove(index + 1, 1);
                                                inserted--;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //index equals 0 means negative, not subtract
                                if (index != separatedEqn.Length - 1)
                                {
                                    if (separatedEqn.ElementAt(index + 1).Equals(' '))
                                    {
                                        separatedEqn = separatedEqn.Remove(index + 1, 1);
                                        inserted--;
                                    }
                                }
                            }
                        }
                        else
                        {
                            multipleTerms = true;
                            //operator is either +, *, or /
                            if (index != separatedEqn.Length - 1)
                            {
                                if (!separatedEqn.ElementAt(index + 1).Equals(' '))
                                {
                                    //insert space after
                                    separatedEqn = separatedEqn.Insert(index + 1, " ");
                                    inserted++;
                                }
                            }
                            if (index != 0)
                            {
                                if (!separatedEqn.ElementAt(index - 1).Equals(' '))
                                {
                                    //insert space before
                                    separatedEqn = separatedEqn.Insert(index, " ");
                                    inserted++;
                                }
                            }
                        }
                    }

                }
                index = index + inserted + 1;
            }
            return multipleTerms;
        }

        private void solveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //Clear the link label panel of links
                linkLabelPanel.Controls.Clear();
                instructionsRTB.Text = "";
                //copy the entered equation from the textbox
                string givenEqn = equationTB.Text.Trim();
                int linkLabelCount = 1;
                if (givenEqn.Length > 0)
                {
                    switch (determineType(givenEqn))
                    {
                        case equationType.noVar:
                            bool multipleTerms = separateMultTerms(givenEqn, out givenEqn);
                            if (multipleTerms)
                            {
                                //add order of operations first then check for other stuff
                                LinkLabel ooo = addLinkLabel("Order of Operations", "OrderOfOperations", linkLabelCount++);
                                if (ooo == null) linkLabelCount--;
                                loadInstructions(ooo);
                            }
                            //if they user entered just a number
                            double parsedNum;
                            if (double.TryParse(givenEqn, out parsedNum))
                            {
                                //Add link label solutions
                                LinkLabel sciNot = addLinkLabel("Write in Scientific Notation", "SciNotation", linkLabelCount++);
                                addLinkLabel("Draw a Factor Tree", "FactorTree", linkLabelCount++);
                                if (sciNot == null) linkLabelCount--;
                                //Load instructions for Scientific Notation
                                loadInstructions(sciNot);
                            }
                            else
                            {
                                if (givenEqn.Contains('!'))
                                {
                                    //if entered a single number with a bang
                                    //remove bang
                                    string strRemovedBang = givenEqn.Remove(givenEqn.IndexOf('!'));
                                    //remove parens if possible
                                    if (strRemovedBang.ElementAt(0).Equals('(') && strRemovedBang.ElementAt(strRemovedBang.Length - 1).Equals(')'))
                                    {
                                        string withoutParens = strRemovedBang.Remove(0, 1);
                                        withoutParens = withoutParens.Remove(withoutParens.Length - 1, 1);
                                        if (!(withoutParens.Contains('(') || withoutParens.Contains(')')))
                                        {
                                            strRemovedBang = withoutParens;
                                        }
                                    }
                                    //check if given was one number
                                    if (double.TryParse(strRemovedBang, out parsedNum))
                                    {
                                        LinkLabel factorial = addLinkLabel("Evaluating Factorial", "Factorial", linkLabelCount++);
                                        loadInstructions(factorial);
                                    }
                                    else
                                    {
                                        //include order of operations
                                        LinkLabel factorial = addLinkLabel("Evaluating Factorial", "Factorial", linkLabelCount++);
                                        if (factorial == null) linkLabelCount--;
                                        LinkLabel ooo = addLinkLabel("Order of Operations", "OrderOfOperations", linkLabelCount++);
                                        if (ooo == null) linkLabelCount--;
                                        loadInstructions(factorial);
                                    }
                                }

                                //Check for absolute value
                                if (givenEqn.Contains("abs") || givenEqn.Contains('|'))
                                {
                                    LinkLabel absLink = addLinkLabel("Absolute Value", "AbsoluteValue", linkLabelCount++);
                                    if (absLink == null) linkLabelCount--;
                                    loadInstructions(absLink);
                                }

                                //Check for sqrt
                                if (givenEqn.Contains("sqrt"))
                                {
                                    LinkLabel sqrt = addLinkLabel("Simplifying Radicals", "Radicals", linkLabelCount++);
                                    if (sqrt == null) linkLabelCount--;
                                    loadInstructions(sqrt);
                                }

                                if (givenEqn.Contains('^'))
                                {
                                    //if entered a number raised to a power
                                    //check exponent for terms, pos, or neg
                                    string exponent = givenEqn.Substring(givenEqn.IndexOf('^') + 1, givenEqn.Length - givenEqn.IndexOf('^') - 1);
                                    bool multiTerm = separateMultTerms(exponent, out exponent);

                                    if (multiTerm)
                                    {
                                        //add order of operations
                                        addLinkLabel("Order of Operations", "OrderOfOperations", linkLabelCount++);
                                        //need to check for fractions
                                        if (exponent.Contains('/'))
                                        {
                                            LinkLabel fracExp = addLinkLabel("Evaluating Fractional Exponents", "FractionExponent", linkLabelCount++);
                                            if (fracExp == null) linkLabelCount--;
                                            LinkLabel rad = addLinkLabel("Simplifying Radicals", "Radicals", linkLabelCount++);
                                            if (rad == null) linkLabelCount--;
                                            loadInstructions(fracExp);
                                        }
                                        else
                                        {
                                            LinkLabel exp = addLinkLabel("Evaluating Exponents", "SimpleExponent", linkLabelCount++);
                                            if (exp == null) linkLabelCount--;
                                            loadInstructions(exp);
                                        }
                                    }
                                    else
                                    {
                                        //default if it includes a ^ show positive exponent
                                        LinkLabel exp = addLinkLabel("Evaluating Exponents", "SimpleExponent", linkLabelCount++);
                                        if (exp == null) linkLabelCount--;
                                        loadInstructions(exp);
                                    }
                                }
                            }
                            insertPictures();
                            break;
                        case equationType.oneVar:
                            instructionsRTB.Text = "You entered a one variable equation";
                            break;
                        case equationType.twoVar:
                            instructionsRTB.Text = "You entered a two variable equation";
                            break;
                        case equationType.multEqn:
                            instructionsRTB.Text = "You entered multiple equations";
                            break;
                        default:
                            instructionsRTB.Text = "Sorry, I don't recognize the problem you entered.";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                instructionsRTB.Text = "There was an error:\n\n" + ex.Message;
            }
        }

    }
}
