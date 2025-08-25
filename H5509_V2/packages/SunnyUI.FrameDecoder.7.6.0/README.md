![输入图片说明](https://images.gitee.com/uploads/images/2021/0324/213615_54240ba9_416720.png)

# SunnyUI.FrameDecoder
欢迎交流，QQ群： 56829229 (SunnyUI技术交流群)，请给源码项目点个Star吧！！！
    
#### 开始
从[Nuget](https://www.nuget.org/packages/SunnyUI.FrameDecoder)安装最新版本。    
~~~
Install-Package SunnyUI.FrameDecoder
~~~

#### 目的
SunnyUI.FrameDecoder 开源TCP、UDP、串口等流式数据解码库    
- 实现解码自由    
- 底层缓冲区采用数据池实现    
- 通过使用池化缓冲区来消除大型对象堆分配    
- 减少GC次数    
- 缓冲区实现IBufferWrite<T>接口    
- 广泛支持较新的内存相关类型，如`Span<byte>`, `ReadOnlySpan<byte>`以及`Memory<byte>`    
- 避免内存碎片    
- 提供出色的性能    
   
#### 介绍 
TCP是面向连接的传输协议，TCP传输的数据是以流的形式，而流数据是没有明确的开始结尾边界，应用层处理接收数据时可能产生半包或者粘包，其主要原因如下：    
- TCP发送方原因：    
因为TCP本身传输的数据包大小就有限制，所以应用发出的消息包过大，TCP会把应用消息包拆分为多个TCP数据包发送出去。    
Negal算法的优化，当应用发送数据包太小，TCP为了减少网络请求次数的开销，它会等待多个消息包一起，打成一个TCP数据包一次发送出去。    
- TCP接收方原因：    
因为TCP缓冲区里的数据都是字符流的形式，没有明确的边界，因为数据没边界，所以应用从TCP缓冲区中读取数据时就没办法指定一个或几个消息一起读，而只能选择一次读取多大的数据流，而这个数据流中就可能包含着某个消息包的一部分数据。    

所以为了解决半包或者粘包，得到应用层需要的数据，实现解码自由，开发了此数据解码库，原理也就是给数据流转时给流式数据加上消息边界。在接收数据时，将流式数据通过相应的解码器（FrameDecoder），解码输出完整的帧数据（FrameData）。    
    
UDP是面向消息的，它有边界协议，可以根据消息的格式区分消息的开始和结尾。那为什么还要解码呢？    
举个例子，实际项目：FPGA通过千兆网卡将数据以UDP协议输出，其数据实际还是流式数据，需要在UDP接收到的数据中寻找包头和包尾，来截取一段需要的数据，这样数据解码器就有意义了。    
    
串口在数据发送和接收时，根据串口的配置（波特率、奇偶校验等）、串口缓冲设置、数据发送间隔的不匹配，也会出现接收数据时可能产生半包或者粘包。    
Modbus-RTU总线是通过时间间隔来判断一帧数据结束的，3.5个字符时间内没有收到新的数据，则认为这一帧数据结束。    
普通的串口数据传输，还是要给数据增加消息边界，通过解码器来获取实际需要的数据。    
    
其实不限于TCP、UDP、串口，只要是从流式数据中获取指定格式的数据，都可以用过解码器来实现。    
    
有些网络库，例如：    
- [Netty](https://netty.io) 有FrameDecoder；    
- [TouchSocket](https://gitee.com/RRQM_Home/TouchSocket) 有数据处理适配器；    
- [HP-Socket](https://gitee.com/ldcsaa/HP-Socket) 则是通过Pack模式，在发送数据时加上消息边界。    
不一一列举，这些库主要是为了解决TCP数据的的解码。    
    
SunnyUI.FrameDecoder则面向流式数据，不拘泥于数据从哪儿来，通过流式数据的解码器来获取到有用的数据。    
    
#### 软件架构
![输入图片说明](https://foruda.gitee.com/images/1674814921393961193/22fd7721_416720.png "屏幕截图")
    
流式数据有两种：    
- byte[]：字节数组     
- string：字符串        
    
针对这两种数据，实现了泛型数据缓存： `DataCache<T>`，其内存分配通过数据池：`DataPool<T>` 实现        
通过`DataCache<byte>`实现字节数据组缓存，通过`DataCache<char>`实现字符串缓存        
    
#### 安装教程
动态库应用环境： VS2012及以上均可，支持.Net Framework 4.5+、包括.Net 6、.Net7    
推荐通过Nuget安装：NuGet\Install-Package SunnyUI.FrameDecoder    
或者通过Nuget搜索：SunnyUI.FrameDecoder 安装。    
![输入图片说明](https://foruda.gitee.com/images/1674815602181487555/1cf088d5_416720.png "屏幕截图")
    
#### 使用说明

**下述说明仅供参考，重构多次后参数可能不一致**

- 字节数据帧解码器
1. DelimiterBasedFrameDecoder 基于分隔符 - 数据帧解码器 
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674816479946157414/d15ea6e8_416720.png "屏幕截图")
    
构造函数：
~~~
/// <summary>
/// 构造函数
/// </summary>
/// <param name="delimiters">分隔符，最好为多字节，并且数据中不会出现分隔符而造成误解码</param>
/// <param name="maxFrameLength">最大数据长度，仅判断数据长度</param>
public DelimiterBasedFrameDecoder(byte[] delimiters, int maxFrameLength = 0)
~~~
示例：
~~~
var bts = new byte[] { 1, 2, 3, 0xFF, 0xFB };
var decoder1 = new DelimiterBasedFrameDecoder(new byte[] { 0xFF, 0xFB });
decoder1.OnDecoder += DelimiterBasedFrameDecoder_OnDecoder;
decoder1.Decode(bts);

private static void DelimiterBasedFrameDecoder_OnDecoder(object sender, IByteEventArgs e)
{
    Console.WriteLine("Value: " + ByteHelper.HexString(e.Value));
}
~~~
输出：
~~~
Value: 010203
~~~

2. FixedLengthFrameDecoder 固定长度 - 数据帧解码器
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674817282416108813/bd98fb85_416720.png "屏幕截图")

构造函数：
~~~
/// <summary>
/// 构造函数
/// </summary>
/// <param name="frameLength">帧固定长度</param>
public FixedLengthFrameDecoder(int frameLength)
~~~
示例：
~~~
var bts = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
var decoder0 = new FixedLengthFrameDecoder(3);
decoder0.OnDecoder += FixedLengthFrameDecoder_OnDecoder;
decoder0.Decode(bts);

private static void FixedLengthFrameDecoder_OnDecoder(object sender, IByteEventArgs e)
{
    Console.WriteLine("Value: " + ByteHelper.HexString(e.Value));
}
~~~
输出：
~~~
Value: 010203
Value: 040506
~~~

3. HeaderDelimiterFrameDecoder 数据头部 - 数据帧解码器
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674817985965823495/4da338bc_416720.png "屏幕截图")

构造函数：
~~~
/// <summary>
/// 构造函数
/// </summary>
/// <param name="headers">数据头部</param>
/// <param name="maxFrameLength">最大数据长度，仅判断数据长度</param>
public HeaderDelimiterFrameDecoder(byte[] headers, int maxFrameLength = 0)
~~~
示例：
~~~
var bts = new byte[] { 0xFF, 0xFB, 1, 2, 3 };
var decoder2 = new HeaderDelimiterFrameDecoder(new byte[] { 0xFF, 0xFB });
decoder2.OnDecoder += HeaderDelimiterFrameDecoder_OnDecoder;
decoder2.Decode(bts);
decoder2.Decode(bts);

private static void HeaderDelimiterFrameDecoder_OnDecoder(object sender, IByteEventArgs e)
{
    Console.WriteLine("Value: " + ByteHelper.HexString(e.Value));
}
~~~
输出：
~~~
Value: 010203
~~~

4. HeaderTagLengthValueTailFrameDecoder 数据头部、标签、数据长度、数据、数据尾部 - 数据帧解码器
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674818700825773094/4d4b5627_416720.png "屏幕截图")

构造函数：
~~~
/// <summary>
/// 构造函数
/// </summary>
/// <param name="headers">数据头部</param>
/// <param name="tails">数据尾部</param>
/// <param name="tagLength">标签长度</param>
/// <param name="valueLengthType">数据长度类型</param>
/// <param name="valueIsLittleEndian">数据长度字节顺序</param>
/// <param name="isFullLength">数据长度是否包含数据头部、标签、数据长度、数据尾部的长度，false为仅数据长度</param>
/// <param name="maxFrameLength">最大数据长度，仅判断数据长度</param>
public HeaderTagLengthValueTailFrameDecoder(byte[] headers, byte[] tails, int tagLength, ValueLengthType valueLengthType, bool valueIsLittleEndian = true, bool isFullLength = false, int maxFrameLength = 0)
~~~
示例：
~~~
var bts = new byte[] { 0xFF, 0xFB, 0xAA, 2, 3, 4, 0xF7, 0xF7 };
var decoder3 = new HeaderTagLengthValueTailFrameDecoder(new byte[] { 0xFF, 0xFB }, new byte[] { 0xF7, 0xF7 }, 1, ValueLengthType.Byte);
decoder3.OnDecoder += HeaderTagLengthValueTailFrameDecoder_OnDecoder;
decoder3.Decode(bts);

private static void HeaderTagLengthValueTailFrameDecoder_OnDecoder(object sender, IByteEventArgs e)
{
    var args = (TagLengthValueFrameDataEventArgs)e;
    Console.WriteLine("Tag: " + ByteHelper.HexString(args.Tag) + ", Value: " + ByteHelper.HexString(args.Value));
}
~~~
输出：
~~~
Tag: AA, Value: 0304
~~~

5. HeaderTailFrameDecoder 数据头部、数据尾部 - 数据帧解码器
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674819046018410867/23bcd333_416720.png "屏幕截图")

构造函数：
~~~
/// <summary>
/// 构造函数
/// </summary>
/// <param name="headers">数据头部</param>
/// <param name="tails">数据尾部</param>
/// <param name="maxFrameLength">最大数据长度，仅判断数据长度</param>
public HeaderTailFrameDecoder(byte[] headers, byte[] tails, int maxFrameLength = 0)
~~~
示例：
~~~
var bts = new byte[] { 0xFF, 0xFB, 2, 3, 4, 0xF7, 0xF7 };
var decoder4 = new HeaderTailFrameDecoder(new byte[] { 0xFF, 0xFB }, new byte[] { 0xF7, 0xF7 });
decoder4.OnDecoder += HeaderTailFrameDecoder_OnDecoder;
decoder4.Decode(bts);

private static void HeaderTailFrameDecoder_OnDecoder(object sender, IByteEventArgs e)
{
    Console.WriteLine("Value: " + ByteHelper.HexString(e.Value));
}
~~~
输出：
~~~
Value: 020304
~~~

6. LengthValueFrameDecoder 数据长度、数据 - 数据帧解码器
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674819293209059193/c52acd93_416720.png "屏幕截图")

构造函数：
~~~
/// <summary>
/// 构造函数
/// </summary>
/// <param name="valueLengthType">数据长度类型</param>
/// <param name="valueIsLittleEndian">数据长度字节顺序</param>
/// <param name="isFullLength">数据长度是否包含数据长度的长度，false为仅数据长度</param>
/// <param name="maxFrameLength">最大数据长度，仅判断数据长度</param>
public LengthValueFrameDecoder(ValueLengthType valueLengthType, bool valueIsLittleEndian = true, bool isFullLength = false, int maxFrameLength = 0)
~~~
示例：
~~~
bts = new byte[] { 2, 3, 4 };
var decoder5 = new LengthValueFrameDecoder(ValueLengthType.Byte);
decoder5.OnDecoder += LengthValueFrameDecoder_OnDecoder;
decoder5.Decode(bts);

private static void LengthValueFrameDecoder_OnDecoder(object sender, IByteEventArgs e)
{
    Console.WriteLine("Value: " + ByteHelper.HexString(e.Value));
}
~~~
输出：
~~~
Value: 0304
~~~

7. TagLengthValueFrameDecoder 标签、数据长度、数据 - 数据帧解码器
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674819856009534964/44b905df_416720.png "屏幕截图")

构造函数：
~~~
/// <summary>
/// 构造函数
/// </summary>
/// <param name="tagLength">标签长度</param>
/// <param name="valueLengthType">数据长度类型</param>
/// <param name="valueIsLittleEndian">数据长度字节顺序</param>
/// <param name="isFullLength">数据长度是否包含标签、数据长度的长度，false为仅数据长度</param>
/// <param name="maxFrameLength">最大数据长度，仅判断数据长度</param>
public TagLengthValueFrameDecoder(int tagLength, ValueLengthType valueLengthType, bool valueIsLittleEndian = true, bool isFullLength = false, int maxFrameLength = 0)
~~~
示例：
~~~
bts = new byte[] { 0xAA, 2, 3, 4 };
var decoder6 = new TagLengthValueFrameDecoder(1, ValueLengthType.Byte);
decoder6.OnDecoder += TagLengthValueFrameDecoder_OnDecoder;
decoder6.Decode(bts);

private static void TagLengthValueFrameDecoder_OnDecoder(object sender, IByteEventArgs e)
{
    var args = (TagLengthValueFrameDataEventArgs)e;
    Console.WriteLine("Tag: " + ByteHelper.HexString(args.Tag) + ", Value: " + ByteHelper.HexString(args.Value));
}
~~~
输出：
~~~
Tag: AA, Value: 0304
~~~

- 字符串解码器
1. NMEA0183FrameDecoder NMEA0183 - 数据帧解码器
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674820226720806126/735613e1_416720.png "屏幕截图")

示例：
~~~
var decoder = new NMEA0183FrameDecoder();
decoder.OnDecoder += NMEA0183Decoder_OnDecoder;
decoder.Decode("4,19.7,M,,,,0000*1F$GPGGA,092204.999,4250.5589,S,147");
decoder.Decode("18.5084,E,1,04,24.4,19.7,M,,,,0000*1F$GPGSA,A,3,01,20,");
decoder.Decode("19,13,,,,,,,,,40.4,24.4,32.2*0A$GPGSV,3,1,10,20,78,331,45,01,59,235,47,22,41,");
decoder.Decode("069,,13,32,252,45*70$GPVTG,092204.999,42");

public static void NMEA0183Decoder_OnDecoder(object sender, IStringEventArgs e)
{
    Console.WriteLine(e.Value);
}
~~~
输出：
~~~
$GPGGA,092204.999,4250.5589,S,14718.5084,E,1,04,24.4,19.7,M,,,,0000*1F
$GPGSA,A,3,01,20,19,13,,,,,,,,,40.4,24.4,32.2*0A
$GPGSV,3,1,10,20,78,331,45,01,59,235,47,22,41,069,,13,32,252,45*70
~~~

2. LineBasedFrameDecoder 基于换行符 - 数据帧解码器
数据格式：
![输入图片说明](https://foruda.gitee.com/images/1674820621995638999/44a3fc3d_416720.png "屏幕截图")

示例：
~~~
var decoder = new LineBasedFrameDecoder();
decoder.OnDecoder += NMEA0183Decoder_OnDecoder;
decoder.Decode("*AA" + '\r' + '\n' + "$GPGGA");
decoder.Decode("*1F" + '\n' + '\r' + "$GPGSA");
decoder.Decode("*0A" + '\r' + "$GPGSV");
decoder.Decode("*70" + '\n' + "$GPVTG");

public static void NMEA0183Decoder_OnDecoder(object sender, IStringEventArgs e)
{
    Console.WriteLine(e.Value);
}
~~~
输出：
~~~
*AA
$GPGGA*1F
$GPGSA*0A
$GPGSV*70
~~~

#### 参与贡献
1.  Fork 本仓库
2.  新建 Feat_xxx 分支
3.  提交代码
4.  新建 Pull Request

#### 新的解码器
如果有新的解码器建议，请添加Issue。