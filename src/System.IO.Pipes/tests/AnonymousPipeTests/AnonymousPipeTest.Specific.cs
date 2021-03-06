﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.IO.Pipes.Tests
{
    /// <summary>
    /// The Specific AnonymousPipe tests cover edge cases or otherwise narrow cases that
    /// show up within particular server/client directional combinations.
    /// </summary>
    public class AnonymousPipeTest_Specific : AnonymousPipeTestBase
    {
        [Fact]
        public static void DisposeLocalCopyOfClientHandle_BeforeServerRead()
        {
            using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(PipeDirection.In))
            {
                using (AnonymousPipeClientStream client = new AnonymousPipeClientStream(PipeDirection.Out, server.ClientSafePipeHandle))
                {
                    byte[] sent = new byte[] { 123 };
                    byte[] received = new byte[] { 0 };
                    client.Write(sent, 0, 1);

                    server.DisposeLocalCopyOfClientHandle();

                    Assert.Equal(1, server.Read(received, 0, 1));
                    Assert.Equal(sent[0], received[0]);
                }
            }
        }

        [Fact]
        public static void ClonedServer_ActsAsOriginalServer()
        {
            using (AnonymousPipeServerStream serverBase = new AnonymousPipeServerStream(PipeDirection.Out))
            {
                using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(PipeDirection.Out, serverBase.SafePipeHandle, serverBase.ClientSafePipeHandle))
                {
                    Assert.True(server.IsConnected);
                    using (AnonymousPipeClientStream client = new AnonymousPipeClientStream(PipeDirection.In, server.GetClientHandleAsString()))
                    {
                        Assert.True(server.IsConnected);
                        Assert.True(client.IsConnected);

                        byte[] sent = new byte[] { 123 };
                        byte[] received = new byte[] { 0 };
                        server.Write(sent, 0, 1);

                        Assert.Equal(1, client.Read(received, 0, 1));
                        Assert.Equal(sent[0], received[0]);
                    }
                    Assert.Throws<IOException>(() => server.WriteByte(5));
                }
            }
        }

        [Fact]
        public static void ClonedClient_ActsAsOriginalClient()
        {
            using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(PipeDirection.Out))
            {
                using (AnonymousPipeClientStream clientBase = new AnonymousPipeClientStream(PipeDirection.In, server.GetClientHandleAsString()))
                {
                    using (AnonymousPipeClientStream client = new AnonymousPipeClientStream(PipeDirection.In, clientBase.SafePipeHandle))
                    {
                        Assert.True(server.IsConnected);
                        Assert.True(client.IsConnected);

                        byte[] sent = new byte[] { 123 };
                        byte[] received = new byte[] { 0 };
                        server.Write(sent, 0, 1);

                        Assert.Equal(1, client.Read(received, 0, 1));
                        Assert.Equal(sent[0], received[0]);
                    }
                }
            }
        }

        [Fact]
        [PlatformSpecific(PlatformID.Linux)]
        public static void Linux_BufferSizeRoundtrips()
        {
            // On Linux, setting the buffer size of the server will also set the buffer size of the
            // client, regardless of the direction of the flow

            int desiredBufferSize;
            using (var server = new AnonymousPipeServerStream(PipeDirection.Out))
            {
                desiredBufferSize = server.OutBufferSize * 2;
                Assert.True(desiredBufferSize > 0);
            }

            using (var server = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.None, desiredBufferSize))
            using (var client = new AnonymousPipeClientStream(PipeDirection.In, server.ClientSafePipeHandle))
            {
                Assert.Equal(desiredBufferSize, server.OutBufferSize);
                Assert.Equal(desiredBufferSize, client.InBufferSize);
            }

            using (var server = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.None, desiredBufferSize))
            using (var client = new AnonymousPipeClientStream(PipeDirection.Out, server.ClientSafePipeHandle))
            {
                Assert.Equal(desiredBufferSize, server.InBufferSize);
                Assert.Equal(desiredBufferSize, client.OutBufferSize);
            }
        }

        [Fact]
        [PlatformSpecific(~PlatformID.Linux)]
        public static void BufferSizeRoundtripping()
        {
            // On systems other than Linux, setting the buffer size of the server will only set
            // set the buffer size of the client if the flow of the pipe is towards the client i.e.
            // the client is defined with PipeDirection.In

            int desiredBufferSize = 10;
            using (var server = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.None, desiredBufferSize))
            using (var client = new AnonymousPipeClientStream(PipeDirection.In, server.ClientSafePipeHandle))
            {
                Assert.Equal(desiredBufferSize, server.OutBufferSize);
                Assert.Equal(desiredBufferSize, client.InBufferSize);
            }

            using (var server = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.None, desiredBufferSize))
            using (var client = new AnonymousPipeClientStream(PipeDirection.Out, server.ClientSafePipeHandle))
            {
                Assert.Equal(desiredBufferSize, server.InBufferSize);
                Assert.Equal(0, client.OutBufferSize);
            }
        }

        [Fact]
        public void PipeTransmissionMode_Returns_Byte()
        {
            using (ServerClientPair pair = CreateServerClientPair())
            {
                Assert.Equal(PipeTransmissionMode.Byte, pair.writeablePipe.TransmissionMode);
                Assert.Equal(PipeTransmissionMode.Byte, pair.readablePipe.TransmissionMode);
            }
        }

        [Theory]
        [InlineData(PipeDirection.Out, PipeDirection.In)]
        [InlineData(PipeDirection.In, PipeDirection.Out)]
        public void ReadModeToByte_Accepted(PipeDirection serverDirection, PipeDirection clientDirection)
        {
            using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(serverDirection))
            using (AnonymousPipeClientStream client = new AnonymousPipeClientStream(clientDirection, server.GetClientHandleAsString()))
            {
                server.ReadMode = PipeTransmissionMode.Byte;
                client.ReadMode = PipeTransmissionMode.Byte;
                Assert.Equal(PipeTransmissionMode.Byte, server.ReadMode);
                Assert.Equal(PipeTransmissionMode.Byte, client.ReadMode);
            }
        }

        [Fact]
        public void MessageReadMode_Throws_NotSupportedException()
        {
            using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(PipeDirection.Out))
            using (AnonymousPipeClientStream client = new AnonymousPipeClientStream(PipeDirection.In, server.GetClientHandleAsString()))
            {
                Assert.Throws<NotSupportedException>(() => server.ReadMode = PipeTransmissionMode.Message);
                Assert.Throws<NotSupportedException>(() => client.ReadMode = PipeTransmissionMode.Message);
            }
        }

        [Fact]
        public void InvalidReadMode_Throws_ArgumentOutOfRangeException()
        {
            using (AnonymousPipeServerStream server = new AnonymousPipeServerStream(PipeDirection.Out))
            using (AnonymousPipeClientStream client = new AnonymousPipeClientStream(PipeDirection.In, server.GetClientHandleAsString()))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => server.ReadMode = (PipeTransmissionMode)999);
                Assert.Throws<ArgumentOutOfRangeException>(() => client.ReadMode = (PipeTransmissionMode)999);
            }
        }
    }
}
