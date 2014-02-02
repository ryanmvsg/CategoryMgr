using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CategoryMgr
{
    /// <summary>
    /// CategoryDef class is used to track data for different objects.
    /// </summary>
    public class CategoryDef
    {
        private string gl_catId = "", gl_catName = "", gl_pCatId = "";

        /// <summary>
        /// Generic constructor.
        /// </summary>
        /// <param name="CategoryId">Category Id.</param>
        /// <param name="CategoryName">The category name.</param>
        public CategoryDef(string CategoryName, string CategoryId = "", string pCatId = "")
        {
            if (CategoryId != "")
                this.gl_catId = CategoryId;
            else
                this.gl_catId = string.Format("{{0}}", Guid.NewGuid().ToString());

            this.gl_catName = CategoryName;

            this.gl_pCatId = pCatId;
        }

        /// <summary>
        /// Gets the category id.
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
    }
}
