using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using Jayrock.Json;

/// <summary>
/// Summary description for FileService
/// </summary>
public class File
{
    //object for DB access
    private DatabaseAccess dbAccess = new DatabaseAccess();

    public File()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public struct PathType
    {
        public const string Directory = "D";
        public const string File = "F";
    }

    public struct ArticleFileType
    {
        public const int News = 0;
        public const int Training = 1;
    }

    public void UploadFile(FileInfo fileInfo, List<DirectoryInfo> directories, byte[] fileContent)
    {
        this.dbAccess.open();

        string sql = "INSERT INTO [File] "
                   + "([FileID], [Type] ,[Name] ,[OriginalName] ,[Description] ,[Content] ,[UploadDate] ,[UploadUser],[Status]) VALUES "
                   + "(newid(), @Type, @Name, @OriginalName, @Description, @Content, @UploadDate, @UploadUser, @Status);select SCOPE_IDENTITY()";

        string sqlDirectoryCount = "SELECT count(*) FROM [Directory] WHERE FullName = @FullName and type=@Type"; 

        string sqlDirectory = "INSERT INTO [Directory] "
                            + "([Name]  "
                            + ",[Type],[Depth], [FullName]) "
                            + "VALUES (@Name, @Type, @Depth, @FullName);select SCOPE_IDENTITY() ";

        string sqlPath = "INSERT INTO [Path] ([ID],[Type],[DirectoryID]) "
                       + "select @ID,@dfType,d.ID from [Directory] d where d.FullName = @FullName and d.type=@Type ";


        Dictionary<string, object> dict;
        List<DirectoryInfo> childDirectories;
        DirectoryInfo tmpDirectory;
        this.dbAccess.open();
        this.dbAccess.BeginTransaction();
        try
        {

            //add directories
            int tmpCount = 0;
            int tmpID;
            string fileDirectory = "";
            for (int i = 0; i < directories.Count; i++)
            {
                tmpDirectory = directories[i];
                fileDirectory += tmpDirectory.Name + "/";
                childDirectories = new List<DirectoryInfo>();
                for (int j = 0; j < i; j++)
                {
                    childDirectories.Add(directories[j]);
                    tmpDirectory.FullName += directories[j].Name + "/";
                }

                dict = new Dictionary<string, object>();
                dict.Add("@PublishedStatus", GlobalSetting.ArticleStatus.Unpublished);
                dict.Add("@FullName", tmpDirectory.FullName + tmpDirectory.Name);
                dict.Add("@Type", tmpDirectory.Type);
                tmpCount = Convert.ToInt32(this.dbAccess.select(sqlDirectoryCount, dict).Rows[0][0]);

                if (tmpCount == 0)
                {

                    dict = new Dictionary<string, object>();

                    dict.Add("@Name", tmpDirectory.Name);
                    dict.Add("@Type", tmpDirectory.Type);
                    dict.Add("@Depth", tmpDirectory.Depth);
                    dict.Add("@FullName", tmpDirectory.FullName + tmpDirectory.Name);

                    tmpID = Convert.ToInt32(this.dbAccess.select(sqlDirectory, dict).Rows[0][0]);

                    if (tmpDirectory.FullName != null)
                    {

                        dict = new Dictionary<string, object>();

                        dict.Add("@ID", tmpID);
                        dict.Add("@dfType", PathType.Directory);
                        dict.Add("@Type", tmpDirectory.Type);
                        dict.Add("@FullName", tmpDirectory.FullName.Remove(tmpDirectory.FullName.Length - 1));

                        this.dbAccess.select(sqlPath, dict);
                    }


                }
            }



            //insert the News and return the row ID
            dict = new Dictionary<string, object>();
            dict.Add("@Type", fileInfo.Type);
            dict.Add("@Name", fileInfo.Name);
            dict.Add("@OriginalName", fileInfo.OriginalName);
            dict.Add("@Description", fileInfo.Description);
            dict.Add("@Content", fileContent);
            dict.Add("@UploadUser", fileInfo.UploadUser);
            dict.Add("@UploadDate", fileInfo.UploadDate);
            dict.Add("@Status", GlobalSetting.ArticleStatus.Unpublished);

            int tmpFileID = Convert.ToInt32(this.dbAccess.select(sql, dict).Rows[0][0]);


            if (fileDirectory != null)
            {

                dict = new Dictionary<string, object>();

                dict.Add("@ID", tmpFileID);
                dict.Add("@dfType", PathType.File);
                dict.Add("@Type", fileInfo.Type);
                dict.Add("@FullName", fileDirectory.Remove(fileDirectory.Length - 1));

                this.dbAccess.select(sqlPath, dict);
            }

            this.refreshDirectoryStatus();
            this.dbAccess.Commit();
        }
        catch
        {
            this.dbAccess.rollback();
            throw;
        }
        finally
        {
            this.dbAccess.close();
        }

    }

    public FileInfo GetFileInfo(int ID)
    {
        FileInfo fileInfo = new FileInfo();

        dbAccess.open();
        try
        {
            string sql = "select * from [File] where ID = " + ID.ToString();
            System.Data.DataTable dt = dbAccess.select(sql);
            foreach (System.Data.DataRow row in dt.Rows)
            {
                fileInfo.Description = row["Description"].ToString();
                fileInfo.ID = Convert.ToInt32(row["ID"]);
                fileInfo.Name = row["Name"].ToString();
                fileInfo.OriginalName = row["OriginalName"].ToString();
                //fileInfo.Path = row["Path"].ToString();
                fileInfo.Type = Convert.ToInt32(row["Type"]);
                fileInfo.UploadDate = Convert.ToDateTime(row["UploadDate"]);
                fileInfo.UploadUser = Convert.ToInt32(row["UploadUser"]);
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            dbAccess.close();
        }

        return fileInfo;
    }

    public byte[] GetFileByID(int ID)
    {
        byte[] fileBytes;
        dbAccess.open();
        try
        {
            string sql = "select content from [File] where ID = " + ID.ToString();
            System.Data.DataTable dt = dbAccess.select(sql);
            fileBytes = (byte[])dt.Rows[0]["content"];
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
            dbAccess.close();
        }

        return fileBytes;
    }

    public string GetPath(string term, string type)
    {
        term = term.Trim();
        int depth = term.Split('/').Length - 1;
        this.dbAccess.open();
        System.Text.StringBuilder result = new System.Text.StringBuilder();

        string sql = string.Format("select FullName from Directory where UPPER(FullName) like '{0}%' and depth = {1} and type= '{2}' order by FullName ", term.ToUpper(), depth, type);

        this.dbAccess.open();
        try
        {
            System.Data.DataTable dt = this.dbAccess.select(sql);

            result.Append("[");
            foreach (System.Data.DataRow row in dt.Rows)
            {
                if (result.Length > 1) result.Append(",");
                result.Append("\"");
                result.Append(row["FullName"].ToString());
                result.Append("\"");
            }
            result.Append("]");
        }
        catch
        {
            throw;
        }
        finally
        {
            this.dbAccess.close();
        }
        return result.ToString();
    }

    public JsonArray GetFileList(string type, string directory)
    {
        JsonArray ja = new JsonArray();
        JsonObject jo;
        String tmpStr;

        string sql = string.Format("select [File].ID, [Directory].FullName directory, [File].Name fileName, [File].OriginalName orginFileName, [File].Description , [File].Status,[File].uploaddate uploadDate from [File] "
                   + "join [Path] on [File].ID = [Path].ID and [Path].Type = 'F' "
                   + "join [Directory] on [Directory].ID = [Path].DirectoryID "
                   + " where [File].Type = '{0}' and ('{1}' = 'All' or [Directory].FullName = '{1}' )"
                   + " order by directory", type, directory);

        this.dbAccess.open();
        try
        {
            System.Data.DataTable dt = dbAccess.select(sql);

            foreach (System.Data.DataRow dr in dt.Rows)
            {
                jo = new JsonObject();
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    if (col.DataType == System.Type.GetType("System.DateTime"))
                    {
                        tmpStr = Convert.ToDateTime(dr[col.ColumnName]).ToString(GlobalSetting.DateTimeFormat);
                    }
                    else
                    {
                        tmpStr = dr[col.ColumnName].ToString();
                    }

                    jo.Accumulate(col.ColumnName.ToLower(), tmpStr);
                }
                ja.Add(jo);
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            this.dbAccess.close();
        }
        return ja;

    }

    public JsonArray GetDirectoryList(string type)
    {
        JsonArray ja = new JsonArray();
        JsonObject jo;
        String tmpStr;

        string sql = " select FullName from Directory "
                   + " where Type = '" + type + "'"
                   + " order by FullName";

        this.dbAccess.open();
        try
        {
            System.Data.DataTable dt = dbAccess.select(sql);

            foreach (System.Data.DataRow dr in dt.Rows)
            {
                jo = new JsonObject();
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    if (col.DataType == System.Type.GetType("System.DateTime"))
                    {
                        tmpStr = Convert.ToDateTime(dr[col.ColumnName]).ToString(GlobalSetting.DateTimeFormat);
                    }
                    else
                    {
                        tmpStr = dr[col.ColumnName].ToString();
                    }
                    jo.Accumulate(col.ColumnName.ToLower(), tmpStr);
                }
                ja.Add(jo);
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            this.dbAccess.close();
        }
        return ja;

    }

    public System.Text.StringBuilder getQuickLinkList(int type)
    {
        string sql = string.Format("select d.id,  d.Name, d.Depth, d2.ID childID, d2.Name childName, 'D' [type], '' OriginalName from Directory d "
                   + "join [Path] p on d.ID = p.DirectoryID "
                   + "join [Directory] d2 on p.ID = d2.ID "
                   + "join [SystemPara] s on s.Category = 'LinkType' and s.ID = d.type "
                   + "where d2.status = '" + GlobalSetting.ArticleStatus.Published + "' and p.[Type]  = 'D' and s.SubSequence = {0} "
                   + "union all "
                   + "select d.id,  d.Name, d.Depth, f.ID childID, f.description childName , 'F' [type], f.OriginalName OriginalName from Directory d "
                   + "join [Path] p on d.ID = p.DirectoryID "
                   + "join [File] f on p.ID = f.ID "
                   + "join [SystemPara] s on s.Category = 'LinkType' and s.ID = f.type "
                   + "where f.status = '" + GlobalSetting.ArticleStatus.Published + "' and p.[Type]  = 'F' and s.SubSequence = {0} "
                   + "order by  depth, d.Name, d.id, [type], childName "
                   + (type == 2? "desc " : ""), type);

        System.Data.DataTable dt = this.dbAccess.select(sql);

        System.Text.StringBuilder listBuilder = new System.Text.StringBuilder();
        this.getQuickLinkSubList(dt, 0, listBuilder);

        return listBuilder;
    }

    /// <summary>
    /// the helper function to contruct the tree view of quick link and newsletter
    /// </summary>
    /// <param name="dt">The data table contains the overall directory and file records</param>
    /// <param name="parentID">the parent folder ID if any</param>
    /// <param name="listBuilder">the string builder object to append all the html</param>
    /// <returns></returns>
    private System.Text.StringBuilder getQuickLinkSubList(System.Data.DataTable dt, int parentID, System.Text.StringBuilder listBuilder)
    {

        if (parentID == 0)
        {
            listBuilder.Append("<ul class='list'>");
            string lastID = string.Empty;
            foreach (System.Data.DataRow row in dt.Rows)
            {
                if (row["Depth"].ToString() != "0" || lastID == row["id"].ToString()) continue;
                lastID = row["id"].ToString();
                listBuilder.Append("<li><a>");
                listBuilder.Append(row["Name"].ToString());
                listBuilder.Append("</a>");
                this.getQuickLinkSubList(dt, Convert.ToInt32(row["id"]), listBuilder);
                listBuilder.Append("</li>");
            }
            listBuilder.Append("</ul>");
        }
        else
        {
            listBuilder.Append("<ul class='margin20px'>");
            foreach (System.Data.DataRow row in dt.Rows)
            {
                if (row["id"].ToString() != parentID.ToString()) continue;

                if (row["type"].ToString() == "D")
                {
                    listBuilder.Append("<li><a>");
                    listBuilder.Append(row["childName"]);
                    listBuilder.Append("</a>");
                    this.getQuickLinkSubList(dt, Convert.ToInt32(row["childID"]), listBuilder);
                }
                else
                {
                    listBuilder.Append(string.Format("<li class='list0'><a href='{0}'>", "Service/FileService.aspx?ID=" + row["childID"].ToString()));
                    listBuilder.Append(row["childName"]);
                    listBuilder.Append("</a>");
                }

                listBuilder.Append("</li>");

            }
            listBuilder.Append("</ul>");
        }

        return listBuilder;

    }

    public void DeleteFiles(string IDs)
    {

        string sql = string.Format("delete from [File] where ID in ({0}); "
                   + "delete from [path] where ID in ({0}) and type = '{1}';"
                   , IDs, PathType.File);
        dbAccess.open();
        try
        {
            dbAccess.BeginTransaction();

            dbAccess.update(sql);

            this.clearOrphanDirectory();
            this.refreshDirectoryStatus();

            dbAccess.Commit();

        }
        catch (Exception ex)
        {
            dbAccess.rollback();
            throw ex;
        }
        finally
        {
            dbAccess.close();
        }

    }

    public void updateFilesStatus(string IDs, string newStatus)
    {

        dbAccess.open();
        try
        {
            string sql = string.Format("update [File] set [Status] = '{1}' where ID in ({0}); "
                                    + "update [Directory] set [Status] = '{1}' "
                                    + "where exists (select 1 from Path where Directory.ID = Path.DirectoryID  "
                                    + "and Path.ID in ({0}) and Path.Type = 'F'); " 
                                     , IDs, newStatus);
            dbAccess.BeginTransaction();

            dbAccess.update(sql);

            this.refreshDirectoryStatus();

            dbAccess.Commit();

        }
        catch (Exception ex)
        {
            dbAccess.rollback();
            throw ex;
        }
        finally
        {
            dbAccess.close();
        }

    }

    public void clearOrphanDirectory()
    {

        int orphanDirectoryCount = 0;
        try
        {
            string sql = "select count(*) from Directory d1 "
                        + "where not exists "
                        + "(select 1 from Path p "
                        + "where p.DirectoryID = d1.ID) ";

            string sqlDelete = "delete from Directory "
                        + "where not exists "
                        + "(select 1 from Path p "
                        + "where p.DirectoryID = Directory.ID); "
                        + "delete from [path] where not exists(select 1 from [Directory] where Directory.ID = path.ID) and type = 'D'; ";


            while (true) {
                orphanDirectoryCount = Convert.ToInt32(dbAccess.select(sql).Rows[0][0]);
                if (orphanDirectoryCount == 0) break;

                dbAccess.update(sqlDelete);
            }  
        }
        catch
        {
            throw;
        }
    }

    public void refreshDirectoryStatus()
    { 
        try
        {
            string sqlCount = string.Format("select count(*) from [Directory] "
                         + "where [Directory].[Status] = '{0}' and not exists "
                         + "( "
                         + "select 1 from [Path]  p "
                         + "join [File] f on p.ID = f.ID and p.Type = 'F' "
                         + "where [Directory].ID = p.DirectoryID and f.[Status] = '{0}' "
                         + "union all select 1 from [Path]  p "
                         + "join [Directory] d on p.ID = d.ID and p.Type = 'D' "
                         + "where [Directory].ID = p.DirectoryID and d.[Status] = '{0}' "
                         + ") ", GlobalSetting.ArticleStatus.Published);

            string sql = string.Format("update [Directory] set [Status] = '{0}' "
                         + "where not exists "
                         + "( "
                         + "select 1 from [Path]  p "
                         + "join [File] f on p.ID = f.ID and p.Type = 'F' "
                         + "where [Directory].ID = p.DirectoryID and f.[Status] = '{1}' "
                         + "union all select 1 from [Path]  p "
                         + "join [Directory] d on p.ID = d.ID and p.Type = 'D' "
                         + "where [Directory].ID = p.DirectoryID and d.[Status] = '{1}' "
                         + ") ", GlobalSetting.ArticleStatus.Unpublished, GlobalSetting.ArticleStatus.Published);



            int enableDirectoryCount = 0;
            while (true)
            {
                dbAccess.update(sql);

                enableDirectoryCount = Convert.ToInt32(dbAccess.select(sqlCount).Rows[0][0]);
                if (enableDirectoryCount == 0) break; 
            }

            sqlCount = string.Format("select count(*) from [Directory] "
                         + "where [Directory].[Status] = '{0}' and exists "
                          + "( "
                          + "select 1 from [Path]  p "
                          + "join [File] f on p.ID = f.ID and p.Type = 'F' "
                          + "where [Directory].ID = p.DirectoryID and f.[Status] = '{1}' "
                          + "union all select 1 from [Path]  p "
                          + "join [Directory] d on p.ID = d.ID and p.Type = 'D' "
                          + "where [Directory].ID = p.DirectoryID and d.[Status] = '{1}' "
                          + ") ", GlobalSetting.ArticleStatus.Unpublished, GlobalSetting.ArticleStatus.Published);
            sql = string.Format("update [Directory] set [Status] = '{0}' "
                          + "where exists "
                          + "( "
                          + "select 1 from [Path]  p "
                          + "join [File] f on p.ID = f.ID and p.Type = 'F' "
                          + "where [Directory].ID = p.DirectoryID and f.[Status] = '{1}' "
                          + "union all select 1 from [Path]  p "
                          + "join [Directory] d on p.ID = d.ID and p.Type = 'D' "
                          + "where [Directory].ID = p.DirectoryID and d.[Status] = '{1}' "
                          + ") ", GlobalSetting.ArticleStatus.Published, GlobalSetting.ArticleStatus.Published);


            enableDirectoryCount = 0;
            while (true)
            {
                dbAccess.update(sql);

                enableDirectoryCount = Convert.ToInt32(dbAccess.select(sqlCount).Rows[0][0]);
                if (enableDirectoryCount == 0) break;
            }  
             
        }
        catch
        {
            throw;
        }
    }
}
