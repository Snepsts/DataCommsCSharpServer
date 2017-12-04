/* CSharpServer.cs
 * 
 * (C) 2017 Michael Ranciglio
 * 
 * Prepared for CS480 at Southeast Missouri State University
 * 
 * Modeled after the DataCommsCppServer.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DataCommsCSharpServer
{
	class CSharpServer
	{
		//SERVER_NAME
		static string serverName = "CS480 Demo Web Server";
		//ERROR 400
		static string fourHunned = "<head></head><body><html><h1>Error 400</h1><p>The server couldn't understand your request.</html></body>\n";
		//ERROR 404
		static string fourHunnednForteeFor = "<head></head><body><html><h1>Error 404</h1><p>Document not found.</html></body>\n";
		// /
		static string index = "<head></head><body><html><h1>Welcome to the CS480\\Demo Server</h1><p>Why not visit: <ul><li><a href =\"http://www2.semo.edu/csdept/ \">Computer Science Home Page</a><li><a href =\"http://cstl-csm.semo.edu/liu/cs480_fall2012/index.htm\"\\>CS480 Home Page<a></ul></html></body>\n";
		static void Main(string[] args)
		{
			byte[] data = new byte[1024];
			string message = "";
			int recvLength;

			if (args.Length > 1) //test for correct # of args
				throw new ArgumentException("Parameters: [<Port>]");

			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, Int32.Parse(args[0]));
			Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(ipep);
			server.Listen(10);

			while (true) {
				Console.WriteLine("Waiting for client...");
				Socket client = server.Accept();
				Console.WriteLine("Connected to " + client.ToString());

				while (true) {
					recvLength = client.Receive(data);

					if (recvLength == 0) //didn't get anything
						break;
					message = Encoding.ASCII.GetString(data, 0, recvLength);
					Console.WriteLine(message);

					if (message.Contains("\r\n\r\n"))
						Console.WriteLine("Header Fully Recieved");
					break;
				}

				string newMessage = "";

				if (recvLength == 0) {
					SendHead(client, 400, fourHunned.Length);
					data = Encoding.ASCII.GetBytes(fourHunned + "\r\n\r\n");
					Console.WriteLine("Sending: " + fourHunned);
					client.Send(data, SocketFlags.None);
				} else {
					if (!message.Contains("GET") || !message.Contains("HTTP/")) { //if they aren't GETting anything then screw them, bad request
						SendHead(client, 400, fourHunned.Length);
						data = Encoding.ASCII.GetBytes(fourHunned + "\r\n\r\n");
						Console.WriteLine("Sending: " + fourHunned);
						client.Send(data, SocketFlags.None);
					} else {

						newMessage = ""; //reinit cuz why not?

						for (int i = 4; i < message.Length; i++) { //start at 4 to get the first item after "GET "
							if (message[i] == ' ') { //break outta these chaiiins when we reach a space (to ignore "HTML/1.0\r\n\r\n")
								break;
							}

							newMessage += message[i];
						}
					}

					Console.WriteLine("newMessage = " + newMessage);
					Console.WriteLine("newMessage.Length = " + newMessage);

					if (newMessage == "/") { // /: index
						SendHead(client, 200, index.Length);
						data = Encoding.ASCII.GetBytes(index + "\r\n\r\n");
						Console.WriteLine("Sending: " + index);
						client.Send(data, SocketFlags.None);
					} else { //404: Not Found
						SendHead(client, 404, fourHunnednForteeFor.Length);
						data = Encoding.ASCII.GetBytes(fourHunnednForteeFor + "\r\n\r\n");
						Console.WriteLine("Sending: " + fourHunnednForteeFor);
						client.Send(data, SocketFlags.None);
					}
				}

				Console.WriteLine("Disconnected from " + client.ToString());
				client.Close();

				Console.WriteLine("Would you like to shut down the server? (Yes or No)");
				bool quitting = Console.ReadLine().ToLower().Contains("y");

				if (quitting) {
					Console.WriteLine("Shutting down...");
					break;
				}
			}

			server.Close();
		}

		static void SendHead(Socket client, int code, int length)
		{
			string codeStr;

			switch(code) {
				case 200:
					codeStr = "OK";
					break;
				case 400:
					codeStr = "Bad Request";
					break;
				case 404:
					codeStr = "Not Found";
					break;
				default:
					codeStr = "Unknown";
					break; //why does C# require breaks? Couldn't they just make that default behavior in the langauge?
			}

			string msg = "HTTP/1.1 " + code + " " + codeStr + "\n";
			byte[] data = Encoding.ASCII.GetBytes(msg);
			Console.WriteLine(msg);
			client.Send(data, SocketFlags.None);

			msg = "Server: " + serverName + "\r\n";
			data = Encoding.ASCII.GetBytes(msg);
			Console.WriteLine(msg);
			client.Send(data, SocketFlags.None);

			msg = "Content-Length: " + length + "\r\n";
			data = Encoding.ASCII.GetBytes(msg);
			Console.WriteLine(msg);
			client.Send(data, SocketFlags.None);

			msg = "Content-Type: text/html\r\n";
			data = Encoding.ASCII.GetBytes(msg);
			Console.WriteLine(msg);
			client.Send(data, SocketFlags.None);

			msg = "\r\n";
			data = Encoding.ASCII.GetBytes(msg);
			Console.WriteLine(msg);
			client.Send(data, SocketFlags.None);
		}
	}
}
