namespace iii;

public record HandshakeJson(Version Version, Players Players,Description Description);

public record Version(string Name, int Protocol);

public record Players(int Max, int Online, IEnumerable<Player> Sample);

public record Player(string Name, Guid Uuid);

public record Description(string Text);