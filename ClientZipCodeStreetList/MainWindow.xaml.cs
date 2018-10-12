using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientZipCodeStreetList
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		int ServerPort = 8005; // server port
		string ServerIP = "127.0.0.1";
		public MainWindow()
		{
			InitializeComponent();
			WindowStartupLocation = WindowStartupLocation.CenterScreen;
		}

		private void FindButton_Click(object sender, RoutedEventArgs e)
		{
			IPAddress ip = IPAddress.Parse(ServerIP);
			IPEndPoint ep = new IPEndPoint(ip, ServerPort);
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				s.Connect(ep);
				if (s.Connected)
				{
					string message = textboxQuery.Text.Trim();
					byte[] data = Encoding.Unicode.GetBytes(message);

					s.Send(data);
					int bytes = 0;
					List<byte> answer = new List<byte>();
					do
					{
						bytes = s.Receive(data);
						answer.AddRange(data);
					} while (bytes > 0);

					listBox.ItemsSource = FromBytesArrayToObject(answer.ToArray()) as List<string>;
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
				try
				{
					s?.Shutdown(SocketShutdown.Both);
					s?.Close();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}
		private object FromBytesArrayToObject(byte[] arr)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(arr, 0, arr.Length);
			memoryStream.Position = 0;
			return binaryFormatter.Deserialize(memoryStream);
		}

		private void textboxQuery_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!Char.IsDigit(e.Text.FirstOrDefault()))
				e.Handled = true;
		}

		private void TextBlock_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				FindButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
		}
	}
}
