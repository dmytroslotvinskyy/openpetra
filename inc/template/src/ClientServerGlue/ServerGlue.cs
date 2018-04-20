// Auto generated by nant generateGlue
// From a template at inc\template\src\ClientServerGlue\ServerGlue.cs
//
// Do not modify this file manually!
//
{#GPLFILEHEADER}

using System;
using System.Threading;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.ServiceModel.Web;
using System.ServiceModel;
using Newtonsoft.Json;
using Ict.Common;
using Ict.Common.Exceptions;
using Ict.Common.Data;
using Ict.Common.Verification;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Server;
using Ict.Petra.Shared;
using Ict.Petra.Server.App.Core.Security;
using Ict.Petra.Server.App.Core;
{#USINGNAMESPACES}

namespace {#WEBSERVICENAMESPACE}
{
/// <summary>
/// this publishes the SOAP web services of OpenPetra.org for module {#TOPLEVELMODULE}
/// </summary>
[WebService(Namespace = "http://www.openpetra.org/webservices/M{#TOPLEVELMODULE}")]
[ScriptService]
public class TM{#TOPLEVELMODULE}WebService : System.Web.Services.WebService
{
    private static SortedList<string, object> FUIConnectors = new SortedList<string, object>();

    /// <summary>
    /// constructor, which is called for each http request
    /// </summary>
    public TM{#TOPLEVELMODULE}WebService() : base()
    {
        TOpenPetraOrgSessionManager.Init();
    }

    /// disconnect an UIConnector object
    [WebMethod(EnableSession = true)]
    public void DisconnectUIConnector(string UIConnectorObjectID)
    {
        string ObjectID = String.Empty;

        try 
        {
            ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;    
        }
        catch (EOPDBInvalidSessionException)
        {
            // Don't do anything in this scenario as in this case the request to disconnect an UIConnector 
            // has likely come after the Client has disconnected: UIConnector Objects' Finalizers in the 
            // 'Client Glue' call the UIConnector Objects' Dispose Methods, which call the present Method. 
            // Those UIConnector Object Finalizers are not only executed when a screen gets closed by the user  
            // (where that screen uses UIConnectors) and the Garbage Collector executes the Finalizer (whenever 
            // the GC gets to that!), but also when a Client gets closed while screens that use UIConnectors 
            // were still open when the Client gets closed...
            TLogging.Log("DisconnectUIConnector for 'TM{#TOPLEVELMODULE}WebService' for UIConnectorObjectID '" + UIConnectorObjectID + "' got called, but there is no Client Session anymore for that client...");
            
            return;
        }
        catch (Exception Exc) 
        {           
            TLogging.Log("DisconnectUIConnector for 'TM{#TOPLEVELMODULE}WebService' for UIConnectorObjectID '" + UIConnectorObjectID + "': encountered Exception:\r\n" + Exc.ToString());
            throw;
        }         

        if (FUIConnectors.ContainsKey(ObjectID))
        {
            // FUIConnectors[ObjectID].Dispose();
            FUIConnectors.Remove(ObjectID);
            TLogging.Log("DisconnectUIConnector for 'TM{#TOPLEVELMODULE}WebService': removed ObjectID '" + ObjectID + "' which was associated with UIConnectorObjectID '" + ObjectID.Substring(0, ObjectID.Length - DomainManager.GClientID.ToString().Length - 1) + "'! (Now there are " + FUIConnectors.Count.ToString() + " instances of UIConnectors held in that Class.)");
        }
    }

    {#WEBCONNECTORS}

    {#UICONNECTORS}
}
}

{##WEBCONNECTOR}
/// web connector method call
[WebMethod(EnableSession = true)]
public {#RETURNTYPE} {#WEBCONNECTORCLASS}_{#UNIQUEMETHODNAME}({#PARAMETERDEFINITION})
{
    {#CHECKUSERMODULEPERMISSIONS}
    try
    {
        {#LOCALVARIABLES}
        {#LOCALRETURN}{#WEBCONNECTORCLASS}.{#METHODNAME}({#ACTUALPARAMETERS});
        {#RETURN}
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}

{##CHECKUSERMODULEPERMISSIONS}
TModuleAccessManager.CheckUserPermissionsForMethod(typeof({#CONNECTORWITHNAMESPACE}), "{#METHODNAME}", "{#PARAMETERTYPES}"{#LEDGERNUMBER});

{##CHECKSERVERADMINPERMISSION}
if (!TServerManagerBase.CheckServerAdminToken(AServerAdminSecurityToken))
{
    TLogging.Log("invalid security token for serveradmin access");
    throw new Exception("Please check server log file");
}

{##UICONNECTORCONSTRUCTOR}
/// create a new UIConnector
[WebMethod(EnableSession = true)]
public System.String Create_{#UNIQUEMETHODNAME}({#PARAMETERDEFINITION})
{
    {#CHECKUSERMODULEPERMISSIONS}
    
    System.Guid ObjectID = Guid.NewGuid();
    FUIConnectors.Add(ObjectID.ToString() + " " + DomainManager.GClientID, new {#UICONNECTORCLASS}({#ACTUALPARAMETERS}));

    TLogging.Log("Instantiated UIConnector '{#UNIQUEMETHODNAME}({#PARAMETERDEFINITION})': its ObjectID is '" + ObjectID.ToString() + " " + DomainManager.GClientID + "' which is associated with UIConnectorObjectID '" + ObjectID.ToString() + "'! (Now there are " + FUIConnectors.Count.ToString() + " instances of UIConnectors held in that Class.)");
    return ObjectID.ToString();
}

{##UICONNECTORMETHOD}
/// access a UIConnector method
[WebMethod(EnableSession = true)]
public {#RETURNTYPE} {#UICONNECTORCLASS}_{#UNIQUEMETHODNAME}(string UIConnectorObjectID{#PARAMETERDEFINITION})
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call {#UICONNECTORCLASS}_{#METHODNAME}, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        {#LOCALVARIABLES}
        {#LOCALRETURN}(({#UICONNECTORCLASS})FUIConnectors[ObjectID]).{#METHODNAME}({#ACTUALPARAMETERS});
        {#RETURN}
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}

{##UICONNECTORPROPERTY}
{#IFDEF SETTER}
/// access a UIConnector property, set value
[WebMethod(EnableSession = true)]
public void {#UICONNECTORCLASS}_Set{#PROPERTYNAME}(string UIConnectorObjectID, {#ENCODEDTYPE} AValue)
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call {#UICONNECTORCLASS}_Set{#PROPERTYNAME}, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        (({#UICONNECTORCLASS})FUIConnectors[ObjectID]).{#PROPERTYNAME} = {#ACTUALPARAMETERS};
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}
{#ENDIF SETTER}
{#IFDEF GETTER}
/// access a UIConnector property, get value
[WebMethod(EnableSession = true)]
public {#ENCODEDTYPE} {#UICONNECTORCLASS}_Get{#PROPERTYNAME}(string UIConnectorObjectID)
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call {#UICONNECTORCLASS}_Get{#PROPERTYNAME}, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        {#GETTER}
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}
{#ENDIF GETTER}

{##GETSUBUICONNECTOR}
if (!FUIConnectors.ContainsKey(UIConnectorObjectID + "{#PROPERTYNAME} " + DomainManager.GClientID))
{
    FUIConnectors.Add(UIConnectorObjectID + "{#PROPERTYNAME} " + DomainManager.GClientID, 
       (({#UICONNECTORCLASS})FUIConnectors[ObjectID]).{#PROPERTYNAME});
}

return UIConnectorObjectID + "{#PROPERTYNAME}";

{##ASYNCEXECPROCESSCONNECTOR}
/// get ProgressInformation
[WebMethod(EnableSession = true)]
public System.String TAsynchronousExecutionProgress_GetProgressInformation(string UIConnectorObjectID)
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call TAsynchronousExecutionProgress_GetProgressInformation, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        return ((TAsynchronousExecutionProgress)FUIConnectors[ObjectID]).ProgressInformation;
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}

/// get ProgressPercentage
[WebMethod(EnableSession = true)]
public Int16 TAsynchronousExecutionProgress_GetProgressPercentage(string UIConnectorObjectID)
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call TAsynchronousExecutionProgress_GetProgressPercentage, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        return ((TAsynchronousExecutionProgress)FUIConnectors[ObjectID]).ProgressPercentage;
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}

/// get ProgressPercentage
[WebMethod(EnableSession = true)]
public string TAsynchronousExecutionProgress_GetProgressState(string UIConnectorObjectID)
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call TAsynchronousExecutionProgress_GetProgressState, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        return THttpBinarySerializer.SerializeObject(((TAsynchronousExecutionProgress)FUIConnectors[ObjectID]).ProgressState);
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}

/// get Result
[WebMethod(EnableSession = true)]
public string TAsynchronousExecutionProgress_GetResult(string UIConnectorObjectID)
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call TAsynchronousExecutionProgress_GetResult, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        return THttpBinarySerializer.SerializeObject(((TAsynchronousExecutionProgress)FUIConnectors[ObjectID]).Result);
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}

/// Get all 3 properties in one call
[WebMethod(EnableSession = true)]
public string TAsynchronousExecutionProgress_ProgressCombinedInfo(string UIConnectorObjectID)
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call TAsynchronousExecutionProgress_ProgressCombinedInfo, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        TAsyncExecProgressState ProgressState;
        Int16 ProgressPercentage;
        String ProgressInformation;
        ((TAsynchronousExecutionProgress)FUIConnectors[ObjectID]).ProgressCombinedInfo(out ProgressState, out ProgressPercentage, out ProgressInformation);
        return THttpBinarySerializer.SerializeObjectWithType(ProgressState)+","+THttpBinarySerializer.SerializeObjectWithType(ProgressPercentage)+","+THttpBinarySerializer.SerializeObjectWithType(ProgressInformation);
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}

/// Cancel
[WebMethod(EnableSession = true)]
public void TAsynchronousExecutionProgress_Cancel(string UIConnectorObjectID)
{
    string ObjectID = UIConnectorObjectID + " " + DomainManager.GClientID;

    if (!FUIConnectors.ContainsKey(ObjectID))
    {
        TLogging.Log("Trying to call TAsynchronousExecutionProgress_Cancel, but the object with this ObjectID " + ObjectID + " does not exist");
        throw new Exception("this object does not exist anymore!");
    }

    try
    {
        ((TAsynchronousExecutionProgress)FUIConnectors[ObjectID]).Cancel();
    }
    catch (Exception e)
    {
        TLogging.Log(e.ToString());
        throw new Exception("Please check server log file");
    }
}
