using System.IO;
using System.Text;

namespace FakeStatsd
{
    public class StreamString
    {
        private readonly Stream _ioStream;
        private readonly UnicodeEncoding _streamEncoding;

        public StreamString(Stream ioStream)
        {
            this._ioStream = ioStream;
            _streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            var len = 0;

            len = _ioStream.ReadByte() * 256;
            len += _ioStream.ReadByte();
            var inBuffer = new byte[len];
            _ioStream.Read(inBuffer, 0, len);

            return _streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = _streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > ushort.MaxValue)
            {
                len = (int)ushort.MaxValue;
            }

            _ioStream.WriteByte((byte)(len / 256));
            _ioStream.WriteByte((byte)(len & 255));
            _ioStream.Write(outBuffer, 0, len);
            _ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
