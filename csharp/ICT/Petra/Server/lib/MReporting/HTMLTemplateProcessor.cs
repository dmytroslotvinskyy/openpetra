//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2018 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using Ict.Petra.Shared.MReporting;
using System.Collections;
using System.Collections.Generic;
using Ict.Common;
using Ict.Common.DB;
using System.IO;

namespace Ict.Petra.Server.MReporting
{
    /// parse and replace HTML templates for reports
    public class HTMLTemplateProcessor
    {
        private string FHTMLTemplate;
        private Dictionary<string, string> FSQLQueries = new Dictionary<string, string>();
        // do not print warning too many times for the same variable
        private static SortedList<string, Int32> VariablesNotFound = new SortedList<string, int>();

        /// <summary>
        /// constructor
        /// </summary>
        public HTMLTemplateProcessor(string AHTMLTemplate, TParameterList AParameters)
        {
            FHTMLTemplate = AHTMLTemplate;

            SeparateSQLQueries();

            FHTMLTemplate = InsertParameters("{{", "}}", FHTMLTemplate, AParameters);
            FHTMLTemplate = InsertParameters("{", "}", FHTMLTemplate, AParameters);

            FHTMLTemplate = EvaluateVisible(FHTMLTemplate, AParameters);
        }

        /// separate the sql queries from the HTML template
        private void SeparateSQLQueries()
        {
            int pos = FHTMLTemplate.IndexOf("<!-- BeginSQL ");
            while (pos != -1)
            {
                int posAfterName = FHTMLTemplate.IndexOf("-->", pos);
                string name = FHTMLTemplate.Substring(pos + "<!-- BeginSQL ".Length, posAfterName - (pos + "<!-- BeginSQL ".Length)).Trim();
                int posAfterSQL = FHTMLTemplate.IndexOf("<!-- EndSQL", pos);
                string sql = FHTMLTemplate.Substring(posAfterName + "-->".Length, posAfterSQL - (posAfterName + "-->".Length)).Trim();
                FSQLQueries.Add(name, sql);

                // remove sql from template
                FHTMLTemplate = FHTMLTemplate.Substring(0, pos) +
                    FHTMLTemplate.Substring(FHTMLTemplate.IndexOf("-->", posAfterSQL)+"-->".Length);
                pos = FHTMLTemplate.IndexOf("<!-- BeginSQL");
            }
        }

        /// <summary>
        /// Gets the SQL query.
        /// </summary>
        public string GetSQLQuery(string queryname, TParameterList AParameters)
        {
            string sql = FSQLQueries[queryname];

            sql = ProcessIfDefs(sql, AParameters);

            // TODO: use prepared statements to pass parameters
            sql = InsertParameters("{{", "}}", sql, AParameters);
            sql = InsertParameters("{#", "#}", sql, AParameters);
            sql = InsertParameters("{LIST ", "}", sql, AParameters);
            sql = InsertParameters("{", "}", sql, AParameters);

            return sql;
        }

        /// <summary>
        ///  the processed HTML
        /// </summary>
        public string GetHTML()
        {
            return FHTMLTemplate;
        }

        private string InsertParameters(string searchOpen, string searchClose, string template, TParameterList parameters)
        {
            int bracket = template.IndexOf(searchOpen);

            while (bracket != -1)
            {
                int firstRealChar = bracket + searchOpen.Length;
                int paramEndIdx = template.IndexOf(searchClose, firstRealChar);

                if (paramEndIdx <= 0)
                {
                    // missing closing bracket; can happen with e.g. #testdate; should be #testdate#
                    if (template.Length > bracket + 20)
                    {
                        throw new Exception("Cannot find closing bracket " + searchClose + " for " + template.Substring(bracket, 20));
                    }
                    else
                    {
                        throw new Exception("Cannot find closing bracket " + searchClose + " for " + template.Substring(bracket));
                    }
                }

                String parameter = template.Substring(firstRealChar, paramEndIdx - firstRealChar);
                bool ParameterExists = false;
                TVariant newvalue;

                if (parameters != null)
                {
                    newvalue = parameters.Get(parameter, -1, -1, eParameterFit.eBestFitEvenLowerLevel);

                    ParameterExists = (newvalue.TypeVariant != eVariantTypes.eEmpty);
                }
                else
                {
                    newvalue = new TVariant();
                }

                if (!ParameterExists)
                {
                    // if date is given, use the parameter itself
                    if ((parameter[0] >= '0') && (parameter[0] <= '9'))
                    {
                        newvalue = new TVariant(parameter);
                    }
                    else
                    {
                        int CountWarning = 1;

                        // do not print warning too many times for the same variable
                        if (!VariablesNotFound.ContainsKey(parameter))
                        {
                            VariablesNotFound.Add(parameter, 1);
                        }
                        else
                        {
                            VariablesNotFound[parameter] = VariablesNotFound[parameter] + 1;
                            CountWarning = VariablesNotFound[parameter];
                        }

                        if (CountWarning < 5)
                        {
                            // this can be alright, for empty values; for example method of giving can be empty; for report GiftTransactions
                            TLogging.Log(
                                "Variable " + parameter + " empty or not found");
                        }
                        else if (CountWarning % 20 == 0)
                        {
                            TLogging.Log("20 times: Variable " + parameter + " empty or not found.");
                        }
                    }
                }

                try
                {
                    if (newvalue.TypeVariant == eVariantTypes.eDateTime)
                    {
                        // remove the time from the timestamp, only use the date at 0:00
                        DateTime date = newvalue.ToDate();
                        newvalue = new TVariant(new DateTime(date.Year, date.Month, date.Day));
                    }

                    string strValue = newvalue.ToString();
                    if (searchOpen == "{LIST ")
                    {
                        string[] elements = newvalue.ToString().Split(new char[] { ',' });
                        strValue = String.Empty;
                        foreach (string element in elements)
                        {
                            if (strValue.Length > 0)
                            {
                                strValue += ",";
                            }
                            strValue += "'" + element + "'";
                        }
                    }
                    else if ((searchOpen == "{#") && (newvalue.TypeVariant == eVariantTypes.eDateTime))
                    {
                        strValue = "'" + newvalue.ToDate().ToString("yyyy-MM-dd") + "'";
                    }
                    else if ((searchOpen != "{{") && 
                             !(parameter.Length > 2 && parameter.Substring(parameter.Length - 2) == "_i") &&
                             (newvalue.TypeVariant == eVariantTypes.eString))
                    {
                        strValue = "'" + newvalue.ToString() + "'";
                    }
                    template = template.Replace(searchOpen + parameter + searchClose, strValue);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        "While trying to format parameter " + parameter + ", there was a problem with formatting." + Environment.NewLine + e.Message);
                }

                bracket = template.IndexOf(searchOpen);
            } // while

            return template;
        }

        string ProcessIfDefs(string s, TParameterList AParameters)
        {
            int posPlaceholder = s.IndexOf("#ifdef ");

            // to avoid issues with ifdefs at the end
            s += "\n";

            while (posPlaceholder > -1)
            {
                string condition = s.Substring(posPlaceholder + "#ifdef ".Length, s.IndexOf("\n", posPlaceholder) - posPlaceholder - "#ifdef ".Length);
                condition = InsertParameters("{{", "}}", condition, AParameters);
                condition = InsertParameters("{#", "#}", condition, AParameters);
                condition = InsertParameters("{", "}", condition, AParameters);

                // TODO: support nested ifdefs???
                int posPlaceholderAfter = s.IndexOf("#endif", posPlaceholder);

                if (posPlaceholderAfter == -1)
                {
                    throw new Exception("The template has a bug: " +
                        "We are missing and #endif");
                }

                if ((condition == "") || (condition == "''") || (condition == "0") || (condition == "'*NOTUSED*'"))
                {
                    // drop the content of the ifdef section
                    s = s.Substring(0, posPlaceholder) + s.Substring(s.IndexOf("\n", posPlaceholderAfter) + 1);
                }
                else
                {
                    s = s.Substring(0, posPlaceholder) +
                         s.Substring(s.IndexOf("\n", posPlaceholder) + 1, posPlaceholderAfter - s.IndexOf("\n", posPlaceholder) - 1) +
                         s.Substring(s.IndexOf("\n", posPlaceholderAfter) + 1);
                }

                posPlaceholder = s.IndexOf("#ifdef ");
            }

            return s;
        }

        string EvaluateVisible(string template, TParameterList AParameters)
        {
            int visiblePos = template.IndexOf("visible=");

            while (visiblePos != -1)
            {
                int firstRealChar = visiblePos + "visible='".Length;
                int paramEndIdx = template.IndexOf('"', firstRealChar);
                string hidden = "style='visibility:hidden'";
                // TODO: evaluate the condition, eg. with jint
                template = template.Replace(template.Substring(visiblePos, paramEndIdx - visiblePos + 1), hidden);
                visiblePos = template.IndexOf("visible=");
            }
            return template;
        }
    }
}
