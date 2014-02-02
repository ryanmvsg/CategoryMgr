using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CategoryMgr
{
    /// <summary>
    /// CategoryAddedEventArgs - used to track categories that get added that need to be put into the document.
    /// </summary>
    public sealed class CategoryAddedEventArgs : EventArgs
    {
        private string gl_catId = "", gl_catName = "", gl_pId = "";

        public CategoryAddedEventArgs(CategoryIdDevice catIdDevice)
            : base()
        {
            this.gl_catId = catIdDevice.CategoryId;
            this.gl_catName = catIdDevice.CategoryName;
            this.gl_pId = catIdDevice.ParentId;
        }

        /// <summary>
        /// Gets the category id that was being added.
        /// </summary>
        public string CategoryId
        {
            get { return this.gl_catId; }
        }

        /// <summary>
        /// Gets parent id.
        /// </summary>
        public string ParentId
        {
            get { return this.gl_pId; }
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
