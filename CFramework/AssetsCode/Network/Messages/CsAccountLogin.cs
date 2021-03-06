// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::FlatBuffers;

public struct CsAccountLogin : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static CsAccountLogin GetRootAsCsAccountLogin(ByteBuffer _bb) { return GetRootAsCsAccountLogin(_bb, new CsAccountLogin()); }
  public static CsAccountLogin GetRootAsCsAccountLogin(ByteBuffer _bb, CsAccountLogin obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public CsAccountLogin __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public long UserID { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetLong(o + __p.bb_pos) : (long)0; } }

  public static Offset<CsAccountLogin> CreateCsAccountLogin(FlatBufferBuilder builder,
      long userID = 0) {
    builder.StartObject(1);
    CsAccountLogin.AddUserID(builder, userID);
    return CsAccountLogin.EndCsAccountLogin(builder);
  }

  public static void StartCsAccountLogin(FlatBufferBuilder builder) { builder.StartObject(1); }
  public static void AddUserID(FlatBufferBuilder builder, long userID) { builder.AddLong(0, userID, 0); }
  public static Offset<CsAccountLogin> EndCsAccountLogin(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<CsAccountLogin>(o);
  }
};

