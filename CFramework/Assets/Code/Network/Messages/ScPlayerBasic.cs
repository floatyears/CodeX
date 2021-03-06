// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

using global::System;
using global::FlatBuffers;

public struct ScPlayerBasic : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static ScPlayerBasic GetRootAsScPlayerBasic(ByteBuffer _bb) { return GetRootAsScPlayerBasic(_bb, new ScPlayerBasic()); }
  public static ScPlayerBasic GetRootAsScPlayerBasic(ByteBuffer _bb, ScPlayerBasic obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public ScPlayerBasic __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int MsgID { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)10004; } }
  public int Level { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Name { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public string Test { get { int o = __p.__offset(10); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
  public ArraySegment<byte>? GetTestBytes() { return __p.__vector_as_arraysegment(10); }
  public double Double_ { get { int o = __p.__offset(12); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)1.0; } }
  public float Float_ { get { int o = __p.__offset(14); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)1000.0f; } }
  public short Short_ { get { int o = __p.__offset(16); return o != 0 ? __p.bb.GetShort(o + __p.bb_pos) : (short)12; } }

  public static Offset<ScPlayerBasic> CreateScPlayerBasic(FlatBufferBuilder builder,
      int msgID = 10004,
      int level = 0,
      int name = 0,
      StringOffset testOffset = default(StringOffset),
      double double_ = 1.0,
      float float_ = 1000.0f,
      short short_ = 12) {
    builder.StartObject(7);
    ScPlayerBasic.AddDouble_(builder, double_);
    ScPlayerBasic.AddFloat_(builder, float_);
    ScPlayerBasic.AddTest(builder, testOffset);
    ScPlayerBasic.AddName(builder, name);
    ScPlayerBasic.AddLevel(builder, level);
    ScPlayerBasic.AddMsgID(builder, msgID);
    ScPlayerBasic.AddShort_(builder, short_);
    return ScPlayerBasic.EndScPlayerBasic(builder);
  }

  public static void StartScPlayerBasic(FlatBufferBuilder builder) { builder.StartObject(7); }
  public static void AddMsgID(FlatBufferBuilder builder, int msgID) { builder.AddInt(0, msgID, 10004); }
  public static void AddLevel(FlatBufferBuilder builder, int level) { builder.AddInt(1, level, 0); }
  public static void AddName(FlatBufferBuilder builder, int name) { builder.AddInt(2, name, 0); }
  public static void AddTest(FlatBufferBuilder builder, StringOffset testOffset) { builder.AddOffset(3, testOffset.Value, 0); }
  public static void AddDouble_(FlatBufferBuilder builder, double double_) { builder.AddDouble(4, double_, 1.0); }
  public static void AddFloat_(FlatBufferBuilder builder, float float_) { builder.AddFloat(5, float_, 1000.0f); }
  public static void AddShort_(FlatBufferBuilder builder, short short_) { builder.AddShort(6, short_, 12); }
  public static Offset<ScPlayerBasic> EndScPlayerBasic(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<ScPlayerBasic>(o);
  }
};

