﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace emb {
	public partial class MainWindow : Window {
		private IDictionary<int,MarketGroup> market_groups = new Dictionary<int,MarketGroup>();
		private IDictionary<int,IList<Type>> types = new Dictionary<int,IList<Type>>();

		public MainWindow() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			IEnumerable<string> mg_reader = System.IO.File.ReadLines("inv_market_groups.csv");
			foreach (string line in mg_reader) {
				MarketGroup mg = new MarketGroup();
				string[] split = line.Split(',');
				int mg_id = int.Parse(split[0]);
				string parent = split[1];
				if (parent == "null")
					mg.parent_id = null;
				else
					mg.parent_id = int.Parse(parent);
				mg.name = split[2].Substring(1, split[2].Length-2);
				mg.has_types = (split[3] == "1");
				market_groups.Add(mg_id, mg);
			}
			add_children(tvGroups.Items, null);

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

		private void add_children(ItemCollection group, int? id) {
			foreach (KeyValuePair<int,MarketGroup> mg in market_groups) {
				if (mg.Value.parent_id == id) {
					TreeViewItem item = new TreeViewItem();
					item.Header = mg.Value.name;
					item.Tag = mg.Key;
					if (!mg.Value.has_types)
						add_children(item.Items, mg.Key);
					group.Add(item);
				}
			}
		}

		private void tvGroups_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			TreeViewItem value = (TreeViewItem)e.NewValue;
			int mg_id = (int)value.Tag;
			lbTypes.Items.Clear();
			if (market_groups[mg_id].has_types)
				foreach (Type t in types[mg_id])
					lbTypes.Items.Add(t.name);
		}
	}

	struct MarketGroup {
		public int? parent_id;
		public string name;
		public bool has_types;
	}

	struct Type {
		public int id;
		public string name;
	}
}