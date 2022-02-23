using System.Buffers.Binary;
using System.Collections;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using iii;


var ipHostInfo = Dns.GetHostEntry("192.168.0.177");
var ipAddress = ipHostInfo.AddressList[0];
var server = new TcpListener(ipAddress, 2000);

try
{
    // Start listening for client requests.
    server.Start();
    Console.Out.WriteLine("listening on port 2000" + ipAddress);

    // Buffer for reading data
    byte[] bytes = new byte[1];


    // Enter the listening loop.
    while (true)
    {
        Console.Write("Waiting for a connection... ");

        // Perform a blocking call to accept requests.
        // You could also use server.AcceptSocket() here.
        var client = server.AcceptTcpClient();
        Console.WriteLine("Connected!");

        // Get a stream object for reading and writing
        var stream = client.GetStream();
        var socket = stream.Socket;

        var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);


        try
        {
            var packet = Packet.Read(reader);
            Console.Out.WriteLine("packetid = {0}", packet.Id);
            Console.Out.WriteLine("packetlength = {0}", packet.Length);

            if (packet.Id == 0)
            {
                Console.Out.WriteLine("packetId = 0, Handshake");

                // var protocolVersion = reader.Read7BitEncodedInt();
                var protocolVersion = packet.Reader.Read7BitEncodedInt();
                Console.Out.WriteLine("protocol Version = {0}", protocolVersion);

                // var stringLength = reader.Read7BitEncodedInt();
                var stringLength = packet.Reader.Read7BitEncodedInt();
                Console.Out.WriteLine("stringlength: " + stringLength);

                var hostAsBytes = packet.Reader.ReadBytes(stringLength);
                var hostAsString = Encoding.UTF8.GetString(hostAsBytes);
                Console.Out.WriteLine(hostAsString);

                var portUInt16 = BinaryPrimitives.ReadUInt16BigEndian(packet.Reader.ReadBytes(2));
                Console.Out.WriteLine("portUInt16 = {0}", portUInt16);

                var nextState = packet.Reader.ReadByte();
                Console.Out.WriteLine("nextState = {0}", nextState);

                Console.Out.WriteLine(reader.Read7BitEncodedInt());
                Console.Out.WriteLine(reader.Read7BitEncodedInt());

                var responseJson = new
                {
                    version = new
                    {
                        name = "1.18.1",
                        protocol = 757,
                    },
                    players = new
                    {
                        max = 100,
                        online = 5,
                        sample = new List<dynamic>()
                        {
                            new
                            {
                                name = "thinkofdeath",
                                id = "4566e69f-c907-48ee-8d71-d7ba5aa00d20"
                            }
                        }
                    },
                    description = new
                    {
                        text = "A Minecraft Server"
                    }
                };

                var json = JsonSerializer.Serialize(responseJson);
                MemoryStream ms = new MemoryStream();
                using var buffer = MemoryOwner<byte>.Allocate(2048);
                BinaryWriter writer = new BinaryWriter(buffer.AsStream(), Encoding.UTF8, leaveOpen: false);
                var jsonAsBytes = Encoding.UTF8.GetBytes(json);
                // writer.Write7BitEncodedInt(jsonAsBytes.Length+3);
                writer.Write7BitEncodedInt(0);
                writer.Write7BitEncodedInt(jsonAsBytes.Length);
                writer.Write(jsonAsBytes);
                
                // var span = ms.GetBuffer().AsSpan(0, (int) ms.Position);
                var span = buffer.Memory.Span.Slice(0,(int) writer.BaseStream.Position);
                
                var networkWriter = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);

                Console.Out.WriteLine(span.Length);
                networkWriter.Write7BitEncodedInt(span.Length);
                networkWriter.Write(span);


                Console.Out.WriteLine("response sent");

                var packetLength = reader.Read7BitEncodedInt();
                var packetId = reader.Read7BitEncodedInt();
                Console.Out.WriteLine(packetId);
                Console.Out.WriteLine(packetLength);
                
                // var longNumber = BinaryPrimitives.ReadInt64BigEndian(reader.ReadBytes(8));
                var longNumber = reader.ReadBytes(8);
                Console.Out.WriteLine(longNumber);

                networkWriter.Write7BitEncodedInt(packetLength);
                networkWriter.Write7BitEncodedInt(packetId);
                networkWriter.Write(longNumber);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        // Loop to receive all the data sent by the client.


        // Shutdown and end connection
        Console.Out.WriteLine("closing connection");
        client.Close();
    }
}
catch (SocketException e)
{
    Console.WriteLine("SocketException: {0}", e);
}
finally
{
    // Stop listening for new clients.
    server.Stop();
}

Console.WriteLine("\nHit enter to continue...");
Console.Read();