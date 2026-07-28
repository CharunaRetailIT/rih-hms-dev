<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportTemplate.aspx.cs" Inherits="HospitalityManagement.Views.Reports.ReportTemplate" %>  
   
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"  
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>  
   
<%--<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">--%>  
<!doctype html>  
 <meta http-equiv="X-UA-Compatible" content="IE=EmulateIE11">  
   
<html xmlns="http://www.w3.org/1999/xhtml">  
<head id="Head1" runat="server">  
    <title></title>  
      
</head>  
<body>  
    <form id="form1" runat="server">  
    <div>  
        <asp:ScriptManager ID="scriptManagerReport" runat="server">  
<%-- <Scripts>  
     <asp:ScriptReference Assembly="ReportViewerForMvc" Name="ReportViewerForMvc.Scripts.PostMessage.js" />  
  </Scripts> --%> 
         </asp:ScriptManager>  
   
       <rsweb:ReportViewer id="rvSiteMapping" runat ="server" ShowPrintButton="false"  Width="99.9%" Height="100%" AsyncRendering="true" ZoomMode="Percent" KeepSessionAlive="true" SizeToReportContent="false" ></rsweb:ReportViewer>                   
    </div>  
    </form>  
</body>  
</html> 