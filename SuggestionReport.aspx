<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SuggestionReport.aspx.cs" Inherits="SuggestionReport" %>

<%@ Register Src="~/Control/MenuBar.ascx" TagPrefix="uc1" TagName="MenuBar" %>
<%@ Register Src="~/Control/Footer.ascx" TagPrefix="uc1" TagName="Footer" %>
<%@ Register Src="~/Control/PublicHeader.ascx" TagPrefix="uc1" TagName="PublicHeader" %> 




<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
 
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <uc1:PublicHeader runat="server" ID="PublicHeader" />
    <script type="text/javascript">


        $(function () {

            $txtDateFrom = $("#txtDateFrom");
            $txtDateTo = $("#txtDateTo"); 
            $btnGenerate = $("#btnGenerate");
             
            $tdType = $("#tdType");

            var SuggestionTypes;
            var getSuggestionType = function () {
                var formData = {
                    action: "getSystemPara",
                    category: "SuggestionType",
                    includeabandon: true
                };
                $.ajax({
                    url: "Service/AjaxService.aspx",
                    data: formData,
                    type: 'POST',
                    success: function (data) {
                        data = eval('(' + data + ')');

                        if (data.error) {
                            var decoded = $("<div/>").html(data.error).text();
                            alert(decoded);
                            return;
                        }

                        SuggestionTypes = data;
                        constuctSuggestionTypeCheckboxes(SuggestionTypes);

                    }
                });
            }
            var constuctSuggestionTypeCheckboxes = function (SuggestionTypes) {
                $.each(SuggestionTypes, function (key, value) {
                    $tdType.append('<input id="chkType_' + value.id + '" type="checkbox" value="'
                        + value.id + '" checked="checked"><label style="margin-right: 10px;" for="chkType_' + value.id + '">' + value.description + '</label>');
                });
            }

            $btnGenerate.click(function () {

                var types = "";
                $tdType.find(":checkbox").each(function () {
                    if ($(this).is(':checked')) {
                        if (types == "") {
                            types = $(this).val();
                        } else {
                            types = types + "," + $(this).val();
                        }
                    }

                });

                if (types == "") types = "0"; 

                window.open('SuggestionReportDetails.aspx?type=' + types
                    + '&from=' + $txtDateFrom.val()
                    + '&to=' + $txtDateTo.val());

            });

            var init = function () {
                getSuggestionType();
            }();
        });

    </script>

    
    <title></title>  
</head>

<body>
    <form id="form1" runat="server">
        <uc1:MenuBar runat="server" ID="MenuBar" />
    <div id="content">
        <div class="clearLeftFloat"></div>
        <div id="navigationBar">
            <p>Staff Portal > Suggestion Report</p> 
        </div>
        <div id="center" class="font12pt">
            <table style="min-width: 80%;">
                <tr>
                    <td colspan="2">
                        <h4 class="bar">Suggestion Report</h4>         
                    </td> 
                </tr>  
                
                <tr runat="server" id="trType" style="display:none;">
                    <td class="titleTd"><span>Type</span></td> 
                    <td id="tdType">   
                    </td>
                     
                </tr> 
                <tr>
                    <td class="titleTd" style="width: 100px;">   
                        Date Range
                    </td>
                    <td><input type="text" id="txtDateFrom" validate="date" style="width:100px;"/> To
                        <input type="text" id="txtDateTo" validate="date"  style="width:100px;"/>
                    </td>
                </tr>
                <tr>
                    <td class="titleTd" style="width: 100px;">   
                        <input type="button" id="btnGenerate" value="Generate">
                    </td>
                    <td>    
                    </td>
                </tr>
            </table>
        </div>
        <div style="clear:both;"></div>
        <uc1:Footer runat="server" ID="Footer" />

    </div>
    </form>
</body>
</html>
