using System.Collections.Generic;

namespace XR.Std.Net.MessagePack
{
    public class MsgPackArray
    {
        readonly List<MsgPack> children;
        readonly MsgPack owner;

        public MsgPackArray(MsgPack msgpackObj, List<MsgPack> listObj)
        {
            owner = msgpackObj;
            children = listObj;
        }

        public MsgPack Add()
        {
            return owner.AddArrayChild();
        }

        public MsgPack Add(string value)
        {
            MsgPack obj = owner.AddArrayChild();
            obj.AsString = value;
            return obj;
        }

        public MsgPack Add(long value)
        {
            MsgPack obj = owner.AddArrayChild();
            obj.SetAsInteger(value);
            return obj;
        }

        public MsgPack Add(double value)
        {
            MsgPack obj = owner.AddArrayChild();
            obj.SetAsFloat(value);
            return obj;
        }

        public MsgPack this[int index]
        {
            get { return children[index]; }
        }

        public int Length
        {
            get { return children.Count; }
        }
    }
}
