using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CategoryMgr
{
    public class AddCategoryResponse
    {
        private string gl_catName = "";
        private string gl_pId = "";

        public AddCategoryResponse(string categoryName, string ParentId = "")
        {
            this.gl_catName = categoryName;
            this.gl_pId = ParentId;
        }

        public static AddCategoryResponse ShowDialog(string categoryName = "", string parentId = "")
        {
            AddCategoryWin mWin = new AddCategoryWin(categoryName);
            mWin.ShowDialog();
            return new AddCategoryResponse(mWin.CategoryName, parentId);
        }

        /// <summary>
        /// Gets the parent id (if present).
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
