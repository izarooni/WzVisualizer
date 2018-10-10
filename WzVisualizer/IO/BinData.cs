using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzVisualizer
{
    public class BinData
    {
        public int ID;
        public Image image;
        public string _name;
        public List<string> properties = new List<string>();

        public string Name {
            get { return _name; }
            set { _name = value ?? "NO-NAME"; }
        }

        public bool Search(string filter)
        {
            if (ID.ToString().Contains(filter)) return true;
            if (Name.ToLower().Contains(filter.ToLower())) return true;
            foreach (string prop in properties)
                if (prop.ToLower().Contains(filter.ToLower())) return true;
            return false;
        }
    }
}
