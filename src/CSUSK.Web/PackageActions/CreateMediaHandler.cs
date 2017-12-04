using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using ClientDependency.Core;
using Newtonsoft.Json.Linq;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Logging;

using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.packager.standardPackageActions;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Core.Services;
using System.IO;
using System.Web;

namespace CSUSK.Web.PackageActions
{
    public class CreateMediaHandler : IPackageAction
    {
        IMediaService _mediaService = ApplicationContext.Current.Services.MediaService;

        public string Alias()
        {
            return "CreateMediaHandler";
        }

        public bool Execute(string packageName, XmlNode xmlData)
        {
            try
            {
                return CreateMediaItems();
            }
            catch (Exception ex)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().GetType(), "INSTALL Package Error", ex);
                return false;
            }
        }

        public bool CreateMediaItems()
        {
            bool success = false;
            int parentId = -1;
            IMedia carousel = CreateMediaItem(parentId, "carousel.jpg");
            IMedia code = CreateMediaItem(parentId, "code.jpg");
            IMedia navigation = CreateMediaItem(parentId, "navigation.jpg");
            IMedia party = CreateMediaItem(parentId, "party.jpg");
            return success;
        }

        private IMedia CreateMediaItem(int parentId, string fileName)
        {
            IMedia newFile = _mediaService.CreateMedia(fileName, parentId, "Image");
            string filePath = HttpContext.Current.Server.MapPath("~/img/" + fileName);
            using (FileStream stream = System.IO.File.Open(filePath, FileMode.Open))
            {
                newFile.SetValue("umbracoFile", fileName, stream);
            }
            _mediaService.Save(newFile);
            return newFile;
        }

        public XmlNode SampleXml()
        {
            const string sample = "";
            return ParseStringToXmlNode(sample);
        }

        private static XmlNode ParseStringToXmlNode(string value)
        {
            var xmlDocument = new XmlDocument();
            var xmlNode = AddTextNode(xmlDocument, "error", "");

            try
            {
                xmlDocument.LoadXml(value);
                return xmlDocument.SelectSingleNode(".");
            }
            catch
            {
                return xmlNode;
            }
        }

        private static XmlNode AddTextNode(XmlDocument xmlDocument, string name, string value)
        {
            var node = xmlDocument.CreateNode(XmlNodeType.Element, name, "");
            node.AppendChild(xmlDocument.CreateTextNode(value));
            return node;
        }

        public bool Undo(string packageName, XmlNode xmlData)
        {
            return true;
            //Can't really undo this.
        }
    }
}