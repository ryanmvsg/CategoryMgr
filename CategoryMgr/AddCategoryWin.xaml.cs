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

namespace CategoryMgr
{
    /// <summary>
    /// Interaction logic for AddCategoryWin.xaml
    /// </summary>
    public partial class AddCategoryWin : Window
    {
        private string gl_catName = "";

        public AddCategoryWin(string catName)
        {
            InitializeComponent();
            this.xAddCatBtn.Click += new RoutedEventHandler(xAddCatBtn_Click);
            this.xCatNameTxt.KeyDown += new KeyEventHandler(xCatNameTxt_KeyDown);
        }

        void xCatNameTxt_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    SaveAndCloseFunc();
                    break;
                    
                default:
                    break;
            }
        }

        void xAddCatBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveAndCloseFunc();
        }

        private void SaveAndCloseFunc()
        {
            try
            {
                this.gl_catName = this.xCatNameTxt.Text;
                this.Close();
            }
#if DEBUG
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured while trying to save and close the Add Category Window. Error: " + ex.ToString());
                System.Diagnostics.Debug.WriteLine("AddCategoryWindow Error", "An error has occured while trying to save and close the Add Category Window. Error: " + ex.ToString());
            }
#else
            catch
            {
                MessageBox.Show("An error has occured while trying to save and close the Add Category Window.");
            }
#endif
        }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        public string CategoryName
        {
            get { return this.gl_catName; }
        }
    }
}
