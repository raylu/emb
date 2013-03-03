using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Xml.Linq;

namespace emb {
	public partial class OrdersControl : UserControl {
		private int type_id;
		private DataTable sell_orders = new DataTable();
		private DataTable buy_orders = new DataTable();

		public OrdersControl(int type_id) {
			InitializeComponent();
			this.type_id = type_id;

			sell_orders.Columns.Add("Station", typeof(string));
			sell_orders.Columns.Add("Price", typeof(string));
			buy_orders.Columns.Add("Station", typeof(string));
			buy_orders.Columns.Add("Price", typeof(string));
			dgSell.DataContext = sell_orders;
			dgSell.ItemsSource = sell_orders.DefaultView;
			dgBuy.DataContext = buy_orders;
			dgBuy.ItemsSource = buy_orders.DefaultView;

			WebClient h = new WebClient();
			string text = h.DownloadString("http://api.eve-central.com/api/quicklook?typeid=" + this.type_id);
			XDocument xml = XDocument.Parse(text);

			foreach (Order order in parse_orders(xml, "sell_orders"))
				sell_orders.Rows.Add(new object[] {order.station, order.price});
			foreach (Order order in parse_orders(xml, "buy_orders"))
				buy_orders.Rows.Add(new object[] {order.station, order.price});
		}

		private IEnumerable<Order> parse_orders(XDocument xml, string type) {
			return (from o in xml.Descendants(type).Elements("order")
				select new Order() {
					station = o.Element("station_name").Value.Trim(),
					price = float.Parse(o.Element("price").Value).ToString("N2"),
				}
			);
		}
	}

	class Order {
		public string station;
		public string price;
	}
}