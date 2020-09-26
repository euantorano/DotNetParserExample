namespace ParserExample
{
    /// <summary>
    /// A simple example packet, consisting of a "command" and "data", both of which are strings.
    /// </summary>
    public readonly struct Packet
    {
        public const byte Soh = 1;

        /// <summary>
        /// The length of the <see cref="Command"/> in UTF-8 encoded bytes.
        /// </summary>
        public readonly int CommandLength;

        /// <summary>
        /// The length of the <see cref="Data"/> in UTF-8 encoded bytes.
        /// </summary>
        public readonly int DataLength;

        /// <summary>
        /// The command for the packet.
        /// </summary>
        public readonly string Command;

        /// <summary>
        /// The data for the packet.
        /// </summary>
        public readonly string Data;

        public Packet(int commandLength, int dataLength, string command, string data)
        {
            CommandLength = commandLength;
            DataLength = dataLength;
            Command = command;
            Data = data;
        }
    }
}