﻿using System;
using System.Threading;
namespace AsuncionIpfs.Utils
{
    public class HttpRequestIdentifier
    {
        // Base32 encoding - in ascii sort order for easy text based sorting 
        private static readonly string s_encode32Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUV";
        // Seed the _requestId for this application instance with 
        // the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001
        // for a roughly increasing _requestId over restarts
        private static long s_requestId = DateTime.UtcNow.Ticks;

        private string _id;

        public string TraceIdentifier
        {
            get
            {
                // Don't incur the cost of generating the request ID until it's asked for
                if (_id == null)
                {
                    _id = GenerateRequestId(Interlocked.Increment(ref s_requestId));
                }
                return _id;
            }
            set => _id = value;
        }

        private static unsafe string GenerateRequestId(long id)
        {
            // The following routine is ~310% faster than calling long.ToString() on x64
            // and ~600% faster than calling long.ToString() on x86 in tight loops of 1 million+ iterations
            // See: https://github.com/aspnet/Hosting/pull/385

            // stackalloc to allocate array on stack rather than heap
            char* charBuffer = stackalloc char[13];

            charBuffer[0] = s_encode32Chars[(int)(id >> 60) & 31];
            charBuffer[1] = s_encode32Chars[(int)(id >> 55) & 31];
            charBuffer[2] = s_encode32Chars[(int)(id >> 50) & 31];
            charBuffer[3] = s_encode32Chars[(int)(id >> 45) & 31];
            charBuffer[4] = s_encode32Chars[(int)(id >> 40) & 31];
            charBuffer[5] = s_encode32Chars[(int)(id >> 35) & 31];
            charBuffer[6] = s_encode32Chars[(int)(id >> 30) & 31];
            charBuffer[7] = s_encode32Chars[(int)(id >> 25) & 31];
            charBuffer[8] = s_encode32Chars[(int)(id >> 20) & 31];
            charBuffer[9] = s_encode32Chars[(int)(id >> 15) & 31];
            charBuffer[10] = s_encode32Chars[(int)(id >> 10) & 31];
            charBuffer[11] = s_encode32Chars[(int)(id >> 5) & 31];
            charBuffer[12] = s_encode32Chars[(int)id & 31];

            // string ctor overload that takes char*
            return new string(charBuffer, 0, 13);
        }
    }
}
