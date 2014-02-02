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
    /// Interaction logic for CategoryEditWin.xaml
    /// </summary>
    public partial class CategoryEditWin : Window
    {
        private string gl_catId = "", gl_catName = "";
        private static RoutedCommand gl_editCmd = new RoutedCommand();

        public CategoryEditWin(string catId)
        {
            InitializeComponent();
            this.gl_catId = catId;
            CommandBinding cb = new CommandBinding(gl_editCmd, gl_editCmd_Executed, gl_editCmd_CanExecute);
            this.CommandBindings.Add(cb);

            this.xEditCatBtn.Command = gl_editCmd;
            this.xEditCatBtn.Click += new RoutedEventHandler(xEditCatBtn_Click);
        }

        private void gl_editCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.xCatNameTxt.Text == "")
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void gl_editCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.gl_catName = this.xCatNameTxt.Text;
            this.Close();
        }

        void xEditCatBtn_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        public string CategoryName
        {
            get { return this.gl_catName; }
        }
    }
}
