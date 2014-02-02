using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using eBay.Service.Util;
using Samples.Helper;

namespace CategoryMgr
{
    public sealed class ContainerBuilders
    {
        private static ApiContext apiContext = null;

        #region ZenCart Container
        public static CategoryContainer BuildCategoryFromZenCart(string Host, string Db, string User, string Password)
        {
            CategoryContainer container = new CategoryContainer();
            string conStr = string.Format("server={0};database={1};user={2};password={3}", Host, Db, User, Password);
            MySqlConnection mCon = null;

            //try
            //{
                mCon = new MySqlConnection(conStr);
                mCon.Open();

                MySqlCommand pCmd1 = new MySqlCommand("SELECT * FROM categories;", mCon);
                MySqlCommand pCmd2 = new MySqlCommand("SELECT * FROM categories_description;", mCon);
                MySqlDataAdapter mysqlAdapter_cat = null;
                MySqlDataAdapter mysqlAdapter_catDesc = null;

                DataSet dsetCatM = new DataSet();
                DataSet dsetCatDescM = new DataSet();

                mysqlAdapter_cat = new MySqlDataAdapter(pCmd1);
                MySqlCommandBuilder cmdBuilder1 = new MySqlCommandBuilder(mysqlAdapter_cat);
                mysqlAdapter_cat.Fill(dsetCatM, "categories");

                mysqlAdapter_catDesc = new MySqlDataAdapter(pCmd2);
                MySqlCommandBuilder cmdBuilder2 = new MySqlCommandBuilder(mysqlAdapter_catDesc);
                mysqlAdapter_catDesc.Fill(dsetCatDescM, "categories_description");

                // Lets get all parent categories so we can build down the tree.
                var parentQuery = from DataRow parentCategory in dsetCatM.Tables["categories"].Rows
                                  where parentCategory.Field<int>("parent_id") == 0
                                  select parentCategory;
                foreach (var parentCategory in parentQuery)
                {
                    string catName = "";
                    // LoadNode(parent category id, container) -> find all cats call -> LoadNode(parent category id, container)
                    var catDesc = from DataRow category in dsetCatDescM.Tables["categories_description"].Rows
                                  where category.Field<int>("categories_id") == parentCategory.Field<int>("categories_id")
                                  select category;
                    foreach (var category in catDesc)
                    {
                        catName = category.Field<string>("categories_name").ToString();
                        break;
                    }
                    string pId = parentCategory.Field<int>("categories_id").ToString();
                    CategoryItm catItm = new CategoryItm(pId, catName);
                    container.IdLinkList.Add(pId, catItm);
                    container.Items.Add(catItm);
                    LoadZenNode(parentCategory.Field<int>("categories_id"), catItm, dsetCatM, dsetCatDescM, container);
                }

                pCmd1.Dispose();
                pCmd1 = null;
                pCmd2.Dispose();
                pCmd2 = null;
                mysqlAdapter_cat.Dispose();
                mysqlAdapter_cat = null;
                mysqlAdapter_catDesc.Dispose();
                mysqlAdapter_catDesc = null;
                dsetCatM.Dispose();
                dsetCatM = null;
                dsetCatDescM.Dispose();
                dsetCatDescM = null;
                mCon.Close();
                mCon.Dispose();
                mCon = null;
                return container;
            //}
            //catch
            //{
             //   return null;
            //}
        }

        private static void LoadZenNode(int parentId, CategoryItm catItm, DataSet dset_CatM, DataSet dset_CatMDesc, CategoryContainer container)
        {
            var categoryQuery = from DataRow category in dset_CatM.Tables["categories"].Rows
                                where category.Field<int>("parent_id") == parentId
                                select category;
            foreach (var category in categoryQuery)
            {
                string catName = "";
                var catDesc = from DataRow category2 in dset_CatMDesc.Tables["categories_description"].Rows
                              where category2.Field<int>("categories_id") == category.Field<int>("categories_id")
                              select category2;
                foreach (var category2 in catDesc)
                {
                    catName = category2.Field<string>("categories_name").ToString();
                    break;
                }
                string cId = category.Field<int>("categories_id").ToString();
                CategoryItm catItm2 = new CategoryItm(cId, catName, catItm);
                if (!container.IdLinkList.ContainsKey(cId))
                {
                    container.IdLinkList.Add(cId, catItm2);
                }
                catItm.Items.Add(catItm2);
                LoadZenNode(int.Parse(catItm2.CategoryId), catItm2, dset_CatM, dset_CatMDesc, container);
            }
        }
        #endregion

        #region eBay Container
        public static CategoryContainer BuildCategoryFromeBaySource(string configPath = @".\eBayCategoriesConfig.xml")
        {
            try
            {
                CategoryContainer container = new CategoryContainer();
                XDocument xDoc = XDocument.Load(configPath);
                return container;
            }
            catch
            {
            }
            return null;
        }

        public static CategoryContainer BuildCategoryFromeBay(bool storeCatBuild = false)
        {
            if (storeCatBuild == false)
            {
                CategoryContainer container = new CategoryContainer();

                apiContext = AppSettingHelper.GetApiContext();
                apiContext.ApiLogManager = new ApiLogManager();
                apiContext.ApiLogManager.ApiLoggerList.Add(new FileLogger("log.txt", true, true, true));
                apiContext.ApiLogManager.EnableLogging = true;
                apiContext.Site = SiteCodeType.US;

                GetCategoriesCall catCall = new GetCategoriesCall(apiContext)
                {
                    EnableCompression = true,
                    ViewAllNodes = true
                };

                catCall.DetailLevelList.Add(DetailLevelCodeType.ReturnAll);
                catCall.GetCategories();

                Dictionary<string, CategoryDef> catIdCMLst = new Dictionary<string, CategoryDef>();

                foreach (CategoryType category in catCall.CategoryList)
                {
                    int categoryId = Int32.Parse(category.CategoryID);
                    int? parentId = Int32.Parse(category.CategoryParentID[0]);
                    if (parentId == categoryId) { parentId = null; }

                    if (parentId == null) { catIdCMLst.Add(category.CategoryID, new CategoryDef(category.CategoryName, category.CategoryID)); }
                    else { catIdCMLst.Add(category.CategoryID, new CategoryDef(category.CategoryName, category.CategoryID, parentId.ToString())); }
                }

                // Lets get all parent categories so we can build down the tree.
                var parentQuery = from parentCategory in catIdCMLst
                                  where parentCategory.Value.ParentId == ""
                                  select parentCategory;
                foreach (var parentCategory in parentQuery)
                {
                    string catName = parentCategory.Value.CategoryName;
                    // LoadNode(parent category id, container) -> find all cats call -> LoadNode(parent category id, container)
                    string pId = parentCategory.Key;
                    CategoryItm catItm = new CategoryItm(pId, catName);
                    container.IdLinkList.Add(pId, catItm);
                    container.Items.Add(catItm);
                    LoadeBayNode(pId, catItm, catIdCMLst, container);
                }

                return container;
            }
            else
            {
                CategoryContainer container = new CategoryContainer();
                apiContext = AppSettingHelper.GetApiContext();
                apiContext.ApiLogManager = new ApiLogManager();
                apiContext.ApiLogManager.ApiLoggerList.Add(new FileLogger("log.txt", true, true, true));
                apiContext.ApiLogManager.EnableLogging = true;
                apiContext.Site = SiteCodeType.US;

                GetStoreCall getStoreCall = new GetStoreCall(apiContext) { EnableCompression = true };
                getStoreCall.DetailLevelList.Add(DetailLevelCodeType.ReturnAll);
                getStoreCall.CategoryStructureOnly = true;
                getStoreCall.Execute();

                StoreCustomCategoryType cType = new StoreCustomCategoryType();
                Dictionary<string, CategoryDef> catIdCMLst = new Dictionary<string, CategoryDef>();

                foreach (StoreCustomCategoryType category in getStoreCall.Store.CustomCategories)
                {
                    CategoryItm catItm = new CategoryItm(category.CategoryID.ToString(), category.Name);
                    container.IdLinkList.Add(category.CategoryID.ToString(), catItm);
                    container.Items.Add(catItm);
                    GeteBayChildCategoriesRoot(category, catItm, (int)category.CategoryID, catIdCMLst, container);
                    //container.Items.Add(catItm);
                }

                // Lets get all parent categories so we can build down the tree.
                /*var parentQuery = from parentCategory in catIdCMLst
                                  select parentCategory;
                foreach (var parentCategory in parentQuery)
                {
                    string catName = parentCategory.Value.CategoryName;
                    // LoadNode(parent category id, container) -> find all cats call -> LoadNode(parent category id, container)
                    string pId = parentCategory.Key;
                    CategoryItm catItm = new CategoryItm(pId, catName);
                    container.IdLinkList.Add(pId, catItm);
                    container.Items.Add(catItm);
                    LoadeBayNode(pId, catItm, catIdCMLst, container);
                }*/

                return container;
            }
            //}
            //catch
            //{
            //   return null;
            //}
        }

        private static void GeteBayChildCategoriesRoot(StoreCustomCategoryType cat, CategoryItm catItm, int pId, Dictionary<string, CategoryDef> catDefIdLst, CategoryContainer container)
        {
            //continue the recursion for each of the child categories
            foreach (StoreCustomCategoryType childcat in cat.ChildCategory)
            {
                //get the category name, ID and whether it is a leaf
                int parentId = pId;
                long id = cat.CategoryID;
                string name = cat.Name;
                bool leaf = (cat.ChildCategory.Count == 0);
                Console.WriteLine("id = " + id + " name = " + name + " leaf= " + leaf);
                CategoryDef catDef = new CategoryDef(childcat.Name, childcat.CategoryID.ToString(), cat.CategoryID.ToString());
                try
                {
                    catDefIdLst.Add(childcat.CategoryID.ToString(), catDef);
                }
                catch
                {
                }

                string catName = name;
                // LoadNode(parent category id, container) -> find all cats call -> LoadNode(parent category id, container)
                //string pId = parentCategory.Key;
                CategoryItm catItm2 = new CategoryItm(childcat.CategoryID.ToString(), childcat.Name, catItm);
                if (!container.IdLinkList.ContainsKey(childcat.CategoryID.ToString()))
                {
                    container.IdLinkList.Add(childcat.CategoryID.ToString(), catItm2);
                }
                catItm.Items.Add(catItm2);

                GeteBayChildCategoriesRoot(childcat, catItm2, (int)id, catDefIdLst, container);
            }
        }

        private static void GeteBayChildCategories(StoreCustomCategoryType cat, CategoryItm catItm, int pId, Dictionary<string, CategoryDef> catDefIdLst, CategoryContainer container)
        {
            //get the category name, ID and whether it is a leaf
            int parentId = pId;
            long id = cat.CategoryID;
            string name = cat.Name;
            bool leaf = (cat.ChildCategory.Count == 0);
            Console.WriteLine("id = " + id + " name = " + name + " leaf= " + leaf);
            CategoryDef catDef = new CategoryDef(cat.Name, id.ToString(), pId.ToString());
            catDefIdLst.Add(id.ToString(), catDef);

            string catName = name;
            // LoadNode(parent category id, container) -> find all cats call -> LoadNode(parent category id, container)
            //string pId = parentCategory.Key;
            CategoryItm catItm2 = new CategoryItm(id.ToString(), catName, catItm);
            if (!container.IdLinkList.ContainsKey(id.ToString()))
            {
                container.IdLinkList.Add(id.ToString(), catItm2);
            }
            catItm.Items.Add(catItm2);

            //condition to end the recursion
            if (leaf)
            {
                return;
            }

            //continue the recursion for each of the child categories
            foreach (StoreCustomCategoryType childcat in cat.ChildCategory)
            {
                GeteBayChildCategories(childcat, catItm2, (int)id, catDefIdLst, container);
            }
        }

        private static void LoadeBayStoreNode(string parentId, CategoryItm catItm, Dictionary<string, CategoryDef> catIdLst, CategoryContainer container)
        {
            var categoryQuery = from category in catIdLst
                                where category.Value.ParentId == parentId
                                select category;
            foreach (var category in categoryQuery)
            {
                string catName = category.Value.CategoryName;
                string cId = category.Value.CategoryId;
                CategoryItm catItm2 = new CategoryItm(cId, catName, catItm);
                if (!container.IdLinkList.ContainsKey(cId))
                {
                    container.IdLinkList.Add(cId, catItm2);
                }
                catItm.Items.Add(catItm2);
                LoadeBayStoreNode(catItm2.CategoryId, catItm2, catIdLst, container);
            }
        }

        private static void LoadeBayNode(string parentId, CategoryItm catItm, Dictionary<string, CategoryDef> catIdLst, CategoryContainer container)
        {
            var categoryQuery = from category in catIdLst
                                where category.Value.ParentId == parentId
                                select category;
            foreach (var category in categoryQuery)
            {
                string catName = category.Value.CategoryName;
                string cId = category.Value.CategoryId;
                CategoryItm catItm2 = new CategoryItm(cId, catName, catItm);
                if (!container.IdLinkList.ContainsKey(cId))
                {
                    container.IdLinkList.Add(cId, catItm2);
                }
                catItm.Items.Add(catItm2);
                LoadeBayNode(catItm2.CategoryId, catItm2, catIdLst, container);
            }
        }
        #endregion

        #region OS Commerce Container
        public static CategoryContainer BuildCategoryFromOSCommerce(string Host, string Db, string User, string Password)
        {
            CategoryContainer container = new CategoryContainer();
            string conStr = string.Format("server={0};database={1};user={2};password={3}", Host, Db, User, Password);
            MySqlConnection mCon = null;

            //try
            //{
            mCon = new MySqlConnection(conStr);
            mCon.Open();

            MySqlCommand pCmd1 = new MySqlCommand("SELECT * FROM categories;", mCon);
            MySqlCommand pCmd2 = new MySqlCommand("SELECT * FROM categories_description;", mCon);
            MySqlDataAdapter mysqlAdapter_cat = null;
            MySqlDataAdapter mysqlAdapter_catDesc = null;

            DataSet dsetCatM = new DataSet();
            DataSet dsetCatDescM = new DataSet();

            mysqlAdapter_cat = new MySqlDataAdapter(pCmd1);
            MySqlCommandBuilder cmdBuilder1 = new MySqlCommandBuilder(mysqlAdapter_cat);
            mysqlAdapter_cat.Fill(dsetCatM, "categories");

            mysqlAdapter_catDesc = new MySqlDataAdapter(pCmd2);
            MySqlCommandBuilder cmdBuilder2 = new MySqlCommandBuilder(mysqlAdapter_catDesc);
            mysqlAdapter_catDesc.Fill(dsetCatDescM, "categories_description");

            // Lets get all parent categories so we can build down the tree.
            var parentQuery = from DataRow parentCategory in dsetCatM.Tables["categories"].Rows
                              where parentCategory.Field<int>("parent_id") == 0
                              select parentCategory;
            foreach (var parentCategory in parentQuery)
            {
                string catName = "";
                // LoadNode(parent category id, container) -> find all cats call -> LoadNode(parent category id, container)
                var catDesc = from DataRow category in dsetCatDescM.Tables["categories_description"].Rows
                              where category.Field<int>("categories_id") == parentCategory.Field<int>("categories_id")
                              select category;
                foreach (var category in catDesc)
                {
                    catName = category.Field<string>("categories_name").ToString();
                    break;
                }
                string pId = parentCategory.Field<int>("categories_id").ToString();
                CategoryItm catItm = new CategoryItm(pId, catName);
                container.IdLinkList.Add(pId, catItm);
                container.Items.Add(catItm);
                LoadZenNode(parentCategory.Field<int>("categories_id"), catItm, dsetCatM, dsetCatDescM, container);
            }

            pCmd1.Dispose();
            pCmd1 = null;
            pCmd2.Dispose();
            pCmd2 = null;
            mysqlAdapter_cat.Dispose();
            mysqlAdapter_cat = null;
            mysqlAdapter_catDesc.Dispose();
            mysqlAdapter_catDesc = null;
            dsetCatM.Dispose();
            dsetCatM = null;
            dsetCatDescM.Dispose();
            dsetCatDescM = null;
            mCon.Close();
            mCon.Dispose();
            mCon = null;
            return container;
            //}
            //catch
            //{
            //   return null;
            //}
        }

        #endregion

        #region ContainerBuilder Properties
        #endregion
    }
}
