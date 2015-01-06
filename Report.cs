using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Report
/// </summary>
public class Report
{
    DatabaseAccess dbAccess = new DatabaseAccess();

    public string getTrainingReport(string serialNo, string type, string decision, string name, DateTime from, DateTime to, 
        string loginID, string userGroup)
    {
        bool blankFromDate = from == DateTime.MinValue;
        bool blankToDate = to == DateTime.MinValue;

        int eventTotal = 0;
        int joinTotal = 0;
        int notAttendTotal = 0;

        System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
        string sql = "select "
                    + "t.ID, t.SerialNo 'Serial No.',  "
                    + "t.Name 'Training Course',  " 
                    + "Convert(varchar,s.StartTime, 103) + ' ' + "
                    + "left(Convert(varchar,s.StartTime, 114),5)+ ' ~ ' + "
                    + "left(Convert(varchar,s.EndTime, 114),5) 'Date & Time', "
                    + "u.FullName 'User Name', "
                    + "case when a.UserAction = 'NotAttend' then 'Not Attend' else a.UserAction  END 'Decision' ,"
                    + "Convert(varchar,a.ActionDate, 103) + ' ' + "
                    + "left(Convert(varchar,a.ActionDate, 114),5) 'Decision Date'"
                    + "FROM ActivityLog a  "
                    + "join [User] u on a.UserID = u.ID "
                    + "join Training t on a.ActivityID = t.ID  "
                    + "join TrainingSche s on s.TrainingID = a.ActivityID and s.ID = a.ExtField "
                    + "where a.ExtField is not null and Category = @Category "
                    + (userGroup != GlobalSetting.SystemRoles.Normal ? "" : "and a.UserID = @LoginID ")
                    + (string.IsNullOrEmpty(name) ? "" : "and t.name like '%' + @Name + '%'  ")
                    + (blankFromDate ? "" : "and cast(s.StartTime As Date) >= @StartTime ")
                    + (blankToDate ? "" : "and cast(s.EndTime As Date) <= @EndTime ")
                    + (string.IsNullOrEmpty(serialNo) ? "" : "and t.SerialNo = @SerialNo ")
                    + "and t.type in (" + type + ") "
                    + "and a.UserAction in (" + decision + ") "
                    + "union all "
                    + "select "
                    + "t.ID,  t.SerialNo 'Serial No.',  "
                    + "t.Name 'Training Course',  "
                    + "Convert(varchar,s.StartTime, 103) + ' ' + "
                    + "left(Convert(varchar,s.StartTime, 114),5)+ ' ~ ' + "
                    + "left(Convert(varchar,s.EndTime, 114),5) 'Date & Time', "
                    + "u.FullName 'User Name',  "
                    + "case when a.UserAction = 'NotAttend' then 'Not Attend' else a.UserAction  END 'Decision' ,"
                    + "Convert(varchar,a.ActionDate, 103) + ' ' + "
                    + "left(Convert(varchar,a.ActionDate, 114),5) 'Decision Date'"
                    + "FROM ActivityLog a  "
                    + "join Training t on a.ActivityID = t.ID  "
                    + "join TrainingSche s on s.TrainingID = t.ID  "
                    + "join [User] u on a.UserID = u.ID "
                    + "where a.ExtField is null and Category = @Category "
                    + (userGroup != GlobalSetting.SystemRoles.Normal ? "" : "and a.UserID = @LoginID ")
                    + (string.IsNullOrEmpty(name) ? "" : "and t.name like '%' + @Name + '%'  ")
                    + (string.IsNullOrEmpty(serialNo) ? "" : "and t.SerialNo = @SerialNo ")
                    + (blankFromDate ? "" : "and cast(s.StartTime As Date) >= @StartTime ")
                    + (blankToDate ? "" : "and cast(s.EndTime As Date) <= @EndTime ")
                    + "and t.type in (" + type + ") "
                    + "and a.UserAction in (" + decision + ") "
                    + "order by t.SerialNo, Decision, u.FullName";

        Dictionary<string, object> dict = new Dictionary<string, object>();

        dict.Add("@Category", GlobalSetting.ArticleCategory.Training);
        dict.Add("@LoginID", loginID);
        if (!string.IsNullOrEmpty(name))
            dict.Add("@Name", name);
        if (!blankFromDate)
            dict.Add("@StartTime", from);
        if (!blankToDate)
            dict.Add("@EndTime", to);
        if (!string.IsNullOrEmpty(serialNo))
            dict.Add("@SerialNo", serialNo);

        dbAccess.open();

        try
        {
            System.Data.DataTable dt = dbAccess.select(sql, dict);
            strBuilder.Append("<table class='ReportTable'>");
            strBuilder.Append("<tr>");
            foreach (System.Data.DataColumn col in dt.Columns)
            {
                if (col.ColumnName == "ID") continue;
                strBuilder.Append("<th>");
                strBuilder.Append(col.ColumnName);
                strBuilder.Append("</th>");
            }
            strBuilder.Append("</tr>");


            string lastIDAndUserName = "";
            string currentIDAndUserName = "";
            foreach (System.Data.DataRow row in dt.Rows)
            {
                currentIDAndUserName = row["ID"].ToString() + row["User Name"].ToString();
                if (lastIDAndUserName != currentIDAndUserName)
                {
                    eventTotal++;
                    if (row["Decision"].ToString() == "Join")
                    {
                        joinTotal++;
                    }
                    else
                    {
                        notAttendTotal++;
                    }
                }
                lastIDAndUserName = currentIDAndUserName;


                strBuilder.Append("<tr>"); 
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    if (col.ColumnName == "ID") continue;
                    strBuilder.Append("<td>");
                    strBuilder.Append(row[col.ColumnName].ToString());
                    strBuilder.Append("</td>");
                }

                strBuilder.Append("</tr>");
            }
            strBuilder.Append("</table>");
            strBuilder.Append("<br/><div><span>Training Total: </span>" + eventTotal.ToString() + "<div>");
            strBuilder.Append("<div><span>Join Total: </span>" + joinTotal.ToString() + "<div>");
            strBuilder.Append("<div><span>Not Attend Total: </span>" + notAttendTotal.ToString() + "<div>");
        }
        catch
        {
            throw;
        }
        finally
        {
            dbAccess.close();
        }

        return strBuilder.ToString();
    }

    public string getSuggestionReport(string type, DateTime from, DateTime to,
        string loginID, string userGroup)
    {
        bool blankFromDate = from == DateTime.MinValue;
        bool blankToDate = to == DateTime.MinValue;

        int eventTotal = 0; 

        System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
        string sql = "select "
                    + "UserName \"User Name\", "
                    + "Email, "
                    + "Suggestion, "
                    + "Convert(varchar,CreateDate, 103) + ' ' + "
                    + "left(Convert(varchar,CreateDate, 114),5) \"Date & Time\" "
                    + "from Suggestion "
                    + "where Type in (" + type + ") "
                    + (blankFromDate ? "" : "and cast(CreateDate As Date) >= @StartTime ")
                    + (blankToDate ? "" : "and cast(CreateDate As Date) <= @EndTime ") ;

        Dictionary<string, object> dict = new Dictionary<string, object>();

        dict.Add("@Category", GlobalSetting.ArticleCategory.Training);
        dict.Add("@LoginID", loginID); 
        if (!blankFromDate)
            dict.Add("@StartTime", from);
        if (!blankToDate)
            dict.Add("@EndTime", to); 

        dbAccess.open();

        try
        {
            System.Data.DataTable dt = dbAccess.select(sql, dict);
            strBuilder.Append("<table class='ReportTable'>");
            strBuilder.Append("<tr>");
            foreach (System.Data.DataColumn col in dt.Columns)
            {
                if (col.ColumnName == "ID") continue;
                strBuilder.Append("<th>");
                strBuilder.Append(col.ColumnName);
                strBuilder.Append("</th>");
            }
            strBuilder.Append("</tr>");

             
            foreach (System.Data.DataRow row in dt.Rows)
            {
                eventTotal++;  
                strBuilder.Append("<tr>");
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    if (col.ColumnName == "ID") continue;
                    strBuilder.Append("<td>");
                     
                    if (col.ColumnName == "Type")
                    {
                        strBuilder.Append(SystemPara.getDescription(Convert.ToInt32(row[col.ColumnName])));
                    }
                    else
                    {
                        strBuilder.Append(row[col.ColumnName].ToString());
                    }

                    strBuilder.Append("</td>");
                }

                strBuilder.Append("</tr>");
            }
            strBuilder.Append("</table>");
            strBuilder.Append("<br/><div><span>Suggestion Total: </span>" + eventTotal.ToString() + "<div>"); 
        }
        catch
        {
            throw;
        }
        finally
        {
            dbAccess.close();
        }

        return strBuilder.ToString();
    }

    public string getEventReport(string serialNo, string type, string decision, string name, DateTime from, DateTime to, string loginID, string userGroup)
    {
        bool blankFromDate = from == DateTime.MinValue;
        bool blankToDate = to == DateTime.MinValue;

        int eventTotal = 0;
        int joinTotal = 0;
        int notAttendTotal = 0;

        System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
        string sql = "select e.SerialNo 'Serial No.', e.Name 'Event Name', "
                    + "Convert(varchar,e.StartTime, 103) + ' ' + left(Convert(varchar,e.StartTime, 114),5)+ ' ~ ' "
                    + "+ left(Convert(varchar,e.EndTime, 114),5) 'Date & Time', "
                    + "u.FullName 'User Name', "
                    + "case when a.UserAction = 'NotAttend' then 'Not Attend' else a.UserAction  END 'Decision' ,"
                    + "Convert(varchar,a.ActionDate, 103) + ' ' + "
                    + "left(Convert(varchar,a.ActionDate, 114),5) 'Decision Date' "
                    + "FROM ActivityLog a "
                    + "join [User] u on a.UserID = u.ID "
                    + "join Event e on a.ActivityID = e.ID "
                    + "where a.Category=@Category "
                    + (userGroup != GlobalSetting.SystemRoles.Normal ? "" : "and a.UserID = @LoginID ")
                    + (string.IsNullOrEmpty(name) ? "" : "and e.Name like '%' + @Name + '%'")
                    + (blankFromDate ? "" : "and cast(e.StartTime As Date) >= @StartTime ")
                    + (blankToDate ? "" : "and cast(e.EndTime As Date) <= @EndTime ")
                    + (string.IsNullOrEmpty(serialNo) ? "" : "and e.SerialNo = @SerialNo ")
                    + "and e.type in (" + type + ")"
                    + "and a.UserAction in (" + decision + ") "
                    + "order by e.StartTime, e.SerialNo, UserAction, u.Name";

        Dictionary<string, object> dict = new Dictionary<string, object>();

        dict.Add("@Category", GlobalSetting.ArticleCategory.Event);

        dict.Add("@LoginID", loginID);

        if (!string.IsNullOrEmpty(name))
            dict.Add("@Name", name);

        if (!blankFromDate)
            dict.Add("@StartTime", from);
        if (!blankToDate)
            dict.Add("@EndTime", to);
        if (!string.IsNullOrEmpty(serialNo))
            dict.Add("@SerialNo", serialNo);
               

        dbAccess.open();

        try
        {
            System.Data.DataTable dt = dbAccess.select(sql, dict);
            strBuilder.Append("<table class='ReportTable'>");
            strBuilder.Append("<tr>");
            foreach (System.Data.DataColumn col in dt.Columns)
            {
                strBuilder.Append("<th>");
                strBuilder.Append(col.ColumnName);
                strBuilder.Append("</th>");
            }
            strBuilder.Append("</tr>");
            foreach (System.Data.DataRow row in dt.Rows)
            {
                eventTotal++;
                if (row["Decision"].ToString() == "Join")
                {
                    joinTotal++;
                }
                else
                {
                    notAttendTotal++;
                }

                strBuilder.Append("<tr>");
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    strBuilder.Append("<td>");
                    strBuilder.Append(row[col.ColumnName].ToString());
                    strBuilder.Append("</td>");
                }

                strBuilder.Append("</tr>");
            }
            strBuilder.Append("</table>");

            strBuilder.Append("<br/><div><span>Event Total: </span>" + eventTotal.ToString() + "<div>");
            strBuilder.Append("<div><span>Join Total: </span>" + joinTotal.ToString() + "<div>");
            strBuilder.Append("<div><span>Not Attend Total: </span>" + notAttendTotal.ToString() + "<div>");

        }
        catch
        {
            throw;
        }
        finally
        {
            dbAccess.close();
        }

        return strBuilder.ToString();
    }
}
