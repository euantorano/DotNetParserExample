using System;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace ParserExample
{
    /// <summary>
    /// Result values when parsing a packet.
    /// </summary>
    public enum ParseResult
    {
        Partial,
        Complete,
        Error
    }

    public static class PacketParser
    {
        public static ParseResult ParseBinaryPacket(
            in ReadOnlySpan<byte> buff,
            out Packet? packet,
            out int bytesConsumed
        )
        {
            bytesConsumed = 0;

            // packets start with a SOH byte - if there is no such byte in the buffer, then we don't have a packet.
            int indexOfSoh = buff.IndexOf(Packet.Soh);

            if (indexOfSoh == -1)
            {
                bytesConsumed = buff.Length;

                packet = default;
                return ParseResult.Partial;
            }

            // slice past the soh, as we do not wish to include it
            ReadOnlySpan<byte> b = buff[(indexOfSoh + 1)..];

            if (b.Length < sizeof(int) * 2)
            {
                // the packet header is two integers encoded as little endian - this means a packet must be at least 8 bytes long
                packet = default;
                return ParseResult.Partial;
            }

            int commandLength = BinaryPrimitives.ReadInt32LittleEndian(b);
            int dataLength = BinaryPrimitives.ReadInt32LittleEndian(b[sizeof(int)..]);

            b = b[(sizeof(int) * 2)..];

            if (b.Length < commandLength)
            {
                // not enough data for the command
                packet = default;
                return ParseResult.Partial;
            }

            string command = Encoding.UTF8.GetString(b[..commandLength]);

            b = b[commandLength..];

            if (b.Length < dataLength)
            {
                // not enough data for the data
                packet = default;
                return ParseResult.Partial;
            }

            string data = Encoding.UTF8.GetString(b[..dataLength]);

            b = b[dataLength..];

            packet = new Packet(commandLength, dataLength, command, data);
            bytesConsumed = buff.Length - b.Length;
            return ParseResult.Complete;
        }

        public static ParseResult ParseBinaryPacket(
            in byte[] buff,
            in int offset,
            in int length,
            out Packet? packet,
            out int bytesConsumed
        )
        {
            bytesConsumed = 0;

            // packets start with a SOH byte - if there is no such byte in the buffer, then we don't have a packet.
            int indexOfSoh = Array.IndexOf(buff, Packet.Soh, offset, length);

            if (indexOfSoh == -1)
            {
                bytesConsumed = length;

                packet = default;
                return ParseResult.Partial;
            }

            if (length < indexOfSoh + 9)
            {
                // the packet header is two integers encoded as little endian following a SOH byte - this means a packet must be at least 9 bytes long
                packet = default;
                return ParseResult.Partial;
            }

            if (!BitConverter.IsLittleEndian)
            {
                // flip the bits as the system is not little endian
                Array.Reverse(buff, indexOfSoh + 1, sizeof(int));
            }

            int commandLength = BitConverter.ToInt32(buff, indexOfSoh + 1);

            if (!BitConverter.IsLittleEndian)
            {
                // flip the bits as the system is not little endian
                Array.Reverse(buff, indexOfSoh + 1 + sizeof(int), sizeof(int));
            }

            int dataLength = BitConverter.ToInt32(buff, indexOfSoh + 1 + sizeof(int));

            if (length < indexOfSoh + 9 + commandLength + dataLength)
            {
                // not enough data for the command and data
                packet = default;
                return ParseResult.Partial;
            }

            string command = Encoding.UTF8.GetString(buff, indexOfSoh + ((2 * sizeof(int)) + 1), commandLength);

            string data = Encoding.UTF8.GetString(buff, indexOfSoh + ((2 * sizeof(int)) + 1) + commandLength, dataLength);

            packet = new Packet(commandLength, dataLength, command, data);
            bytesConsumed = indexOfSoh + ((2 * sizeof(int)) + 1) + commandLength + dataLength;
            return ParseResult.Complete;
        }

        public static ParseResult ParseTextPacket(
            in ReadOnlySpan<byte> buff,
            out Packet? packet,
            out string? error,
            out int bytesConsumed
        )
        {
            bytesConsumed = 0;

            // packets start with a SOH byte - if there is no such byte in the buffer, then we don't have a packet.
            int indexOfSoh = buff.IndexOf(Packet.Soh);

            if (indexOfSoh == -1)
            {
                bytesConsumed = buff.Length;

                packet = default;
                error = default;
                return ParseResult.Partial;
            }

            // slice past the soh, as we do not wish to include it
            ReadOnlySpan<byte> b = buff[(indexOfSoh + 1)..];

            if (b.Length < 16)
            {
                // the packet header is two integers encoded as ASCII hex - each length is therefore 8 bytes long
                packet = default;
                error = default;
                return ParseResult.Partial;
            }

            if (!Utf8Parser.TryParse(b[..8], out int commandLength, out int consumed, 'X') || consumed != 8)
            {
                b = b[16..];
                bytesConsumed = buff.Length - b.Length;

                packet = default;
                error = "Failed to parse command length";
                return ParseResult.Error;
            }

            b = b[8..];

            if (!Utf8Parser.TryParse(b[..8], out int dataLength, out consumed, 'X') || consumed != 8)
            {
                b = b[8..];
                bytesConsumed = buff.Length - b.Length;

                packet = default;
                error = "Failed to parse data length";
                return ParseResult.Error;
            }

            b = b[8..];

            if (b.Length < commandLength)
            {
                // not enough data for the command
                packet = default;
                error = default;
                return ParseResult.Partial;
            }

            string command = Encoding.UTF8.GetString(b[..commandLength]);

            b = b[commandLength..];

            if (b.Length < dataLength)
            {
                // not enough data for the data
                packet = default;
                error = default;
                return ParseResult.Partial;
            }

            string data = Encoding.UTF8.GetString(b[..dataLength]);

            b = b[dataLength..];

            packet = new Packet(commandLength, dataLength, command, data);
            error = default;
            bytesConsumed = buff.Length - b.Length;
            return ParseResult.Complete;
        }

        public static ParseResult ParseTextPacket(
            byte[] buff,
            int offset,
            int length,
            out Packet? packet,
            out string? error,
            out int bytesConsumed
        )
        {
            bytesConsumed = 0;

            // packets start with a SOH byte - if there is no such byte in the buffer, then we don't have a packet.
            int indexOfSoh = Array.IndexOf(buff, Packet.Soh, offset, length);

            if (indexOfSoh == -1)
            {
                bytesConsumed = length;

                packet = default;
                error = default;
                return ParseResult.Partial;
            }

            if (length < indexOfSoh + 17)
            {
                // the packet header is two integers encoded as ASCII hex - each length is therefore 8 bytes long
                packet = default;
                error = default;
                return ParseResult.Partial;
            }

            string commandLengthString = Encoding.UTF8.GetString(buff, indexOfSoh + 1, 8);
            if (
                !int.TryParse(
                    commandLengthString,
                    NumberStyles.HexNumber,
                    NumberFormatInfo.InvariantInfo,
                    out int commandLength
                )
            )
            {
                bytesConsumed = indexOfSoh + 17;

                packet = default;
                error = "Failed to parse command length";
                return ParseResult.Error;
            }

            string dataLengthString = Encoding.UTF8.GetString(buff, indexOfSoh + 9, 8);
            if (
                !int.TryParse(
                    dataLengthString,
                    NumberStyles.HexNumber,
                    NumberFormatInfo.InvariantInfo,
                    out int dataLength
                )
            )
            {
                bytesConsumed = indexOfSoh + 17;

                packet = default;
                error = "Failed to parse data length";
                return ParseResult.Error;
            }

            if (length < indexOfSoh + 17 + commandLength + dataLength)
            {
                // not enough data for the command and data
                packet = default;
                error = default;
                return ParseResult.Partial;
            }

            string command = Encoding.UTF8.GetString(buff, indexOfSoh + 17, commandLength);

            string data = Encoding.UTF8.GetString(buff, indexOfSoh + 17 + commandLength, dataLength);

            packet = new Packet(commandLength, dataLength, command, data);
            error = default;
            bytesConsumed = indexOfSoh + 17 + commandLength + dataLength;
            return ParseResult.Complete;
        }
    }
}