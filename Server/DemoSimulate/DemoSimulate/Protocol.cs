//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: Protocol.proto
namespace Protocol
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"CardInfo")]
  public partial class CardInfo : global::ProtoBuf.IExtensible
  {
    public CardInfo() {}
    
    private int _num;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"num", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int num
    {
      get { return _num; }
      set { _num = value; }
    }
    private CardType _type;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"type", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public CardType type
    {
      get { return _type; }
      set { _type = value; }
    }
    private int _weight;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"weight", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int weight
    {
      get { return _weight; }
      set { _weight = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"PlayerRoomInfo")]
  public partial class PlayerRoomInfo : global::ProtoBuf.IExtensible
  {
    public PlayerRoomInfo() {}
    
    private int _id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int id
    {
      get { return _id; }
      set { _id = value; }
    }
    private readonly global::System.Collections.Generic.List<CardInfo> _cards = new global::System.Collections.Generic.List<CardInfo>();
    [global::ProtoBuf.ProtoMember(2, Name=@"cards", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<CardInfo> cards
    {
      get { return _cards; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"PlayCards")]
  public partial class PlayCards : global::ProtoBuf.IExtensible
  {
    public PlayCards() {}
    
    private readonly global::System.Collections.Generic.List<CardInfo> _cards = new global::System.Collections.Generic.List<CardInfo>();
    [global::ProtoBuf.ProtoMember(1, Name=@"cards", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<CardInfo> cards
    {
      get { return _cards; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
    [global::ProtoBuf.ProtoContract(Name=@"CardType")]
    public enum CardType
    {
            
      [global::ProtoBuf.ProtoEnum(Name=@"HeiTao", Value=0)]
      HeiTao = 0,
            
      [global::ProtoBuf.ProtoEnum(Name=@"HongTao", Value=1)]
      HongTao = 1,
            
      [global::ProtoBuf.ProtoEnum(Name=@"MeiHua", Value=2)]
      MeiHua = 2,
            
      [global::ProtoBuf.ProtoEnum(Name=@"FangKuai", Value=3)]
      FangKuai = 3,
            
      [global::ProtoBuf.ProtoEnum(Name=@"PowerW", Value=4)]
      PowerW = 4,
            
      [global::ProtoBuf.ProtoEnum(Name=@"SmallW", Value=5)]
      SmallW = 5
    }
  
}