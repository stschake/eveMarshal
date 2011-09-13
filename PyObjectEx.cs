using System.Collections.Generic;
using System.IO;

namespace eveMarshal
{

    public class PyObjectEx : PyObject
    {
        private const byte PackedTerminator = 0x2D;

        public bool IsType2 { get; private set; }
        public PyObject Header { get; private set; }
        public Dictionary<PyObject, PyObject> Dictionary { get; private set; }
        public List<PyObject> List { get; private set; }

        public PyObjectEx(bool isType2, PyObject header)
            : base(PyObjectType.ObjectEx)
        {
            IsType2 = isType2;
            Header = header;
            Dictionary = new Dictionary<PyObject, PyObject>();
        }

        public PyObjectEx()
            : base(PyObjectType.ObjectEx)
        {
            
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            if (op == MarshalOpcode.ObjectEx2)
                IsType2 = true;

            Dictionary = new Dictionary<PyObject, PyObject>();
            List = new List<PyObject>();
            Header = context.ReadObject(source);

            while (source.BaseStream.Position < source.BaseStream.Length)
            {
                var b = source.ReadByte();
                if (b == PackedTerminator)
                    break;
                source.BaseStream.Seek(-1, SeekOrigin.Current);
                List.Add(context.ReadObject(source));
            }

            while (source.BaseStream.Position < source.BaseStream.Length)
            {
                var b = source.ReadByte();
                if (b == PackedTerminator)
                    break;
                source.BaseStream.Seek(-1, SeekOrigin.Current);
                var key = context.ReadObject(source);
                var value = context.ReadObject(source);
                Dictionary.Add(key, value);
            }
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(IsType2 ? MarshalOpcode.ObjectEx2 : MarshalOpcode.ObjectEx1);
            Header.Encode(output);
            // list
            output.Write(PackedTerminator);
            // dict
            output.Write(PackedTerminator);
        }
    }

}