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
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Win32;

namespace CategoryMgr
{
    /// <summary>
    /// Interaction logic for recentCatWin.xaml
    /// </summary>
    public partial class recentCatWin : Window
    {
        private string gl_grpName = null;
        private XDocument gl_xDoc = null;
        private XElement gl_xElem = null;
        private List<string> gl_catLst = null;

        public recentCatWin(string groupName)
        {
            InitializeComponent();
            this.xOkBtn.Click += new RoutedEventHandler(xOkBtn_Click);
            this.xCancelBtn.Click += new RoutedEventHandler(xCancelBtn_Click);
            this.gl_catLst = new List<string>();
            this.gl_grpName = groupName;
            try
            {
                this.gl_xDoc = XDocument.Load(@".\recentCatSelection.xml");
            }
#if DEBUG
            catch (System.IO.FileNotFoundException fne)
            {
                System.Diagnostics.Debug.WriteLine("Recent Category Selection Error", fne.ToString());
#else
            catch
            {
#endif
                if (MessageBox.Show("Could not load the recent category file. Would you like to locate it yourself (if not you can't use the recent category selection window)?", "File Not Found Error", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    OpenFileDialog oFD = new OpenFileDialog();
                    oFD.Filter = "XML files (*.xml)|*.xml";
                    oFD.FilterIndex = 0;
                    oFD.ShowDialog();
                    if (oFD.FileName != "")
                    {
                        try
                        {
                            this.gl_xDoc = XDocument.Load(oFD.FileName);
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    //
                }
            }

            this.gl_xElem = this.gl_xDoc.XPathSelectElement(string.Format("RecentCategories/Group[@Name='{0}']", groupName));
            if (this.gl_xElem != null)
            {
                try
                {
                    var cQuery = from category in this.gl_xElem.Descendants("Category")
                                 select new
                                 {
                                     CategoryId = category.Attribute("Id").Value,
                                     CategoryPath = category.Attribute("Path").Value
                                 };
                    foreach (var categoryPath in cQuery)
                    {
                        recentCatCtrl rCtrl = new recentCatCtrl(categoryPath.CategoryId, categoryPath.CategoryPath);
                        rCtrl.RecentCategorySelected += new RecentCategorySelectedEventHandler(rCtrl_RecentCategorySelected);
                        rCtrl.RecentCategoryUnselected += new RecentCategoteUnselectedEventHandler(rCtrl_RecentCategoryUnselected);
                        rCtrl.xEnabledChk.Unchecked += new RoutedEventHandler(xEnabledChk_Unchecked);
                        this.xMainStk.Children.Add(rCtrl);
                    }
                }
#if DEBUG
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("recentCatWin Query Error: {0}", ex.ToString());
#else
                catch
                {
#endif
                }
            }
        }

        void rCtrl_RecentCategoryUnselected(recentCatCtrl sender)
        {
            try
            {
                this.gl_catLst.Remove(sender.CategoryId);
            }
#if DEBUG
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("rCtrl_RecentCategoryUnselected Error: {0}", ex.ToString());
#else
            catch
            {
#endif
            }
        }

        void rCtrl_RecentCategorySelected(recentCatCtrl sender)
        {
            try
            {
                this.gl_catLst.Add(sender.CategoryId);
            }
#if DEBUG
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("rCtrl_RecentCategorySelected Error: {0}", ex.ToString());
#else
            catch
            {
#endif
            }
        }

        void xCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.gl_catLst.Clear();
            this.Close();
        }

        void xOkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void xEnabledChk_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is recentCatCtrl)
            {
                recentCatCtrl rCtrl = (sender as recentCatCtrl);
                this.gl_catLst.Remove(rCtrl.CategoryId);
            }
        }

        void xEnabledChk_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is recentCatCtrl)
            {
                recentCatCtrl rCtrl = (sender as recentCatCtrl);
                this.gl_catLst.Add(rCtrl.CategoryId);
            }
        }

        /// <summary>
        /// Gets the category list.
        /// </summary>
        public List<string> CategoryList
        {
            get { return this.gl_catLst; }
        }
    }
}
