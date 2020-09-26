using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace ParserExample.Tests
{
    [TestClass]
    public class PacketParserTests
    {
        [DataTestMethod]
        [DataRow(
            5,
            5,
            "hello",
            "world",
            19,
            true,
            new byte[] {
                1,
                5, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'h', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o',
                (byte) 'w', (byte) 'o', (byte) 'r', (byte) 'l', (byte) 'd'
            },
            DisplayName = "Parse packet with command and data using span"
        )]
        [DataRow(
            5,
            5,
            "hello",
            "world",
            19,
            false,
            new byte[] {
                1,
                5, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'h', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o',
                (byte) 'w', (byte) 'o', (byte) 'r', (byte) 'l', (byte) 'd'
            },
            DisplayName = "Parse packet with command and data using array"
        )]

        [DataRow(
            5,
            5,
            "hello",
            "world",
            21,
            true,
            new byte[] {
                0, 7,
                1,
                5, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'h', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o',
                (byte) 'w', (byte) 'o', (byte) 'r', (byte) 'l', (byte) 'd'
            },
            DisplayName = "Parse packet with command and data with extra data at the start using span"
        )]
        [DataRow(
            5,
            5,
            "hello",
            "world",
            21,
            false,
            new byte[] {
                0, 7,
                1,
                5, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'h', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o',
                (byte) 'w', (byte) 'o', (byte) 'r', (byte) 'l', (byte) 'd'
            },
            DisplayName = "Parse packet with command and data with extra data at the start using array"
        )]

        [DataRow(
            5,
            0,
            "hello",
            "",
            14,
            true,
            new byte[] {
                1,
                5, 0, 0, 0,
                0, 0, 0, 0,
                (byte) 'h', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o'
            },
            DisplayName = "Parse packet with command only using span"
        )]
        [DataRow(
            5,
            0,
            "hello",
            "",
            14,
            false,
            new byte[] {
                1,
                5, 0, 0, 0,
                0, 0, 0, 0,
                (byte) 'h', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o'
            },
            DisplayName = "Parse packet with command only using array"
        )]

        [DataRow(
            0,
            5,
            "",
            "world",
            14,
            true,
            new byte[] {
                1,
                0, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'w', (byte) 'o', (byte) 'r', (byte) 'l', (byte) 'd'
            },
            DisplayName = "Parse packet with command only using span"
        )]
        [DataRow(
            0,
            5,
            "",
            "world",
            14,
            false,
            new byte[] {
                1,
                0, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'w', (byte) 'o', (byte) 'r', (byte) 'l', (byte) 'd'
            },
            DisplayName = "Parse packet with data only using array"
        )]
        public void TestParseBinaryPacketComplete(
            int expectedCommandLength,
            int expectedDataLength,
            string expectedCommand,
            string expectedData,
            int expectedBytesConsumed,
            bool parseAsSpan,
            byte[] data
        )
        {
            Packet? packet;
            int bytesConsumed;

            if (parseAsSpan)
            {
                ReadOnlySpan<byte> buff = data;

                Assert.AreEqual(
                    ParseResult.Complete,
                    PacketParser.ParseBinaryPacket(buff, out packet, out bytesConsumed)
                );
            }
            else
            {
                Assert.AreEqual(
                    ParseResult.Complete,
                    PacketParser.ParseBinaryPacket(data, 0, data.Length, out packet, out bytesConsumed)
                );
            }

            Assert.IsTrue(packet.HasValue);

            Assert.AreEqual(expectedCommandLength, packet!.Value.CommandLength);
            Assert.AreEqual(expectedDataLength, packet.Value.DataLength);
            Assert.AreEqual(expectedCommand, packet.Value.Command);
            Assert.AreEqual(expectedData, packet.Value.Data);

            Assert.AreEqual(expectedBytesConsumed, bytesConsumed);
        }

        [DataTestMethod]
        [DataRow(
            0,
            true,
            new byte[] {},
            DisplayName = "Parse empty buffer using span"
        )]
        [DataRow(
            0,
            false,
            new byte[] { },
            DisplayName = "Parse empty buffer using array"
        )]

        [DataRow(
            0,
            true,
            new byte[]
            {
                1
            },
            DisplayName = "Parse buffer containing SOH only using span"
        )]
        [DataRow(
            0,
            false,
            new byte[]
            {
                1
            },
            DisplayName = "Parse buffer containing SOH only using array"
        )]

        [DataRow(
            0,
            true,
            new byte[]
            {
                1,
                5, 0, 0, 0,
                5, 0
            },
            DisplayName = "Parse buffer without full header only using span"
        )]
        [DataRow(
            0,
            false,
            new byte[]
            {
                1,
                5, 0, 0, 0,
                5, 0
            },
            DisplayName = "Parse buffer without full header only using array"
        )]

        [DataRow(
            0,
            true,
            new byte[]
            {
                1,
                5, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'h', (byte) 'e'
            },
            DisplayName = "Parse buffer without full command only using span"
        )]
        [DataRow(
            0,
            false,
            new byte[]
            {
                1,
                5, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'h', (byte) 'e'
            },
            DisplayName = "Parse buffer without full command only using array"
        )]

        [DataRow(
            0,
            true,
            new byte[]
            {
                1,
                5, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'h', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o',
                (byte) 'w'
            },
            DisplayName = "Parse buffer without full data only using span"
        )]
        [DataRow(
            0,
            false,
            new byte[]
            {
                1,
                5, 0, 0, 0,
                5, 0, 0, 0,
                (byte) 'h', (byte) 'e', (byte) 'l', (byte) 'l', (byte) 'o',
                (byte) 'w'
            },
            DisplayName = "Parse buffer without full data only using array"
        )]
        public void TestParseBinaryPacketPartial(
            int expectedBytesConsumed,
            bool parseAsSpan,
            byte[] data
        )
        {
            int bytesConsumed;

            if (parseAsSpan)
            {
                ReadOnlySpan<byte> buff = data;

                Assert.AreEqual(
                    ParseResult.Partial,
                    PacketParser.ParseBinaryPacket(buff, out _, out bytesConsumed)
                );
            }
            else
            {
                Assert.AreEqual(
                    ParseResult.Partial,
                    PacketParser.ParseBinaryPacket(data, 0, data.Length, out _, out bytesConsumed)
                );
            }

            Assert.AreEqual(expectedBytesConsumed, bytesConsumed);
        }

        [DataTestMethod]
        [DataRow(
            5,
            5,
            "hello",
            "world",
            27,
            true,
            "\x00010000000500000005helloworld",
            DisplayName = "Parse packet with command and data using span"
        )]
        [DataRow(
            5,
            5,
            "hello",
            "world",
            27,
            false,
            "\x00010000000500000005helloworld",
            DisplayName = "Parse packet with command and data using array"
        )]

        [DataRow(
            5,
            5,
            "hello",
            "world",
            33,
            true,
            "foobar\x00010000000500000005helloworld",
            DisplayName = "Parse packet with command and data with extra data at the start using span"
        )]
        [DataRow(
            5,
            5,
            "hello",
            "world",
            33,
            false,
            "foobar\x00010000000500000005helloworld",
            DisplayName = "Parse packet with command and data with extra data at the start using array"
        )]

        [DataRow(
            0,
            5,
            "",
            "world",
            22,
            true,
            "\x00010000000000000005world",
            DisplayName = "Parse packet with command only using span"
        )]
        [DataRow(
            0,
            5,
            "",
            "world",
            22,
            false,
            "\x00010000000000000005world",
            DisplayName = "Parse packet with command only using array"
        )]

        [DataRow(
            5,
            0,
            "hello",
            "",
            22,
            true,
            "\x00010000000500000000hello",
            DisplayName = "Parse packet with data only using span"
        )]
        [DataRow(
            5,
            0,
            "hello",
            "",
            22,
            false,
            "\x00010000000500000000hello",
            DisplayName = "Parse packet with data only using array"
        )]
        public void TestParseTextPacketComplete(
            int expectedCommandLength,
            int expectedDataLength,
            string expectedCommand,
            string expectedData,
            int expectedBytesConsumed,
            bool parseAsSpan,
            string data
        )
        {
            byte[] buff = Encoding.UTF8.GetBytes(data);

            Packet? packet;
            string? error;
            int bytesConsumed;

            if (parseAsSpan)
            {
                Assert.AreEqual(
                    ParseResult.Complete,
                    PacketParser.ParseTextPacket(buff, out packet, out error, out bytesConsumed)
                );
            }
            else
            {
                Assert.AreEqual(
                    ParseResult.Complete,
                    PacketParser.ParseTextPacket(buff, 0, data.Length, out packet, out error, out bytesConsumed)
                );
            }

            Assert.IsTrue(packet.HasValue);
            Assert.IsNull(error);

            Assert.AreEqual(expectedCommandLength, packet!.Value.CommandLength);
            Assert.AreEqual(expectedDataLength, packet.Value.DataLength);
            Assert.AreEqual(expectedCommand, packet.Value.Command);
            Assert.AreEqual(expectedData, packet.Value.Data);

            Assert.AreEqual(expectedBytesConsumed, bytesConsumed);
        }

        [DataTestMethod]
        [DataRow(
            0,
            true,
            "",
            DisplayName = "Parse empty buffer using span"
        )]
        [DataRow(
            0,
            false,
            "",
            DisplayName = "Parse empty buffer using array"
        )]

        [DataRow(
            0,
            true,
            "\x0001",
            DisplayName = "Parse buffer containing SOH only using span"
        )]
        [DataRow(
            0,
            false,
            "\x0001",
            DisplayName = "Parse buffer containing SOH only using array"
        )]

        [DataRow(
            0,
            true,
            "\x00010000000500",
            DisplayName = "Parse buffer without full header only using span"
        )]
        [DataRow(
            0,
            false,
            "\x00010000000500",
            DisplayName = "Parse buffer without full header only using array"
        )]

        [DataRow(
            0,
            true,
            "\x00010000000500000005he",
            DisplayName = "Parse buffer without full command only using span"
        )]
        [DataRow(
            0,
            false,
            "\x00010000000500000005he",
            DisplayName = "Parse buffer without full command only using array"
        )]

        [DataRow(
            0,
            true,
            "\x00010000000500000005hellow",
            DisplayName = "Parse buffer without full data only using span"
        )]
        [DataRow(
            0,
            false,
            "\x00010000000500000005hellow",
            DisplayName = "Parse buffer without full data only using array"
        )]
        public void TestParseTextPacketPartial(
            int expectedBytesConsumed,
            bool parseAsSpan,
            string data
        )
        {
            byte[] buff = Encoding.UTF8.GetBytes(data);

            string? error;
            int bytesConsumed;

            if (parseAsSpan)
            {
                Assert.AreEqual(
                    ParseResult.Partial,
                    PacketParser.ParseTextPacket(buff, out _, out error, out bytesConsumed)
                );
            }
            else
            {
                Assert.AreEqual(
                    ParseResult.Partial,
                    PacketParser.ParseTextPacket(buff, 0, buff.Length, out _, out error, out bytesConsumed)
                );
            }

            Assert.IsNull(error);

            Assert.AreEqual(expectedBytesConsumed, bytesConsumed);
        }

        [DataTestMethod]
        [DataRow(
            "Failed to parse command length",
            17,
            true,
            "\x0001000000!500000005",
            DisplayName = "Parse buffer containing invalid command length as span")]
        [DataRow(
            "Failed to parse command length",
            17,
            false,
            "\x0001000000!500000005",
            DisplayName = "Parse buffer containing invalid command length as array")]

        [DataRow(
            "Failed to parse data length",
            17,
            true,
            "\x000100000005000000-5",
            DisplayName = "Parse buffer containing invalid data length as span")]
        [DataRow(
            "Failed to parse data length",
            17,
            false,
            "\x000100000005000000-5",
            DisplayName = "Parse buffer containing invalid data length as array")]
        public void TestParseTextPacketError(
            string expectedError,
            int expectedBytesConsumed,
            bool parseAsSpan,
            string data
        )
        {
            byte[] buff = Encoding.UTF8.GetBytes(data);

            string? error;
            int bytesConsumed;

            if (parseAsSpan)
            {
                Assert.AreEqual(
                    ParseResult.Error,
                    PacketParser.ParseTextPacket(buff, out _, out error, out bytesConsumed)
                );
            }
            else
            {
                Assert.AreEqual(
                    ParseResult.Error,
                    PacketParser.ParseTextPacket(buff, 0, buff.Length, out _, out error, out bytesConsumed)
                );
            }

            Assert.AreEqual(expectedError, error);

            Assert.AreEqual(expectedBytesConsumed, bytesConsumed);
        }
    }
}
