using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using VoDA.FtpServer.Enums;

namespace VoDA.FtpServer.Extensions
{
    internal static class StreamExtension
    {
        public static long CopyToStream(this Stream input, Stream output, int bufferSize, TransferType transferType,
            CancellationToken token, long startIndex = 0, Action<long, long>? progressEvent = null)
        {
            int count = 0;
            long total = 0;
            ulong iterations = 0;
            bool tenMultiplierApplied = false;
            bool hunderedMultiplierApplied = false;
            if (input.CanSeek)
                input.Seek(startIndex, SeekOrigin.Begin);
            if (transferType == TransferType.Image)
            {
                //while (!token.IsCancellationRequested && (count = input.Read(buffer, 0, buffer.Length)) > 0)
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var buffer = new byte[bufferSize];
                        if ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                        {

                            if (iterations > 100 && tenMultiplierApplied == false)
                            {
                                bufferSize = bufferSize * 10;
                                tenMultiplierApplied = true;
                            }
                            if (iterations > 1000 && hunderedMultiplierApplied == false)
                            {
                                bufferSize = bufferSize * 10;
                                hunderedMultiplierApplied = true;
                            }
                            output.Write(buffer, 0, count);
                            output.Flush();
                            total += count;
                            progressEvent?.Invoke(input.CanSeek ? input.Length : 0, total);
                            iterations++;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            else
            {
                var buffer = new char[bufferSize];
                using var readStream = new StreamReader(input, Encoding.ASCII);
                using var writeStream = new StreamWriter(output, Encoding.ASCII);
                while (!token.IsCancellationRequested &&
                       (count = readStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writeStream.Write(buffer, 0, count);
                    total += count;
                    progressEvent?.Invoke(input.CanSeek ? input.Length : 0, total);
                }
            }

            return total;
        }
    }
}