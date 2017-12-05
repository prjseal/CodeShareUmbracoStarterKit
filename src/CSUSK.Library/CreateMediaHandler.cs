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
                LogHelper.Info(System.Reflection.MethodBase.GetCurrentMethod().GetType(), "Executing CreateMediaHandler");
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
            const string SLIDE_IMAGE_ALIAS = "slideImage";
            const string MAIN_IMAGE_ALIAS = "mainImage";

            try
            {
                IContentService _contentService = ApplicationContext.Current.Services.ContentService;


                int parentId = -1;
                IMedia codeImage = CreateMediaItem(parentId, "code.jpg");
                IMedia carouselImage = CreateMediaItem(parentId, "carousel.jpg");
                IMedia navigationImage = CreateMediaItem(parentId, "navigation.jpg");
                IMedia partyImage = CreateMediaItem(parentId, "party.jpg");

                Guid homeId = new Guid("6352118a-8432-4a53-9572-b5555b9cf3e3");
                IContent homePage = _contentService.GetById(homeId);
                string carouselData = homePage.GetValue<string>("mainCarousel");

                JArray carouselObject = JArray.Parse(carouselData);

                carouselObject[0][SLIDE_IMAGE_ALIAS] = codeImage.GetUdi().ToString();
                carouselObject[1][SLIDE_IMAGE_ALIAS] = navigationImage.GetUdi().ToString();
                carouselObject[2][SLIDE_IMAGE_ALIAS] = carouselImage.GetUdi().ToString();

                homePage.SetValue("mainCarousel", carouselObject.ToString());
                _contentService.SaveAndPublishWithStatus(homePage);

                IContent aboutPage = homePage.Children().Where(x => x.Name == "About").FirstOrDefault();
                aboutPage.SetValue(MAIN_IMAGE_ALIAS, partyImage.GetUdi().ToString());
                _contentService.Save(aboutPage);

                IContent blog = homePage.Children().Where(x => x.Name == "Blog").FirstOrDefault();

                IContent article1 = blog.Children().Where(x => x.Name == "CodeShare Starter Kit is released").FirstOrDefault();
                article1.SetValue(MAIN_IMAGE_ALIAS, partyImage.GetUdi().ToString());
                _contentService.Save(article1);

                IContent article2 = blog.Children().Where(x => x.Name == "Something interesting").FirstOrDefault();
                article2.SetValue(MAIN_IMAGE_ALIAS, codeImage.GetUdi().ToString());
                _contentService.Save(article2);

                IContent article3 = blog.Children().Where(x => x.Name == "Another blog post").FirstOrDefault();
                article3.SetValue(MAIN_IMAGE_ALIAS, carouselImage.GetUdi().ToString());
                _contentService.Save(article3);


                SaveAndPublishChildPages(homePage.Children(), _contentService);

                LogHelper.Info(System.Reflection.MethodBase.GetCurrentMethod().GetType(), "Created Media Items");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().GetType(), "Error when creating media items.", ex);
                return false;
            }
        }

        private void SaveAndPublishChildPages(IEnumerable<IContent> items, IContentService _contentService)
        {
            foreach(IContent item in items)
            {
                _contentService.SaveAndPublishWithStatus(item);
                if(item.Children() != null && item.Children().Any())
                {
                    SaveAndPublishChildPages(item.Children(), _contentService);
                }
            }
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
            const string sample = "<Action runat=\"install\" undo=\"true\" alias=\"CreateMediaHandler\"></Action>";
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