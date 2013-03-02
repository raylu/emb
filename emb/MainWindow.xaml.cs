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
		public MainWindow() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			IEnumerable<string> mg_reader = System.IO.File.ReadLines("inv_market_groups.csv");
			List<MarketGroup> market_groups = new List<MarketGroup>();
			foreach (string line in mg_reader) {
				MarketGroup mg = new MarketGroup();
				string[] split = line.Split(',');
				mg.id = Int32.Parse(split[0]);
				string parent = split[1];
				if (parent == "null")
					mg.parent_id = null;
				else
					mg.parent_id = Int32.Parse(parent);
				mg.name = split[2].Substring(1, split[2].Length-2);
				mg.has_types = (split[3] == "1");
				market_groups.Add(mg);
			}
			add_children(groups.Items, market_groups, null);
		}

		private void add_children(ItemCollection group, IEnumerable<MarketGroup> market_groups, int? id) {
			foreach (MarketGroup mg in market_groups) {
				if (mg.parent_id == id) {
					TreeViewItem item = new TreeViewItem();
					item.Header = mg.name;
					if (!mg.has_types)
						add_children(item.Items, market_groups, mg.id);
					group.Add(item);
				}
			}
		}
	}

	struct MarketGroup {
		public int id;
		public int? parent_id;
		public string name;
		public bool has_types;
	}
}