using UnityEngine;
using System.Text;

namespace SG
{
    /// <summary>
    /// MemoryStream.ReadLine has an interesting oddity: it doesn't always advance the stream's position by the correct amount:
    /// http://social.msdn.microsoft.com/Forums/en-AU/Vsexpressvcs/thread/b8f7837b-e396-494e-88e1-30547fcf385f
    /// Solution? Custom line reader with the added benefit of not having to use streams at all.
    /// </summary>
    public class ByteReader
    {
        static string ReadLine(byte[] buffer, int start, int count) { return Encoding.UTF8.GetString(buffer, start, count); }

        static BetterList<string> mTemp = new BetterList<string>();

        byte[] mBuffer;
        int mOffset = 0;

        public ByteReader(byte[] bytes) { mBuffer = bytes; }
        public ByteReader(TextAsset asset) { mBuffer = asset.bytes; }

        /// <summary>
        /// Read a single line of Comma-Separated Values from the file.
        /// </summary>
        public BetterList<string> GetRow()
        {
            mTemp.Clear();
            string line = "";
            bool insideQuotes = false;
            int wordStart = 0;

            while (canRead)
            {
                if (insideQuotes)
                {
                    string s = ReadLine(false);
                    if (s == null) return null;
                    s = s.Replace("\\n", "\n");
                    line += "\n" + s;
                }
                else
                {
                    line = ReadLine(true);
                    if (line == null) return null;
                    line = line.Replace("\\n", "\n");
                    wordStart = 0;
                }

                for (int i = wordStart, imax = line.Length; i < imax; ++i)
                {
                    char ch = line[i];

                    if (ch == ',')
                    {
                        if (!insideQuotes)
                        {
                            mTemp.Add(line.Substring(wordStart, i - wordStart));
                            wordStart = i + 1;
                        }
                    }
                    else if (ch == '"')
                    {
                        if (insideQuotes)
                        {
                            if (i + 1 >= imax)
                            {
                                mTemp.Add(line.Substring(wordStart, i - wordStart).Replace("\"\"", "\""));
                                return mTemp;
                            }

                            if (line[i + 1] != '"')
                            {
                                mTemp.Add(line.Substring(wordStart, i - wordStart).Replace("\"\"", "\""));
                                insideQuotes = false;

                                if (line[i + 1] == ',')
                                {
                                    ++i;
                                    wordStart = i + 1;
                                }
                            }
                            else ++i;
                        }
                        else
                        {
                            wordStart = i + 1;
                            insideQuotes = true;
                        }
                    }
                }

                if (wordStart < line.Length)
                {
                    if (insideQuotes) continue;
                    mTemp.Add(line.Substring(wordStart, line.Length - wordStart));
                }
                return mTemp;
            }
            return null;
        }

        bool canRead => mBuffer != null && mOffset < mBuffer.Length;

        string ReadLine(bool skipEmptyLines)
        {
            int max = mBuffer.Length;

            // Skip empty characters
            if (skipEmptyLines)
                while (mOffset < max && mBuffer[mOffset] < 32) ++mOffset;

            int end = mOffset;

            if (end < max)
            {
                for (; ; )
                {
                    if (end < max)
                    {
                        int ch = mBuffer[end++];
                        if (ch != '\n' && ch != '\r') continue;
                    }
                    else ++end;

                    string line = ReadLine(mBuffer, mOffset, end - mOffset - 1);
                    mOffset = end;
                    return line;
                }
            }
            mOffset = max;
            return null;
        }
    }
}