
using System.Net;
using System.Net.Sockets;

// var ipHostInfo = Dns.GetHostEntry("192.168.0.177");  
// var ipAddress = ipHostInfo.AddressList[0];
// var ipEndPoint = new IPEndPoint(ipAddress, 2000);  
// var client = new TcpClient();
// client.Connect(ipEndPoint);

const byte number = 0b_1111_0000;
const byte number2 = 0b_0011_1100;

byte numberAndOperator = number & number2;

Console.Out.WriteLine(numberAndOperator);
Console.WriteLine(Convert.ToString(numberAndOperator,2));

