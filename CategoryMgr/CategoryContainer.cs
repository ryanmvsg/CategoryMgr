using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CategoryMgr
{
    public sealed class CategoryContainer
    {
        Dictionary<string, CategoryItm> gl_catIdLst = null;
        private List<CategoryItm> gl_Items = null;

        public CategoryContainer(Dictionary<string, CategoryItm> catIdLst = null, List<CategoryItm> catLst = null)
        {
            if (catLst == null)
                this.gl_Items = new List<CategoryItm>();
            else
                this.gl_Items = catLst;

            if (catIdLst == null)
                this.gl_catIdLst = new Dictionary<string, CategoryItm>();
            else
                this.gl_catIdLst = catIdLst;
        }

        public bool AddCategoryLink(CategoryItm catItm)
        {
            try
            {
                if (!this.gl_catIdLst.ContainsKey(catItm.CategoryId))
                {
                    this.gl_catIdLst.Add(catItm.CategoryId, catItm);
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public List<CategoryItm> BuildCategoryToRootList(string catId)
        {
            List<CategoryItm> catLst = new List<CategoryItm>();
            if (this.gl_catIdLst.ContainsKey(catId))
            {
                CategoryItm cItm = this.gl_catIdLst[catId];
                LoadNodes(cItm, catLst);
            }
            else
            {
                return null;
            }
            return catLst;
        }

        private void LoadNodes(CategoryItm catItm, List<CategoryItm> catLst)
        {
            catLst.Add(catItm);
            if (catItm.Parent != null && catItm.Parent is CategoryItm)
            {
                LoadNodes(catItm.Parent, catLst);
            }
        }

        /// <summary>
        /// Gets a duplicate of this container (this is good for methods that need to provide data of categories but
        /// don't want the ability to alter the main source).
        /// </summary>
        /// <returns>The category container duplicated or null if it fails for any reason.</returns>
        public CategoryContainer DuplicateContainer()
        {
            try
            {
                return (new CategoryContainer(new Dictionary<string, CategoryItm>(this.gl_catIdLst), new List<CategoryItm>(this.gl_Items)));
            }
            catch
            {
                return null;
            }
        }

        public bool ExportContainerToFile(string path = @".\container.xml")
        {
            try
            {
                XDocument xDoc = XDocument.Parse("<Categories><CategoryRoot></CategoryRoot></Categories>");
                var categoryQuery = from category in this.gl_Items
                                    select category;
                foreach (var category in categoryQuery)
                {
                    string CategoryId = category.CategoryId;
                    string CategoryName = category.CategoryName;
                    CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, "");
                    //catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                    XElement tItm = new XElement("Category", new XAttribute("Id", CategoryId), new XAttribute("CategoryName", CategoryName));
                    //tItm.Selected += new RoutedEventHandler(tItm_Selected);
                    //tItm.Header = catDev;
                    xDoc.Root.Element("CategoryRoot").Add(tItm);
                    //this.gl_catDevLst.Add(CategoryId, catDev);

                    if (category.Items.Count > 0)
                    {
                        LoadAllSourceNodes(tItm, category, CategoryId);
                    }
                }
                xDoc.Save(path);
                return true;
            }
            catch
            {
            }
            return false;
        }

        private bool LoadAllSourceNodes(XElement tItm, CategoryItm xContainer, string parentId = "")
        {
            try
            {
                var categoryQuery = from category in xContainer.Items
                                    select category;

                foreach (var category in categoryQuery)
                {
                    string CategoryId = category.CategoryId;
                    string CategoryName = category.CategoryName;
                    //CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, parentId);
                    //catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                    XElement tItm2 = new XElement("Category", new XAttribute("Id", CategoryId), new XAttribute("CategoryName", CategoryName));
                    //tItm2.Selected += new RoutedEventHandler(tItm_Selected);
                    //tItm2.Header = catDev;
                    tItm.Add(tItm2);
                    //this.gl_catDevLst.Add(CategoryId, catDev);

                    if (category.Items.Count > 0)
                    {
                        LoadAllSourceNodes(tItm2, category, CategoryId);
                    }
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        public List<CategoryItm> Items
        {
            get { return this.gl_Items; }
        }

        public Dictionary<string, CategoryItm> IdLinkList
        {
            get { return this.gl_catIdLst; }
        }
    }
}
