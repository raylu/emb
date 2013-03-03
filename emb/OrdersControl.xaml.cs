using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Xml.Linq;

namespace emb {
	public partial class OrdersControl : UserControl {
		private int type_id;
		private IDictionary<int,string> regions;

		public OrdersControl(int type_id, IDictionary<int,string> regions) {
			InitializeComponent();
			this.type_id = type_id;
			this.regions = regions;

			DataTable sell_orders = new DataTable();
			DataTable buy_orders = new DataTable();
			setup_table(sell_orders, dgSell);
			setup_table(buy_orders, dgBuy);

			WebClient h = new WebClient();
			string text = h.DownloadString("http://api.eve-central.com/api/quicklook?typeid=" + this.type_id);
			XDocument xml = XDocument.Parse(text);
			//Console.WriteLine(xml);

			foreach (Order order in parse_orders(xml, "sell_orders"))
				sell_orders.Rows.Add(new object[] {order.region, order.station, order.price, order.volume});
			foreach (Order order in parse_orders(xml, "buy_orders"))
				buy_orders.Rows.Add(new object[] {order.region, order.station, order.price, order.volume});
		}

		private void setup_table(DataTable table, DataGrid grid_control) {
			table.Columns.Add("Region", typeof(string));
			table.Columns.Add("Station", typeof(string));
			table.Columns.Add("Price", typeof(string));
			table.Columns.Add("Volume", typeof(int));
			grid_control.DataContext = table;
			grid_control.ItemsSource = table.DefaultView;
		}

		private IEnumerable<Order> parse_orders(XDocument xml, string type) {
			return (from x in xml.Descendants(type).Elements("order")
				select new Order(x, regions)
			);
		}
	}

	class Order {
		public string region;
		public string station;
		public string price;
		public int volume;

		public Order(XElement x, IDictionary<int,string> regions) {
			region = regions[int.Parse(x.Element("region").Value)];
			station = x.Element("station_name").Value.Trim();
			price = float.Parse(x.Element("price").Value).ToString("N2");
			volume = int.Parse(x.Element("vol_remain").Value);
		}
	}
}