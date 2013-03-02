using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace emb {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private IDictionary<int,IList<Type>> types = new Dictionary<int,IList<Type>>();

		public MainWindow() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			IEnumerable<string> mg_reader = System.IO.File.ReadLines("inv_market_groups.csv");
			List<MarketGroup> market_groups = new List<MarketGroup>();
			foreach (string line in mg_reader) {
				MarketGroup mg = new MarketGroup();
				string[] split = line.Split(',');
				mg.id = int.Parse(split[0]);
				string parent = split[1];
				if (parent == "null")
					mg.parent_id = null;
				else
					mg.parent_id = int.Parse(parent);
				mg.name = split[2].Substring(1, split[2].Length-2);
				mg.has_types = (split[3] == "1");
				market_groups.Add(mg);
			}
			add_children(tvGroups.Items, market_groups, null);

			IEnumerable<string> type_reader = System.IO.File.ReadLines("inv_types.csv");
			foreach (string line in type_reader) {
				string[] split = parse_csv_line(line, 3, 1);
				Type t = new Type();
				t.id = int.Parse(split[0]);
				t.name = split[1].Substring(1, split[1].Length-2);
				int mg_id = int.Parse(split[2]);
				if (!types.ContainsKey(mg_id))
					types.Add(mg_id, new List<Type>());
				types[mg_id].Add(t);
			}
		}

		private string[] parse_csv_line(string line, int fields, int string_index) {
			string[] split = line.Split(new char[]{','}, fields);
			string str = split[string_index];
			if (str[str.Length-1] != '\"') {
				string next = split[string_index+1];
				int end = next.LastIndexOf('\"');
				str += "," + next.Substring(0, end);
				next = next.Substring(end+2);
				split[string_index] = str;
				split[string_index+1] = next;
			}
			return split;
		}

		private void add_children(ItemCollection group, IEnumerable<MarketGroup> market_groups, int? id) {
			foreach (MarketGroup mg in market_groups) {
				if (mg.parent_id == id) {
					TreeViewItem item = new TreeViewItem();
					item.Header = mg.name;
					item.Tag = mg.id;
					if (!mg.has_types)
						add_children(item.Items, market_groups, mg.id);
					group.Add(item);
				}
			}
		}

		private void tvGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			TreeViewItem value = (TreeViewItem)e.NewValue;
			int mg_id = (int)value.Tag;
			lbTypes.Items.Clear();
			if (!types.ContainsKey(mg_id)) return;
			foreach (Type t in types[mg_id]) {
				lbTypes.Items.Add(t.name);
			}
		}
	}

	struct MarketGroup {
		public int id;
		public int? parent_id;
		public string name;
		public bool has_types;
	}

	struct Type {
		public int id;
		public string name;
	}
}