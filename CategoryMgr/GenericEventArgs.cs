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
    #region CategorySelectedEventArgs Class
    public sealed class CategorySelectedEventArgs : EventArgs
    {
        private string gl_catId = "";
        private bool gl_catIsSelected = false;
        private string gl_catName = "";
        private string gl_pCatId = "";

        public CategorySelectedEventArgs(string catId, string catName, bool catIsSelected = false, string pCatId = "")
            : base()
        {
            this.gl_catId = catId;
            this.gl_catName = catName;
            this.gl_catIsSelected = catIsSelected;
            this.gl_pCatId = pCatId;
        }

        /// <summary>
        /// Gets the category id that triggered the event.
        /// </summary>
        public string CategoryId
        {
            get { return this.gl_catId; }
        }

        /// <summary>
        /// Gets the parent category id (if one is present).
        /// </summary>
        public string ParentId
        {
            get { return this.gl_pCatId; }
        }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        public string CategoryName
        {
            get { return this.gl_catName; }
        }

        /// <summary>
        /// Gets whether or not the category is selected or not.
        /// </summary>
        public bool CategoryIsSelected
        {
            get { return this.gl_catIsSelected; }
        }
    }
    #endregion

    #region CategoryDeletedEventArgs
    /// <summary>
    /// Tracks which categories get removed.
    /// </summary>
    public sealed class CategoryDeletedEventArgs : EventArgs
    {
        private string gl_catId = "";
        private string gl_pId = "";

        public CategoryDeletedEventArgs(string categoryId, string parentId = "")
            : base()
        {
            this.gl_catId = categoryId;
            this.gl_pId = parentId;
        }

        /// <summary>
        /// Gets the category id.
        /// </summary>
        public string CategoryId
        {
            get { return this.gl_catId; }
        }

        /// <summary>
        /// Gets the parent id.
        /// </summary>
        public string ParentId
        {
            get { return this.gl_pId; }
        }
    }
    #endregion

    #region CategoryEditedEventArgs
    public sealed class CategoryEditedEventArgs : EventArgs
    {
        private string gl_catId = "", gl_catNewName = "";

        public CategoryEditedEventArgs(string catId, string CategoryNewName)
            : base()
        {
            this.gl_catId = catId;
            this.gl_catNewName = CategoryNewName;
        }

        /// <summary>
        /// Gets the id of the category being edited.
        /// </summary>
        public string CategoryId
        {
            get { return this.gl_catId; }
        }

        /// <summary>
        /// Gets the category name that the old name will be overwritten by.
        /// </summary>
        public string CategoryName
        {
            get { return this.gl_catNewName; }
        }
    }
    #endregion
}