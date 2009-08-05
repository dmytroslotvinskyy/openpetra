/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       timop
 *
 * Copyright 2004-2009 by OM International
 *
 * This file is part of OpenPetra.org.
 *
 * OpenPetra.org is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenPetra.org is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
 *
 ************************************************************************/
using System;
using System.Data;
using System.Data.Odbc;
using System.Collections;
using System.Web;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using Ict.Common;
using Ict.Tools.CodeGeneration;
using Ict.Tools.DBXML;

namespace Ict.Tools.CodeGeneration.DataStore
{
    public class codeGenerationTable
    {
        public static void InsertTableDefinition(ProcessTemplate Template, TTable currentTable, TTable origTable, string WhereToInsert)
        {
            ProcessTemplate snippet = Template.GetSnippet("TYPEDTABLE");
            string derivedTable = "";

            if (origTable != null)
            {
                snippet.SetCodelet("BASECLASSTABLE", TTable.NiceTableName(currentTable.strName) + "Table");
                derivedTable = "new ";
            }
            else
            {
                snippet.SetCodelet("BASECLASSTABLE", "TTypedDataTable");
            }

            snippet.SetCodelet("NEW", derivedTable);
            snippet.SetCodeletComment("TABLE_DESCRIPTION", currentTable.strDescription);
            snippet.SetCodelet("TABLENAME", currentTable.strDotNetName);

            if (currentTable.strVariableNameInDataset != null)
            {
                snippet.SetCodelet("TABLEVARIABLENAME", currentTable.strVariableNameInDataset);
            }
            else
            {
                snippet.SetCodelet("TABLEVARIABLENAME", currentTable.strDotNetName);
            }

            snippet.SetCodelet("DBTABLENAME", currentTable.strName);
            snippet.SetCodelet("TABLEID", currentTable.order.ToString());

            if (currentTable.HasPrimaryKey())
            {
                TConstraint primKey = currentTable.GetPrimaryKey();
                bool first = true;
                string primaryKeyColumns = "";

                foreach (string columnName in primKey.strThisFields)
                {
                    string toAdd = currentTable.grpTableField.List.IndexOf(currentTable.GetField(columnName)).ToString();

                    if (!first)
                    {
                        toAdd = ", " + toAdd;
                        primaryKeyColumns += ",";
                    }

                    first = false;

                    snippet.AddToCodelet("COLUMNPRIMARYKEYORDER", toAdd);
                    primaryKeyColumns += "Column" + TTable.NiceFieldName(currentTable.GetField(columnName));
                }

                if (primaryKeyColumns.Length > 0)
                {
                    snippet.SetCodelet("PRIMARYKEYCOLUMNS", primaryKeyColumns);
                    snippet.SetCodelet("PRIMARYKEYCOLUMNSCOUNT", primKey.strThisFields.Count.ToString());
                }
            }
            else
            {
                snippet.AddToCodelet("COLUMNPRIMARYKEYORDER", "");
            }

            int colOrder = 0;

            foreach (TTableField col in currentTable.grpTableField.List)
            {
                ProcessTemplate tempTemplate = null;
                string columnOverwrite = "";
                bool writeColumnProperties = true;

                if ((origTable != null) && (origTable.GetField(col.strName, false) != null))
                {
                    columnOverwrite = "new ";

                    if (origTable.GetField(col.strName).iOrder == colOrder)
                    {
                        // same order number, save some lines of code by not writing them
                        writeColumnProperties = false;
                    }
                }

                if (writeColumnProperties && (columnOverwrite.Length == 0))
                {
                    tempTemplate = Template.GetSnippet("DATACOLUMN");
                    tempTemplate.SetCodeletComment("COLUMN_DESCRIPTION", col.strDescription);
                    tempTemplate.SetCodelet("COLUMNNAME", TTable.NiceFieldName(col));
                    snippet.InsertSnippet("DATACOLUMNS", tempTemplate);
                }

                if (writeColumnProperties)
                {
                    tempTemplate = Template.GetSnippet("COLUMNIDS");
                    tempTemplate.SetCodelet("COLUMNNAME", TTable.NiceFieldName(col));
                    tempTemplate.SetCodelet("COLUMNORDERNUMBER", colOrder.ToString());
                    tempTemplate.SetCodelet("NEW", columnOverwrite);
                    snippet.InsertSnippet("COLUMNIDS", tempTemplate);
                }

                tempTemplate = Template.GetSnippet("COLUMNINFO");
                tempTemplate.SetCodelet("COLUMNORDERNUMBER", colOrder.ToString());
                tempTemplate.SetCodelet("COLUMNNAME", TTable.NiceFieldName(col));
                tempTemplate.SetCodelet("COLUMNDBNAME", col.strName);
                tempTemplate.SetCodelet("COLUMNLABEL", col.strLabel);
                tempTemplate.SetCodelet("COLUMNODBCTYPE", codeGenerationPetra.ToOdbcTypeString(col));
                tempTemplate.SetCodelet("COLUMNLENGTH", col.iLength.ToString());
                tempTemplate.SetCodelet("COLUMNNOTNULL", col.bNotNull.ToString().ToLower());
                tempTemplate.SetCodelet("COLUMNCOMMA", colOrder + 1 < currentTable.grpTableField.List.Count ? "," : "");
                snippet.InsertSnippet("COLUMNINFO", tempTemplate);

                tempTemplate = Template.GetSnippet("INITCLASSADDCOLUMN");
                tempTemplate.SetCodelet("COLUMNDBNAME", col.strName);
                tempTemplate.SetCodelet("COLUMNDOTNETTYPE", col.GetDotNetType());
                snippet.InsertSnippet("INITCLASSADDCOLUMN", tempTemplate);

                tempTemplate = Template.GetSnippet("INITVARSCOLUMN");
                tempTemplate.SetCodelet("COLUMNDBNAME", col.strName);
                tempTemplate.SetCodelet("COLUMNNAME", TTable.NiceFieldName(col));
                snippet.InsertSnippet("INITVARSCOLUMN", tempTemplate);

                if (writeColumnProperties)
                {
                    tempTemplate = Template.GetSnippet("STATICCOLUMNPROPERTIES");
                    tempTemplate.SetCodelet("COLUMNDBNAME", col.strName);
                    tempTemplate.SetCodelet("COLUMNNAME", TTable.NiceFieldName(col));
                    tempTemplate.SetCodelet("COLUMNLENGTH", col.iLength.ToString());
                    tempTemplate.SetCodelet("NEW", columnOverwrite);
                    snippet.InsertSnippet("STATICCOLUMNPROPERTIES", tempTemplate);
                }

                colOrder++;
            }

            Template.InsertSnippet(WhereToInsert, snippet);
        }

        public static void InsertRowDefinition(ProcessTemplate Template, TTable currentTable, TTable origTable, string WhereToInsert)
        {
            ProcessTemplate snippet = Template.GetSnippet("TYPEDROW");

            if (origTable != null)
            {
                snippet.SetCodelet("BASECLASSROW", TTable.NiceTableName(currentTable.strName) + "Row");
                snippet.SetCodelet("OVERRIDE", "override ");
            }
            else
            {
                snippet.SetCodelet("BASECLASSROW", "System.Data.DataRow");
                snippet.SetCodelet("OVERRIDE", "virtual ");
            }

            snippet.SetCodeletComment("TABLE_DESCRIPTION", currentTable.strDescription);
            snippet.SetCodelet("TABLENAME", currentTable.strDotNetName);

            foreach (TTableField col in currentTable.grpTableField.List)
            {
                ProcessTemplate tempTemplate = null;
                string columnOverwrite = "";

                if ((origTable != null) && (origTable.GetField(col.strName, false) != null))
                {
                    columnOverwrite = "new ";
                }

                if (columnOverwrite.Length == 0)
                {
                    tempTemplate = Template.GetSnippet("ROWCOLUMNPROPERTY");
                    tempTemplate.SetCodelet("COLUMNDBNAME", col.strName);
                    tempTemplate.SetCodelet("COLUMNNAME", TTable.NiceFieldName(col));
                    tempTemplate.SetCodelet("COLUMNHELP", col.strDescription.Replace(Environment.NewLine, " "));
                    tempTemplate.SetCodelet("COLUMNLABEL", col.strLabel);
                    tempTemplate.SetCodelet("COLUMNLENGTH", col.iLength.ToString());
                    tempTemplate.SetCodelet("COLUMNDOTNETTYPE", col.GetDotNetType());

                    if (col.GetDotNetType().Contains("DateTime"))
                    {
                        tempTemplate.SetCodelet("ACTIONGETNULLVALUE", "return DateTime.MinValue;");
                    }
                    else if (col.GetDotNetType().ToLower().Contains("string"))
                    {
                        tempTemplate.SetCodelet("ACTIONGETNULLVALUE", "return String.Empty;");
                    }
                    else
                    {
                        tempTemplate.SetCodelet("ACTIONGETNULLVALUE", "throw new System.Data.StrongTypingException(\"Error: DB null\", null);");
                    }

                    tempTemplate.SetCodeletComment("COLUMN_DESCRIPTION", col.strDescription);
                    snippet.InsertSnippet("ROWCOLUMNPROPERTIES", tempTemplate);

                    tempTemplate = Template.GetSnippet("FUNCTIONSFORNULLVALUES");
                    tempTemplate.SetCodelet("COLUMNNAME", TTable.NiceFieldName(col));
                    snippet.InsertSnippet("FUNCTIONSFORNULLVALUES", tempTemplate);
                }

                if ((col.strDefault.Length > 0) && (col.strDefault != "NULL"))
                {
                    string defaultValue = col.strDefault;

                    if (defaultValue == "SYSDATE")
                    {
                        defaultValue = "DateTime.Today";
                    }
                    else if (col.strType == "bit")
                    {
                        defaultValue = (defaultValue == "1").ToString().ToLower();
                    }
                    else if (col.strType == "varchar")
                    {
                        defaultValue = '"' + defaultValue + '"';
                    }

                    snippet.AddToCodelet("ROWSETNULLORDEFAULT", "this[this.myTable.Column" + TTable.NiceFieldName(
                            col) + ".Ordinal] = " + defaultValue + ";" + Environment.NewLine);
                }
                else
                {
                    snippet.AddToCodelet("ROWSETNULLORDEFAULT", "this.SetNull(this.myTable.Column" + TTable.NiceFieldName(
                            col) + ");" + Environment.NewLine);
                }
            }

            Template.InsertSnippet(WhereToInsert, snippet);
        }

        public static Boolean WriteTypedTable(TDataDefinitionStore AStore, string strGroup, string AFilePath, string ANamespaceName, string AFileName)
        {
            Console.WriteLine("processing namespace Typed Tables " + strGroup.Substring(0, 1).ToUpper() + strGroup.Substring(1));

            TAppSettingsManager opts = new TAppSettingsManager(false);
            string templateDir = opts.GetValue("TemplateDir", true);
            ProcessTemplate Template = new ProcessTemplate(templateDir + Path.DirectorySeparatorChar +
                "ORM" + Path.DirectorySeparatorChar +
                "DataTable.cs");

            Template.AddToCodelet("NAMESPACE", ANamespaceName);

            // load default header with license and copyright
            StreamReader sr = new StreamReader(templateDir + Path.DirectorySeparatorChar + "EmptyFileComment.txt");
            string fileheader = sr.ReadToEnd();
            sr.Close();
            fileheader = fileheader.Replace(">>>> Put your full name or just a shortname here <<<<", "auto generated");
            Template.AddToCodelet("GPLFILEHEADER", fileheader);

            foreach (TTable currentTable in AStore.GetTables())
            {
                if (currentTable.strGroup == strGroup)
                {
                    InsertTableDefinition(Template, currentTable, null, "TABLELOOP");
                    InsertRowDefinition(Template, currentTable, null, "TABLELOOP");
                }
            }

            Template.FinishWriting(AFilePath + AFileName + ".cs", ".cs", true);

            return true;
        }
    }
}