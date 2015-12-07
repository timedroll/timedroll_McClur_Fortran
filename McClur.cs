using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication4
{
    static class McClur
    {
        private static readonly string[] FortranTypes = { "INTEGER", "REAL", "COMPLEX", "CHARACTER", "LOGICAL", "DIMENSION" };

        private static string DeleteComments(string FortranCode)
        {
            Regex CommentC = new Regex(@"^C.*" + "\n",RegexOptions.Multiline);
            Match MatchC = CommentC.Match(FortranCode);
            while (MatchC.Success == true)
            {
                FortranCode = FortranCode.Replace(MatchC.Value, "");
                MatchC = MatchC.NextMatch();
            }
            Regex Comment = new Regex(@"!.*");
            Match Match = Comment.Match(FortranCode);
            while (Match.Success == true)
            {
                FortranCode = FortranCode.Replace(Match.Value, "");
                Match = Match.NextMatch();
            }
            return FortranCode;
        }

        private static string DeleteDeclarations(string FortranCode)
        {
            FortranCode = FortranCode.ToUpper();
            for (int i = 0; i < FortranTypes.Length; i++)
            {
                Regex Declaration = new Regex(@"^.*" + FortranTypes[i] + @".*" + "\n", RegexOptions.Multiline);
                Match Match = Declaration.Match(FortranCode);
                while (Match.Success == true)
                {
                    FortranCode = FortranCode.Replace(Match.Value, "");
                    Match = Match.NextMatch();
                }
            }
            return FortranCode.ToLower();
        }

        private static string DeleteStrings(string FortranCode)
        {
            Regex FortranString = new Regex(@"['""][^'""]['""]");
            Match Match = FortranString.Match(FortranCode);
            while (Match.Success == true)
            {
                FortranCode = FortranCode.Replace(Match.Value, "");
                Match = Match.NextMatch();
            }

            return FortranCode.ToLower();
        }

         public static int GetMetricValue(string FortranCode)
         {
            int MetricValue = 0;
            FortranCode = DeleteComments(FortranCode);
            FortranCode = DeleteDeclarations(FortranCode);
            FortranCode = DeleteStrings(FortranCode);
            string[] SplitedFortranCode = FortranCode.Split('\n');
            int i = 0;
            while  (i < SplitedFortranCode.Length)
            {
                if (SplitedFortranCode[i].Contains("do"))
                {
                    i++;
                    string CurrentLoop = "";
                    int NestedLoopsCount = 0;
                    while (NestedLoopsCount != -1)
                    {
                        if (SplitedFortranCode[i].Contains("end do"))
                        {
                            NestedLoopsCount--;
                        }
                        else if (SplitedFortranCode[i].Contains("do"))
                        {
                            NestedLoopsCount++;
                        }
                        if (NestedLoopsCount != -1)
                        {
                            CurrentLoop = CurrentLoop + SplitedFortranCode[i] + '\n';
                        }
                        i++;
                    }
                    if (CurrentLoop != "")
                        CurrentLoop = CurrentLoop.Remove(CurrentLoop.Length - 1);
                    MetricValue += 2 * GetMetricValue(CurrentLoop);
                }
                else if (SplitedFortranCode[i].Contains("if"))
                {
                    bool ElseFlag = false;
                    i++;
                    string CurrentIf = "", CurrentElse = "";
                    int NestedIfCount = 0;
                    while (NestedIfCount != -1)
                    {
                        if (SplitedFortranCode[i].Contains("end if"))
                        {
                            NestedIfCount--;
                        }
                        else if (SplitedFortranCode[i].Contains("if"))
                        {
                            NestedIfCount++;
                        }
                        if (SplitedFortranCode[i].Contains("else"))
                        {
                            ElseFlag = true;
                            i++;
                            continue;
                        }
                        if (NestedIfCount != -1 && !ElseFlag)
                        {
                            CurrentIf = CurrentIf + SplitedFortranCode[i] + '\n';
                        }
                        else if (NestedIfCount != -1 && ElseFlag)
                        {
                            CurrentElse += SplitedFortranCode[i] + '\n';
                        }
                        i++;
                    }
                    if (CurrentIf != "")
                        CurrentIf = CurrentIf.Remove(CurrentIf.Length - 1);
                    if (CurrentElse != "")
                        CurrentElse = CurrentElse.Remove(CurrentElse.Length - 1);
                    int CurrentIfMetricValue = GetMetricValue(CurrentIf);
                    int CurrentElseMetricValue = GetMetricValue(CurrentElse);
                    if (CurrentIfMetricValue > CurrentElseMetricValue)
                        MetricValue += 2 * CurrentIfMetricValue;
                    else
                        MetricValue += 2 * CurrentElseMetricValue;
                }
                else if (SplitedFortranCode[i] != "" && (SplitedFortranCode[i] != "end"))
                {
                    MetricValue++;
                }
                i++;                 
            }
            return MetricValue;
         }       
    }
}
