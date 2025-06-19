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
        //public static long CopyToStream(this Stream input, Stream output, int bufferSize, TransferType transferType,
        //    CancellationToken token, long startIndex = 0, Action<long, long>? progressEvent = null)
        //{
        //    int count = 0;
        //    long total = 0;
        //    if (input.CanSeek)
        //        input.Seek(startIndex, SeekOrigin.Begin);
        //    if (transferType == TransferType.Image)
        //    {
        //        var buffer = new byte[bufferSize];
        //        while (!token.IsCancellationRequested && (count = input.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            try
        //            {
        //                output.Write(buffer, 0, count);
        //                output.Flush();
        //                total += count;
        //                progressEvent?.Invoke(input.CanSeek ? input.Length : 0, total);
        //            }
        //            catch
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var buffer = new char[bufferSize];
        //        using var readStream = new StreamReader(input, Encoding.ASCII);
        //        using var writeStream = new StreamWriter(output, Encoding.ASCII);
        //        while (!token.IsCancellationRequested &&
        //               (count = readStream.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            writeStream.Write(buffer, 0, count);
        //            total += count;
        //            progressEvent?.Invoke(input.CanSeek ? input.Length : 0, total);
        //        }
        //    }

        //    return total;
        //}
        public static long CopyToStream(this Stream input, Stream output, TransferType transferType,
            CancellationToken token, long startIndex = 0, Action<long, long>? progressEvent = null)
        {
            int count = 0;
            long total = 0;
            ulong iterations = 0;
            if (input.CanSeek)
            {
                input.Seek(startIndex, SeekOrigin.Begin);
            }
            if (transferType == TransferType.Image)
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        long bufferSize = GetBufferSizeByIteration(iterations);
                        var buffer = new byte[bufferSize];
                        if ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, count);
                            output.Flush();
                            total += count;
                            progressEvent?.Invoke(input.CanSeek ? input.Length : 0, total);
                            iterations++;
                        }
                        else
                        {
                            break;
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
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        long bufferSize = GetBufferSizeByIteration(iterations);
                        var buffer = new char[bufferSize];
                        using (var readStream = new StreamReader(input, Encoding.ASCII))
                        {
                            using (var writeStream = new StreamWriter(output, Encoding.ASCII))
                            {
                                if ((count = readStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    writeStream.Write(buffer, 0, count);
                                    total += count;
                                    progressEvent?.Invoke(input.CanSeek ? input.Length : 0, total);
                                    iterations++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            return total;
        }

        private static long GetBufferSizeByIteration(ulong iterations)
        {
            long bufferSize = 4096; // 4KB initial buffer size
            if (iterations == 100)
            {
                bufferSize = bufferSize * 10;
            }
            if (iterations == 1000)
            {
                bufferSize = bufferSize * 10;
            }

            return bufferSize;
        }
    }
}