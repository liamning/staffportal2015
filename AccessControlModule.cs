using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.DirectoryServices;
using System.Web.Hosting;

/// <summary>
/// Summary description for AccessControlModule
/// </summary>
public class AccessControlModule : IHttpModule
{

    public static string GetProperty(SearchResult searchResult, string PropertyName)
    {
        if (searchResult.Properties.Contains(PropertyName))
        {
            return searchResult.Properties[PropertyName][0].ToString();
        }
        else
        {
            return string.Empty;
        }
    }

    public static void getADInfo(string loginName, string sid, ref string email, ref string fullName)
    {
        try
        {

            string[] loginArray = loginName.Split('\\');
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + loginArray[0] + "");
            //DirectoryEntry entry = new DirectoryEntry("LDAP://192.168.11.242/DC=BLUECROSS");
            //DirectoryEntry entry = new DirectoryEntry("LDAP://192.168.0.242", "Administrator", "JKTeam123", AuthenticationTypes.Secure);
            DirectorySearcher Dsearch = new DirectorySearcher(entry);
            String Name = loginArray[1];
            //Dsearch.Filter = "(cn=" + Name + ")";

            Dsearch.Filter = "(objectSid=" + sid + ")";

            using (HostingEnvironment.Impersonate())
            {
                foreach (SearchResult sResultSet in Dsearch.FindAll())
                {
                    email = GetProperty(sResultSet, "mail");
                    fullName = GetProperty(sResultSet, "displayName");
                    break;
                }
            }


        }
        catch (Exception ex)
        {
            Log.log(ex.StackTrace, Log.Type.Exception);
        }
    }

    public void Dispose()
    {

    }

    public void Init(HttpApplication context)
    {
        context.AcquireRequestState += new EventHandler(context_AcquireRequestState);
    }

    void context_AcquireRequestState(object sender, EventArgs e)
    {

        if (GlobalSetting.LoginMethod == "AUTO")
        {
            AutoLogin(sender, e);
        }
        else
        {
            NormalLogin(sender, e);
        }

    }

    public static void AutoLogin(object sender, EventArgs e)
    {
        HttpApplication app = (HttpApplication)sender;

        //get the request url
        //string path = app.Context.Request.Path.Replace(app.Context.Request.ApplicationPath,"").ToUpper();
        string path = app.Context.Request.Path.ToUpper();

        if (path.Contains(".ASPX"))
        {

            if (app.Session["LOGINID"] == null)
            {
                //System.Security.Principal.IPrincipal user;

                //user = System.Web.HttpContext.Current.User;

                //System.Security.Principal.IIdentity identity;

                // identity = user.Identity;
                // string test = Environment.UserDomainName;

                WindowsIdentity currentWindowLogin = WindowsIdentity.GetCurrent();

                UserInfo userinfo = new UserInfo();
                string email = "", fullName = "";
                getADInfo(currentWindowLogin.Name, currentWindowLogin.User.ToString(), ref email, ref fullName);


                userinfo.SID = currentWindowLogin.User.ToString();
                userinfo.Name = currentWindowLogin.Name;

                if (string.IsNullOrEmpty(fullName)) fullName = userinfo.Name.Split('\\')[1];

                userinfo.FullName = fullName;
                userinfo.Domain = userinfo.Name.Split('\\')[0];
                userinfo.Email = email;
                //userinfo.LastLoginDateTime = DateTime.Now;
                userinfo.LoginDateTime = DateTime.Now;

                bool isValidDomain = false;
                if (GlobalSetting.ValidDomains[0] == "")
                {
                    isValidDomain = true;
                }
                else
                {
                    foreach (string domain in GlobalSetting.ValidDomains)
                    {
                        //check whether the domain is in the valid list
                        if (domain.Equals(userinfo.Domain))
                        {
                            isValidDomain = true;
                            break;
                        }
                    }
                }

                if (!isValidDomain)
                {
                    app.Context.Response.Redirect("PageNotFound.html");
                }
                else
                {
                    User userHandler = new User();
                    userHandler.login(userinfo);

                    app.Session["LOGINID"] = userinfo.ID;
                    app.Session["LOGINNAME"] = userinfo.FullName;
                    app.Session["USERGROUP"] = userinfo.UserGroup;
                    app.Session["EMAIL"] = userinfo.Email;
                    app.Session["LASTLOGINDATETIME"] = userinfo.LastLoginDateTime.ToString(GlobalSetting.DateTimeFormat + " HH:mm:ss");
                }

            }


            AccessControl(app, path);


        }
    }

    public static void NormalLogin(object sender, EventArgs e)
    {
        HttpApplication app = (HttpApplication)sender;

        //get the request url
        string path = app.Context.Request.Path.ToUpper();

        if (path.EndsWith("ASPX"))
        {

            //check whether the user has logined the website
            if (!path.EndsWith("LOGIN.ASPX") && !path.EndsWith("REGISTER.ASPX"))
            {
                if (app.Session["LOGINID"] == null)
                {
                    app.Context.Response.Redirect("login.aspx");
                    //app.Context.Server.Transfer("login.aspx");
                }
            }

            AccessControl(app, path);
        }
    }

    public static void AccessControl(HttpApplication app, string path)
    {

        //redirect to home page if some invalid pages are requested
        if (app.Session["USERGROUP"].ToString().Equals(GlobalSetting.SystemRoles.Normal))
        {
            if (GlobalSetting.AdminFunctionDict.ContainsKey(path) || GlobalSetting.SuperFunctionDict.ContainsKey(path))
            {
                app.Context.Response.Redirect("PageNotFound.html");
            }
        }
        else if (app.Session["USERGROUP"].ToString().Equals(GlobalSetting.SystemRoles.Admin))
        {
            if (GlobalSetting.SuperFunctionDict.ContainsKey(path))
            {
                app.Context.Response.Redirect("PageNotFound.html");
            }
        }

        if (GlobalSetting.DisabledFunctionDict.ContainsKey(path))
        {
            app.Context.Response.Redirect("PageNotFound.html");
        }
    }
}
