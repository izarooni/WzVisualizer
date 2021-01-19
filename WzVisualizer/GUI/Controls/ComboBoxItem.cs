namespace WzVisualizer.GUI.Controls {
    public class ComboBoxItem {
        public ComboBoxItem(string name, object value) {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }

        public override string ToString() => Name;
    }
}