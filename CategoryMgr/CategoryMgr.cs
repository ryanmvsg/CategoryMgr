using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using Microsoft.Win32;

namespace CategoryMgr
{
    public sealed class CategoryMgr
    {
        private string gl_configPath = @".\Categories.xml";
        private XDocument gl_xDoc = null;
        //private TreeView xCatTreeLst = null;
        private CategoryContainer gl_container = null;
        private CategoryContainer gl_eBayContainer = null;
        //private Dictionary<string, CategoryDef> gl_catDef = new Dictionary<string, CategoryDef>();

        public CategoryMgr(string configPath = @".\Categories.xml")
        {
            try
            {
                this.gl_xDoc = XDocument.Load(this.gl_configPath);
                //this.xCatTreeLst = new TreeView();
                this.gl_container = new CategoryContainer();
                this.gl_eBayContainer = new CategoryContainer();
            }
#if DEBUG
            catch(FileNotFoundException fnfe)
            {
                MessageBox.Show(string.Format("Configuration file was not found for the category manager. Categories will not be loaded. Please locate the categories.xml file. Error: {0}", fnfe.ToString()));
                OpenFileDialog oFD = new OpenFileDialog();
                oFD.Filter = "XML files (*.xml)|*.xml";
                oFD.ShowDialog();
                if (oFD.FileName != "")
                {
                    try
                    {
                        this.gl_xDoc = XDocument.Load(this.gl_configPath);
                        //this.xCatTreeLst = new TreeView();
                        this.gl_container = new CategoryContainer();
                        this.gl_eBayContainer = new CategoryContainer();
                    }
#if DEBUG
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
#else
                    catch
                    {
#endif
                        if (MessageBox.Show("An error was thrown while trying to load the xml file you chose. Unexpected behaviour could arise. You should restart this application once you have located the categories file. Or we could create a new one for you. Would you like us to create a new template for you?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                        {
                            try
                            {
                                //
                            }
#if DEBUG
                            catch(Exception ex2)
                            {
                                System.Diagnostics.Debug.WriteLine(ex2.ToString());
#else
                            catch
                            {
#endif
                            }
                        }
                    }
                }
            }
#else
            catch(FileNotFoundException)
            {
                MessageBox.Show("Configuration file was not found for the category manager. Categories will not be loaded.");
            }
#endif
            LoadInitCategories();

            /*foreach (KeyValuePair<string, CategoryDef> catDefItm in catDef)
            {
                var selectQuery = from KeyValuePair<string, CategoryIdDevice> category in this.gl_catDevLst
                                  where category.Key == catDefItm.Key
                                  select category;
                foreach (var category in selectQuery)
                {
                    try
                    {
                        this.gl_catDevLst[category.Key].CategoryIsSelected = true;
                    }
                    catch
                    {
                    }
                }
            }*/
        }

        public bool LoadInitCategories()
        {
            try
            {
                var categoryQuery = from category in this.gl_xDoc.Root.Element("CategoryRoot").Elements("Category")
                                    select category;
                foreach (var category in categoryQuery)
                {
                    string CategoryId = category.Attribute("Id").Value.ToString();
                    string CategoryName = category.Attribute("CategoryName").Value.ToString();
                    //CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, "");
                    CategoryItm tItm = new CategoryItm(CategoryId, CategoryName, null);
                    //tItm.Header = catDev;
                    //(this.xCatTreeLst.Items[0] as TreeViewItem).Items.Add(tItm);
                    this.gl_container.Items.Add(tItm);
                    this.gl_container.AddCategoryLink(tItm);
                    //this.gl_catDevLst.Add(CategoryId, catDev);

                    if (category.HasElements == true)
                    {
                        LoadAllNodes(tItm, category, CategoryId);
                    }
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        public bool LoadAllNodes(CategoryItm tItm, XContainer xContainer, string parentId = "")
        {
            try
            {
                var categoryQuery = from category in xContainer.Elements("Category")
                                    select category;

                foreach (var category in categoryQuery)
                {
                    string CategoryId = category.Attribute("Id").Value.ToString();
                    string CategoryName = category.Attribute("CategoryName").Value.ToString();
                    //CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, parentId);
                    //catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                    CategoryItm tItm2 = new CategoryItm(CategoryId, CategoryName, tItm);
                    this.gl_container.AddCategoryLink(tItm2);
                    //tItm2.Selected += new RoutedEventHandler(tItm_Selected);
                    //tItm2.Header = catDev;
                    tItm.Items.Add(tItm2);
                    //this.gl_catDevLst.Add(CategoryId, catDev);

                    if (category.HasElements == true)
                    {
                        LoadAllNodes(tItm2, category, CategoryId);
                    }
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        public CategoryContainer GetCopiedContainer()
        {
            return this.gl_container.DuplicateContainer();
        }

        public CategoryContainer GetCategoryContainer()
        {
            return this.gl_container;
        }

        public CategoryListCtrl GetCategoryLstCtrl(XDocument xDoc, Dictionary<string, CategoryDef> catIdLst, List<string> impCatLst = null, bool isEditable = true,
            bool loadEachNode = false, int limit = 0, bool lockOutParentNodeSelection = false)
        {
            try
            {
                CategoryListCtrl catLstCtrl = new CategoryListCtrl(xDoc, catIdLst, impCatLst, isEditable, loadEachNode, limit, lockOutParentNodeSelection);
                catLstCtrl.CategoryAdded += new CategoryAddedEventHandler(catLstCtrl_CategoryAdded);
                catLstCtrl.CategoryRemoved += new CategoryRemovedEventHandler(catLstCtrl_CategoryRemoved);
                catLstCtrl.CategorySelected += new CategoryUnOrSelectedEventHandler(catLstCtrl_CategorySelected);
                catLstCtrl.CategoryDeSelected += new CategoryUnOrSelectedEventHandler(catLstCtrl_CategoryDeSelected);
                catLstCtrl.CategoryEdited += new CategoryEditedEventHandler(catLstCtrl_CategoryEdited);
                return catLstCtrl;
            }
            catch
            {
            }

            return null;
        }

        public CategoryListCtrl GetCategoryLstCtrl(Dictionary<string, CategoryDef> catIdLst, List<string> impCatLst = null, bool isEditable = true, int limit = 0)
        {
            try
            {
                CategoryListCtrl catLstCtrl = new CategoryListCtrl(this.gl_xDoc, catIdLst, impCatLst, isEditable, false, limit);
                catLstCtrl.CategoryAdded += new CategoryAddedEventHandler(catLstCtrl_CategoryAdded);
                catLstCtrl.CategoryRemoved += new CategoryRemovedEventHandler(catLstCtrl_CategoryRemoved);
                catLstCtrl.CategoryEdited += new CategoryEditedEventHandler(catLstCtrl_CategoryEdited);
                return catLstCtrl;
            }
            catch
            {
            }

            return null;
        }

        public CategoryListCtrl GetCategoryLstCtrl(CategoryContainer container, List<string> impCatLst = null, bool isEditable = true, int limit = 0)
        {
            try
            {
                CategoryListCtrl catLstCtrl = new CategoryListCtrl(container, impCatLst, isEditable, false, limit);
                catLstCtrl.CategoryAdded += new CategoryAddedEventHandler(catLstCtrl_CategoryAdded);
                catLstCtrl.CategoryRemoved += new CategoryRemovedEventHandler(catLstCtrl_CategoryRemoved);
                catLstCtrl.CategoryEdited += new CategoryEditedEventHandler(catLstCtrl_CategoryEdited);
                return catLstCtrl;
            }
            catch
            {
            }

            return null;
        }

        public bool AddCategory(string categoryName, string categoryId = "", string parentId = "{ROOT}")
        {
            try
            {
                string catId = null;

                if (categoryId == "")
                {
                    catId = "{" + Guid.NewGuid().ToString() + "}";
                }
                else
                {
                    catId = categoryId;
                }

                if (parentId != "{ROOT}")
                {
                    CategoryItm catItm = null;

#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Parent id is set. Attempting to locate the CategoryItm object. Parent Id: {0}", parentId);
#endif
                    if (this.gl_container.IdLinkList.TryGetValue(parentId, out catItm) == true)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Found the CategoryItm parent id.");
#endif
                        CategoryItm catItm2 = new CategoryItm(catId, categoryName, catItm);
                        catItm.Items.Add(catItm2);
                        this.gl_container.IdLinkList.Add(catItm2.CategoryId, catItm2);
                        return true;
                    }
#if DEBUG
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Did not find the parent id.");
                    }
#endif
                }
                else
                {
                    CategoryItm catItm2 = new CategoryItm(catId, categoryName);
                    this.gl_container.Items.Add(catItm2);
                    this.gl_container.IdLinkList.Add(catItm2.CategoryId, catItm2);
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public bool RefresheBayCategories()
        {
            try
            {
                this.gl_eBayContainer = ContainerBuilders.BuildCategoryFromeBay();
#if DEBUG
                MessageBox.Show("eBay container built.");
#endif
                return true;
            }
            catch
            {
            }
            return false;
        }

        void catLstCtrl_CategoryEdited(object sender, CategoryEditedEventArgs e)
        {
            try
            {
                var categoryQuery = from category in this.gl_xDoc.Root.Element("CategoryRoot").Descendants("Category")
                                    where category.Attribute("Id").Value.ToString() == e.CategoryId
                                    select category;
                foreach (var category in categoryQuery)
                {
                    category.Attribute("CategoryName").Value = e.CategoryName;
                }
            }
            catch
            {
            }
            this.CommitChanges();
        }

        void catLstCtrl_CategoryRemoved(object sender, CategoryDeletedEventArgs e)
        {
            try
            {
                if (e.ParentId != "{ROOT}")
                {
                    try
                    {
                        var lookupQuery = from category in this.gl_xDoc.Descendants("Category")
                                          where category.Attribute("Id").Value == e.ParentId
                                          select category;

                        foreach (var category in lookupQuery)
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("Category was found and removed");
#endif
                            category.Remove();
                            break;
                        }
                    }
#if DEBUG
                    catch(Exception e2)
                    {
                        MessageBox.Show(string.Format("Error: {0}", e2.ToString()));
                    }
#else
                    catch
                    {
                        MessageBox.Show("An error occured while trying to remove your category. It may not have been removed.", "Error");
                    }
#endif
                }
            }
            catch
            {
            }
            this.CommitChanges();
        }

        void catLstCtrl_CategoryAdded(object sender, CategoryAddedEventArgs e)
        {
            try
            {
                if (e.ParentId != "" && e.ParentId != "{ROOT}")
                {
                    var lookupQuery = from category in this.gl_xDoc.Descendants("Category")
                                      where category.Attribute("Id").Value == e.ParentId
                                      select category;

                    foreach (var category in lookupQuery)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Category was found. Category found: {0}", category.Attribute("CategoryName").Value.ToString());
#endif
                        category.Add(new XElement("Category", new XAttribute("Id", e.CategoryId.ToString()), new XAttribute("CategoryName", e.CategoryName.ToString())));
                    }
                }
                else
                {
                    this.gl_xDoc.Root.Element("CategoryRoot").Add(new XElement("Category", new XAttribute("Id", e.CategoryId.ToString()), new XAttribute("CategoryName", e.CategoryName.ToString())));
                }
            }
            catch
            {
            }

            try
            {
                this.CommitChanges();
            }
            catch
            {
            }
        }

        private void catLstCtrl_CategoryDeSelected(CategorySelectedEventArgs e)
        {
            //
        }

        private void catLstCtrl_CategorySelected(CategorySelectedEventArgs e)
        {
            //
        }

        public static List<string> BuildPathIfNotExist(CategoryContainer catContainer, string path, bool appendToFirstFound = true)
        {
            try
            {
                string[] wPath = path.Split(new Char[] { '/' });

                if (wPath.Length < 2)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("The path does not contain a path long enough to travel down.");
#endif
                    return null;
                }

                List<string> tmpList = CategoryMgr.GetCategoryIdList(catContainer, path);
                if (tmpList == null)
                {
                    if (appendToFirstFound == true)
                    {
                        // The path does not exist. Attempt to build it.
                        CategoryItm lItm = null;
                        string cPath = wPath[0];
                        //for (int i = 1; i < wPath.Length; i++)
                        //{
                            cPath += "/" + wPath[1];
                            List<string> tmpL = CategoryMgr.GetCategoryIdList(catContainer, cPath);
                            List<string> cTmpL = null;
                            int offset = 1;
                            while (tmpL != null)
                            {
                                cTmpL = tmpL;
                                offset++;
                                cPath += "/" + wPath[offset];
                                tmpL = CategoryMgr.GetCategoryIdList(catContainer, cPath);
                            }

                            if (cTmpL != null)
                            {
                                string Id = (cTmpL[cTmpL.Count - 1]).ToString();
                                CategoryItm catItm = catContainer.IdLinkList[Id];

                                // Ok, so we know the most bottom level.
                                CategoryItm catItm2 = null;
                                for (int i2 = offset; i2 < wPath.Length; i2++)
                                {
                                    if (catItm2 == null)
                                    {
                                        CategoryItm catItm3 = new CategoryItm("{" + Guid.NewGuid().ToString() + "}", wPath[i2], catItm);
                                        if (!catContainer.IdLinkList.ContainsKey(catItm3.CategoryId))
                                        {
                                            catContainer.IdLinkList.Add(catItm3.CategoryId, catItm3);
                                        }
                                        catItm.Items.Add(catItm3);
                                        catItm2 = catItm3;
                                    }
                                    else
                                    {
                                        CategoryItm catItm3 = new CategoryItm("{" + Guid.NewGuid().ToString() + "}", wPath[i2], catItm2);
                                        if (!catContainer.IdLinkList.ContainsKey(catItm3.CategoryId))
                                        {
                                            catContainer.IdLinkList.Add(catItm3.CategoryId, catItm3);
                                        }
                                        catItm2.Items.Add(catItm3);
                                        catItm2 = catItm3;
                                    }
                                }
                                tmpList = CategoryMgr.GetCategoryIdList(catContainer, path);
                            }
                            else
                            {
                                // Ok, so we know that we can't find a partial list to go by,
                                // lets start by creating a whole new one.

                                CategoryItm pCatItm = null;
                                bool foundRootCat = false;
                                string rootCatId = "";
                                // try one last attempt to see if there is a root category available.
                                var rootQuery = from CategoryItm catItm in catContainer.Items
                                                where catItm.CategoryName == wPath[0]
                                                select catItm;
                                foreach (var category in rootQuery)
                                {
                                    foundRootCat = true;
                                    rootCatId = category.CategoryId;
                                    break;
                                }

                                if (foundRootCat == false)
                                {
                                    pCatItm = new CategoryItm("{" + Guid.NewGuid().ToString() + "}", wPath[0]);
                                    catContainer.Items.Add(pCatItm);
                                }
                                else
                                {
                                    pCatItm = catContainer.IdLinkList[rootCatId];
                                }

                                catContainer.Items.Add(pCatItm);
                                if (!catContainer.IdLinkList.ContainsKey(pCatItm.CategoryId))
                                {
                                    catContainer.IdLinkList.Add(pCatItm.CategoryId, pCatItm);
                                }
                                for (int i2 = 1; i2 < wPath.Length; i2++)
                                {
                                    CategoryItm catItm = new CategoryItm(Guid.NewGuid().ToString(), wPath[i2], pCatItm);
                                    pCatItm.Items.Add(catItm);
                                    if (!catContainer.IdLinkList.ContainsKey(catItm.CategoryId))
                                    {
                                        catContainer.IdLinkList.Add(catItm.CategoryId, catItm);
                                    }
                                    pCatItm = catItm;
                                }

                                return (tmpList = CategoryMgr.GetCategoryIdList(catContainer, path));
                            }
                        return tmpList;
                    }
                    else
                    {
                        CategoryItm pCatItm = new CategoryItm("{" + Guid.NewGuid().ToString() + "}", wPath[0]);
                        catContainer.Items.Add(pCatItm);
                        catContainer.IdLinkList.Add(pCatItm.CategoryId, pCatItm);
                        for (int i = 1; i < wPath.Length; i++)
                        {
                            CategoryItm catItm = new CategoryItm("{" + Guid.NewGuid().ToString() + "}", wPath[i], pCatItm);
                            pCatItm.Items.Add(catItm);
                            if (!catContainer.IdLinkList.ContainsKey(catItm.CategoryId))
                            {
                                catContainer.IdLinkList.Add(catItm.CategoryId, catItm);
                            }
                            pCatItm = catItm;
                        }
                        return (tmpList = CategoryMgr.GetCategoryIdList(catContainer, path));
                    }
                    
                    //catContainer.Items.Add
                }
                else
                {
                    return tmpList;
                }
            }
#if DEBUG
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error while trying to build the path. Error: {0}", e.ToString());
            }
#else
            catch
            {
            }
#endif
            return null;
        }

        public static string BuildCategoryPath(List<CategoryItm> catLst, bool inverse = false)
        {
            if (catLst.Count == 1)
            {
                return catLst[0].CategoryName;
            }

            if (catLst.Count < 2)
            {
                return null;
            }

            try
            {
                string wPath = "";
                if (inverse == false)
                {
                    wPath += catLst[(catLst.Count - 1)].CategoryName;
                    for (int i = (catLst.Count - 2); i >= 0; i--)
                    {
                        wPath += "/" + catLst[i].CategoryName;
                    }
                    return wPath;
                }
                else
                {
                    wPath += catLst[0].CategoryName;
                    for (int i = 1; i < catLst.Count; i++)
                    {
                        wPath += "/" + catLst[i].CategoryName;
                    }
                    return wPath;
                }
            }
            catch
            {
                return null;
            }
        }

        public static List<string> BuildZenCartPathIfNotExist(CategoryContainer catContainer, string path, string Host, string Db, string User,
            string Pwd, bool appendToFirstFound = true)
        {
            MySqlConnection mCon1 = null;

            try
            {
                string conStr = string.Format("server={0};database={1};user={2};password={3}", Host, Db, User, Pwd);
                mCon1 = new MySqlConnection(conStr);
                mCon1.Open();

                string[] wPath = path.Split(new Char[] { '/' });

                if (wPath.Length < 2)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("The path does not contain a path long enough to travel down.");
#endif
                    return null;
                }

                List<string> tmpList = CategoryMgr.GetCategoryIdList(catContainer, path);
                if (tmpList == null)
                {
                    if (appendToFirstFound == true)
                    {
                        // The path does not exist. Attempt to build it.
                        CategoryItm lItm = null;
                        bool c = false;
                        string cPath = wPath[0];
                        //for (int i = 1; i < wPath.Length; i++)
                        //{
                        cPath += "/" + wPath[1];
                        List<string> tmpL = CategoryMgr.GetCategoryIdList(catContainer, cPath);
                        List<string> cTmpL = null;
                        int offset = 1;
                        while (tmpL != null)
                        {
                            cTmpL = tmpL;
                            offset++;
                            cPath += "/" + wPath[offset];
                            tmpL = CategoryMgr.GetCategoryIdList(catContainer, cPath);
                        }

                        if (cTmpL != null)
                        {
                            string Id = (cTmpL[cTmpL.Count - 1]).ToString();
                            CategoryItm catItm = catContainer.IdLinkList[Id];

                            // Ok, so we know the most bottom level.
                            CategoryItm catItm2 = null;
                            for (int i2 = offset; i2 < wPath.Length; i2++)
                            {
                                if (catItm2 == null)
                                {
                                    MySqlCommand mCmd1 = new MySqlCommand(string.Format("INSERT INTO categories(parent_id, categories_status) VALUES('{0}', '1');", catItm.CategoryId), mCon1);
                                    mCmd1.ExecuteNonQuery();
                                    MySqlCommand mCmd2 = new MySqlCommand(string.Format("INSERT INTO categories_description(categories_id, categories_name) VALUES('{0}', '{1}');",
                                        mCmd1.LastInsertedId.ToString(), wPath[i2]), mCon1);
                                    mCmd2.ExecuteNonQuery();

                                    CategoryItm catItm3 = new CategoryItm(mCmd1.LastInsertedId.ToString(), wPath[i2], catItm);
                                    if (!catContainer.IdLinkList.ContainsKey(catItm3.CategoryId))
                                    {
                                        catContainer.IdLinkList.Add(catItm3.CategoryId, catItm3);
                                    }
                                    catItm.Items.Add(catItm3);
                                    catItm2 = catItm3;
                                    mCmd1.Dispose();
                                    mCmd1 = null;
                                    mCmd2.Dispose();
                                    mCmd2 = null;
                                }
                                else
                                {
                                    MySqlCommand mCmd1 = new MySqlCommand(string.Format("INSERT INTO categories(parent_id, categories_status) VALUES('{0}', '1');", catItm2.CategoryId), mCon1);
                                    mCmd1.ExecuteNonQuery();
                                    MySqlCommand mCmd2 = new MySqlCommand(string.Format("INSERT INTO categories_description(categories_id, categories_name) VALUES('{0}', '{1}');",
                                        mCmd1.LastInsertedId.ToString(), wPath[i2]), mCon1);
                                    mCmd2.ExecuteNonQuery();

                                    CategoryItm catItm3 = new CategoryItm(mCmd1.LastInsertedId.ToString(), wPath[i2], catItm2);
                                    if (!catContainer.IdLinkList.ContainsKey(catItm3.CategoryId))
                                    {
                                        catContainer.IdLinkList.Add(catItm3.CategoryId, catItm3);
                                    }
                                    catItm2.Items.Add(catItm3);
                                    catItm2 = catItm3;
                                }
                            }
                            tmpList = CategoryMgr.GetCategoryIdList(catContainer, path);
                        }
                        else
                        {
                            // Ok, so we know that we can't find a partial list to go by,
                            // lets start by creating a whole new one.

                            bool foundRootCat = false;
                            string rootCatId = "";
                            // try one last attempt to see if there is a root category available.
                            var rootQuery = from CategoryItm catItm in catContainer.Items
                                            where catItm.CategoryName == wPath[0]
                                            select catItm;
                            foreach (var category in rootQuery)
                            {
                                foundRootCat = true;
                                rootCatId = category.CategoryId;
                                break;
                            }

                            MySqlCommand mCmd1 = null;
                            if (foundRootCat == false)
                            {
                                mCmd1 = new MySqlCommand("INSERT INTO categories(categories_status) VALUES('1');", mCon1);
                                mCmd1.ExecuteNonQuery();
                                MySqlCommand mCmd2 = new MySqlCommand(string.Format("INSERT INTO categories_description(categories_id, categories_name) VALUES('{0}', '{1}');",
                                    mCmd1.LastInsertedId.ToString(), wPath[0]), mCon1);
                                mCmd2.ExecuteNonQuery();

                                rootCatId = mCmd1.LastInsertedId.ToString();

                                mCmd1.Dispose();
                                mCmd1 = null;
                                mCmd2.Dispose();
                                mCmd2 = null;
                            }
                            else
                            {
                                //
                            }

                            CategoryItm pCatItm = new CategoryItm(rootCatId, wPath[0]);
                            catContainer.Items.Add(pCatItm);
                            if (!catContainer.IdLinkList.ContainsKey(pCatItm.CategoryId))
                            {
                                catContainer.IdLinkList.Add(pCatItm.CategoryId, pCatItm);
                            }
                            for (int i2 = 1; i2 < wPath.Length; i2++)
                            {
                                MySqlCommand mCmd3 = new MySqlCommand(string.Format("INSERT INTO categories(parent_id, categories_status) VALUES('{0}', '1');", pCatItm.CategoryId), mCon1);
                                mCmd3.ExecuteNonQuery();
                                MySqlCommand mCmd4 = new MySqlCommand(string.Format("INSERT INTO categories_description(categories_id, categories_name) VALUES('{0}', '{1}');",
                                    mCmd3.LastInsertedId.ToString(), wPath[i2]), mCon1);
                                mCmd4.ExecuteNonQuery();

                                CategoryItm catItm = new CategoryItm(mCmd3.LastInsertedId.ToString(), wPath[i2], pCatItm);
                                pCatItm.Items.Add(catItm);
                                if (!catContainer.IdLinkList.ContainsKey(catItm.CategoryId))
                                {
                                    catContainer.IdLinkList.Add(catItm.CategoryId, catItm);
                                }
                                mCmd3.Dispose();
                                mCmd3 = null;
                                mCmd4.Dispose();
                                mCmd4 = null;
                                pCatItm = catItm;
                            }

                            if (mCon1 != null)
                            {
                                mCon1.Close();
                                mCon1.Dispose();
                                mCon1 = null;
                            }
                            return (tmpList = CategoryMgr.GetCategoryIdList(catContainer, path));
                        }

                        if (mCon1 != null)
                        {
                            mCon1.Close();
                            mCon1.Dispose();
                            mCon1 = null;
                        }
                        return tmpList;
                    }
                    else
                    {
                        MySqlCommand mCmd1 = new MySqlCommand("INSERT INTO categories(categories_status) VALUES('1');", mCon1);
                        mCmd1.ExecuteNonQuery();
                        MySqlCommand mCmd2 = new MySqlCommand(string.Format("INSERT INTO categories_description(categories_id, categories_name) VALUES('{0}', '{1}');",
                            mCmd1.LastInsertedId.ToString(), wPath[0]), mCon1);
                        mCmd2.ExecuteNonQuery();

                        CategoryItm pCatItm = new CategoryItm(mCmd1.LastInsertedId.ToString(), wPath[0]);
                        catContainer.Items.Add(pCatItm);
                        catContainer.IdLinkList.Add(pCatItm.CategoryId, pCatItm);
                        mCmd1.Dispose();
                        mCmd1 = null;
                        mCmd2.Dispose();
                        mCmd2 = null;
                        for (int i = 1; i < wPath.Length; i++)
                        {
                            MySqlCommand mCmd3 = new MySqlCommand(string.Format("INSERT INTO categories(parent_id, categories_status) VALUES('{0}', '1');", pCatItm.CategoryId), mCon1);
                            mCmd3.ExecuteNonQuery();
                            MySqlCommand mCmd4 = new MySqlCommand(string.Format("INSERT INTO categories_description(categories_id, categories_name) VALUES('{0}', '{1}');",
                                mCmd3.LastInsertedId.ToString(), wPath[i]), mCon1);
                            mCmd4.ExecuteNonQuery();

                            CategoryItm catItm = new CategoryItm(mCmd3.LastInsertedId.ToString(), wPath[i], pCatItm);
                            pCatItm.Items.Add(catItm);
                            if (!catContainer.IdLinkList.ContainsKey(catItm.CategoryId))
                            {
                                catContainer.IdLinkList.Add(catItm.CategoryId, catItm);
                            }
                            mCmd3.Dispose();
                            mCmd3 = null;
                            mCmd4.Dispose();
                            mCmd4 = null;
                            pCatItm = catItm;
                        }

                        if (mCon1 != null)
                        {
                            mCon1.Close();
                            mCon1.Dispose();
                            mCon1 = null;
                        }
                        return (tmpList = CategoryMgr.GetCategoryIdList(catContainer, path));
                    }
                    
                    //catContainer.Items.Add
                }
                else
                {
                    return tmpList;
                }
            }
#if DEBUG
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error while trying to build the path. Error: {0}", e.ToString());
            }
#else
            catch
            {
            }
#endif
            return null;
        }

        public static List<string> GetCategoryIdList(CategoryContainer catContainer, string path)
        {
            List<string> list = new List<string>();
            string[] wPath = path.Split(new Char[] { '/' });
            
            if (wPath.Length < 2)
                return null;

            CategoryContainer container = catContainer;
            bool foundAllPaths = false;
            for (int i = 0; i < container.Items.Count; i++)
            {
                if (container.Items[i].CategoryName == wPath[0])
                {
                    list.Add(container.Items[i].CategoryId);
                    // enter sub routine to find the child items to see if they match.
                    FindNextNodeC(container.Items[i], wPath, 1, list, ref foundAllPaths);
                    
                    if (foundAllPaths == true)
                        break;

                    list.Clear();
                }
            }

            if (foundAllPaths == false)
                return null;

            return list;
        }

        private static void FindNextNodeC(CategoryItm catItm, string[] path, int cLevel, List<string> list, ref bool foundAllPaths)
        {
            for (int i = 0; i < catItm.Items.Count; i++)
            {
                if (catItm.Items[i].CategoryName == path[cLevel])
                {
                    list.Add(catItm.Items[i].CategoryId);
                    if (cLevel == (path.Length - 1))
                    {
                        foundAllPaths = true;
                        break;
                    }

                    FindNextNodeC(catItm.Items[i], path, (cLevel + 1), list, ref foundAllPaths);
                }
            }
        }

        //public 

        public bool DeleteAllCategories()
        {
            try
            {
                this.gl_xDoc.Root.Element("CatergoryRoot").RemoveAll();
                return true;
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Gets the eBay container.
        /// </summary>
        public CategoryContainer eBayContainer
        {
            get { return this.gl_eBayContainer; }
        }

        public bool CommitChanges()
        {
            try
            {
                this.gl_xDoc.Save(this.gl_configPath);
                return true;
            }
            catch
            {
            }
            return false;
        }
    }
}
