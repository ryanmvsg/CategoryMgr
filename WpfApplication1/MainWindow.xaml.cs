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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CategoryMgr.CategoryMgr gl_catMgr = null;

        public MainWindow()
        {
            InitializeComponent();
            this.gl_catMgr = new CategoryMgr.CategoryMgr();
            List<string> catIdLst = CategoryMgr.CategoryMgr.GetCategoryIdList(this.gl_catMgr.GetCopiedContainer(), "Laptops/Dell/Test 1");
            List<CategoryMgr.CategoryItm> catItmLst = this.gl_catMgr.GetCopiedContainer().BuildCategoryToRootList("{20201-402-0429-ATEDHRSW3}");
            if (catItmLst != null)
            {
                foreach (CategoryMgr.CategoryItm catItm in catItmLst)
                {
                    if (catItm.Parent == null)
                        System.Diagnostics.Debug.WriteLine("Category: {0} Id: {1}", catItm.CategoryName, catItm.CategoryId);
                    else
                        System.Diagnostics.Debug.WriteLine("Category: {0} Id: {1} Parent Id: {2}", catItm.CategoryName, catItm.CategoryId, catItm.Parent.CategoryId);
                }
            }

            CategoryMgr.CategoryContainer container = CategoryMgr.ContainerBuilders.BuildCategoryFromZenCart("localhost", "zencart", "root", "sr12m12sr");
            CategoryMgr.CategoryContainer containerSource = this.gl_catMgr.GetCopiedContainer();
            //List<string> catList = CategoryMgr.CategoryMgr.BuildZenCartPathIfNotExist(container, "Laptops/Dell/Test 2/Test 4", "127.0.0.1", "zencart", "root", "sr12m12sr", true);

            // Hypothetical Product 1
            string catId1 = "{505930-35903502-2045-25035-56357}";
            List<CategoryMgr.CategoryItm> pCatLst = containerSource.BuildCategoryToRootList(catId1);
            string tmpPath = CategoryMgr.CategoryMgr.BuildCategoryPath(pCatLst);
            List<string> tmp = CategoryMgr.CategoryMgr.BuildZenCartPathIfNotExist(container, tmpPath, "127.0.0.1", "zencart", "root", "sr12m12sr", true);
        }
    }
}
