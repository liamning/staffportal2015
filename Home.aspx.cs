using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
public partial class _Home : System.Web.UI.Page
{
    public string headline;
    public string summary;
    public string summaryimage;
    public string summaryimage2;
    public string articleLink;
    public System.Text.StringBuilder latestNews;
    public System.Text.StringBuilder latestTraining;
    public System.Text.StringBuilder latestEvent;
    public System.Text.StringBuilder otherSystems;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            this.setMaxLength();
            this.ControlDataBind();
        }

        //get latest article
        News article = new News();
        List<NewsInfo> latestList = article.getLatestNews(5);

        latestNews = new System.Text.StringBuilder();
        int i = 0;
        foreach (NewsInfo news in latestList)
        {
            i++;
            if (latestNews.Length == 0)
            {
                //the height light news
                NewsInfo articleInfo = latestList[0];
                headline = articleInfo.Headline.Replace("\r\n", " ");
                summary = articleInfo.Summary.Replace("\r\n", " ");
                List<ImageInfo> imageList = articleInfo.getImageList();
                summaryimage = "Service/ImageService.aspx?ID=" + imageList[0].ID.ToString();
                summaryimage2 = "Service/ImageService.aspx?ID=" + imageList[1].ID.ToString();
                articleLink = "ViewArticle.aspx?ID=" + articleInfo.ID.ToString();
            }
            //append top 5 news 
            if (news.NewIconInfo != null
                && DateTime.Now.Date < news.NewIconInfo.ExpiryDate)
            {
                //latestNews.Append(string.Format("<li style='word-wrap:break-word;'><a href='{1}' style='position: relative;'><span class='blueFont'>{2:" 
                //    + GlobalSetting.DateTimeFormat 
                //    + "}</span><span style='margin-right: 25px;'>&nbsp&nbsp&nbsp{0}&nbsp&nbsp&nbsp</span><img src='Resource/Image/New_icons.gif' style='position: absolute; bottom: 0.5px;'></a></li>", 
                //    news.Title,
                //    "ViewArticle.aspx?ID=" + news.ID.ToString(), 
                //    news.EffectiveDate));

                latestNews.Append(string.Format("<li class='newIcon' style='word-wrap:break-word;'><a href='{1}' style='position: relative;'><span class='blueFont'>{2:"
                    + GlobalSetting.DateTimeFormat + "}</span>&nbsp&nbsp&nbsp{0}&nbsp</a></li>", news.Title,
                                        "ViewArticle.aspx?ID=" + news.ID.ToString(), news.EffectiveDate));
            }
            else
            {
                latestNews.Append(string.Format("<li style='word-wrap:break-word;'><a href='{1}' style='position: relative;'><span class='blueFont'>{2:"
                    + GlobalSetting.DateTimeFormat + "}</span>&nbsp&nbsp&nbsp{0}&nbsp</a></li>", news.Title,
                                        "ViewArticle.aspx?ID=" + news.ID.ToString(), news.EffectiveDate));
            }

        } 

        //get top 5 training 
        Training trainingHandler = new Training();
        List<TrainingInfo> latestTrainingList = trainingHandler.getLatestTrainings(5);
        latestTraining = new System.Text.StringBuilder();
        foreach (TrainingInfo training in latestTrainingList)
        {

            if (training.NewIconInfo != null
                && DateTime.Now.Date < training.NewIconInfo.ExpiryDate)
            {
                latestTraining.Append(string.Format("<li class='newIcon' style='word-wrap:break-word;'><a href='{1}'><span class='blueFont'>{2:" + GlobalSetting.DateTimeFormat + "}</span>&nbsp&nbsp&nbsp{0}&nbsp</a></li>", training.Name,
                                        "ViewTraining.aspx?ID=" + training.ID.ToString(), training.Schedule[0].StartTime));
            }
            else
            {
                latestTraining.Append(string.Format("<li style='word-wrap:break-word;'><a href='{1}'><span class='blueFont'>{2:" + GlobalSetting.DateTimeFormat + "}</span>&nbsp&nbsp&nbsp{0}&nbsp</a></li>", training.Name,
                                        "ViewTraining.aspx?ID=" + training.ID.ToString(), training.Schedule[0].StartTime));
            }
        }

        //get top 5 Event 
        Event eventHandler = new Event();
        List<EventInfo> eventList = eventHandler.getLatestEvent(5);
        latestEvent = new System.Text.StringBuilder();
        foreach (EventInfo eventInfo in eventList)
        {

            if (eventInfo.NewIconInfo != null
                && DateTime.Now.Date < eventInfo.NewIconInfo.ExpiryDate)
            {
                latestEvent.Append(string.Format("<li class='newIcon' style='word-wrap:break-word;'><a href='{1}'><span class='blueFont'>{2:" + GlobalSetting.DateTimeFormat + "}</span>&nbsp&nbsp&nbsp{0}</a></li>", eventInfo.Name,
                                        "ViewEvent.aspx?ID=" + eventInfo.ID.ToString(), eventInfo.StartTime));
            }
            else
            {
                latestEvent.Append(string.Format("<li style='word-wrap:break-word;'><a href='{1}'><span class='blueFont'>{2:" + GlobalSetting.DateTimeFormat + "}</span>&nbsp&nbsp&nbsp{0}</a></li>", eventInfo.Name,
                                        "ViewEvent.aspx?ID=" + eventInfo.ID.ToString(), eventInfo.StartTime));
            }
        }

        File file = new File();
        System.Text.StringBuilder quickLinkBuilder = file.getQuickLinkList(1);
        divQuickLinks.InnerHtml = quickLinkBuilder.ToString();
        System.Text.StringBuilder newsLetters = file.getQuickLinkList(2);
        divNewsLetters.InnerHtml = newsLetters.ToString();



        //get top 5 other system links 
        OtherSystemLink otherSystemLink = new OtherSystemLink();
        List<OtherSystemLinkInfo> otherSystemLinkList = otherSystemLink.getSystemLinkDetailList();
        otherSystems = new System.Text.StringBuilder();
        foreach (OtherSystemLinkInfo item in otherSystemLinkList)
        {
            otherSystems.Append(string.Format("<li style='word-wrap:break-word;'><a href='{1}'>{0}</a></li>", 
                                item.Name,
                                "javascript: var win = window.open(\"" + item.Link + "\");"));

            
        }

    }



    protected void ControlDataBind()
    {
        System.Data.DataTable EventType = SystemPara.getSystemPara("SuggestionType");

        foreach (System.Data.DataRow row in EventType.Rows)
        {
            this.comSuggestionType.Items.Add(new ListItem(row["Description"].ToString(), row["ID"].ToString()));
        }

    }



    protected void setMaxLength()
    {
        txtSuggestion.Attributes["maxLength"] = GlobalSetting.FieldLength.Suggestion.Content;
        txtSuggestionOtherEmail.Attributes["maxLength"] = GlobalSetting.FieldLength.EmailAddress;
        txtSuggestionTel.Attributes["maxLength"] = GlobalSetting.FieldLength.PhoneNumber;
    }
}

