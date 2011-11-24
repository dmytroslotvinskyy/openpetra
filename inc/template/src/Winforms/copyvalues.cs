{##SHOWDATAFORCOLUMN}
{#IFDEF NOTDEFAULTTABLE}
{#IFDEF CANBENULL}
{#SETROWVALUEORNULL}
{#ENDIF CANBENULL}
{#IFNDEF CANBENULL}
{#SETROWVALUE}
{#ENDIFN CANBENULL}
{#ENDIF NOTDEFAULTTABLE}
{#IFNDEF NOTDEFAULTTABLE}
{#IFDEF CANBENULL}
{#SETVALUEORNULL}
{#ENDIF CANBENULL}
{#IFNDEF CANBENULL}
{#SETCONTROLVALUE}
{#ENDIFN CANBENULL}
{#ENDIFN NOTDEFAULTTABLE}

{##GETDATAFORCOLUMNTHATCANBENULL}
{#IFDEF CANBENULL}
{#IFDEF NOTDEFAULTTABLE}
{#GETROWVALUEORNULL}
{#ENDIF NOTDEFAULTTABLE}
{#IFNDEF NOTDEFAULTTABLE}
{#GETVALUEORNULL}
{#ENDIFN NOTDEFAULTTABLE}
{#ENDIF CANBENULL}
{#IFNDEF CANBENULL}
{#ROW}.{#COLUMNNAME} = {#CONTROLVALUE};
{#ENDIFN CANBENULL}

{##SETROWVALUEORNULL}
if ({#NOTDEFAULTTABLE} == null || (({#NOTDEFAULTTABLE}.Rows.Count > 0) && ({#NOTDEFAULTTABLE}[0].Is{#COLUMNNAME}Null())))
{
    {#SETNULLVALUE}
}
else
{
    if ({#NOTDEFAULTTABLE}.Rows.Count > 0)
    {
        {#SETCONTROLVALUE}
    }
}

{##SETROWVALUE}
if ({#NOTDEFAULTTABLE} != null)
{
    {#SETCONTROLVALUE}
}
else
{
    {#SETNULLVALUE}
}

{##GETROWVALUEORNULL}
if (({#NOTDEFAULTTABLE} != null) && ({#NOTDEFAULTTABLE}.Rows.Count > 0))
{
    if ({#DETERMINECONTROLISNULL})
    {
        {#NOTDEFAULTTABLE}[0].Set{#COLUMNNAME}Null();
    }
    else
    {
        {#NOTDEFAULTTABLE}[0].{#COLUMNNAME} = {#CONTROLVALUE};
    }
}

{##GETROWVALUEORNULLSTRING}
if (({#NOTDEFAULTTABLE} != null) && ({#NOTDEFAULTTABLE}.Rows.Count > 0))
{
    {#ROW}.{#COLUMNNAME} = {#CONTROLVALUE};
}

{##SETVALUEORNULL}
if ({#ROW}.Is{#COLUMNNAME}Null())
{
    {#SETNULLVALUE}
}
else
{
    {#SETCONTROLVALUE}
}

{##GETVALUEORNULL}
if ({#DETERMINECONTROLISNULL})
{
    {#ROW}.Set{#COLUMNNAME}Null();
}
else
{
    {#ROW}.{#COLUMNNAME} = {#CONTROLVALUE};
}