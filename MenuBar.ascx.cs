using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Control_MenuBar : System.Web.UI.UserControl
{


    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            this.setControlVisible();
            //this.setMaxLength();
            //this.ControlDataBind();
        }

    }
     

    protected void setControlVisible()
    {
        if (Session["USERGROUP"].ToString().Equals(GlobalSetting.SystemRoles.Normal))
        {
            liAdmin.Visible = false;
            liAdmin1.Visible = false;
            liAdmin2.Visible = false;
        }


        if (!Session["USERGROUP"].ToString().Equals(GlobalSetting.SystemRoles.Super))
        {
            liChangeUserRole.Visible = false; 
            liSuggestionReport.Visible = false;
        }


        if (GlobalSetting.LoginMethod == "AUTO")
        {
            spanLogout.Visible = false;
            liApprove.Visible = false;
        }

        if (GlobalSetting.DisabledFunctionDict.ContainsKey("/TRAININGREPORT.ASPX"))
        {
            liTrainingReport.Visible = false; 
        }

        if (GlobalSetting.DisabledFunctionDict.ContainsKey("/EVENTREPORT.ASPX"))
        { 
            liEventReport.Visible = false;
        }
    }

     
}
