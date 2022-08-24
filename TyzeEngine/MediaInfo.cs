/*  Copyright (c) MediaArea.net SARL. All Rights Reserved.
 *
 *  Use of this source code is governed by a BSD-style license that can
 *  be found in the License.html file in the root of the source tree.
 */

//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
// Microsoft Visual C# wrapper for MediaInfo Library
// See MediaInfo.h for help
//
// To make it working, you must put MediaInfo.Dll
// in the executable folder
//
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591 // Disable XML documentation warnings

namespace TyzeEngine;

internal enum StreamKind
{
    General,
    Video,
    Audio,
    Text,
    Other,
    Image,
    Menu,
}

internal enum InfoKind
{
    Name,
    Text,
    Measure,
    Options,
    NameText,
    MeasureText,
    Info,
    HowTo
}

internal enum InfoOptions
{
    ShowInInform,
    Support,
    ShowInSupported,
    TypeOfValue
}

[Flags]
internal enum InfoFileOptions
{
    FileOption_Nothing      = 0x00,
    FileOption_NoRecursive  = 0x01,
    FileOption_CloseAll     = 0x02,
    FileOption_Max          = 0x04
};

[Flags]
internal enum Status
{
    None        =       0x00,
    Accepted    =       0x01,
    Filled      =       0x02,
    Updated     =       0x04,
    Finalized   =       0x08,
}

internal class MediaInfo
{
    //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_New();
    [DllImport("MediaInfo.dll")]
    private static extern void MediaInfo_Delete(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Open(IntPtr Handle, IntPtr FileName);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open_Buffer_Init(IntPtr Handle, Int64 File_Size, Int64 File_Offset);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Open(IntPtr Handle, Int64 File_Size, Int64 File_Offset);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open_Buffer_Continue(IntPtr Handle, IntPtr Buffer, IntPtr Buffer_Size);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Open_Buffer_Continue(IntPtr Handle, Int64 File_Size, byte[] Buffer, IntPtr Buffer_Size);
    [DllImport("MediaInfo.dll")]
    private static extern Int64  MediaInfo_Open_Buffer_Continue_GoTo_Get(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern Int64  MediaInfoA_Open_Buffer_Continue_GoTo_Get(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Open_Buffer_Finalize(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Open_Buffer_Finalize(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern void   MediaInfo_Close(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Inform(IntPtr Handle, IntPtr Reserved);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Inform(IntPtr Handle, IntPtr Reserved);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfoA_Option(IntPtr Handle, IntPtr Option,  IntPtr Value);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_State_Get(IntPtr Handle);
    [DllImport("MediaInfo.dll")]
    private static extern IntPtr MediaInfo_Count_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber);

    //MediaInfo class
    internal MediaInfo()
    {
        try
        {
            _handle = MediaInfo_New();
        }
        catch
        {
            _handle = (IntPtr)0;
        }

        _mustUseAnsi = !Environment.OSVersion.ToString().Contains("Windows");
    }

    ~MediaInfo()
    {
        if (_handle == (IntPtr)0) 
            return; MediaInfo_Delete(_handle);
    }
        
    internal int Open(string fileName)
    {
        if (_handle == (IntPtr)0)
            return 0;
            
        if (!_mustUseAnsi) 
            return (int)MediaInfo_Open(_handle, fileName);
            
        var fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);
        var toReturn = (int)MediaInfoA_Open(_handle, fileNamePtr);
        Marshal.FreeHGlobal(fileNamePtr);
        return toReturn;

    }
        
    public int Open_Buffer_Init(long fileSize, long fileOffset)
        => _handle == (IntPtr)0 ? 0 : (int)MediaInfo_Open_Buffer_Init(_handle, fileSize, fileOffset);
        
    public int Open_Buffer_Continue(IntPtr buffer, IntPtr bufferSize)
        => _handle == (IntPtr)0 ? 0 : (int)MediaInfo_Open_Buffer_Continue(_handle, buffer, bufferSize);
        
    public long Open_Buffer_Continue_GoTo_Get()
        => _handle == (IntPtr)0 ? 0 : MediaInfo_Open_Buffer_Continue_GoTo_Get(_handle);
        
    public int Open_Buffer_Finalize()
        => _handle == (IntPtr)0 ? 0 : (int)MediaInfo_Open_Buffer_Finalize(_handle);

    public void Close()
    {
        if (_handle == (IntPtr)0) 
            return; 
            
        MediaInfo_Close(_handle);
    }
        
    public string Inform()
    {
        if (_handle == (IntPtr)0)
            return "Unable to load MediaInfo library";
            
        return _mustUseAnsi 
            ? Marshal.PtrToStringAnsi(MediaInfoA_Inform(_handle, (IntPtr)0)) 
            : Marshal.PtrToStringUni(MediaInfo_Inform(_handle, (IntPtr)0));
    }
        
    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo, InfoKind kindOfSearch)
    {
        if (_handle == (IntPtr)0)
            return "Unable to load MediaInfo library";
            
        if (!_mustUseAnsi)
            return Marshal.PtrToStringUni(MediaInfo_Get(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, parameter,
                (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
            
        var parameterPtr=Marshal.StringToHGlobalAnsi(parameter);
        var toReturn=Marshal.PtrToStringAnsi(MediaInfoA_Get(_handle, (IntPtr)streamKind, 
            (IntPtr)streamNumber, parameterPtr, (IntPtr)kindOfInfo, (IntPtr)kindOfSearch));
        Marshal.FreeHGlobal(parameterPtr);
            
        return toReturn;

    }
        
    public string Get(StreamKind streamKind, int streamNumber, int parameter, InfoKind kindOfInfo)
    {
        if (_handle == (IntPtr)0)
            return "Unable to load MediaInfo library";
            
        return _mustUseAnsi 
            ? Marshal.PtrToStringAnsi(MediaInfoA_GetI(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, 
                (IntPtr)parameter, (IntPtr)kindOfInfo)) 
            : Marshal.PtrToStringUni(MediaInfo_GetI(_handle, (IntPtr)streamKind, (IntPtr)streamNumber, 
                (IntPtr)parameter, (IntPtr)kindOfInfo));
    }
        
    public string Option(string option, string value)
    {
        if (_handle == (IntPtr)0)
            return "Unable to load MediaInfo library";
            
        if (!_mustUseAnsi) 
            return Marshal.PtrToStringUni(MediaInfo_Option(_handle, option, value));
            
        var optionPtr=Marshal.StringToHGlobalAnsi(option);
        var valuePtr=Marshal.StringToHGlobalAnsi(value);
        var toReturn=Marshal.PtrToStringAnsi(MediaInfoA_Option(_handle, optionPtr, valuePtr));
        Marshal.FreeHGlobal(optionPtr);
        Marshal.FreeHGlobal(valuePtr);
            
        return toReturn;

    }

    public int State_Get() => _handle == (IntPtr)0 ? 0 : (int)MediaInfo_State_Get(_handle);
        
    public int Count_Get(StreamKind streamKind, int streamNumber)
    {
        if (_handle == (IntPtr)0) 
            return 0; 
            
        return (int)MediaInfo_Count_Get(_handle, (IntPtr)streamKind, (IntPtr)streamNumber);
    }
        
    private readonly IntPtr _handle;
    private readonly bool _mustUseAnsi;

    //Default values, if you know how to set default values in C#, say me
    public string Get(StreamKind streamKind, int streamNumber, string parameter, InfoKind kindOfInfo)
        => Get(streamKind, streamNumber, parameter, kindOfInfo, InfoKind.Name);

    public string Get(StreamKind streamKind, int streamNumber, string parameter)
        => Get(streamKind, streamNumber, parameter, InfoKind.Text, InfoKind.Name);

    public string Get(StreamKind streamKind, int streamNumber, int parameter)
        => Get(streamKind, streamNumber, parameter, InfoKind.Text);

    public string Option(string option) => Option(option, "");

    public int Count_Get(StreamKind streamKind) => Count_Get(streamKind, -1);
}



// public class MediaInfoList
// {
//     //Import of DLL functions. DO NOT USE until you know what you do (MediaInfo DLL do NOT use CoTaskMemAlloc to allocate memory)
//     [DllImport("MediaInfo.dll")]
//     private static extern IntPtr MediaInfoList_New();
//     [DllImport("MediaInfo.dll")]
//     private static extern void MediaInfoList_Delete(IntPtr Handle);
//     [DllImport("MediaInfo.dll")]
//     private static extern IntPtr MediaInfoList_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName, IntPtr Options);
//     [DllImport("MediaInfo.dll")]
//     private static extern void MediaInfoList_Close(IntPtr Handle, IntPtr FilePos);
//     [DllImport("MediaInfo.dll")]
//     private static extern IntPtr MediaInfoList_Inform(IntPtr Handle, IntPtr FilePos, IntPtr Reserved);
//     [DllImport("MediaInfo.dll")]
//     private static extern IntPtr MediaInfoList_GetI(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);
//     [DllImport("MediaInfo.dll")]
//     private static extern IntPtr MediaInfoList_Get(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);
//     [DllImport("MediaInfo.dll")]
//     private static extern IntPtr MediaInfoList_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);
//     [DllImport("MediaInfo.dll")]
//     private static extern IntPtr MediaInfoList_State_Get(IntPtr Handle);
//     [DllImport("MediaInfo.dll")]
//     private static extern IntPtr MediaInfoList_Count_Get(IntPtr Handle, IntPtr FilePos, IntPtr StreamKind, IntPtr StreamNumber);
//
//     //MediaInfo class
//     public MediaInfoList() { Handle = MediaInfoList_New(); }
//     ~MediaInfoList() { MediaInfoList_Delete(Handle); }
//     public int Open(String FileName, InfoFileOptions Options) { return (int)MediaInfoList_Open(Handle, FileName, (IntPtr)Options); }
//     public void Close(int FilePos) { MediaInfoList_Close(Handle, (IntPtr)FilePos); }
//     public String Inform(int FilePos) { return Marshal.PtrToStringUni(MediaInfoList_Inform(Handle, (IntPtr)FilePos, (IntPtr)0)); }
//     public String Get(int FilePos, StreamKind StreamKind, int StreamNumber, String Parameter, InfoKind KindOfInfo, InfoKind KindOfSearch) { return Marshal.PtrToStringUni(MediaInfoList_Get(Handle, (IntPtr)FilePos, (IntPtr)StreamKind, (IntPtr)StreamNumber, Parameter, (IntPtr)KindOfInfo, (IntPtr)KindOfSearch)); }
//     public String Get(int FilePos, StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo) { return Marshal.PtrToStringUni(MediaInfoList_GetI(Handle, (IntPtr)FilePos, (IntPtr)StreamKind, (IntPtr)StreamNumber, (IntPtr)Parameter, (IntPtr)KindOfInfo)); }
//     public String Option(String Option, String Value) { return Marshal.PtrToStringUni(MediaInfoList_Option(Handle, Option, Value)); }
//     public int State_Get() { return (int)MediaInfoList_State_Get(Handle); }
//     public int Count_Get(int FilePos, StreamKind StreamKind, int StreamNumber) { return (int)MediaInfoList_Count_Get(Handle, (IntPtr)FilePos, (IntPtr)StreamKind, (IntPtr)StreamNumber); }
//     private IntPtr Handle;
//
//     //Default values, if you know how to set default values in C#, say me
//     public void Open(String FileName) { Open(FileName, 0); }
//     public void Close() { Close(-1); }
//     public String Get(int FilePos, StreamKind StreamKind, int StreamNumber, String Parameter, InfoKind KindOfInfo) { return Get(FilePos, StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name); }
//     public String Get(int FilePos, StreamKind StreamKind, int StreamNumber, String Parameter) { return Get(FilePos, StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name); }
//     public String Get(int FilePos, StreamKind StreamKind, int StreamNumber, int Parameter) { return Get(FilePos, StreamKind, StreamNumber, Parameter, InfoKind.Text); }
//     public String Option(String Option_) { return Option(Option_, ""); }
//     public int Count_Get(int FilePos, StreamKind StreamKind) { return Count_Get(FilePos, StreamKind, -1); }
// }
//
// //NameSpace