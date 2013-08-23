using System;

namespace FCG.LoadTester
{
    public class Response
    {
        public Uri Url;
        public ResponseCodes CodeType;
        public ResponseDataTypes DataType;
        public dynamic DataBag;
        public byte[] DataBytes;
        public string DataString;
    }
}