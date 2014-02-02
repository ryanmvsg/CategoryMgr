using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Linq;

namespace CategoryMgr
{
    /// <summary>
    /// Interaction logic for CategoryListCtrl.xaml
    /// </summary>
    public partial class CategoryListCtrl : UserControl
    {
        private XDocument gl_xDoc = null;
        private Dictionary<string, CategoryDef> gl_catDefLst = null;
        private Dictionary<string, CategoryIdDevice> gl_catDevLst = null;
        private TreeViewItem gl_cSelectedItm = null;
        private CategoryContainer gl_catContainer = null;
        private bool gl_isEditable = true;
        private bool gl_loadEachNode = false;
        private bool gl_lockOutParentNodes = false;
        private static RoutedCommand gl_AddCmd = new RoutedCommand();
        private static RoutedCommand gl_delCmd = new RoutedCommand();
        private static RoutedCommand gl_editCmd = new RoutedCommand();
        private static RoutedCommand gl_expandAllCmd = new RoutedCommand();
        private int gl_lockCnt = 0;
        private List<string> gl_catImpLst = null;

        public CategoryListCtrl(XDocument xDoc, Dictionary<string, CategoryDef> catDef, List<string> catImpLst = null, bool isEditable = true,
            bool loadEachNode = false, int limit = 0, bool lockOutParentSelectionNodes = false)
        {
            InitializeComponent();

            this.gl_isEditable = isEditable;
            this.gl_loadEachNode = loadEachNode;
            this.gl_lockCnt = limit;
            this.gl_lockOutParentNodes = lockOutParentSelectionNodes;
            this.CategoryDeSelected += new CategoryUnOrSelectedEventHandler(CategoryListCtrl_CategoryDeSelected);
            this.CategorySelected += new CategoryUnOrSelectedEventHandler(CategoryListCtrl_CategorySelected);
            if (catImpLst != null)
            {
                this.gl_catImpLst = catImpLst;
            }
            else
            {
                this.gl_catImpLst = new List<string>();
            }
            LoadSettingsFromXDoc(xDoc, catDef, catImpLst);
        }

        public CategoryListCtrl(string xmlPath, Dictionary<string, CategoryDef> catDef, List<string> catImpLst = null, bool isEditable = true,
            bool loadEachNode = false, int limit = 0, bool lockOutParentSelectionNodes = false)
        {
            InitializeComponent();

            XDocument xDoc = null;
            try
            {
                xDoc = XDocument.Load(xmlPath);
            }
            catch
            {
            }
            this.gl_isEditable = isEditable;
            this.gl_loadEachNode = loadEachNode;
            this.gl_lockCnt = limit;
            this.gl_lockOutParentNodes = lockOutParentSelectionNodes;
            this.CategoryDeSelected += new CategoryUnOrSelectedEventHandler(CategoryListCtrl_CategoryDeSelected);
            this.CategorySelected += new CategoryUnOrSelectedEventHandler(CategoryListCtrl_CategorySelected);
            if (catImpLst != null)
            {
                this.gl_catImpLst = catImpLst;
            }
            else
            {
                this.gl_catImpLst = new List<string>();
            }
            LoadSettingsFromXDoc(xDoc, catDef, catImpLst);
        }

        public CategoryListCtrl(CategoryContainer container, List<string> catImpLst = null, bool isEditable = true, bool loadEachNode = false, int limit = 0, bool lockOutParentSelectionNodes = false)
        {
            InitializeComponent();

            this.gl_isEditable = isEditable;
            this.gl_loadEachNode = loadEachNode;
            this.gl_lockCnt = limit;
            this.gl_lockOutParentNodes = lockOutParentSelectionNodes;
            this.CategoryDeSelected += new CategoryUnOrSelectedEventHandler(CategoryListCtrl_CategoryDeSelected);
            this.CategorySelected += new CategoryUnOrSelectedEventHandler(CategoryListCtrl_CategorySelected);
            if (catImpLst != null)
            {
                this.gl_catImpLst = catImpLst;
            }
            else
            {
                this.gl_catImpLst = new List<string>();
            }
            LoadSettingsFromContainer(container, catImpLst);
        }

        private void LoadSettingsFromXDoc(XDocument xDoc, Dictionary<string, CategoryDef> catDef, List<string> catImpLst = null)
        {
            this.gl_catDefLst = catDef;
            this.gl_catDevLst = new Dictionary<string, CategoryIdDevice>();
            this.gl_xDoc = xDoc;
            string CategoryId = "{ROOT}";
            string CategoryName = "Site Root";
            CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName);
            catDev.xIsEnabledChk.IsEnabled = false;
            catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
            TreeViewItem tItm = new TreeViewItem();
            tItm.Selected += new RoutedEventHandler(tItm_Selected);
            tItm.Header = catDev;
            this.xCatTreeLst.Items.Add(tItm);
            this.gl_catDevLst.Add(CategoryId, catDev);

            CommandBinding cb = new CommandBinding(gl_AddCmd, gl_addCmdExecuted, gl_addCmdCanExecute);
            this.CommandBindings.Add(cb);

            CommandBinding cb2 = new CommandBinding(gl_delCmd, gl_delCmdExecuted, gl_delCmdCanExecute);
            this.CommandBindings.Add(cb2);

            CommandBinding cb3 = new CommandBinding(gl_editCmd, gl_editCmd_Executed, gl_editCmd_CanExecute);
            this.CommandBindings.Add(cb3);

            CommandBinding cb4 = new CommandBinding(gl_expandAllCmd, this.ExpandAllBtnCmd_Executed, this.ExpandAllBtnCmd_CanExecute);
            this.CommandBindings.Add(cb4);

            this.xAddCatBtn.Command = gl_AddCmd;
            this.xDeleteCatBtn.Command = gl_delCmd;
            this.xEditCatBtn.Command = gl_editCmd;
            this.xExpandAllBtn.Command = gl_expandAllCmd;
            this.xRecentBtn.Click += new RoutedEventHandler(xRecentBtn_Click);
            this.CategoryAdded += new CategoryAddedEventHandler(CategoryListCtrl_CategoryAdded);
            this.CategoryRemoved += new CategoryRemovedEventHandler(CategoryListCtrl_CategoryRemoved);
            this.CategoryEdited += new CategoryEditedEventHandler(CategoryListCtrl_CategoryEdited);

            LoadInitCategories();

            if (catImpLst != null)
            {
                foreach (string catDefItm in catImpLst)
                {
                    var selectQuery = from KeyValuePair<string, CategoryIdDevice> category in this.gl_catDevLst
                                      where category.Key == catDefItm
                                      select category;
                    foreach (var category in selectQuery)
                    {
                        try
                        {
                            this.gl_catDevLst[category.Key].CategoryIsSelected = true;
                        }
#if DEBUG
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("catImpLst Selection Error", ex.ToString());
                        }
#else
                        catch
                        {
                        }
#endif
                    }
                }
            }

            this.xAddCatBtn.Click += new RoutedEventHandler(xAddCatBtn_Click);
        }

        void CategoryListCtrl_CategorySelected(CategorySelectedEventArgs e)
        {
            //
        }

        void CategoryListCtrl_CategoryDeSelected(CategorySelectedEventArgs e)
        {
            //
        }

        void xRecentBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                recentCatWin rWin = new recentCatWin("eBay");
                rWin.ShowDialog();
                List<string> catLst = rWin.CategoryList;
                for (int i = 0; i < catLst.Count; i++)
                {
                    if (this.gl_catDevLst.ContainsKey(catLst[i]))
                    {
                        this.gl_catDevLst[catLst[i]].xIsEnabledChk.IsChecked = true;
                    }
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("xRecentBtn_Click Error: {0}", ex.ToString());
            }
#else
            catch
            {
            }
#endif
        }

        private void LoadSettingsFromContainer(CategoryContainer container, List<string> catImpLst = null)
        {
            this.gl_catDefLst = new Dictionary<string, CategoryDef>();
            this.gl_catDevLst = new Dictionary<string, CategoryIdDevice>();
            this.gl_xDoc = null;
            this.gl_catContainer = container;
            string CategoryId = "{ROOT}";
            string CategoryName = "Site Root";
            CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName);
            catDev.xIsEnabledChk.IsEnabled = false;
            catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
            TreeViewItem tItm = new TreeViewItem();
            tItm.Selected += new RoutedEventHandler(tItm_Selected);
            tItm.Header = catDev;
            this.xCatTreeLst.Items.Add(tItm);
            this.gl_catDevLst.Add(CategoryId, catDev);

            CommandBinding cb = new CommandBinding(gl_AddCmd, gl_addCmdExecuted, gl_addCmdCanExecute);
            this.CommandBindings.Add(cb);

            CommandBinding cb2 = new CommandBinding(gl_delCmd, gl_delCmdExecuted, gl_delCmdCanExecute);
            this.CommandBindings.Add(cb2);

            CommandBinding cb3 = new CommandBinding(gl_editCmd, gl_editCmd_Executed, gl_editCmd_CanExecute);
            this.CommandBindings.Add(cb3);

            this.xAddCatBtn.Command = gl_AddCmd;
            this.xDeleteCatBtn.Command = gl_delCmd;
            this.xEditCatBtn.Command = gl_editCmd;
            this.CategoryAdded += new CategoryAddedEventHandler(CategoryListCtrl_CategoryAdded);
            this.CategoryRemoved += new CategoryRemovedEventHandler(CategoryListCtrl_CategoryRemoved);
            this.CategoryEdited += new CategoryEditedEventHandler(CategoryListCtrl_CategoryEdited);

            LoadInitCategoriesFromSource();

            if (catImpLst != null)
            {
                foreach (string catDefItm in catImpLst)
                {
                    var selectQuery = from KeyValuePair<string, CategoryIdDevice> category in this.gl_catDevLst
                                      where category.Key == catDefItm
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
                }
            }

            this.xAddCatBtn.Click += new RoutedEventHandler(xAddCatBtn_Click);
        }

        void CategoryListCtrl_CategoryEdited(object sender, CategoryEditedEventArgs e)
        {
            //
        }

        void CategoryListCtrl_CategoryRemoved(object sender, CategoryDeletedEventArgs e)
        {
            //
        }

        void CategoryListCtrl_CategoryAdded(object sender, CategoryAddedEventArgs e)
        {
            //
        }

        private void ExpandAllBtnCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.gl_cSelectedItm == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }


        private void ExpandAllBtnCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.gl_loadEachNode == true)
            {
                try
                {
                    CategoryIdDevice catIdDev = (this.gl_cSelectedItm.Header as CategoryIdDevice);
                    var categoryQuery = from category in this.gl_xDoc.Root.Descendants("Category")
                                        where category.Attribute("Id").Value == catIdDev.CategoryId
                                        select category;
                    foreach (var pCategory in categoryQuery)
                    {
                        var childQuery = from category in pCategory.Elements("Category")
                                         select category;
                        foreach (var category in childQuery)
                        {
                            try
                            {
                                this.gl_cSelectedItm.IsExpanded = true;
                                string CategoryId = category.Attribute("Id").Value.ToString();
                                string CategoryName = category.Attribute("CategoryName").Value.ToString();
                                if (!this.gl_catDevLst.ContainsKey(CategoryId))
                                {
                                    CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, "");
                                    catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                                    TreeViewItem tItm = new TreeViewItem();
                                    tItm.Selected += new RoutedEventHandler(tItm_Selected);
                                    tItm.Header = catDev;
                                    this.gl_cSelectedItm.Items.Add(tItm);
                                    this.gl_catDevLst.Add(CategoryId, catDev);

                                    if (category.HasElements == true)
                                    {
                                        LoadAllNodes(tItm, category, CategoryId, true);
                                    }
                                }
                                else
                                {
                                    (this.gl_catDevLst[CategoryId].Parent as TreeViewItem).IsExpanded = true;
                                }
                            }

#if DEBUG
                            catch(Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("ExpandAllBtnCmd_Executed Error: {0}", ex.ToString());
#else
                            catch
                            {
#endif
                            
                            }
                        }
                    }
                }
#if DEBUG
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("ExpandAllBtnCmd_Executed Error", ex.ToString());
#else
            catch
            {
#endif
                }
            }
        }

        private void gl_editCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.gl_cSelectedItm == null || this.gl_isEditable == false)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void gl_editCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.gl_cSelectedItm != null)
            {
                try
                {
                    CategoryIdDevice catDev = (this.gl_cSelectedItm.Header as CategoryIdDevice);
                    EditCategoryResponse editC = EditCategoryResponse.ShowDialog(catDev.CategoryId);
                    catDev.CategoryName = editC.CategoryName;
                    this.CategoryEdited(this, new CategoryEditedEventArgs(catDev.CategoryId, editC.CategoryName));
                }
                catch
                {
                }
            }
        }

        private void gl_delCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.gl_cSelectedItm == null || this.gl_isEditable == false)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void gl_delCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.gl_cSelectedItm != null)
            {
                CategoryIdDevice catDev = (this.gl_cSelectedItm.Header as CategoryIdDevice);
                if (catDev.CategoryId == "{ROOT}")
                {
                    MessageBox.Show("You may not remove the root of your site. Please select a different category to remove.", "Error", MessageBoxButton.OK);
                    return;
                }

                try
                {
                    if (catDev.ParentId != "{ROOT}")
                    {
                        string CategoryId = catDev.CategoryId;
                        string pId = catDev.ParentId;
                        TreeViewItem pItm = (this.gl_cSelectedItm.Parent as TreeViewItem);
                        pItm.Items.Remove(this.gl_cSelectedItm);
                        this.CategoryRemoved(this, new CategoryDeletedEventArgs(CategoryId, pId));
                    }
                }
                catch
                {
                }
            }
        }

        private void gl_addCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.gl_cSelectedItm == null || this.gl_isEditable == false)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void gl_addCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.gl_cSelectedItm != null)
            {
                CategoryIdDevice catId = (this.gl_cSelectedItm.Header as CategoryIdDevice);
                AddCategoryResponse addC = AddCategoryResponse.ShowDialog(catId.CategoryName, catId.CategoryId);
                string CategoryId = "{" + Guid.NewGuid().ToString() + "}";
                string CategoryName = addC.CategoryName;
                CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, catId.CategoryId);
                catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                TreeViewItem tItm = new TreeViewItem();
                if (this.gl_loadEachNode == true)
                {
                    tItm.Expanded += new RoutedEventHandler(tItm_Expanded);
                }
                tItm.Selected += new RoutedEventHandler(tItm_Selected);
                tItm.Header = catDev;
                this.gl_cSelectedItm.Items.Add(tItm);
                this.gl_catDevLst.Add(CategoryId, catDev);
                this.CategoryAdded(this, new CategoryAddedEventArgs(catDev));
            }
        }

        void tItm_Expanded(object sender, RoutedEventArgs e)
        {
            if (this.gl_xDoc != null)
            {
                if (sender is TreeViewItem)
                {
                    TreeViewItem ptItm = (sender as TreeViewItem);
                    if (ptItm.Items.Count == 1)
                    {
                        if ((ptItm.Items[0] as TreeViewItem).Header.ToString() == "Expand to see list")
                        {
                            ptItm.Items.Clear();
                            CategoryIdDevice catIdDev = (CategoryIdDevice)(sender as TreeViewItem).Header;
                            string catId = catIdDev.CategoryId;
                            string pCatId = catIdDev.ParentId;
                            //XElement xelem = this.gl_xDoc.XPathSelectElement(string.Format("Categories/CategoryRoot/Category[@Id='{0}']", catId));
                            //if (xelem != null)
                            //{
                            var parentQuery = from category in this.gl_xDoc.Root.Descendants("Category")
                                              where category.Attribute("Id").Value == catId
                                              select category;
                                foreach (var category in parentQuery)
                                {
                                    var childNodeQuery = from category2 in category.Elements("Category")
                                                         select category2;
                                    foreach (var childCategory in childNodeQuery)
                                    {
                                        string newCatId = childCategory.Attribute("Id").Value;
                                        string newCatName = childCategory.Attribute("CategoryName").Value;
                                        // Loop through each category in the parent id list.
                                        CategoryIdDevice catDev = new CategoryIdDevice(newCatId, newCatName, false, catId);
                                        //string CategoryId = "{" + Guid.NewGuid().ToString() + "}";
                                        //string CategoryName = addC.CategoryName;
                                        //CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, catId.CategoryId);
                                        catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                                        TreeViewItem tItm = new TreeViewItem();
                                        tItm.Expanded += new RoutedEventHandler(tItm_Expanded);
                                        tItm.Selected += new RoutedEventHandler(tItm_Selected);
                                        tItm.Header = catDev;
                                        TreeViewItem tItm2 = new TreeViewItem();
                                        tItm2.Header = "Expand to see list";
                                        tItm.Items.Add(tItm2);
                                        ptItm.Items.Add(tItm);
                                        //this.gl_cSelectedItm.Items.Add(tItm);
                                        if (!this.gl_catDevLst.ContainsKey(newCatId))
                                        {
                                            this.gl_catDevLst.Add(newCatId, catDev);
                                            try
                                            {
                                                if (this.gl_catImpLst.Contains(newCatId))
                                                {
                                                    try
                                                    {
                                                        this.gl_catDevLst[newCatId].CategoryIsSelected = true;
                                                    }
#if DEBUG
                                                    catch (Exception ex2)
                                                    {
                                                        System.Diagnostics.Debug.WriteLine("Category Dev List Selected Error", ex2.ToString());
                                                    }
#else
                                                    catch
                                                    {
                                                    }
#endif
                                                }
                                            }
#if DEBUG
                                            catch (Exception ex)
                                            {
                                                System.Diagnostics.Debug.WriteLine("tItm_Expanded/Selected Error: {0}", ex.ToString());
                                            }
#else
                                            catch
                                            {
                                            }
#endif
                                        }
                                        else
                                        {
#if DEBUG
                                            System.Diagnostics.Debug.WriteLine("Id already exists.");
#endif
                                        }
                                        this.CategoryAdded(this, new CategoryAddedEventArgs(catDev));
                                    }
                                }
                            //}
                        }
                    }
                }
            }
            else if (this.gl_catContainer != null)
            {
            }
            else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Error, container and xDoc are null.");
#endif
            }
        }

        void xAddCatBtn_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        public bool LoadInitCategories()
        {
            if (this.gl_loadEachNode == false)
            {
                try
                {
                    var categoryQuery = from category in this.gl_xDoc.Root.Element("CategoryRoot").Elements("Category")
                                        select category;
                    foreach (var category in categoryQuery)
                    {
                        string CategoryId = category.Attribute("Id").Value.ToString();
                        string CategoryName = category.Attribute("CategoryName").Value.ToString();
                        CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, "");
                        catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                        TreeViewItem tItm = new TreeViewItem();
                        tItm.Selected += new RoutedEventHandler(tItm_Selected);
                        tItm.Header = catDev;
                        (this.xCatTreeLst.Items[0] as TreeViewItem).Items.Add(tItm);
                        this.gl_catDevLst.Add(CategoryId, catDev);

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
            }
            else
            {
                try
                {
                    var categoryQuery = from category in this.gl_xDoc.Root.Element("CategoryRoot").Elements("Category")
                                        select category;
                    foreach (var category in categoryQuery)
                    {
                        string CategoryId = category.Attribute("Id").Value.ToString();
                        string CategoryName = category.Attribute("CategoryName").Value.ToString();
                        CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, "");
                        catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                        TreeViewItem tItm = new TreeViewItem();
                        tItm.Selected += new RoutedEventHandler(tItm_Selected);
                        tItm.Expanded += new RoutedEventHandler(tItm_Expanded);
                        tItm.Header = catDev;
                        (this.xCatTreeLst.Items[0] as TreeViewItem).Items.Add(tItm);
                        this.gl_catDevLst.Add(CategoryId, catDev);

                        TreeViewItem tItm2 = new TreeViewItem();
                        tItm2.Header = "Expand to see list";
                        tItm.Items.Add(tItm2);
                    }
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        public bool LoadInitCategoriesFromSource()
        {
            try
            {
                var categoryQuery = from category in this.gl_catContainer.Items
                                    select category;
                foreach (var category in categoryQuery)
                {
                    string CategoryId = category.CategoryId;
                    string CategoryName = category.CategoryName;
                    CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, "");
                    catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                    TreeViewItem tItm = new TreeViewItem();
                    tItm.Selected += new RoutedEventHandler(tItm_Selected);
                    tItm.Header = catDev;
                    (this.xCatTreeLst.Items[0] as TreeViewItem).Items.Add(tItm);
                    this.gl_catDevLst.Add(CategoryId, catDev);

                    if (category.Items.Count > 0)
                    {
                        LoadAllSourceNodes(tItm, category, CategoryId);
                    }
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        void tItm_Selected(object sender, RoutedEventArgs e)
        {
            if(sender is TreeViewItem)
            { /*this.gl_cSelectedItm = (sender as TreeViewItem);*/ this.gl_cSelectedItm = (e.OriginalSource as TreeViewItem); e.Handled = true; }

            //
        }

        public bool LoadAllSourceNodes(TreeViewItem tItm, CategoryItm xContainer, string parentId = "")
        {
            try
            {
                var categoryQuery = from category in xContainer.Items
                                    select category;

                foreach (var category in categoryQuery)
                {
                    string CategoryId = category.CategoryId;
                    string CategoryName = category.CategoryName;
                    CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, parentId);
                    catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                    TreeViewItem tItm2 = new TreeViewItem();
                    tItm2.Selected += new RoutedEventHandler(tItm_Selected);
                    tItm2.Header = catDev;
                    tItm.Items.Add(tItm2);
                    this.gl_catDevLst.Add(CategoryId, catDev);

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

        public bool LoadAllNodes(TreeViewItem tItm, XContainer xContainer, string parentId = "", bool autoExpand = false)
        {
            try
            {
                var categoryQuery = from category in xContainer.Elements("Category")
                                    select category;

                foreach (var category in categoryQuery)
                {
                    string CategoryId = category.Attribute("Id").Value.ToString();
                    string CategoryName = category.Attribute("CategoryName").Value.ToString();
                    CategoryIdDevice catDev = new CategoryIdDevice(CategoryId, CategoryName, false, parentId);
                    catDev.CategorySelected += new CategoryUnOrSelectedEventHandler(catDev_CategorySelected);
                    TreeViewItem tItm2 = new TreeViewItem();
                    tItm2.Selected += new RoutedEventHandler(tItm_Selected);
                    tItm2.Header = catDev;
                    tItm.Items.Add(tItm2);

                    if (autoExpand == true)
                    {
                        tItm.IsExpanded = true;
                        tItm2.IsExpanded = true;
                    }
                    this.gl_catDevLst.Add(CategoryId, catDev);

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

        void catDev_CategorySelected(CategorySelectedEventArgs e)
        {
            try
            {
                switch (e.CategoryIsSelected)
                {
                    case false:
                        if (this.gl_catDefLst.ContainsKey(e.CategoryId))
                        {
                            this.gl_catDefLst.Remove(e.CategoryId);
                            this.CategoryDeSelected(new CategorySelectedEventArgs(e.CategoryId, e.CategoryName, false, e.ParentId));
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("Removed selected category from list. Category Id: {0}", e.CategoryId.ToString());
#endif
                        }
                        break;

                    case true:
                        if (!this.gl_catDefLst.ContainsKey(e.CategoryId))
                        {
                            if (this.gl_lockCnt > this.gl_catDefLst.Count || this.gl_lockCnt == 0)
                            {
                                if (this.gl_lockOutParentNodes == false)
                                {
                                    this.gl_catDefLst.Add(e.CategoryId, new CategoryDef(e.CategoryName, e.CategoryId, e.ParentId));
                                    this.CategorySelected(new CategorySelectedEventArgs(e.CategoryId, e.CategoryName, true, e.ParentId));
#if DEBUG
                                    System.Diagnostics.Debug.WriteLine("Added selected category from list. Category Id: {0}", e.CategoryId.ToString());
#endif
                                }
                                else
                                {
                                    if (this.gl_xDoc != null)
                                    {
                                        try
                                        {
                                            var parentQueryNode = from parentNode in this.gl_xDoc.Root.Descendants("Category")
                                                                  where parentNode.Attribute("Id").Value == e.CategoryId
                                                                  select parentNode;
                                            foreach (var parentNode in parentQueryNode)
                                            {
                                                if (parentNode.HasElements == true)
                                                {
                                                    try
                                                    {
                                                        CategoryIdDevice catIdDev = null;
                                                        if (this.gl_catDevLst.TryGetValue(e.CategoryId, out catIdDev) == true)
                                                        {
                                                            catIdDev.xIsEnabledChk.IsChecked = false;
                                                        }
                                                        MessageBox.Show("You may not choose parent categories. Please continue to expand the categories and choose child nodes.", "Parent Category Selection Not Allowed", MessageBoxButton.OK);
                                                    }
#if DEBUG
                                                    catch (Exception caex)
                                                    {
                                                        System.Diagnostics.Debug.WriteLine("Parent Node Lockout Failure: {0}", string.Format("Category could not be switched off (UI control). Category Id: {0} Category Name: {1}\nXDocument: {2}\n\nError: {3}", e.CategoryId, e.CategoryName, this.gl_xDoc.ToString(), caex.ToString()));
                                                    }
#else
                                                    catch
                                                    {
                                                    }
#endif
                                                }
                                                else
                                                {
                                                    this.gl_catDefLst.Add(e.CategoryId, new CategoryDef(e.CategoryName, e.CategoryId, e.ParentId));
                                                    this.CategorySelected(new CategorySelectedEventArgs(e.CategoryId, e.CategoryName, true, e.ParentId));
#if DEBUG
                                                    System.Diagnostics.Debug.WriteLine("Added selected category from list. Category Id: {0}", e.CategoryId.ToString());
#endif
                                                }
                                                break;
                                            }
                                        }
                                        catch
                                        {
                                            try
                                            {
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    else
                                    {
                                        this.gl_catDefLst.Add(e.CategoryId, new CategoryDef(e.CategoryName, e.CategoryId, e.ParentId));
                                        this.CategorySelected(new CategorySelectedEventArgs(e.CategoryId, e.CategoryName, true, e.ParentId));
#if DEBUG
                                        System.Diagnostics.Debug.WriteLine("Added selected category from list. Category Id: {0}", e.CategoryId.ToString());
#endif
                                    }
                                }
                            }
                            else
                            {
                                CategoryIdDevice catIdDev = null;
                                if (this.gl_catDevLst.TryGetValue(e.CategoryId, out catIdDev) == true)
                                {
                                    catIdDev.xIsEnabledChk.IsChecked = false;
                                }
                                MessageBox.Show("You have reached your selection limit. Limit: " + this.gl_lockCnt.ToString(), "Category Selection Limit", MessageBoxButton.OK);
                            }
                        }
                        break;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Gets the currently selection lock for categories.
        /// </summary>
        public int SelectionLock
        {
            get { return this.gl_lockCnt; }
        }

        /// <summary>
        /// Gets a copy of the selected categories.
        /// </summary>
        public Dictionary<string, CategoryDef> SelectedCategories
        {
            get { return (new Dictionary<string, CategoryDef>(this.gl_catDefLst)); }
        }

        public List<string> GetSelectedCategoriesIds()
        {
            List<string> tmp = new List<string>();
            foreach (string id in this.gl_catDefLst.Keys)
            {
                tmp.Add(id.ToString());
            }
            return tmp;
        }

        public event CategoryAddedEventHandler CategoryAdded;
        public event CategoryRemovedEventHandler CategoryRemoved;
        public event CategoryEditedEventHandler CategoryEdited;
        public event CategoryUnOrSelectedEventHandler CategorySelected;
        public event CategoryUnOrSelectedEventHandler CategoryDeSelected;
    }
}
