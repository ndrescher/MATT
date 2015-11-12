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
        enum operandTypes { numNum, numMult, varMult, varVar, multMult, numVar };

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
                string dir = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\MATT_Instructions.xlsx";
                workbook = workbooks.Open(dir);
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

                this.ActiveControl = equationTB;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private equationType determineType(string enteredEqn, out char var1, out char var2)
        {
            //get rid of "abs" and "sqrt" so they're not mistaken as variables
            enteredEqn = enteredEqn.Replace("abs", "");
            enteredEqn = enteredEqn.Replace("sqrt", "");

            var1 = ' ';
            var2 = ' ';

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
                char v1 = ' ';
                char v2 = ' ';
                int equations = 0;
                foreach (string s in enteredEqn.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        if (!s.Contains('='))
                        {
                            isEqn = false;
                        }
                        int v1Index = s.IndexOfAny(alphabet);
                        if (v1Index != -1)
                        {
                            if(v1.Equals(' ')){
                            v1 = s[v1Index];
                            }
                            string r = s.Replace(v1, ' ');
                            int v2Index = r.IndexOfAny(alphabet);
                            if (v2Index != -1 && v2.Equals(' '))
                            {
                                v2 = r[v2Index];
                            }
                        }
                        equations++;
                    }
                }
                if (isEqn && equations > 1)
                {
                    var1 = v1;
                    var2 = v2;
                    return equationType.multEqn;
                }
            }

            //find the first variable and remove all instances
            char firstVar = enteredEqn[enteredEqn.IndexOfAny(alphabet)];
            string removedFirstVar = enteredEqn.Replace(firstVar, ' ');
            //if there aren't any more letters, it must be a one variable eqn
            if (removedFirstVar.IndexOfAny(alphabet) == -1)
            {
                var1 = firstVar;
                var2 = ' ';
                return equationType.oneVar;
            }
            else
            {
                char secondVar = removedFirstVar[removedFirstVar.IndexOfAny(alphabet)];
                string removedSecondVar = removedFirstVar.Replace(secondVar, ' ');
                if (removedSecondVar.IndexOfAny(alphabet) == -1)
                {
                    var1 = firstVar;
                    var2 = secondVar;
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
            //determine location with adequate spacing
            int verticalSpace = 20;
            int x = 0;
            int y = verticalSpace * (index - 1);
            if (index > 4 && index < 9)
            {
                y = verticalSpace * (index % 5);
                x = 250;
            }
            else if (index > 8)
            {
                x = 500;
                y = verticalSpace * (index % 9);
            }
            //add label at location to panel and return
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

        private string compactCoefficients(string inputEqn, char var1)
        {
            string separatedEqn = inputEqn;
            //compact coefficients (3*x => 3x)
            if (var1 != ' ')
            {
                string[] splitSpace = separatedEqn.Split(' ');
                int totalStars = 0;
                //to be removed keeps track as in remove the first and third * in the equation
                List<int> toBeRemoved = new List<int>();
                for (int i = 0; i < splitSpace.Length; i++)
                {
                    string cur = splitSpace[i];
                    string prev;
                    string next;

                    prev = i > 0 ? splitSpace[i - 1] : "";
                    next = i < splitSpace.Length - 1 ? splitSpace[i + 1] : "";

                    if (cur.Equals("*"))
                    {
                        totalStars++;
                        double coef;
                        if (double.TryParse(prev, out coef))
                        {
                            string sepNext;
                            bool multNext = separateMultTerms(next, var1, out sepNext);
                            //only want to compact if next is x or (x)
                            if (sepNext.Equals(var1.ToString()))
                            {
                                //remove the * and spaces from the separetedEqn
                                toBeRemoved.Add(totalStars);
                            }
                        }
                    }
                }//end for splitSpace

                //remove the indicated * and spaces
                toBeRemoved.Sort();
                for (int j = toBeRemoved.Count - 1; j >= 0; j--)
                {
                    int place = toBeRemoved[j];
                    int removeIdx = separatedEqn.TakeWhile(c => (place -= (c == '*' ? 1 : 0)) > 0).Count();
                    separatedEqn = separatedEqn.Remove(removeIdx - 1, 3);
                }
            }// end if has variable
            return separatedEqn;
        }

        private bool separateMultTerms(string givenEqn, char var1, out string separatedEqn)
        {
            separatedEqn = givenEqn.Trim();

            //trim off the equals sign if there is one
            if (separatedEqn.Contains('='))
            {
                //split and get rid of shorter half
                string longest = "";
                foreach (string s in separatedEqn.Split('='))
                {
                    if (s.Length > longest.Length)
                    {
                        longest = s;
                    }
                }
                //set the equation as the longer string
                separatedEqn = longest.Trim();
            }

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

            //check for multiplying a sqrt ie 9sqrt(x)
            if (separatedEqn.Contains("sqrt("))
            {
                int sqrtIndex = separatedEqn.IndexOf("sqrt(");
                if (sqrtIndex > 0) //not the first term entered
                {
                    char beforeSqrt = separatedEqn.ElementAt(sqrtIndex - 1);
                    double num;
                    if (double.TryParse(beforeSqrt + "", out num))
                    {
                        //sqrt was preceded directly by a number ie 9sqrt( -> insert *
                        separatedEqn = separatedEqn.Insert(sqrtIndex, "*");
                    }
                }
            }

            //remove outer parens if unnecessary (eg (-10x+7) )
            if (separatedEqn.ElementAt(0).Equals('(') && separatedEqn.ElementAt(separatedEqn.Length - 1).Equals(')'))
            {
                string withoutParens = separatedEqn.Remove(0, 1);
                withoutParens = withoutParens.Remove(withoutParens.Length - 1, 1);
                string s = separatedEqn;
                var stack = new Stack<int>();
                bool isSurroundedByParens = false;
                for (int i = 0; i < s.Length; i++)
                {
                    switch (s[i])
                    {
                        case '(':
                            stack.Push(i);
                            isSurroundedByParens = false;
                            break;
                        case ')':
                            int idx = stack.Any() ? stack.Pop() : -1;
                            isSurroundedByParens = (idx == 0);
                            break;
                        default:
                            isSurroundedByParens = false;
                            break;
                    }
                }
                if (isSurroundedByParens)
                {
                    // set as string w/o outer parens
                    separatedEqn = withoutParens;
                }
            }

            //loop through chars in string and insert/remove spaces
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
                        string star = " * ";
                        if (parens > 1)
                        {
                            //this ( is inside parens
                            star = "*";
                        }
                        //if a ( is preceded by a number, insert a * (ie 5(6) -> 5 * (6) )
                        if (double.TryParse(separatedEqn.ElementAt(index - 1) + "", out result))
                        {
                            separatedEqn = separatedEqn.Insert(index, star);
                            inserted += star.Length;
                        }
                        else if (separatedEqn.ElementAt(index - 1).Equals(')'))
                        {
                            //if ( is preceded by ) (eg (8)(9) ) then insert multiplication
                            separatedEqn = separatedEqn.Insert(index, star);
                            inserted += star.Length;
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
                                int prevCharIndex = index - 1;
                                if (separatedEqn.ElementAt(index - 1).Equals(' '))
                                {
                                    prevCharIndex = prevCharIndex - 1;
                                    hasSpace = true;
                                }
                                if (prevCharIndex >= 0)
                                {

                                    //assume is subrtraction unless is (- or <op>-
                                    if (separatedEqn.ElementAt(prevCharIndex).Equals('(') || basicOperators.Contains(separatedEqn.ElementAt(prevCharIndex)))
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
                                    else
                                    {
                                        //check if previous character is a number (eg 8-9) or a paren (eg (8+1)-9)
                                        //if (double.TryParse(separatedEqn.ElementAt(prevCharIndex) + "", out result) || separatedEqn.ElementAt(prevCharIndex).Equals(')'))
                                        {
                                            //subraction -> insert spaces
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

            //get rid of excess * (3*x => 3x)
            separatedEqn = compactCoefficients(separatedEqn, var1);

            if (!separatedEqn.Contains(' '))
                return false;
            else
                return true;
        }

        private double findHighestDegree(string givenEqn, char variable, out bool multDegrees)
        {
            double highestDegree = Int32.MinValue;
            int indexOf = givenEqn.IndexOf(variable);
            while (indexOf != -1)
            {
                //not at the end of the string
                if (indexOf != givenEqn.Length - 1)
                {
                    //check for exponent
                    if (givenEqn.ElementAt(indexOf + 1).Equals('^'))
                    {
                        //get the value of the exponent
                        double expVal = 0;
                        string exponent = givenEqn.Remove(0, indexOf + 2);
                        separateMultTerms(exponent, variable, out exponent);
                        exponent = exponent.Replace('(', ' ');
                        exponent = exponent.Trim();
                        exponent = exponent.Replace(')', ' ');
                        int endOfExp = exponent.IndexOf(' ');
                        if (endOfExp != -1)
                            exponent = exponent.Remove(endOfExp);

                        bool fracExp = separateMultTerms(exponent, variable, out exponent);
                        if (!fracExp)
                        {
                            double parsedExp;
                            if (double.TryParse(exponent, out parsedExp))
                            {
                                //exponent was single number (eg x^2)
                                expVal = parsedExp;
                            }
                        }
                        else
                        {
                            //exponent has operators in it, most likely fractional exponent
                            if (exponent.Contains('/') && !(exponent.Contains('*') || exponent.Contains('+')))
                            {
                                //can't check for - because of negative numbers...
                                double numerator, demoninator;
                                double.TryParse(exponent.Substring(0, exponent.IndexOf('/')).Trim(), out numerator);
                                double.TryParse(exponent.Substring(exponent.IndexOf('/') + 1, exponent.Length - 1 - exponent.IndexOf('/')).Trim(), out demoninator);
                                expVal = numerator / demoninator;
                            }
                        }
                        //check value against currect highestDegree
                        if (highestDegree != Int32.MinValue)
                            multDegrees = true;
                        if (highestDegree < expVal)
                            highestDegree = expVal;
                    }
                    else
                    {
                        //found variable without exponent
                        if (highestDegree != Int32.MinValue)
                            multDegrees = true;
                        if (highestDegree <= 1)
                            highestDegree = 1;
                    }
                }
                else
                {
                    //the variable was at the end of the string (no ^)
                    if (highestDegree != Int32.MinValue)
                        multDegrees = true;
                    if (highestDegree <= 1)
                        highestDegree = 1;
                }
                givenEqn = givenEqn.Remove(indexOf, 1);
                indexOf = givenEqn.IndexOf(variable);
            }
            multDegrees = false;
            return highestDegree;

        }

        private int solveOneVarOneTerm(string separatedEqn, char variable1, int linkLabelCount)
        {
            //build list of indices of variables
            string copySepEqn = separatedEqn;
            List<int> indicesOfVar = new List<int>();
            int curIndex = copySepEqn.LastIndexOf(variable1);
            while (curIndex != -1)
            {
                indicesOfVar.Add(curIndex);
                copySepEqn = copySepEqn.Remove(curIndex, 1);
                curIndex = copySepEqn.LastIndexOf(variable1);
            }
            foreach (int indexOfVariable in indicesOfVar)
            {
                //check for coefficient
                //int indexOfVariable = separatedEqn.IndexOf(variable1);
                char afterVar = '\n'; //cannot enter a \n in the textbox
                string inFrontOfVar = "\n";
                if (indexOfVariable != -1 && indexOfVariable != 0)
                {
                    inFrontOfVar = separatedEqn.Substring(0, indexOfVariable);
                    //must be a number or ( logically or else would be multiple terms
                    if (!inFrontOfVar.Equals('('))
                    {
                        if (inFrontOfVar.Contains("sqrt"))
                        {
                            LinkLabel sqrt = addLinkLabel("Remove Radicals", "RadicalsEqn", linkLabelCount++);
                            if (sqrt == null) linkLabelCount--;
                            loadInstructions(sqrt);
                        }

                        inFrontOfVar = inFrontOfVar.Trim();
                        int coefIndex = inFrontOfVar.LastIndexOf(' ');
                        if (coefIndex == -1) // if no space then its just a number
                            coefIndex = 0;
                        string coefficient = inFrontOfVar.Substring(coefIndex); //coef is from space to end
                        double coefNum;
                        if (coefficient.Equals("*") || double.TryParse(coefficient, out coefNum))
                        {
                            LinkLabel coef = addLinkLabel("Solve Equation with Coefficients", "DivideBothSides", linkLabelCount++);
                            if (coef == null) linkLabelCount--;
                            loadInstructions(coef);
                        }
                    }
                }

                if (indexOfVariable != separatedEqn.Length - 1)
                    afterVar = separatedEqn.ElementAt(indexOfVariable + 1);

                //if x^power get rid of with radical
                if (afterVar.Equals('^'))
                {
                    LinkLabel exp = addLinkLabel("Solve Equation with Exponents", "SqrtBothSides", linkLabelCount++);
                    if (exp == null) linkLabelCount--;
                    loadInstructions(exp);
                }

                if (afterVar.Equals('\n') && inFrontOfVar.Equals("\n"))
                {
                    //entered x by itself....graph?
                    linkLabelPanel.Controls.Clear();//should be only "set to 0 to find roots"
                    instructionsRTB.Text = "I need more information about your problem before I can try to help you. \n\n-Matt";
                }
            }
            return linkLabelCount;
        }

        private int oneVarMultipleTerms(int linkLabelCount, char variable1, string separatedEqn)
        {
            List<string> possibleOperators = new List<string>();
            string[] opsArray = { "=", "+", "-", "/", "*", "^", "|", "!" };
            possibleOperators.AddRange(opsArray);

            List<string> usedOperators = new List<string>();
            List<operandTypes> usedOperandTypes = new List<operandTypes>();
            string[] splitEqn = separatedEqn.Split(' ');

            //make a list of the operators and the types of operands used in the equation
            for (int i = 0; i < splitEqn.Length; i++)
            {
                //set variables for each part of an expression <operator> <operand> <operator>
                string currentTerm = splitEqn[i];
                string prevTerm = "";
                string nextTerm = "";
                if (i != 0)
                    prevTerm = splitEqn[i - 1];
                if (i + 1 != splitEqn.Length)
                    nextTerm = splitEqn[i + 1];

                //if term is an operator
                if (possibleOperators.Contains(currentTerm))
                {
                    //add this operator to the list of operators used in the equation
                    usedOperators.Add(currentTerm);

                    //make sure the operatorands are both present
                    if (!string.IsNullOrWhiteSpace(prevTerm) && !string.IsNullOrWhiteSpace(nextTerm))
                    {
                        //separate terms to see if mult or single
                        string separatePrev = "";
                        bool multPrevTerms = separateMultTerms(prevTerm, variable1, out separatePrev);
                        string separateNext = "";
                        bool multNextTerms = separateMultTerms(nextTerm, variable1, out separateNext);

                        //if mult terms then check if could be simplified
                        bool combineTerms = false;
                        if (multPrevTerms)
                        {
                            List<int> degrees = listDegreesR(variable1, separatePrev);
                            if (degrees.Count != degrees.Distinct().Count())
                            {
                                if (!separatePrev.Contains("/") && !separatePrev.Contains("*"))
                                {
                                    combineTerms = true;
                                }
                            }
                        }
                        if (multNextTerms)
                        {
                            List<int> degrees = listDegreesR(variable1, separateNext);
                            if (degrees.Count != degrees.Distinct().Count())
                            {
                                if (!separateNext.Contains("/") && !separateNext.Contains("*"))
                                    combineTerms = true;
                            }
                        }

                        if (combineTerms)
                        {
                            LinkLabel combine = addLinkLabel("Combine Like Terms", "CombineLikeTerms", linkLabelCount++);
                            if (combine == null) linkLabelCount--;
                            loadInstructions(combine);
                        }

                        //check if constant or variable operands

                        //both terms contain the variable
                        if (prevTerm.Contains(variable1) && nextTerm.Contains(variable1))
                        {
                            //check if single or multiple terms in operands

                            //both variables, both multiple terms
                            if (multNextTerms && multPrevTerms)
                                usedOperandTypes.Add(operandTypes.multMult);
                            //both variable, only one is multiple terms
                            else if (multNextTerms ^ multPrevTerms)
                                usedOperandTypes.Add(operandTypes.varMult);
                            //both variable, neither is multiple terms
                            else
                                usedOperandTypes.Add(operandTypes.varVar);
                        }
                        //only one of the terms contains the variable
                        else if (prevTerm.Contains(variable1) ^ nextTerm.Contains(variable1))
                        {
                            //check for single or multiple terms in operands 
                            //(if mult term has no var, then its considered a single number)

                            //neither are multiple terms
                            if (!multPrevTerms && !multNextTerms)
                                usedOperandTypes.Add(operandTypes.numVar);
                            //both are mult, but one has no var
                            else if (multPrevTerms && multPrevTerms)
                                usedOperandTypes.Add(operandTypes.numMult);
                            //if one is mult, we need to know if the mult has a var or not
                            else
                            {
                                //the multiple terms operand has the variable, the other operand must be a number
                                if ((multPrevTerms && prevTerm.Contains(variable1)) || (multNextTerms && nextTerm.Contains(variable1)))
                                    usedOperandTypes.Add(operandTypes.numMult);
                                //otherwise, the multiple terms operand does not have the var and becomes a number
                                else
                                    usedOperandTypes.Add(operandTypes.numVar);
                            }
                        }
                        //neither of the operands has a variable
                        else
                        {
                            //dont need to check for multiple terms because without a var they woule become numbers
                            usedOperandTypes.Add(operandTypes.numNum);
                        }
                    }
                }
            } // end for loop

            //build instructions based on the operand types found
            if (usedOperandTypes.Contains(operandTypes.numNum))
            {
                instructionsRTB.Text += "Combine all constant terms to simplify the equation. Constant terms are any"
                    + " terms that do not include a variable.\n\n";
            }

            //var <op> var type
            if (usedOperandTypes.Contains(operandTypes.varVar))
            {
                //make a list of the indices of varVar operations
                List<operandTypes> copyUsedOperands = new List<operandTypes>();
                foreach (operandTypes ot in usedOperandTypes)
                {
                    copyUsedOperands.Add(ot);
                }
                List<int> indices = new List<int>(); //list of indexes of varVar in usedOperandTypes
                int curVV = copyUsedOperands.LastIndexOf(operandTypes.varVar);
                while (curVV != -1)
                {
                    indices.Add(curVV);
                    copyUsedOperands.RemoveAt(curVV);
                    curVV = copyUsedOperands.LastIndexOf(operandTypes.varVar);
                }
                //indices of operandsTypes should match up with those in usedOperators
                List<string> operatorsWithVarVar = new List<string>();
                foreach (int index in indices)
                {
                    operatorsWithVarVar.Add(usedOperators.ElementAt(index));
                }

                //if x+x || x-x and same degree, combine like terms
                if (operatorsWithVarVar.Contains("+") || operatorsWithVarVar.Contains("-"))
                {
                    //get list of degrees to check for terms with same degree
                    List<int> degrees = listDegreesR(variable1, separatedEqn);
                    if (degrees.Count != degrees.Distinct().Count())
                    {
                        if (!separatedEqn.Contains("sqrt(") && !separatedEqn.Contains(")^"))
                        {
                            //there are terms with the same degree, combine them
                            LinkLabel combine = addLinkLabel("Combine Like Terms", "CombineLikeTerms", linkLabelCount++);
                            if (combine == null) linkLabelCount--;
                            loadInstructions(combine);
                        }
                    }
                    //3x^2+x/2
                }
                //if x*x || x/x, cancel or combine degrees
                if (operatorsWithVarVar.Contains("*") || operatorsWithVarVar.Contains("/"))
                {
                    //Need to check for sqrt! 3x/Sqrt(4x+1) means multiply by the conjugate
                    string denom = getDenominator(splitEqn);
                    //check for sqrt in denominator
                    if (denom.Contains("sqrt"))
                    {
                        //multiply by conjugate
                        LinkLabel conjugate = addLinkLabel("Rationalize the Denominator", "Rationalize", linkLabelCount++);
                        if (conjugate == null) linkLabelCount--;
                        loadInstructions(conjugate);
                    }
                    else
                    {
                        LinkLabel multDiv = addLinkLabel("Multiplying and Dividing Variables", "MultDivVariables", linkLabelCount++);
                        if (multDiv == null) linkLabelCount--;
                        loadInstructions(multDiv);
                    }
                }
            }// end varVar

            if (separatedEqn.IndexOf('/') == -1 && separatedEqn.IndexOf('*') == -1) // if it does not have * or /
            {
                //only operands were + and/or -
                List<int> degrees = listDegreesR(variable1, separatedEqn);
                double highestDegree = degrees.Max();
                bool combineTerms = false;
                if (degrees.Count != degrees.Distinct().Count())
                    combineTerms = true;
                if (combineTerms)
                {
                    LinkLabel combine = addLinkLabel("Combine Like Terms", "CombineLikeTerms", linkLabelCount++);
                    if (combine == null) linkLabelCount--;
                    loadInstructions(combine);
                }
                if (degrees.Distinct().Count() > 1)
                {
                    //still have mult terms after combining
                    if (highestDegree > 1)
                    {
                        //(3x+1)^2 + 9. remove ^ and check for multTerms?
                        int pwrTermIdx = 0;
                        foreach (string term in splitEqn)
                        {
                            if (term.Contains("^"))
                                break;
                            pwrTermIdx++;
                        }
                        string pwrTerm = splitEqn.ElementAt(pwrTermIdx);
                        string woPower = pwrTerm.Remove(pwrTerm.IndexOf("^"));

                        string woPwrSep;
                        bool multExpBase = separateMultTerms(woPower, variable1, out woPwrSep);

                        if (multExpBase)
                        {
                            //need to expand the polynomial
                            LinkLabel foil = addLinkLabel("Expanding a Polynomial", "FOIL", linkLabelCount++);
                            if (foil == null) linkLabelCount--;
                            loadInstructions(foil);

                            //then combine like terms
                            LinkLabel combine = addLinkLabel("Combine Like Terms", "CombineLikeTerms", linkLabelCount++);
                            if (combine == null) linkLabelCount--;
                            loadInstructions(combine);

                            //next would probably be either factor or move constants...
                        }
                        else
                        {
                            if (usedOperandTypes.Distinct().Count() > 1)// has varVar but also numVar at least
                            {
                                //ex: 4x^2-3x+9 factor to solve
                                LinkLabel factor = addLinkLabel("Factoring Polynomial Equations", "Factoring", linkLabelCount++);
                                if (factor == null) linkLabelCount--;
                                loadInstructions(factor);
                            }
                            else
                            {
                                //only has one type of operands
                                if (usedOperandTypes.Contains(operandTypes.varVar))
                                {
                                    //3x^2+x
                                    LinkLabel pull = addLinkLabel("Factoring Out a Common Term", "PullOutAnX", linkLabelCount++);
                                    if (pull == null) linkLabelCount--;
                                    loadInstructions(pull);
                                }
                                else if (usedOperandTypes.Contains(operandTypes.numVar))
                                {
                                    //3x^2+5
                                    //no * or / to be here, one operandType = numVar, more than one degree
                                    LinkLabel constants = addLinkLabel("Remove Contant Terms", "RemoveConstants", linkLabelCount++);
                                    if (constants == null) linkLabelCount--;
                                    loadInstructions(constants);

                                    //check for coefficients and powers
                                    string varTerm = separatedEqn;
                                    foreach (string term in splitEqn)
                                    {
                                        if (term.Contains(variable1))
                                            varTerm = term;
                                    }
                                    linkLabelCount = solveOneVarOneTerm(varTerm, variable1, linkLabelCount);
                                }
                            }
                        }
                    }
                    else
                    {
                        string givenSepEqn;
                        bool multGivenDegrees = separateMultTerms(equationTB.Text, variable1, out givenSepEqn);
                        //ex: 3x+7 move constants to solve
                        if (givenSepEqn.Length == separatedEqn.Length) // only these three terms given
                        {
                            LinkLabel solve = addLinkLabel("Remove Constant Terms", "RemoveConstants", linkLabelCount++);
                            if (solve == null) linkLabelCount--;
                            loadInstructions(solve);

                            linkLabelCount = solveOneVarOneTerm(separatedEqn, variable1, linkLabelCount);
                        }
                    }
                }
            }
            else if (usedOperandTypes.Contains(operandTypes.numVar))
            {
                //has * or / and has numVar need to check that the / or * is actually the operator for numVar
                //make a list of the indices of numVar operations
                List<operandTypes> copyUsedOperands = new List<operandTypes>();
                foreach (operandTypes ot in usedOperandTypes)
                {
                    copyUsedOperands.Add(ot);
                }
                List<int> indices = new List<int>(); //list of indexes of numVar in usedOperandTypes
                int curNM = copyUsedOperands.LastIndexOf(operandTypes.numVar);
                while (curNM != -1)
                {
                    indices.Add(curNM);
                    copyUsedOperands.RemoveAt(curNM);
                    curNM = copyUsedOperands.LastIndexOf(operandTypes.numVar);
                }
                //indices of operandTypes should match up with those in usedOperators
                List<string> operatorsWithNV = new List<string>();
                foreach (int index in indices)
                {
                    operatorsWithNV.Add(usedOperators.ElementAt(index));
                }


                string denom = getDenominator(splitEqn); //returns "" if not /
                if (operatorsWithNV.Contains("*") || operatorsWithNV.Contains("/"))
                {
                    //check for sqrt in denomiator
                    if (denom.Contains("sqrt"))
                    {
                        //multiply by the conjugate
                        LinkLabel conjugate = addLinkLabel("Rationalize the Denominator", "Rationalize", linkLabelCount++);
                        if (conjugate == null) linkLabelCount--;
                        loadInstructions(conjugate);
                    }
                    else
                    {
                        bool numVarAddSub = false;
                        if (operatorsWithNV.Contains("+") || operatorsWithNV.Contains("-"))
                        {
                            numVarAddSub = true;
                            LinkLabel remove = addLinkLabel("Remove Constants", "RemoveConstants", linkLabelCount++);
                            if (remove == null) linkLabelCount--;
                            loadInstructions(remove);
                        }
                        else
                        {
                            //only * or /
                            if (separatedEqn.Contains("sqrt") && !string.IsNullOrEmpty(denom))
                            {
                                LinkLabel ll = addLinkLabel("Manipulating Coefficients", "MultDivNumVar", linkLabelCount++);
                                if (ll == null) linkLabelCount--;
                                loadInstructions(ll);


                                LinkLabel coef = addLinkLabel("Remove Coefficient", "DivideBothSides", linkLabelCount++);
                                if (coef == null) linkLabelCount--;
                                loadInstructions(coef);
                            }
                        }

                        //numVar with * or / without sqrt in denom
                        if (separatedEqn.Contains("* sqrt"))
                        {
                            LinkLabel coef = addLinkLabel("Remove Coefficient", "DivideBothSides", linkLabelCount++);
                            if (coef == null) linkLabelCount--;
                            loadInstructions(coef);
                        }
                        else
                        {
                            List<int> degrees = listDegreesR(variable1, separatedEqn);
                            int highDeg = degrees.Max();
                            if (degrees.Distinct().Count() == degrees.Max() + 1)
                            {
                                //2,1,0 count =3, a term for each degree
                                if (numVarAddSub && highDeg > 1)// || usedOperandTypes.Contains(operandTypes.numMult))
                                {
                                    //numVar operator is + or - NOT / or *
                                    LinkLabel factor = addLinkLabel("Factoring Polynomial Equations", "Factoring", linkLabelCount++);
                                    if (factor == null) linkLabelCount--;
                                    loadInstructions(factor);
                                }
                                else
                                {
                                    //3x^2+x/2 need to check for mult of same type of operand
                                    if (usedOperandTypes.Contains(operandTypes.varVar) || usedOperandTypes.Contains(operandTypes.multMult))
                                    {
                                        LinkLabel pull = addLinkLabel("Factoring Out a Common Term", "PullOutAnX", linkLabelCount++);
                                        if (pull == null) linkLabelCount--;
                                        loadInstructions(pull);
                                    }
                                    else
                                    {
                                        //has numVar with * or /; does not have varVar, numNum, or multMult which means could have numMult or varMult
                                        //x/2
                                        if (splitEqn.Length == 3)
                                        {
                                            //just one numVar operation
                                            LinkLabel mult = addLinkLabel("Remove Constants in the Denominator", "RemoveDenomConst", linkLabelCount++);
                                            if (mult == null) linkLabelCount--;
                                            loadInstructions(mult);
                                        }
                                        else
                                        {
                                            //3(x-1)+x/3
                                            LinkLabel mult = addLinkLabel("Manipulating Coefficients", "MultDivNumVar", linkLabelCount++);
                                            if (mult == null) linkLabelCount--;
                                            loadInstructions(mult);

                                            LinkLabel coef = addLinkLabel("Remove Constants in the Denominator", "RemoveDenomConst", linkLabelCount++);
                                            if (coef == null) linkLabelCount--;
                                            loadInstructions(coef);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //known: does not have term of each degree; does not have * sqrt; no sqrt denom; has * or / with numVar
                                //9*x^2+3;  3+2*x^2 but not 3+2x^2;   3x/2 not x/2
                                //for some reason doesnt come here is no coef...
                                if (operatorsWithNV.Contains("/"))
                                {
                                    LinkLabel ll = addLinkLabel("Manipulating Coefficients", "MultDivNumVar", linkLabelCount++);
                                    if (ll == null) linkLabelCount--;
                                    loadInstructions(ll);
                                }

                                LinkLabel coef = addLinkLabel("Remove Coefficient", "DivideBothSides", linkLabelCount++);
                                if (coef == null) linkLabelCount--;
                                loadInstructions(coef);

                                if (separatedEqn.Contains(variable1 + "^"))
                                {
                                    LinkLabel sqrtBoth = addLinkLabel("Removing Exponents", "SqrtBothSides", linkLabelCount++);
                                    if (sqrtBoth == null) linkLabelCount--;
                                    loadInstructions(sqrtBoth);
                                }
                            }
                        }
                    }

                    if (separatedEqn.Contains("sqrt"))
                    {
                        LinkLabel sqrt = addLinkLabel("Remove Radicals", "RadicalsEqn", linkLabelCount++);
                        if (sqrt == null) linkLabelCount--;
                        loadInstructions(sqrt);
                    }
                }
                else
                {
                    //we know it has / or * but it's not spaced so must have *or/ in subterm that
                    //is regarded as a single term (sqrt or ()^#) and then -or+ another term
                    //mult/div means either simplifyCoef or MultDivNumVar
                    if (separatedEqn.Contains("sqrt"))
                    {
                        //check for simplifying inside radical.
                        string radical;
                        int sqrtIndex = separatedEqn.IndexOf("sqrt(");
                        string woSqrt = separatedEqn.Substring(sqrtIndex + 5);
                        radical = woSqrt.Remove(woSqrt.IndexOf(")"));

                        string sepRad;
                        bool multTerms = separateMultTerms(radical, variable1, out sepRad);
                        if (multTerms)
                            linkLabelCount = oneVarMultipleTerms(linkLabelCount, variable1, sepRad);

                        //remove constants and then square both sides
                        if (splitEqn.Length <= 3)
                        {
                            //remove constants and sqrt both sides
                            LinkLabel remove = addLinkLabel("Remove Constants", "RemoveConstants", linkLabelCount++);
                            if (remove == null) linkLabelCount--;
                            loadInstructions(remove);

                            LinkLabel sqrt = addLinkLabel("Remove Radicals", "RadicalsEqn", linkLabelCount++);
                            if (sqrt == null) linkLabelCount--;
                            loadInstructions(sqrt);
                        }
                        else
                        {
                            string afterSqrt = woSqrt.Substring(woSqrt.IndexOf(")") + 2); //add everything after closing paren except space
                            bool multiplication = false;
                            bool addSub = false;
                            if (sqrtIndex > 0)
                            {
                                //need to look at function before
                                if (separatedEqn.Contains("* sqrt"))
                                {
                                    //coef
                                    multiplication = true;
                                }
                                else if (separatedEqn.Contains("+ sqrt") || separatedEqn.Contains("- sqrt"))
                                {
                                    //constant or variable? move everything to the other side before squaring?
                                    addSub = true;
                                }
                            }

                            //look at function after
                            if (afterSqrt.Length > 0)
                            {
                                char opAfterSqrt = afterSqrt[0];
                                if (opAfterSqrt.Equals('*'))
                                {
                                    multiplication = true;

                                }
                                else if (opAfterSqrt.Equals('+') || opAfterSqrt.Equals('-'))
                                {
                                    //constant or variable? move everything to the other side before squaring?
                                    addSub = true;

                                }
                            }

                            //instructions for other terms
                            if (multiplication)
                            {
                                LinkLabel coef = addLinkLabel("Remove Coefficients", "DivideBothSides", linkLabelCount++);
                                if (coef == null) linkLabelCount--;
                                loadInstructions(coef);
                            }

                            if (addSub)
                            {
                                instructionsRTB.Text += "Move all the terms to the opposite side of the radical before raising both"
                                    + " sides of the equation to the reciprical of the power of the radical.";
                                LinkLabel terms = addLinkLabel("Move All Terms to Other Side", "RemoveConstants", linkLabelCount++);
                                if (terms == null) linkLabelCount--;
                                loadInstructions(terms);
                            }

                            //remove the sqrt
                            LinkLabel sqrt = addLinkLabel("Remove Radicals", "RadicalsEqn", linkLabelCount++);
                            if (sqrt == null) linkLabelCount--;
                            loadInstructions(sqrt);
                        }

                    }
                    else if (separatedEqn.Contains(")^"))
                    {
                        //check for simplification or else foil
                        string expBase = separatedEqn.Remove(separatedEqn.IndexOf(")^"));
                        int startParenIndex;
                        for (startParenIndex = expBase.Length - 1; startParenIndex > 0; startParenIndex--)
                        {
                            char cur = expBase[startParenIndex];
                            if (cur.Equals("("))
                                break;
                        }
                        expBase = expBase.Substring(startParenIndex + 1);
                        string sepExpBase;
                        bool multBase = separateMultTerms(expBase, variable1, out sepExpBase);
                        //we know it has / or *, if it also has + or - then we need to foil
                        if (sepExpBase.Contains("+") || sepExpBase.Contains("-"))
                        {
                            LinkLabel foil = addLinkLabel("Expaning Polynomial Equations", "FOIL", linkLabelCount++);
                            if (foil == null) linkLabelCount--;
                            loadInstructions(foil);

                            LinkLabel combine = addLinkLabel("Combine Like Terms", "CombineLikeTerms", linkLabelCount++);
                            if (combine == null) linkLabelCount--;
                            loadInstructions(combine);

                            //probably factor again?
                            LinkLabel factor = addLinkLabel("Factoring Polynomials", "Factoring", linkLabelCount++);
                            if (factor == null) linkLabelCount--;
                            loadInstructions(factor);
                        }
                        else
                        {
                            //only has * or / in base
                            LinkLabel coef = addLinkLabel("Simplify the Coefficient", "MultDivNumVar", linkLabelCount++);
                            if (coef == null) linkLabelCount--;
                            loadInstructions(coef);

                            LinkLabel fracPwr = addLinkLabel("Applying Exponents to Fractions", "FracToPower", linkLabelCount++);
                            if (fracPwr == null) linkLabelCount--;
                            loadInstructions(fracPwr);

                            LinkLabel constants = addLinkLabel("Remove Constants", "RemoveConstants", linkLabelCount++);
                            if (constants == null) linkLabelCount--;
                            loadInstructions(constants);

                            LinkLabel coefRemove = addLinkLabel("Remove Coefficient", "DivideBothSides", linkLabelCount++);
                            if (coefRemove == null) linkLabelCount--;
                            loadInstructions(coefRemove);
                        }
                    }
                    else
                    {
                        //3x + 9/x + 1
                        //any numVar with division and another variable term
                        List<int> degrees = listDegreesR(variable1, separatedEqn);
                        if (degrees.Count != degrees.Distinct().Count())
                        {
                            LinkLabel combine = addLinkLabel("Combine Like Terms", "CombineLikeTerms", linkLabelCount++);
                            if (combine == null) linkLabelCount--;
                            loadInstructions(combine);
                        }
                        else
                        {
                            LinkLabel pull = addLinkLabel("Factor-Out a Common Term", "PullOutAnX", linkLabelCount++);
                            if (pull == null) linkLabelCount--;
                            loadInstructions(pull);
                        }
                    }
                }
            }

            //for numMult 3*(x+1) or varMult (x+1)/x
            if (usedOperandTypes.Contains(operandTypes.numMult) || usedOperandTypes.Contains(operandTypes.varMult))
            {
                //make a list of the indices of numMult operations
                List<operandTypes> copyUsedOperands = new List<operandTypes>();
                foreach (operandTypes ot in usedOperandTypes)
                {
                    copyUsedOperands.Add(ot);
                }
                List<int> indices = new List<int>(); //list of indexes of numMult in usedOperandTypes
                int curNM = copyUsedOperands.LastIndexOf(operandTypes.numMult);
                while (curNM != -1)
                {
                    indices.Add(curNM);
                    copyUsedOperands.RemoveAt(curNM);
                    curNM = copyUsedOperands.LastIndexOf(operandTypes.numMult);
                }
                //add to list the indices of varMult operations
                copyUsedOperands.Clear(); //reset the list to check for varMult
                foreach (operandTypes ot in usedOperandTypes)
                {
                    copyUsedOperands.Add(ot);
                }
                int curVM = copyUsedOperands.LastIndexOf(operandTypes.varMult);
                while (curVM != -1)
                {
                    indices.Add(curVM);
                    copyUsedOperands.RemoveAt(curVM);
                    curVM = copyUsedOperands.LastIndexOf(operandTypes.varMult);
                }

                //indices of operandTypes should match up with those in usedOperators
                List<string> operatorsWithNMVM = new List<string>();
                foreach (int index in indices)
                {
                    operatorsWithNMVM.Add(usedOperators.ElementAt(index));
                }

                //if operator is * then distribute
                if (operatorsWithNMVM.Contains("*"))
                {
                    LinkLabel dist = addLinkLabel("Distribute", "Distribution", linkLabelCount++);
                    if (dist == null) linkLabelCount--;
                    loadInstructions(dist);
                }
                //if operator is / then look for cancellation
                if (operatorsWithNMVM.Contains("/"))
                {
                    string denom = getDenominator(splitEqn);
                    //check for sqrt in denominator
                    if (denom.Contains("sqrt"))
                    {
                        //multiply by conjugate
                        LinkLabel conjugate = addLinkLabel("Rationalize the Denominator", "Rationalize", linkLabelCount++);
                        if (conjugate == null) linkLabelCount--;
                        loadInstructions(conjugate);
                    }
                    else
                    {
                        //partial fraction decomposition?
                        LinkLabel cancel = addLinkLabel("Simplifying Quotients", "CancelDivision", linkLabelCount++);
                        if (cancel == null) linkLabelCount--;
                        loadInstructions(cancel);
                    }
                }
                //if operator is + or - then need to look at mult term op. (mult) +/- (num/var)
                if (operatorsWithNMVM.Contains("+") || operatorsWithNMVM.Contains("-"))
                {
                    //got here: (3x+x)-4 -> combine (part correct), (3x/x)-4 ->combine, 
                    //(3x/8)-4 -> nothing, (3x+9)-4->correct: combine, const, coef
                    //(3x/9) + 4x - 3, ((3x-9)/(7x+5)) + 9
                    //should get mult term and put it into oneVarMultTerms?
                    foreach (string term in splitEqn)
                    {
                        if (term.Contains("(") && term.Contains(")"))
                        {
                            string sepTerm = "";
                            bool multPrevTerms = separateMultTerms(term, variable1, out sepTerm);
                            linkLabelCount = oneVarMultipleTerms(linkLabelCount, variable1, sepTerm);
                        }
                    }

                    bool multDegreesSepEqn = false;
                    int highestDegreeSepEqn;
                    List<int> degreesSepEqn = listDegreesR(variable1, separatedEqn);
                    if (degreesSepEqn.Distinct().Count() > 1) multDegreesSepEqn = true;
                    highestDegreeSepEqn = degreesSepEqn.Max();

                    //need to add further instructions, remove constants or factor
                    if (splitEqn.Length == 3) //just the mult, op, and var/num ie (3x/7)+3
                    {
                        //if only one has a variable (numMult)
                        if (splitEqn.ElementAt(0).Contains(variable1) ^ splitEqn.ElementAt(2).Contains(variable1))
                        {
                            //need to check for factoring
                            string multTerm;
                            if (splitEqn.ElementAt(0).Contains(variable1))
                                multTerm = splitEqn.ElementAt(0);
                            else
                                multTerm = splitEqn.ElementAt(2);
                            bool multDegrees;
                            double highestDegree = findHighestDegree(multTerm, variable1, out multDegrees);

                            //check if foil instructions are there
                            bool foil = false;
                            List<Control> controls = new List<Control>();
                            foreach (Control c in linkLabelPanel.Controls)
                            {
                                controls.Add(c);
                            }
                            if (controls.Any(item => item.Tag.Equals("FOIL")))
                            {
                                foil = true;
                            }

                            if (highestDegree >= 2 || foil)
                            {
                                LinkLabel combine = addLinkLabel("Combine Like Terms", "CombineLikeTerms", linkLabelCount++);
                                if (combine == null) linkLabelCount--;
                                loadInstructions(combine);

                                LinkLabel factor = addLinkLabel("Factoring Polynomials", "Factoring", linkLabelCount++);
                                if (factor == null) linkLabelCount--;
                                loadInstructions(factor);
                            }
                            else
                            {
                                LinkLabel remove = addLinkLabel("Remove Contant Terms", "RemoveConstants", linkLabelCount++);
                                if (remove == null) linkLabelCount--;
                                loadInstructions(remove);

                                if (!controls.Any(item => item.Tag.Equals("RemoveDenomConst")))
                                {
                                    //need to make sure actually has coef ie (x+3)-9; (x+3)-9x doesnt get here but (x+3x)-9 does
                                    string[] varSplit = separatedEqn.Split(variable1);
                                    bool multVars = varSplit.Count() > 2;
                                    bool hasCoef = false;
                                    if (!multVars)
                                    {
                                        for (int i = 0; i < varSplit.Length - 1; i++)//the last one is what is after the var, not before
                                        {
                                            string beforeVar = varSplit[i];
                                            beforeVar = beforeVar.Trim();
                                            char lastChar = beforeVar.ElementAt(beforeVar.Length - 1);
                                            if (lastChar.Equals('*'))
                                            {
                                                lastChar = beforeVar.ElementAt(beforeVar.Length - 3);//move to the operand -> # *
                                            }
                                            double result;
                                            hasCoef = double.TryParse(lastChar + "", out result);
                                        }
                                    }

                                    //if has coef or mult x's
                                    if (multVars || hasCoef)
                                    {
                                        LinkLabel coef = addLinkLabel("Removing Coefficients", "DivideBothSides", linkLabelCount++);
                                        if (coef == null) linkLabelCount--;
                                        loadInstructions(coef);
                                    }
                                }
                            }
                        }
                    }
                    else if (multDegreesSepEqn)
                    {
                        double totTermsToFactor = 2 * highestDegreeSepEqn + 1;
                        if (splitEqn.Length == totTermsToFactor)
                        {
                            //if (degreesSepEqn.Distinct().Count() >= highestDegreeSepEqn + 1) //(hopefully)at least a term for each degree?
                            {
                                LinkLabel factor = addLinkLabel("Factoring Polynomials", "Factoring", linkLabelCount++);
                                if (factor == null) linkLabelCount--;
                                loadInstructions(factor);
                            }
                        }
                    }
                }
            } // end varMult || numMult

            if (usedOperandTypes.Contains(operandTypes.multMult))
            {
                //make a list of the indices of multMult operations
                List<operandTypes> copyUsedOperands = new List<operandTypes>();
                foreach (operandTypes ot in usedOperandTypes)
                {
                    copyUsedOperands.Add(ot);
                }
                List<int> indices = new List<int>(); //list of indexes of varVar in usedOperandTypes
                int curVV = copyUsedOperands.LastIndexOf(operandTypes.multMult);
                while (curVV != -1)
                {
                    indices.Add(curVV);
                    copyUsedOperands.RemoveAt(curVV);
                    curVV = copyUsedOperands.LastIndexOf(operandTypes.multMult);
                }
                //indices of operandsTypes should match up with those in usedOperators
                List<string> operatorsWithMM = new List<string>();
                foreach (int index in indices)
                {
                    operatorsWithMM.Add(usedOperators.ElementAt(index));
                }

                //if +||- ignore parens and combine
                if (operatorsWithMM.Contains("-") || operatorsWithMM.Contains("+"))
                {
                    instructionsRTB.Text += "When adding or subtracting expressions, you can ignore the parenthesis and combine like terms normally."
                        + " For example, (3x + 1) + (4x - 2) is the same as 3x + 1 + 4x - 2 = 7x - 1. Remember that if you are subtracting a polynomial,"
                        + " to distribute the negative sign.\n\n";
                }
                //if * FOIL
                if (operatorsWithMM.Contains("*"))
                {
                    LinkLabel foil = addLinkLabel("Multiplying Polynomials", "FOIL", linkLabelCount++);
                    if (foil == null) linkLabelCount--;
                    loadInstructions(foil);
                }
                //if / look into numerators and denominators
                if (operatorsWithMM.Contains("/"))
                {
                    //improvement: look at demoninator and numerator for specific instructions
                    LinkLabel cancel = addLinkLabel("Simplifying Quotients", "CancelDivision", linkLabelCount++);
                    if (cancel == null) linkLabelCount--;
                    loadInstructions(cancel);
                }
            } // end if multMult

            return linkLabelCount;
        }

        private static string getDenominator(string[] splitEqn)
        {
            //find denominator
            int divIndex = 0;
            foreach (string term in splitEqn)
            {
                if (term.Equals("/"))
                    break;
                divIndex++;
            }
            string denom = "";
            if (divIndex != 0 && divIndex != splitEqn.Length)
            {
                denom = splitEqn.ElementAt(divIndex + 1);
            }
            return denom;
        }

        private List<int> listDegreesR(char variable1, string separatedEqn)
        {
            List<string> ops = new List<string>();
            string[] opsArray = { "=", "+", "-", "/", "*", "^", "|", "!" };
            ops.AddRange(opsArray);

            List<int> degrees = new List<int>(); //list of the degree of each term
            foreach (string term in separatedEqn.Split(' '))
            {
                if (!ops.Contains(term))
                {
                    string separatedTerm;
                    bool multiSubTerm = separateMultTerms(term, variable1, out separatedTerm);
                    if (!term.Contains(variable1))
                    {
                        if (!multiSubTerm)
                            degrees.Add(0);
                        else
                        {//multiple terms but no variables
                            foreach (string subterm in separatedTerm.Split(' '))
                            {
                                degrees.Add(0);
                            }
                        }
                    }
                    else if (!term.Contains("^"))
                    {
                        if (!multiSubTerm)
                            degrees.Add(1);
                        else
                        {//multiple terms, has variable, but no exponents
                            foreach (string subterm in separatedTerm.Split(' '))
                            {
                                if (!subterm.Contains(variable1))
                                    degrees.Add(0);
                                else
                                    degrees.Add(1);
                            }
                        }
                    }
                    else
                    {//has variable and exponent (at least one)
                        if (!multiSubTerm)
                        {
                            //get value of exponent in this single term
                            int indexOfCarat = separatedTerm.IndexOf("^");
                            string strExp = separatedTerm.Substring(indexOfCarat + 1, separatedTerm.Length - indexOfCarat - 1);

                            string separatedExp;
                            bool multTermExp = separateMultTerms(strExp, variable1, out separatedExp);

                            //try to parse with the separatedExp b/c if it was (#) then separateMultTerms will remove parens
                            double doubleExp;
                            bool parsedExpSuccess = double.TryParse(separatedExp, out doubleExp);

                            if (parsedExpSuccess)
                                degrees.Add((int)doubleExp);
                            else
                            {
                                if (multTermExp && !strExp.Contains(variable1))
                                {
                                    //multiple terms, but no variables.
                                    degrees.Add((int)evalNoVars(separatedExp));
                                }
                            }
                        }
                        else
                        {//has at least one var and exp, but could have more b/c its mult terms
                            foreach (string subterm in separatedTerm.Split(' '))
                            {
                                string sepSubTerm;
                                bool m = separateMultTerms(subterm, variable1, out sepSubTerm);
                                degrees.AddRange(listDegreesR(variable1, sepSubTerm));
                            }
                        }
                    }
                }
            }
            return degrees;
        }

        private double evalNoVars(string expression)
        {
            //source: http://stackoverflow.com/questions/333737/c-sharp-evaluating-string-342-yield-int-18
            var loDataTable = new DataTable();
            var loDataColumn = new DataColumn("Eval", typeof(double), expression);
            loDataTable.Columns.Add(loDataColumn);
            loDataTable.Rows.Add(0);
            return (double)(loDataTable.Rows[0]["Eval"]);
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
                char variable1;
                char variable2;
                if (givenEqn.Length > 0)
                {
                    switch (determineType(givenEqn, out variable1, out variable2))
                    {
                        case equationType.noVar:
                            caseNoVariables(ref givenEqn, ref linkLabelCount, variable1);
                            insertPictures();
                            break;
                        case equationType.oneVar:
                            caseOneVariable(ref givenEqn, ref linkLabelCount, variable1);
                            insertPictures();
                            break;
                        case equationType.twoVar:
                            if (givenEqn.Contains("x"))
                                variable1 = 'x';
                            if (givenEqn.Contains('y'))
                                variable2 = 'y';

                            //if shorter side of equation isnt just y= or x= then subtract it from the rhs
                            //ex: y^2 + 1 = x^2 + 8  -->  x^2 + 8 - (y^2+1)
                            if (givenEqn.Contains('='))
                            {
                                string[] splitOnEquals = givenEqn.Split('=');
                                if (splitOnEquals.Length > 2)
                                {
                                    instructionsRTB.Text = "Oh no! You entered more than one equals sign (=). Please re-enter your equation.\n\n-Matt";
                                    break; // abort
                                }
                                string lhs = splitOnEquals[0].Trim();
                                string rhs = splitOnEquals[1].Trim();

                                //make rhs the longer term
                                if (lhs.Length > rhs.Length)
                                {
                                    string temp = rhs;
                                    rhs = lhs;
                                    lhs = temp;
                                }

                                //if (lhs.Length > 1)
                                {
                                    //more than just y=, add -lhs onto the end of rhs
                                    //need parens b/c need to distribute neg.
                                    givenEqn = rhs + "- (" + lhs + ")";
                                }
                            }

                            //info wrt variable 1
                            string separatedEqnX;
                            bool multDegX = separateMultTerms(givenEqn, variable1, out separatedEqnX);
                            List<int> degreesX = listDegreesR(variable1, separatedEqnX);
                            int highestDegreeX = degreesX.Max();

                            //info wrtY (variable2)
                            string separatedEqnY;
                            bool multDegY = separateMultTerms(givenEqn, variable2, out separatedEqnY);
                            List<int> degreesY = listDegreesR(variable2, separatedEqnY);
                            int highestDegreeY = degreesY.Max();

                            if (highestDegreeX == 1 && highestDegreeY == 1)
                            {
                                //linear equation
                                //if (!(givenEqn.Contains("y=") || givenEqn.Contains("y =") || givenEqn.Contains("=y")))
                                if(degreesY.Count > 2) //more than just the x term and the y term
                                {
                                    //solve for y, put it in y=mx+b form
                                    instructionsRTB.Text += "Solve the equation for the second variable in order to put in slope-intercept form";
                                    //move everything to the other side and get rid of coef
                                    LinkLabel solve = addLinkLabel("Put Equation into Slope-Intercept Form", "SolveForY", linkLabelCount++);
                                    if (solve == null) linkLabelCount--;
                                    loadInstructions(solve);
                                }

                                LinkLabel mxb = addLinkLabel("Graphing an Equation in Slope-Intercept Form", "GraphYEq", linkLabelCount++);
                                if (mxb == null) linkLabelCount--;
                                loadInstructions(mxb);

                                //other instructions for linear equations
                                LinkLabel standard = addLinkLabel("Put Equation into Standard Form", "StandardForm", linkLabelCount++);
                                if (standard == null) linkLabelCount--;

                                LinkLabel graph = addLinkLabel("Graphing a Linear Equation", "GraphLinear", linkLabelCount++);
                                if (graph == null) linkLabelCount--;
                            }
                            else if (highestDegreeX == 2 && highestDegreeY == 2)
                            {
                                //circle or ellipse or hyperbola

                                //if has ^2 term and ^1 term then prob. need to factor to put in form of equation
                                if (degreesX.Contains(1) || degreesY.Contains(1))
                                {
                                    LinkLabel factor = addLinkLabel("Factor to Find Center", "Factoring", linkLabelCount++);
                                    if (factor == null) linkLabelCount--;
                                    loadInstructions(factor);
                                }

                                if (separatedEqnX.Contains(" / ") || separatedEqnX.Contains("^2/") || separatedEqnX.Contains("^2)/"))
                                {//need to make sure division is right (not a fraction)

                                    // + ellipse
                                    if (separatedEqnX.Contains(" + ")) // equation has a spaced +
                                    {
                                        LinkLabel ellipse = addLinkLabel("The Equation of an Ellipse", "EllipseEqn", linkLabelCount++);
                                        if (ellipse == null) linkLabelCount--;
                                        loadInstructions(ellipse);
                                    }
                                    else
                                    {
                                        //- hyperbola
                                        LinkLabel hyp = addLinkLabel("The Equation of a Hyperbola", "Hyperbola", linkLabelCount++);
                                        if (hyp == null) linkLabelCount--;
                                        loadInstructions(hyp);
                                    }
                                }
                                else
                                {
                                    //circle
                                    LinkLabel cir = addLinkLabel("The Equation of a Circle", "CircleEqn", linkLabelCount++);
                                    if (cir == null) linkLabelCount--;
                                    loadInstructions(cir);
                                }
                            }
                            else
                            {
                                //the degrees mismatch. not 1,1 or 2,2
                                //Vertical Parabola
                                if (highestDegreeX == 2 && highestDegreeY == 1)
                                {
                                    //y=x^2 , y = 3x^2+5x-7
                                    if (degreesX.Contains(1))
                                    {
                                        LinkLabel factor = addLinkLabel("Factor to Find Vertex", "Factoring", linkLabelCount++);
                                        if (factor == null) linkLabelCount--;
                                        loadInstructions(factor);
                                    }
                                    LinkLabel par = addLinkLabel("Graphing a Parabola", "VParabola", linkLabelCount++);
                                    if (par == null) linkLabelCount--;
                                    loadInstructions(par);
                                    
                                }
                                else
                                {
                                    //parabolas opening left or right
                                    if (highestDegreeY == 2 && highestDegreeX == 1)//x=y^2
                                    {
                                        LinkLabel par = addLinkLabel("Graphing a Parabola", "HParabola", linkLabelCount++);
                                        if (par == null) linkLabelCount--;
                                        loadInstructions(par);
                                    }
                                    else
                                    {
                                        instructionsRTB.Text = "Pick a topic from below to view.";

                                        LinkLabel intercepts = addLinkLabel("Finding X and Y Intercepts", "Intercepts", linkLabelCount++);
                                        if (intercepts == null) linkLabelCount--;

                                        //move everything to the other side and get rid of coef
                                        LinkLabel solve = addLinkLabel("Put Equation into Slope-Intercept Form", "SolveForY", linkLabelCount++);
                                        if (solve == null) linkLabelCount--;

                                        LinkLabel graph = addLinkLabel("Graphing a Non-Linear Equation", "GraphNonLinear", linkLabelCount++);
                                        if (graph == null) linkLabelCount--;
                                    }
                                }
                            }

                            insertPictures();
                            break;
                        case equationType.multEqn:
                            LinkLabel mult = addLinkLabel("Solving Systems of Equations", "MultEqn", linkLabelCount++);
                            if (mult == null) linkLabelCount--;
                            loadInstructions(mult);

                            insertPictures();
                            break;
                        default:
                            instructionsRTB.Text = "Sorry, I don't recognize the problem you entered. Make sure you"
                                + " typed everything in correctly and try to simplify your equation as much as you can"
                                + " before asking me for help.\n\n-Matt";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                instructionsRTB.Text = "There was an error:\n\n" + ex.Message;
            }
        }

        private void caseOneVariable(ref string givenEqn, ref int linkLabelCount, char variable1)
        {
            string separatedEqn;
            bool mT = separateMultTerms(givenEqn, variable1, out separatedEqn);
            bool multDegrees;
            double degree = findHighestDegree(givenEqn, variable1, out multDegrees);
            bool sentToOneVarMult = false;

            //find operators involved
            List<string> includedOps = new List<string>();
            List<string> operators = new List<string>();
            string[] opsArray = { "=", "+", "-", "/", "*", "^", "|", "!" };
            operators.AddRange(opsArray);
            foreach (char c in separatedEqn)
            {
                if (operators.Contains(c.ToString()))
                {
                    if (!includedOps.Contains(c.ToString()))
                    {
                        includedOps.Add(c.ToString());
                    }
                }
            }

            if (separatedEqn.Contains("sqrt"))
            {
                includedOps.Add("sqrt");
            }
            if (separatedEqn.Contains("abs"))
            {
                includedOps.Add("abs");
            }

            if (!mT)
            {
                //if not given = #, then set equal to zero to find roots
                if (!includedOps.Contains("="))
                {
                    LinkLabel roots = addLinkLabel("Finding the Roots of an Equation", "FindRoots", linkLabelCount++);
                    if (roots == null) linkLabelCount--;
                    loadInstructions(roots);

                    givenEqn += " = 0";
                    includedOps.Add("=");
                }

                if (includedOps.Contains("sqrt"))
                {
                    //must be root(everything) or else would be mult terms
                    if (givenEqn.Contains('='))
                    {
                        LinkLabel sqrt = addLinkLabel("Solving Equations with Radicals", "RadicalsEqn", linkLabelCount++);
                        if (sqrt == null) linkLabelCount--;
                        loadInstructions(sqrt);

                        //need to remove sqrt and plun into oneVarMultTerms section

                        string removeSqrt = separatedEqn.Substring(separatedEqn.IndexOf("sqrt") + 4, separatedEqn.Length - 4);
                        bool multiTermBase = separateMultTerms(removeSqrt, variable1, out removeSqrt);

                        sentToOneVarMult = true;
                        linkLabelCount = oneVarMultipleTerms(linkLabelCount, variable1, removeSqrt);
                    }
                }

                else if (includedOps.Contains("^"))
                {
                    //must be (everything)^# or would be mult terms
                    //check for fractional exponent
                    string exponent = separatedEqn.Substring(separatedEqn.LastIndexOf('^') + 1, separatedEqn.Length - separatedEqn.LastIndexOf('^') - 1);
                    bool multiTermExp = separateMultTerms(exponent, variable1, out exponent);

                    string removeExp = separatedEqn.Remove(separatedEqn.LastIndexOf('^'));
                    bool multiTermExpBase = separateMultTerms(removeExp, variable1, out removeExp);

                    if (multiTermExp)
                    {
                        //need to check for fractions
                        if (exponent.Contains('/'))
                        {
                            LinkLabel fracExp = addLinkLabel("Evaluating Fractional Exponents", "FractionExponent", linkLabelCount++);
                            if (fracExp == null) linkLabelCount--;
                            loadInstructions(fracExp);
                            //must be root(everything) or else would be mult terms
                            LinkLabel sqrt = addLinkLabel("Solving Equations with Radicals", "RadicalsEqn", linkLabelCount++);
                            if (sqrt == null) linkLabelCount--;
                            loadInstructions(sqrt);

                            //remove the sqrt/power and then plug into oneVarMultTerms section
                            sentToOneVarMult = true;
                            linkLabelCount = oneVarMultipleTerms(linkLabelCount, variable1, removeExp);
                        }
                        else
                        {
                            //add order of operations
                            addLinkLabel("Order of Operations", "OrderOfOperations", linkLabelCount++);
                        }
                    }

                    //if given polynomial to a power
                    if (multiTermExpBase)
                    {
                        bool multiDegrees = false;
                        double highestDegreeInBase = findHighestDegree(removeExp, variable1, out multiDegrees);
                        List<string> baseOperators = new List<string>();
                        if (multiDegrees)
                        {
                            LinkLabel ll = addLinkLabel("Multiplying Polynomials", "FOIL", linkLabelCount++);
                            if (ll == null) linkLabelCount--;
                            loadInstructions(ll);

                            //if (multidegrees)^# (3x^2-x+2x)^2 
                            //instruct to foil and then plug the equation into multiTerm section
                            sentToOneVarMult = true;
                            linkLabelCount = oneVarMultipleTerms(linkLabelCount, variable1, removeExp);
                        }
                        else
                        {
                            //check if multiple variable terms in the base
                            bool multTermsAfterCombining = false;
                            int variableTerms = 0;
                            foreach (string term in removeExp.Split(' '))
                            {
                                if (!term.Contains(variable1))
                                {
                                    if (!operators.Contains(term))
                                    {
                                        multTermsAfterCombining = true;
                                    }
                                    else
                                    {
                                        baseOperators.Add(term);
                                    }
                                }
                                else
                                    variableTerms++;
                            }

                            if (variableTerms > 1) // multiple terms with the same degree (in base)
                            {
                                LinkLabel combine = addLinkLabel("Combining Terms", "CombineLikeTerms", linkLabelCount++);
                                if (combine == null) linkLabelCount--;
                                loadInstructions(combine);
                            }

                            if (multTermsAfterCombining) //(still) multiple terms in base
                            {
                                //check if actually foil or if (3x/5)^2
                                if ((baseOperators.Distinct().Contains("/") || baseOperators.Contains("*")) && !baseOperators.Contains("+"))
                                {
                                    //cant check on - b/c of negative numbers
                                    if (baseOperators.Contains("*"))
                                    {
                                        //dont know if it is or numVar or numMult...
                                        LinkLabel power = addLinkLabel("Exponents", "SimpleExponent", linkLabelCount++);
                                        if (power == null) linkLabelCount--;
                                        loadInstructions(power);

                                        LinkLabel coefRemove = addLinkLabel("Remove Coefficient", "DivideBothSides", linkLabelCount++);
                                        if (coefRemove == null) linkLabelCount--;
                                        loadInstructions(coefRemove);
                                    }
                                    else if (baseOperators.Contains("/"))
                                    {
                                        LinkLabel coef = addLinkLabel("Simplify the Coefficient", "MultDivNumVar", linkLabelCount++);
                                        if (coef == null) linkLabelCount--;
                                        loadInstructions(coef);

                                        LinkLabel fracPwr = addLinkLabel("Applying Exponents to Fractions", "FracToPower", linkLabelCount++);
                                        if (fracPwr == null) linkLabelCount--;
                                        loadInstructions(fracPwr);

                                        LinkLabel coefRemove = addLinkLabel("Remove Coefficient", "DivideBothSides", linkLabelCount++);
                                        if (coefRemove == null) linkLabelCount--;
                                        loadInstructions(coefRemove);
                                    }
                                }
                                else
                                {
                                    LinkLabel ll = addLinkLabel("Expanding Equations with Exponents", "FOIL", linkLabelCount++);
                                    if (ll == null) linkLabelCount--;
                                    loadInstructions(ll);
                                }

                                //combine like terms?
                                //LinkLabel solve = addLinkLabel("Combine Like Terms", "CombineLikeTerms", linkLabelCount++);
                                //if (solve == null) linkLabelCount--;
                                //loadInstructions(solve);

                                sentToOneVarMult = true; //dont send to solveOneTerm b/c this is all needed instructions
                            }
                            else
                            {
                                //has become power to a power
                                LinkLabel pp = addLinkLabel("Raising a Power to a Power", "RaisingAPower", linkLabelCount++);
                                if (pp == null) linkLabelCount--;
                                loadInstructions(pp);
                            }
                        }
                    }// end multi term base
                    else
                    {
                        if (removeExp.Contains("^"))
                        {
                            //power to a power
                            LinkLabel pp = addLinkLabel("Raising a Power to a Power", "RaisingAPower", linkLabelCount++);
                            if (pp == null) linkLabelCount--;
                            loadInstructions(pp);
                        }
                    }
                }// end if ^

                //solving equations single term equations

                if (!sentToOneVarMult)
                {
                    linkLabelCount = solveOneVarOneTerm(separatedEqn, variable1, linkLabelCount);
                }
            }// end if !multipleTerms
            else
            {
                //instructionsRTB.Text = "one var, mult terms";
                sentToOneVarMult = true;
                linkLabelCount = oneVarMultipleTerms(linkLabelCount, variable1, separatedEqn);
            }
        }

        private void caseNoVariables(ref string givenEqn, ref int linkLabelCount, char variable1)
        {
            bool multipleTerms = separateMultTerms(givenEqn, ' ', out givenEqn);
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
                    bool multiTermExp = separateMultTerms(exponent, variable1, out exponent);

                    if (multiTermExp)
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
        }

    }
}
