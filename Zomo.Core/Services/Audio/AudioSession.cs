using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Zomo.Core.Common;
using Timer = System.Timers.Timer;

namespace Zomo.Core.Services.Audio
{
    public enum DisconnectReason : UInt16
    {
        NoUsers = 0,
        Idle = 1,
    }

    public class AudioSession : IDisposable
    {
        private DiscordSocketClient _client;
        private IAudioClient _audioClient;
        private AudioOutStream _audioOutStream;
        private Process _currentProcess;
        
        private bool _shouldLoop = true;
        
        private Queue<Song> _songs = new Queue<Song>();
        SemaphoreSlim _songLock = new SemaphoreSlim(1, 1);
        private Song? _currentSong;
        
        private IVoiceChannel _channel;
        private ISocketMessageChannel _messageChannel = null;
        
        //Time out
        private DateTime? _timeoutTimer;
        private TimeSpan _timeoutDuration;
        
        // kb/s
        private int _currentBitrate;
        

        public ulong Id => _channel.Id;

        public bool Connected => _audioClient != null;

        public Queue<Song> Songs => _songs;
        public Song? CurrentlyPlaying => _currentSong;

        public AudioSession(IVoiceChannel channel, ISocketMessageChannel messageChannel, DiscordSocketClient client, int bitrate = -1)
        {
            _channel = channel;
            _messageChannel = messageChannel;
            _client = client;
            _timeoutDuration = TimeSpan.FromMinutes(3.5);

            if (bitrate > 128)
            {
                bitrate = 128;
                ThreadLog($"Given bitrate is too high, setting to 128.");
            }
            else if (bitrate == -1)
            {
                bitrate = channel.Bitrate / 1000;
                ThreadLog($"Using channel bitrate of {bitrate} kb/s.");
            }
            
            _currentBitrate = bitrate;
        }

        private static void ThreadLog(string msg)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.ManagedThreadId.ToString().Log(msg);
            else
                Thread.CurrentThread.Name.Log(msg);
        }

        public async Task AddSong(Uri uri)
        {
            if (uri.IsFile)
            {
                throw new Exception("Files are not supported currently.");
                return;
            }

            //Future:
            //Find a static class that can handle this url
            //example: https://youtube.com/watch
            //         This would be handled by YoutubeAudio since it has a attrib that says it handles that type of url
            //         Youtube Audio would allow us to get more info on that url like author, title, rating, length.
            Song song = new Song(uri);

            ThreadLog("Waiting on lock to enqueue song.");
            await _songLock.WaitAsync();
            try
            {
                _songs.Enqueue(song);
                ThreadLog("Enqueued song!");
            }
            finally
            {
                _songLock.Release();
                ThreadLog("Released lock.");
            }
        }

        public async Task ConnectAsync()
        {
            try
            {
                Thread.CurrentThread.Name = $"{_channel.Guild.Name} Audio Session";
                _audioClient = await _channel.ConnectAsync();
                _audioOutStream = _audioClient.CreatePCMStream(AudioApplication.Music, _currentBitrate * 1024);

                while (_shouldLoop)
                {
                    _currentSong = null;

                    await _songLock.WaitAsync();
                    try
                    {
                        _currentSong = _songs.Any() ? _songs.Dequeue() : (Song?) null;
                    }
                    finally
                    {
                        _songLock.Release();
                    }

                    if (!_currentSong.HasValue)
                    {
                        //We have no songs to play, we should start the timer
                        if (_timeoutTimer == null)
                        {
                            ThreadLog("No songs, giving timer value");
                            _timeoutTimer = DateTime.Now.Add(_timeoutDuration);
                        }

                        if (DateTime.Now >= _timeoutTimer.Value)
                        {
                            ThreadLog("Leaving due to timer.");
                            Dispose(DisconnectReason.Idle);
                        }

                        Thread.Sleep(650);

                        continue;
                    }

                    _timeoutTimer = null;

                    using (_currentProcess = CreateStream(_currentSong.Value))
                    {
                        try
                        {
                            await _currentProcess.StandardOutput.BaseStream.CopyToAsync(_audioOutStream);
                        }
                        finally
                        {
                            await _audioOutStream.FlushAsync();
                        }
                    }

                    _currentProcess = null;
                }

                await _audioOutStream.DisposeAsync();
                await LeaveAsync();
            }
            catch (Exception ex)
            {
                this.Log("Audio Session", ex.ToString());
            }
        }

        private static Process CreateStream(Song song)
        {
            /*
             * Maybe just use lib ffmpeg and pipe youtube-dl though that.
             * no idea if lib ffmpeg exists, just a idea.
             */
            var proc = new Process();

            string args =
                $"-c \"youtube-dl --quiet -o - \"{song.Uri}\" |" +
                $" ffmpeg -hide_banner -loglevel panic -i pipe:0 -ac 2 -filter:a \"volume=0.5\" -f s16le -ar 48000 pipe:1\"";

            proc.StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            proc.ErrorDataReceived += ProcOnErrorDataReceived;
            proc.Start();
            return proc;
        }

        private static void ProcOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger.Write("Audio Session", $"Error in process \"{e.Data}\"");
        }

        public async Task LeaveAsync()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                _shouldLoop = false;
                _currentProcess?.Kill(true);
                _audioClient.StopAsync();
                _channel.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Dispose(DisconnectReason reason)
        {
            string message = "";

            switch (reason)
            {
                case DisconnectReason.NoUsers:
                    message = "Disconnected: No users.";
                    break;
                case DisconnectReason.Idle:
                    message = "Disconnected: Idle for too long.";
                    break;
            }

            if (message != "") _messageChannel?.SendMessageAsync(message);
            Dispose();
        }
    }
}