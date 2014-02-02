using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CategoryMgr
{
    public class EditCategoryResponse
    {
        private string gl_catId = "", gl_catName = "";

        public EditCategoryResponse(string catId, string catName)
        {
            this.gl_catId = catId;
            this.gl_catName = catName;
        }

        public static EditCategoryResponse ShowDialog(string catId)
        {
            CategoryEditWin editWin = new CategoryEditWin(catId);
            editWin.ShowDialog();
            return new EditCategoryResponse(catId, editWin.CategoryName);
        }

        public string CategoryId
        {
            get { return this.gl_catId; }
        }

        public string CategoryName
        {
            get { return this.gl_catName; }
        }
    }
}
