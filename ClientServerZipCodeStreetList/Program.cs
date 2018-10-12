using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ServerZipCodeStreetList
{
	/*Используя класс Socket реализовать клиент-серверное приложение,
	которое позволяет клиенту по почтовому индексу получить список улиц, 
	соответствующих этому индексу. Данные об улицах могут хранится в файловой или реляционной базе данных или в xml файле. 
	клиент - Windows Forms или WPF приложение, 
	сервер - консольное приложение. Запрос клиента выполняется в отдельном (не интерфейсном) потоке, 
	сервер - синхронный.*/
	class Program
	{
		static int port = 8005; // server port
		static void Main(string[] args)
		{
			IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, port);
			Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				listenSocket.Bind(iPEndPoint);

				listenSocket.Listen(100);
				Console.WriteLine("Server started. Waiting for messages...");
				while (true)
				{
					Socket handle = listenSocket.Accept();
					if (handle.Available > 0)
						Task.Run(() => taskRun(handle));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message, "Error");
				Console.ReadKey();
			}
		}
		static void taskRun(Socket handle)
		{
			Console.WriteLine($"{handle.RemoteEndPoint}");

			string query = "";
			byte[] buffer = new byte[1024];
			int bytes = 0;
			while (handle?.Available > 0)
			{
				bytes = handle.Receive(buffer);
				query += Encoding.Unicode.GetString(buffer);
			}

			query = query.Replace("\0", string.Empty);
			Console.WriteLine(DateTime.Now.ToString() + ": " + query);

			List<string> answer = FindInXml(query);
			handle?.Send(ObjectToBytesArray(answer));

			handle?.Shutdown(SocketShutdown.Both);
			handle?.Close();
		}
		static List<string> FindInXml(string str)
		{
			List<string> list = new List<string>(); ;

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load("data.xml");
			XmlNode finded = xmlDocument.SelectSingleNode($"//ZipCode[@Code='{str}']");
			if (finded != null)
			{
				foreach (XmlNode item in finded.ChildNodes)
				{
					list.Add(item.InnerText);
				}
			}

			return list;
		}
		static byte[] ObjectToBytesArray(object obj)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, obj);
			return memoryStream.ToArray();
		}
	}
}
