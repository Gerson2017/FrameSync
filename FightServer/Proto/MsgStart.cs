//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: proto/MsgStart.proto

    [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"MsgStart")]
    public partial class MsgStart : global::ProtoBuf.IExtensible
    {
        public MsgStart() {}
    
        private string _protoName = @"MsgStart";
        [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"protoName", DataFormat = global::ProtoBuf.DataFormat.Default)]
        [global::System.ComponentModel.DefaultValue(@"MsgStart")]
        public string protoName
        {
            get { return _protoName; }
            set { _protoName = value; }
        }
        private bool _res = default(bool);
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"res", DataFormat = global::ProtoBuf.DataFormat.Default)]
        [global::System.ComponentModel.DefaultValue(default(bool))]
        public bool res
        {
            get { return _res; }
            set { _res = value; }
        }
        private readonly global::System.Collections.Generic.List<uint> _guid = new global::System.Collections.Generic.List<uint>();
        [global::ProtoBuf.ProtoMember(3, Name=@"guid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public global::System.Collections.Generic.List<uint> guid
        {
            get { return _guid; }
        }
  
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
