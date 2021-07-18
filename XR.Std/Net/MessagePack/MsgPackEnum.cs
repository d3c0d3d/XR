using System.Collections;
using System.Collections.Generic;

namespace XR.Std.Net.MessagePack
{
    public class MsgPackEnum : IEnumerator
    {
        readonly List<MsgPack> children;
        int position = -1;

        public MsgPackEnum(List<MsgPack> obj)
        {
            children = obj;
        }
        object IEnumerator.Current
        {
            get { return children[position]; }
        }

        bool IEnumerator.MoveNext()
        {
            position++;
            return (position < children.Count);
        }

        void IEnumerator.Reset()
        {
            position = -1;
        }
    }
}
