using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASW_Background_Screens
{
    public class UserInputResult
    {
        public DialogResult DialogResult { get; set; }  // OK, Cancel, or cancel with JustDelete flag
        public bool JustDeleteIt { get; set; }
        public string TextBoxDate { get; set; }
        public string TextBoxDescription { get; set; }
    }
}
