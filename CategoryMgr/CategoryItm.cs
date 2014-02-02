using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CategoryMgr
{
    public class CategoryItm
    {
        private List<CategoryItm> gl_catItms = null;
        private CategoryItm gl_parent = null;
        private string gl_catName = "";
        private string gl_catId = "";

        public CategoryItm(string CategoryId, string CategoryName, CategoryItm parent = null)
        {
            this.gl_catItms = new List<CategoryItm>();
            this.gl_parent = parent;
            this.gl_catId = CategoryId;
            this.gl_catName = CategoryName;
        }

        public string CategoryName
        {
            get { return this.gl_catName; }
        }

        public string CategoryId
        {
            get { return this.gl_catId; }
        }

        /// <summary>
        /// Gets the parent item (if one is present).
        /// </summary>
        public CategoryItm Parent
        {
            get { return this.gl_parent; }
        }

        public List<CategoryItm> Items
        {
            get { return this.gl_catItms; }
        }
    }
}
