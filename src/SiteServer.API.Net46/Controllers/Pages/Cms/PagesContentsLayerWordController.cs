﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using SiteServer.API.Common;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Create;
using SiteServer.CMS.Core.Office;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Model;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Pages.Cms
{
    [RoutePrefix("pages/cms/contentsLayerWord")]
    public class PagesContentsLayerWordController : ControllerBase
    {
        private const string Route = "";
        private const string RouteUpload = "actions/upload";

        [HttpGet, Route(Route)]
        public IHttpActionResult GetConfig()
        {
            try
            {
                var request = GetRequest();

                var siteId = request.GetQueryInt("siteId");
                var channelId = request.GetQueryInt("channelId");

                if (!request.IsAdminLoggin ||
                    !request.AdminPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentAdd))
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var isChecked = CheckManager.GetUserCheckLevel(request.AdminPermissionsImpl, siteInfo, siteId, out var checkedLevel);
                var checkedLevels = CheckManager.GetCheckedLevels(siteInfo, isChecked, checkedLevel, false);

                return Ok(new
                {
                    Value = checkedLevels,
                    CheckedLevel = CheckManager.LevelInt.CaoGao
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(RouteUpload)]
        public IHttpActionResult Upload()
        {
            try
            {
                var request = GetRequest();

                var siteId = request.GetQueryInt("siteId");
                var channelId = request.GetQueryInt("channelId");

                if (!request.IsAdminLoggin ||
                    !request.AdminPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentAdd))
                {
                    return Unauthorized();
                }

                var fileName = request.GetPostString("fileName");

                var fileCount = request.Files.Count;

                string filePath = null;

                if (fileCount > 0)
                {
                    var file = request.Files[0];

                    if (string.IsNullOrEmpty(fileName)) fileName = Path.GetFileName(file.FileName);

                    var extendName = fileName.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal)).ToLower();
                    if (extendName == ".doc" || extendName == ".docx")
                    {
                        filePath = PathUtils.GetTemporaryFilesPath(fileName);
                        DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                        file.SaveAs(filePath);
                    }
                }

                FileInfo fileInfo = null;
                if (!string.IsNullOrEmpty(filePath))
                {
                    fileInfo = new FileInfo(filePath);
                }
                if (fileInfo != null)
                {
                    return Ok(new
                    {
                        fileName,
                        length = fileInfo.Length,
                        ret = 1
                    });
                }

                return Ok(new
                {
                    ret = 0
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit()
        {
            try
            {
                var request = GetRequest();

                var siteId = request.GetPostInt("siteId");
                var channelId = request.GetPostInt("channelId");
                var isFirstLineTitle = request.GetPostBool("isFirstLineTitle");
                var isFirstLineRemove = request.GetPostBool("isFirstLineRemove");
                var isClearFormat = request.GetPostBool("isClearFormat");
                var isFirstLineIndent = request.GetPostBool("isFirstLineIndent");
                var isClearFontSize = request.GetPostBool("isClearFontSize");
                var isClearFontFamily = request.GetPostBool("isClearFontFamily");
                var isClearImages = request.GetPostBool("isClearImages");
                var checkedLevel = request.GetPostInt("checkedLevel");
                var fileNames = TranslateUtils.StringCollectionToStringList(request.GetPostString("fileNames"));

                if (!request.IsAdminLoggin ||
                    !request.AdminPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentAdd))
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var styleInfoList = TableStyleManager.GetContentStyleInfoList(siteInfo, channelInfo);
                var isChecked = checkedLevel >= siteInfo.CheckContentLevel;

                var contentIdList = new List<int>();

                foreach (var fileName in fileNames)
                {
                    if (string.IsNullOrEmpty(fileName)) continue;

                    var (title, content) = WordManager.GetWord(siteInfo, isFirstLineTitle, isFirstLineRemove,
                        isClearFormat,
                        isFirstLineIndent, isClearFontSize, isClearFontFamily, isClearImages, fileName);

                    var contentInfo = new ContentInfo
                    {
                        ChannelId = channelInfo.Id,
                        SiteId = siteId,
                        AdminId = request.AdminId,
                        AddUserName = request.AdminName,
                        AddDate = DateTime.Now,
                        Checked = isChecked,
                        CheckedLevel = checkedLevel,
                        Title = title,
                        Content = content
                    };

                    contentInfo.LastEditUserName = contentInfo.AddUserName;
                    contentInfo.LastEditDate = contentInfo.AddDate;

                    contentInfo.Id = channelInfo.ContentDao.Insert(siteInfo, channelInfo, contentInfo);

                    contentIdList.Add(contentInfo.Id);
                }

                if (isChecked)
                {
                    foreach (var contentId in contentIdList)
                    {
                        CreateManager.CreateContent(siteId, channelInfo.Id, contentId);
                    }
                    CreateManager.TriggerContentChangedEvent(siteId, channelInfo.Id);
                }

                return Ok(new
                {
                    Value = contentIdList
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }
    }
}
