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
		IEnumerable<Order> sell_orders;
		IEnumerable<Order> buy_orders;
		DataTable visible_sell_orders = new DataTable();
		DataTable visible_buy_orders = new DataTable();

		public OrdersControl(int type_id, IDictionary<int,string> regions, string filter) {
			InitializeComponent();
			this.type_id = type_id;
			this.regions = regions;

			setup_table(this.visible_sell_orders, dgSell);
			setup_table(this.visible_buy_orders, dgBuy);

			WebClient h = new WebClient();
			string text = h.DownloadString("http://api.eve-central.com/api/quicklook?typeid=" + this.type_id);
			XDocument xml = XDocument.Parse(text);

			this.sell_orders = parse_orders(xml, "sell_orders");
			this.buy_orders = parse_orders(xml, "buy_orders");
			this.filter(filter);
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

		public void filter(string q) {
			this.visible_sell_orders.Clear();
			q = q.ToLower();
			foreach (Order order in this.sell_orders)
				if (q.Length == 0 || order.station.ToLower().Contains(q))
					this.visible_sell_orders.Rows.Add(new object[] { order.region, order.station, order.price, order.volume });
			foreach (Order order in this.buy_orders)
				if (q.Length == 0 || order.station.ToLower().Contains(q))
					this.visible_buy_orders.Rows.Add(new object[] {order.region, order.station, order.price, order.volume});
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