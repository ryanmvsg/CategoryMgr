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
using System.Xml.Linq;
using Microsoft.Win32;

namespace CategoryMgr
{
    /// <summary>
    /// Interaction logic for recentCatCtrl.xaml
    /// </summary>
    public partial class recentCatCtrl : UserControl
    {
        private string gl_grpId = null, gl_grpPath;

        public recentCatCtrl(string grpId, string grpPath)
        {
            InitializeComponent();
            this.gl_grpPath = grpPath;
            this.xPathNameTxtBlk.Text = grpPath;
            this.gl_grpId = grpId;
            this.RecentCategorySelected += new RecentCategorySelectedEventHandler(recentCatCtrl_RecentCategorySelected);
            this.RecentCategoryUnselected += new RecentCategoteUnselectedEventHandler(recentCatCtrl_RecentCategoryUnselected);
            this.xEnabledChk.Checked += new RoutedEventHandler(xEnabledChk_Checked);
            this.xEnabledChk.Unchecked += new RoutedEventHandler(xEnabledChk_Unchecked);
        }

        void recentCatCtrl_RecentCategoryUnselected(recentCatCtrl sender)
        {
            //
        }

        private void recentCatCtrl_RecentCategorySelected(recentCatCtrl sender)
        {
            //
        }

        void xEnabledChk_Unchecked(object sender, RoutedEventArgs e)
        {
            this.RecentCategoryUnselected(this);
        }

        void xEnabledChk_Checked(object sender, RoutedEventArgs e)
        {
            this.RecentCategorySelected(this);
        }

        /// <summary>
        /// Gets the category id.
        /// </summary>
        public string CategoryId
        {
            get { return this.gl_grpId; }
        }

        /// <summary>
        /// Gets the category path.
        /// </summary>
        public string CategoryPath
        {
            get { return this.gl_grpPath; }
        }

        public event RecentCategorySelectedEventHandler RecentCategorySelected;
        public event RecentCategoteUnselectedEventHandler RecentCategoryUnselected;
    }
}
