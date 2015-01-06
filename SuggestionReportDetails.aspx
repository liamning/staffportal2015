<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SuggestionReportDetails.aspx.cs" Inherits="SuggestionReportDetails" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title></title>
    
    <link rel="stylesheet" href="Resource/CSS/ReportStyle.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h3>Suggestion Report</h3>

        
        <ul>
          <li style="display:none;">Type.: <span runat="server" id="txtType"></span></li> 
          <li>Date Range: <span runat="server" id="txtDateRange"></span></li>

        </ul>
    <%=reportStr%>
    </div>
    </form>
</body>
</html>
