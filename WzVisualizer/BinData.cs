using System;
using System.Collections.Generic;
using System.Drawing;

namespace WzVisualizer {

    [Serializable]
    public class BinData {
        public string _name;

        public BinData() { }

        public BinData(int id, Bitmap icon, string name, string properties) {
            ID = id;
            Image = icon;
            Name = name;
            string[] lines = properties.Split(new[] {"\r\n"}, StringSplitOptions.None);
            foreach (string line in lines) {
                Properties.Add(line);
            }
        }

        public int ID { get; set; }

        public string Name {
            get => _name ?? "NO-NAME";
            set => _name = value ?? "NO-NAME";
        }

        public Image Image { get; set; }

        public List<string> Properties { get; } = new List<string>();

        public bool Search(string filter) {
            if (ID.ToString().Contains(filter)) return true;
            if (Name.ToLower().Contains(filter.ToLower())) return true;
            foreach (string prop in Properties) {
                if (prop.ToLower().Contains(filter.ToLower())) {
                    return true;
                }
            }

            return false;
        }
    }
}