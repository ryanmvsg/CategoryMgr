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

namespace CategoryMgr
{
    /// <summary>
    /// Interaction logic for CategoryIdDevice.xaml
    /// </summary>
    public partial class CategoryIdDevice : UserControl
    {
        private string gl_catName = "";
        private bool gl_catSelected = false;
        private string gl_catId = "";
        private string gl_pCatId = "";

        public CategoryIdDevice(string CategoryId, string CategoryName, bool isSelected = false, string ParentId = "")
        {
            InitializeComponent();

            this.gl_catId = CategoryId;
            this.CategoryName = CategoryName;
            this.gl_pCatId = ParentId;
            this.CategoryIsSelected = isSelected;
            this.xIsEnabledChk.Checked += new RoutedEventHandler(xIsEnabledChk_Checked);
            this.xIsEnabledChk.Unchecked += new RoutedEventHandler(xIsEnabledChk_Unchecked);
            this.CategorySelected += new CategoryUnOrSelectedEventHandler(CategoryIdDevice_CategorySelected);
        }

        void xIsEnabledChk_Unchecked(object sender, RoutedEventArgs e)
        {
            this.gl_catSelected = false;
            this.CategorySelected(new CategorySelectedEventArgs(this.gl_catId, this.gl_catName, this.gl_catSelected));
        }

        void xIsEnabledChk_Checked(object sender, RoutedEventArgs e)
        {
            this.gl_catSelected = true;
            this.CategorySelected(new CategorySelectedEventArgs(this.gl_catId, this.gl_catName, this.gl_catSelected));
        }

        void CategoryIdDevice_CategorySelected(CategorySelectedEventArgs e)
        {
            //
        }

        public string ParentId
        {
            get { return this.gl_pCatId; }
        }

        /// <summary>
        /// Gets the category id.
        /// </summary>
        public string CategoryId
        {
            get { return this.gl_catId; }
        }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string CategoryName
        {
            get { return this.gl_catName; }
            set { this.gl_catName = value; this.xCategoryTxtBlk.Text = value; }
        }

        /// <summary>
        /// Gets or sets whether the category is selected or not.
        /// </summary>
        public bool CategoryIsSelected
        {
            get { return this.gl_catSelected; }
            set { this.gl_catSelected = value; this.xIsEnabledChk.IsChecked = value; }
        }

        public event CategoryUnOrSelectedEventHandler CategorySelected;
    }
}
