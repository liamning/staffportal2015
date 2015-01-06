<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarHeader.ascx.cs" Inherits="Control_CalendarHeader" %>
    

	<link rel="stylesheet" href="Resource/jquery-ui-1.10.4.custom/css/custom-theme/jquery-ui-1.10.4.custom.min.css" />
	<script src="Resource/jquery-ui-1.10.4.custom/js/jquery-ui-1.10.4.custom.min.js"></script> 
    <style>
    .ui-datepicker { width: 16.5em;  padding: .2em .2em 0; display: none; font-family: Arial; font-size: 9pt; }
    .ui-datepicker table {width: 100%;font-size: .9em; border-collapse: collapse;   } 
    td a {  padding: 4px!important; margin:0px; }
    td.highlight a {background: #489DC0!important;  border: 1px #489DC0 solid !important; padding: 4px!important;  }
    .ui-datepicker-calendar td a {cursor: default!important;}
    .ui-datepicker-prev, .ui-datepicker-next {cursor: pointer!important;} 
    .ui-widget-content {
    border-top-color: rgb(221, 221, 221);
    border: 1px solid #ddd; 
    border-bottom:0px !important; 
    padding-right:3px;
    color: #333;
    }
    </style>

<script>

    $(function () {

        var $eventDiv = $("#eventDiv");
        var globalData;
        var ispollingData = false;

        var appendEventSummary = function (data, year, month) {
            $eventDiv.html("");
            var hasEvent = false;
            var eventList = "Next Training & Events:<br />";
            var page = "ViewEvent";
            for (var i = 0, eventItem; eventItem = data[i]; i++) {
                if (year == eventItem.starttime.getFullYear()
                    && month - 1 == eventItem.starttime.getMonth()) {
                    page = (eventItem.type == "Training" ? "ViewTraining" : "ViewEvent");
                    eventList = eventList + eventItem.starttime.getDate() + "/" + (eventItem.starttime.getMonth() + 1) + "/" + eventItem.starttime.getFullYear() + "<br />";
                    eventList = eventList + "<font color=#3894C4><a href='" + page + ".aspx?ID="
                                + eventItem.id + "'>" + eventItem.name + "</a></font><br />";
                    hasEvent = true;
                }
            }
            if (hasEvent) {
                $eventDiv.append("<p style='padding: 0px 5px 5px 5px; font-size: 9pt; font-family:Arial;'><b>" + eventList + "</b></p>");
            }
        }

        var delay_getEventByMonth = function (year, month) {
            if (ispollingData) {
                setTimeout(function () { delay_getEventByMonth(year, month); }, 100);
            } else {
                getEventByMonth(year, month);
            }
        }

        //create the calendar
        $("#datepicker").datepicker({
            onChangeMonthYear: function (year, month, inst) {
                delay_getEventByMonth(year, month);
            },
            beforeShowDay: function (date) {
                if (globalData) {
                    for (var i = 0, eventItem; eventItem = globalData[i]; i++) {
                        if (date.getFullYear() == eventItem.starttime.getFullYear()
                                    && date.getMonth() == eventItem.starttime.getMonth()
                                    && date.getDate() == eventItem.starttime.getDate()) {
                            return [true, 'highlight', ""];
                        }
                    }
                }
                return [true, ""];
            }
        });

        //get Event List
        var getEventByMonth = function (year, month) {
            ispollingData = true;
            $.ajax({
                type: 'POST',
                url: "Service/AjaxService.aspx",
                data: {
                    action: "getEventByMonth",
                    DateOfMonth: year + "-" + month + "-01"
                },
                success: function (data) {
                    data = eval('(' + data + ')');

                    if (data.error) {
                        var decoded = $("<div/>").html(data.error).text();
                        alert(decoded);
                        return;
                    }

                    globalData = data;
                    ispollingData = false;

                    $.each(globalData, function (index, value) {
                        value.starttime = convertDate(value.starttime);
                        value.endtime = convertDate(value.endtime);
                    });

                    $("#datepicker").datepicker("refresh");
                    appendEventSummary(data, year, month);
                }
            });
        }

        getEventByMonth(new Date().getFullYear(), new Date().getMonth() + 1);


    });
</script>

