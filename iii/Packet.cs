namespace iii;

public class Packet
{
    public int Length { get; set; }
    public PacketId Id { get; set; }
    public byte[] Data { get; set; }

    public MemoryStream DataStream { get; set; }

    public BinaryReader Reader { get; set; }
    
 
    public bool Write(BinaryWriter binaryWriter)    
    {
        binaryWriter.Write7BitEncodedInt(Length);
        binaryWriter.Write7BitEncodedInt((int)Id);
        binaryWriter.Write(Data);
        return true;
    }
    
    public static Packet Read(BinaryReader binaryReader, bool isRequestPacket = false)
    {
        var packet = new Packet();
        packet.Length = binaryReader.Read7BitEncodedInt();
        packet.Id = (PacketId)binaryReader.Read7BitEncodedInt();
        if (!isRequestPacket)
        {
            packet.Data = binaryReader.ReadBytes(packet.Length-1);
        }
        packet.DataStream = new MemoryStream(packet.Data);
        packet.Reader = new BinaryReader(packet.DataStream);
        return packet;
    }
}