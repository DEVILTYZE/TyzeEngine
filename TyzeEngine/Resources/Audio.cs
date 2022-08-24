using System;
using System.IO;
using OpenTK.Audio.OpenAL;

namespace TyzeEngine.Resources;

public class Audio : Resource
{
    private int _buffer;

    public bool IsEnabled { get; private set; }
    
    public Audio(string path) : base(path)
    {
        _buffer = -1;
        IsEnabled = false;
    }

    public override void Load()
    {
        var (format, frequency) = GetAudioInfo();
        
        Handle = AL.GenSource();

        var fileInfo = new FileInfo(Path);
        var data = new byte[fileInfo.Length];
        
        using (var file = File.OpenRead(Path))
        {
            var length = file.Read(data, 0, data.Length);
            Array.Resize(ref data, length);
        }

        _buffer = AL.GenBuffer();
        AL.BufferData(_buffer, format, data, frequency);
        AL.Source(Handle, ALSourcei.Buffer, _buffer);
        AL.Source(Handle, ALSourceb.Looping, false);
    }

    public override void Enable()
    {
        if (IsEnabled)
            Disable();

        IsEnabled = true;
        AL.SourcePlay(Handle);
    }

    public override void Disable()
    {
        if (!IsEnabled)
            return;
        
        AL.SourceStop(Handle);
    }

    protected override void Dispose(bool disposing)
    {
        if (Disposed)
            return;
        
        AL.DeleteSource(Handle);
        
        if (_buffer != Constants.ErrorCode)
            AL.DeleteBuffer(_buffer);
        
        base.Dispose(disposing);
    }

    private (ALFormat, int) GetAudioInfo()
    {
        ALFormat GetFormat(int countChannels, int bitDepth)
        {
            return countChannels switch
            {
                1 => bitDepth switch
                {
                    Constants.BitDepth8 => ALFormat.Mono8,
                    Constants.BitDepth16 or _ => ALFormat.Mono16
                },
                2 or _ => bitDepth switch
                {
                    Constants.BitDepth8 => ALFormat.Stereo8,
                    Constants.BitDepth16 or _ => ALFormat.Stereo16
                }
            };
        }
        
        var audioInfo = new MediaInfo();
        audioInfo.Open(Path);
        var frequency = int.Parse(audioInfo.Get(StreamKind.Audio, 0, "SamplingRate"));
        var countOfChannels = int.Parse(audioInfo.Get(StreamKind.Audio, 0, "Channel(s)"));
        var bitDepth = int.Parse(audioInfo.Get(StreamKind.Audio, 0, "BitDepth"));
        var duration = int.Parse(audioInfo.Get(StreamKind.Audio, 0, "Duration"));
        var format = GetFormat(countOfChannels, bitDepth);
        audioInfo.Close();

        return (format, frequency);
    }
}