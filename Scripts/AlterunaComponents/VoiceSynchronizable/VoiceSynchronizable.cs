using System;
using System.Collections.Generic;
using System.Reflection;
using Alteruna.Multiplayer.Core;
using Alteruna.Multiplayer.Core.MethodArguments;
using Alteruna.Multiplayer.Core.PacketProcessing;
using Alteruna.Multiplayer.Core.PacketProcessing.Reader;
using Alteruna.Multiplayer.Core.PacketProcessing.Writer;
using UnityEngine;
using UnityEngine.Serialization;
using Application = UnityEngine.Application;

namespace Alteruna.Multiplayer.Unity
{
    /// <summary>
    /// Synchronizable component for voice chat.
    /// </summary>
    /// <remarks>
    /// <img src="../images/Alteruna.VoiceSynchronizable.png" />
    /// </remarks>
    [DisallowMultipleComponent, AddComponentMenu("Alteruna/Audio/Voice Synchronizable"), UnityEngine.Scripting.APIUpdating.MovedFrom(true, "Alteruna", "Alteruna.Trinity")]
    public partial class VoiceSynchronizable : Synchronizable
    {

        /// <summary>
        /// Audio input method.
        /// </summary>
        /// <remarks>
        /// Can be set to a custom implementation of <see cref="IAudioInput"/>.
        /// </remarks>
        public IAudioInput Microphone {
            get
            {
                return _microphone;
            }
            set
            {
                _microphone = value;

                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    if (!_microphone.SupportedPlatforms.HasFlag(SupportedPlatforms.WebGL))
                    {
                        //if (Application.isEditor) // only for if using target platform
                        //{
                        //    Debug.LogWarning("Current audio input does not supported WebGL.");
                        //}
                        //else
                        {
                            Debug.LogError("Current audio input does not supported WebGL.");
                            enabled = false;
                            _incompatible = true;
                            return;
                        }
                    }
                }
                else
                {
                    if (!_microphone.SupportedPlatforms.HasFlag(SupportedPlatforms.NonWebGL))
                    {
                        Debug.LogError("Current audio input does not supported non-WebGL platforms.");
                        enabled = false;
                        _incompatible = true;
                        return;
                    }
                }
                _incompatible = false;
            }
        }

        [NonSerialized]
        private IAudioInput _microphone = null;
        
        
        private static VoiceSynchronizable _localInput;
        private static string _selectedDevice = "";
        
        /// <summary>
        /// Time before looping.
        /// </summary>
        private const int AUDIO_CLIP_LENGTH = 2;
        
        /// <summary>
        /// Minimum number of samples required to send data.
        /// </summary>
        private const int SAMPLES_SYNC_MINIMUM = 1024;

        /// <summary>
        /// Source for playback.
        /// </summary>
        public AudioSource PlaybackSource;

        /// <summary>
        /// Target compression method.
        /// </summary>
        [Tooltip("Target compression method."), SerializeField]
        private CompressionMethod compression = CompressionMethod.None;
        
        /// <summary>
        /// Encryption.
        /// Less bits will decrease bandwidth but often decrease quality.
        /// </summary>
        [Tooltip("Less bits will decrease bandwidth but often decrease quality."), FormerlySerializedAs("Encryption")] [SerializeField]
        private EncryptionType encryption = EncryptionType.Bit12;
        
        /// <summary>
        /// Audio sample-rate.
        /// </summary>
        [Tooltip("Audio sample-rate."), Range(11000, 44100), SerializeField]
        private int AudioFrequency = 22050;
        
        /// <summary>
        /// How often to send data from the buffer.
        /// Lower values will decrease latency but increase bandwidth.
        /// </summary>
        [Tooltip("How often to send audio data."), Range(0.02f, 1f)]
        public float SendFrequency = 0.1f;
        
        /// <summary>
        /// Silence threshold.
        /// Values bellow this is considered silence.
        /// </summary>
        [Tooltip("Silence threshold."), Range(0f, 1f)]
        public float SilenceCutoff = 0.05f;
        
        /// <summary>
        /// Time in seconds to record after silence threshold is reached.
        /// </summary>
        [Tooltip("Time in seconds to record after silence threshold is reached."), Range(0.1f, 2f)]
        public float SilenceTimeout = 0.75f;
        
        /// <summary>
        /// Playback volume.
        /// </summary>
        [Tooltip("Playback volume."), Range(0.1f, 2f), ]
        public float Volume = 1f;

        /// <summary>
        /// Maximum number of unordered pending packages.
        /// When buffer count been meet, a package is considered lost and will be skipped.
        /// Will increase RAM usage.
        /// </summary>
        [Tooltip("Maximum number of unordered pending packages."), Min(0)]
        public int PackageLossBuffer = 0;
        
        /*
        /// <summary>
        /// Slow down playback when expected data does not arrive.
        /// </summary>
        public bool SlowdownOnMissingData = true;
        */
        [NonSerialized]
        private AudioClip _micClip;
        
        [NonSerialized]
        private readonly List<PendingIncomingVoiceStream> _pendingStreams = new List<PendingIncomingVoiceStream>();
        
        [NonSerialized]
        private Vector3 _mStartPos;
        [NonSerialized]
        private Vector3 _mStartScale;
        
        [NonSerialized]
        private float[] _mAudioBuffer;
        
        [NonSerialized]
        private float _mTimer;
        [NonSerialized]
        private float _peekVolume;
        [NonSerialized]
        private float _lastSample;
        [NonSerialized]
        private float _lastSampleDelta;
        [NonSerialized]
        private float _silenceTimer;
        
        /// <summary>
        /// Maximum numbers of samples to send at once
        /// </summary>
        [NonSerialized]
        private int _audioBufferSize;
        [NonSerialized]
        private int _mBufferLength;
        [NonSerialized]
        private int _mReadIndex;
        [NonSerialized]
        private int _mWriteIndex;
        [NonSerialized]
        private int _mLastPos;
        
        [NonSerialized]
        private byte _mSyncId;
        
        [NonSerialized]
        private bool _recordMic = false;
        [NonSerialized]
        private bool _possessed = false;
        [NonSerialized]
        private bool _incompatible = false;
        [NonSerialized]
        private bool _streaming = true;
        
        /// <summary>
        /// Highest volume recorded this frame.
        /// </summary>
        public float PeakVolume => _peekVolume;
        
        /// <summary>
        /// True when object is possessed by local user and is recording.
        /// False when object acts as receiver.
        /// </summary>
        public bool IsSender => _recordMic;
        
        /// <summary>
        /// True if the microphone is active and sending data.
        /// </summary>
        public bool IsActive => _peekVolume > SilenceCutoff;
        
        /// <summary>
        /// Value between 0 and 1 representing the activity of the microphone.
        /// 1 means that that the volume is above the silence threshold.
        /// 0 means that the volume is below the silence threshold and have been for at least SilenceTimeout.
        /// </summary>
        public float Activity => Mathf.Max((SilenceTimeout - _silenceTimer) / SilenceTimeout, 0);
        
        /// <summary>
        /// Name of the input device.
        /// </summary>
        public static string DeviceName
        {
            get
            {
                if (_localInput != null)
                {
                    return _localInput.DeviceNameLocal;
                }

                if (Application.platform == RuntimePlatform.WebGLPlayer) return "No available device";
                return UnityEngine.Microphone.devices.Length > 0 ? UnityEngine.Microphone.devices[0] : "No available device";
            }
        }
        
        private string DeviceNameLocal
        {
            get
            {
                if (_microphone.devices.Length == 0) return "No available device";
                return _selectedDevice.Length == 0 ? "Default" : _selectedDevice;
            }
        }

        /// <summary>
        /// Get the local input controller.
        /// The recording device is set to the default microphone.
        /// </summary>
        public static VoiceSynchronizable LocalInputController => _localInput;


        private void OnValidate()
        {
            if (encryption == EncryptionType.Bit12 && AudioFrequency % 2 == 1)
            {
                AudioFrequency--;
            }
        }


        private void Awake()
        {
            _streaming = Application.platform != RuntimePlatform.WebGLPlayer;
            _audioBufferSize = AUDIO_CLIP_LENGTH * AudioFrequency;
            if (_streaming) _audioBufferSize *= 2;

            if (encryption == EncryptionType.Bit12 && _audioBufferSize % 2 == 1)
            {
                Debug.LogWarning("Audio buffer size must be even for 12-bit encryption.");
                AudioFrequency--;
                _audioBufferSize = AUDIO_CLIP_LENGTH * AudioFrequency;
            }
            _mAudioBuffer = new float[_audioBufferSize];
        }


        private void Start()
        {
            if (!_possessed) enabled = false;
        }
        
        public override void Possessed(bool isMe, User user)
        {
            _possessed = true;
            if (isMe)
            {
                if (_microphone == null)
                {
                    Microphone = new UnityMicrophone();
                }
                if (_incompatible) return;
                if (PlaybackSource != null && PlaybackSource.isPlaying)
                {
                    PlaybackSource.Stop();
                }
                if (_localInput != null && _localInput != this)
                {
                    Debug.LogWarning("Multiple voice inputs detected. Disabling this one.");
                    enabled = false;
                    return;
                }
                _localInput = this;
                SetDeviceLocal();
                enabled = true;
            }
            else
            {
                if (_localInput == this)
                {
                    _localInput = null;
                    _microphone.End(_selectedDevice);
                }

                // Try get free AudioSource if there is not one assigned, if finals, create one.
                if (PlaybackSource == null)
                {
                    if (!TryGetComponent(out PlaybackSource) || PlaybackSource.clip != null)
                    {
                        PlaybackSource = gameObject.AddComponent<AudioSource>();
                        PlaybackSource.playOnAwake = false;
                    }
                }

                if (_streaming)
                {
                    PlaybackSource.clip = AudioClip.Create("Voice", _audioBufferSize, 1, AudioFrequency, true, OnAudioRead, OnAudioSetPosition);
                    PlaybackSource.loop = true;
                }
                else
                {
                    PlaybackSource.loop = false;
                }
            }
        }
        
        private void OnAudioRead(float[] data)
        {
            /*
            int diff = 0;
            if (_mWriteIndex > _mReadIndex)
            {
                diff = _mWriteIndex - _mReadIndex;
            }
            else if (_mWriteIndex < _mReadIndex)
            {
                diff = _micClip.samples - _mReadIndex + _mWriteIndex;
            }
*/
            for (int i = 0, l = data.Length; i < l; i++)
            {
                if (_mReadIndex == _mWriteIndex)
                {
                    // Create a fading oscillating effect when data rounds out
                    _lastSampleDelta -= _lastSample / 10f;
                    _lastSample += _lastSampleDelta;
                    _lastSampleDelta *= 0.9f;
                    data[i] = _lastSample;
                }
                /*
                else if (diff < SAMPLES_SYNC_MINIMUM / 4 && (i & 1) == 0 && _mReadIndex > 0)
                {
                    data[i] = (_mAudioBuffer[_mReadIndex - 1] + _mAudioBuffer[_mReadIndex]) / 2f * Volume;
                }*/
                else
                {
                    //diff--;
                    data[i] = _mAudioBuffer[_mReadIndex] * Volume;
                    _lastSampleDelta = data[i] - _lastSample;
                    _lastSample = data[i];
                    _mReadIndex = (_mReadIndex + 1) % _audioBufferSize;
                }
            }
        }
        
        private void OnAudioPeek(float[] data)
        {
            int readIndex = _mReadIndex;
            
            for (int i = 0, l = data.Length; i < l; i++)
            {
                if (readIndex == _mWriteIndex)
                {
                    // Create a fading oscillating effect when data rounds out
                    _lastSampleDelta -= _lastSample / 10f;
                    _lastSample += _lastSampleDelta;
                    _lastSampleDelta *= 0.9f;
                    data[i] = _lastSample;
                }
                else
                {
                    data[i] = _mAudioBuffer[readIndex] * Volume;
                    _lastSampleDelta = data[i] - _lastSample;
                    _lastSample = data[i];
                    readIndex = (readIndex + 1) % _audioBufferSize;
                }
            }
        }
        
        private void OnAudioSetPosition(int newPosition)
        {
            //mReadIndex = (newPosition) % AUDIO_BUFFER_SIZE;
        }


        private void Update()
        {
            if (!Multiplayer.InRoom)
                return;

            if (!_recordMic)
            {
                if (PlaybackSource.isPlaying && _mReadIndex == _mWriteIndex && _lastSample + Mathf.Abs(_lastSampleDelta) < 0.0002f)
                {
                    PlaybackSource.Stop();
                }
                
                return;
            }

            //if (!permissions.MicAuthorized)
            //return;

            int pos = _microphone.GetPosition(_selectedDevice);
            
            if (pos > _mLastPos)
            {
                int diff = pos - _mLastPos;
                float[] samples = new float[diff * _micClip.channels];
                _micClip.GetData(samples, _mLastPos);
                Store(samples);
            }
            else if (pos < _mLastPos)
            {
                int diff = _micClip.samples - _mLastPos + pos;
                float[] samples = new float[diff * _micClip.channels];
                _micClip.GetData(samples, _mLastPos);
                Store(samples);
            }
            _mLastPos = pos;

            _mTimer += Time.unscaledDeltaTime;
            if (_mTimer >= SendFrequency || _mBufferLength >= _audioBufferSize / 2)
            {
                if (_mBufferLength > SAMPLES_SYNC_MINIMUM)
                    PrepareSync();
            }
        }
        
        private void Store(float[] samples)
        {
            _mBufferLength += samples.Length / _micClip.channels;
            
            for (int i = 0; i < samples.Length; i += _micClip.channels)
            {
                _mAudioBuffer[_mWriteIndex] = samples[i];
                _mWriteIndex = (_mWriteIndex + 1) % _audioBufferSize;
                _peekVolume = Mathf.Max(_peekVolume, Mathf.Abs(samples[i]));
            }

            if (_peekVolume > SilenceCutoff)
            {
                _silenceTimer = 0;
            }
            else
            {
                _silenceTimer += Time.unscaledDeltaTime;
            }
        }

        private void PrepareSync()
        {
            if (_peekVolume > SilenceCutoff || _silenceTimer < SilenceTimeout)
            {
                Multiplayer.Sync(this, Reliability.Reliable);
            }
            _peekVolume = 0f;
        }

        public override void AssembleData(Writer writer, SerializeInfo info)
        {
            if (compression != CompressionMethod.None) writer.StartCompress();
            
            writer.Write(_mSyncId++);
            
            // Send number of samples being synced as it will vary depending on user activity
            
            switch (encryption)
            {
                case EncryptionType.Bit8:
                {
                    writer.Write(_mBufferLength);
                    int current = 0;
                    for (int i = 0; i < _mBufferLength; i++)
                    {
                        int sample = (int)(_mAudioBuffer[_mReadIndex] * short.MaxValue);
                        int delta = Mathf.Clamp(sample - current, -128, 127);
                        current += delta;
                        writer.Write((byte)(delta+128));
                        _mReadIndex = (_mReadIndex + 1) % _audioBufferSize;
                    }
                    _mBufferLength = 0;
                }
                    break;
                case EncryptionType.Bit12:
                {
                    int newBufferLength = 0;
                    if (_mBufferLength % 2 == 1)
                    {
                        _mBufferLength--;
                        newBufferLength = 1;
                    }
                    
                    writer.Write(Mathf.CeilToInt(_mBufferLength * 1.5f));
                    
                    int current = 0;
                    for (int i = 0; i < _mBufferLength; i += 2)
                    {
                        // First sample
                        int sample = (int)(_mAudioBuffer[_mReadIndex] * short.MaxValue);
                        int delta = Mathf.Clamp(sample - current, -2048, 2047);
                        current += delta;
                        delta += 2048;
                        byte high = (byte)(delta >> 8);
                        writer.Write((byte)(delta & 0xFF));
                        _mReadIndex = (_mReadIndex + 1) % _audioBufferSize;
                        
                        // Second sample
                        sample = (int)(_mAudioBuffer[_mReadIndex] * short.MaxValue);
                        delta = Mathf.Clamp(sample - current, -2048, 2047);
                        current += delta;
                        delta += 2048;
                        writer.Write((byte)(delta & 0xFF));
                        _mReadIndex = (_mReadIndex + 1) % _audioBufferSize;
                        
                        // Rest of the samples
                        writer.Write((byte)(((delta & 0xF00) >> 4) | high));
                    }
                    _mBufferLength = newBufferLength;
                }
                    break;
                case EncryptionType.Bit16:
                {
                    writer.Write(_mBufferLength*2);
                    for (int i = 0; i < _mBufferLength; i++)
                    {
                        writer.Write((short)(_mAudioBuffer[_mReadIndex] * short.MaxValue));
                        _mReadIndex = (_mReadIndex + 1) % _audioBufferSize;
                    }
                    _mBufferLength = 0;
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _mTimer = 0.0f;
            
            if (compression != CompressionMethod.None) writer.EndCompress(compression);
        }

        public override void DisassembleData(Reader reader, UnserializeInfo info)
        {
            if (compression != CompressionMethod.None) reader.Decompress();
            
            PendingIncomingVoiceStream stream = new PendingIncomingVoiceStream(reader);

            if (_mWriteIndex == 0 && _mReadIndex == 0)
            {
                _mSyncId = (byte)(stream.SyncId - 1);
            }

            while ((byte)(_mSyncId + 1) != stream.SyncId)
            {
                bool found = false;
                for (var i = 0; i < _pendingStreams.Count; i++)
                {
                    if ((byte)(_mSyncId + 1) != _pendingStreams[i].SyncId)
                    {
                        Read(_pendingStreams[i]);
                        _pendingStreams.RemoveAt(i);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // if we have over 3 packages waiting, a package is likely lost, so we skip it
                    if (_pendingStreams.Count < PackageLossBuffer)
                    {
                        _pendingStreams.Add(stream);
                        return;
                    }
                    _mSyncId++;
                }
            }
            Read(stream);

            // Read all pending streams if they are in order
            for (var i = 0; i < _pendingStreams.Count; i++)
            {
                if ((byte)(_mSyncId + 1) != _pendingStreams[i].SyncId)
                {
                    Read(_pendingStreams[i]);
                    _pendingStreams.RemoveAt(i);
                    i = -1;
                }
            }

            if (!_streaming)
            {
                _mReadIndex = (_mReadIndex + PlaybackSource.timeSamples) % _audioBufferSize;
                int newSamples = _mWriteIndex - _mReadIndex;
                if (newSamples < SendFrequency * AudioFrequency) return;
                
                PlaybackSource.clip = AudioClip.Create("Voice", newSamples, 1, AudioFrequency, false, OnAudioPeek);
                if (!PlaybackSource.isPlaying) PlaybackSource.Play();
                return;
            }
            
            int samples = _mWriteIndex - _mReadIndex;
            if (samples < 0) samples += _audioBufferSize;
            
            if (!PlaybackSource.isPlaying && samples >= SendFrequency * AudioFrequency * 4) PlaybackSource.Play();
            
        }

        /// <summary>
        /// Set the default microphone as the recording device.
        /// </summary>
        /// <returns>False when no device available.</returns>
        public static bool SetDevice()
        {
            if (_localInput != null)
            {
                return _localInput.SetDeviceLocal();
            }

            if (Application.platform != RuntimePlatform.WebGLPlayer && UnityEngine.Microphone.devices.Length > 0)
            {
                _selectedDevice = UnityEngine.Microphone.devices[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the default microphone as the recording device.
        /// </summary>
        /// <returns>False when no device available.</returns>
        private bool SetDeviceLocal()
        {
            if (_microphone.devices.Length <= 0) return false;
            SetDeviceLocal(_microphone.devices[0]);
            return true;
        }

        /// <summary>
        /// Set the microphone device by name.
        /// </summary>
        /// <param name="deviceName">Name of input device.</param>
        public static void SetDevice(string deviceName)
        {
            if (_localInput != null)
            {
                _localInput.SetDeviceLocal(deviceName);
            }
            else
            {
                _selectedDevice = deviceName;
            }
        }
        
        /// <summary>
        /// Set the microphone device by name.
        /// </summary>
        /// <param name="deviceName">Name of input device.</param>
        private void SetDeviceLocal(string deviceName)
        {
            _selectedDevice = deviceName;
            _micClip = _microphone.Start(deviceName, true, AUDIO_CLIP_LENGTH, AudioFrequency);
            _recordMic = true;
        }

        /// <summary>
        /// Set the microphone device by index.
        /// </summary>
        /// <param name="deviceId">Input device index.</param>
        /// <returns>False when no device available.</returns>
        public static bool SetDevice(int deviceId)
        {
            if (_localInput != null)
            {
                return _localInput.SetDeviceLocal(deviceId);
            }
            
            if (Application.platform != RuntimePlatform.WebGLPlayer && UnityEngine.Microphone.devices.Length > deviceId)
            {
                _selectedDevice = UnityEngine.Microphone.devices[deviceId];
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Set the microphone device by index.
        /// </summary>
        /// <param name="deviceId">Input device index.</param>
        /// <returns>False when no device available.</returns>
        private bool SetDeviceLocal(int deviceId)
        {
            if (_microphone.devices.Length <= deviceId) return false;
            SetDeviceLocal(_microphone.devices[deviceId]);
            return true;
        }

        /// <summary>
        /// Clear the microphone device and stop recording.
        /// </summary>
        public static void ClearDevice()
        {
            if (_localInput != null)
            {
                _localInput.ClearDeviceLocal();
            }
            else
            {
                _selectedDevice = "";
            }
        }
        
        /// <summary>
        /// Clear the microphone device and stop recording.
        /// </summary>
        public void ClearDeviceLocal()
        {
            if (_selectedDevice != "")
            {
                _microphone.End(_selectedDevice);
                _selectedDevice = "";
                _recordMic = false;
            }
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_localInput == this)
            {
                _localInput = null;
                _microphone.End(_selectedDevice);
            }
        }
        

        public override void Reset()
        {
            base.Reset();

            // get free AudioSource if there is not one assigned
            if (PlaybackSource == null)
            {
                if (TryGetComponent(out PlaybackSource) && PlaybackSource.clip != null)
                {
                    PlaybackSource = null;
                }
            }
        }
        
        /// <summary>
        /// Get the list of available input devices.
        /// </summary>
        /// <returns>Array of device names.</returns>
        public static string[] AvailableInputDevices()
        {
            if (_localInput != null) return _localInput._microphone.devices;
            if (Application.platform != RuntimePlatform.WebGLPlayer) return UnityEngine.Microphone.devices;
            return Array.Empty<string>();
        }

        private void Read(PendingIncomingVoiceStream stream)
        {
            _mSyncId = stream.SyncId;
            
            switch (encryption)
            {
                case EncryptionType.Bit8:
                {
                    int lastSample = 0;
                    for (int i = 0; i < stream.Length; i++)
                    {
                        lastSample += stream.Data[i]-128;
                        _mAudioBuffer[_mWriteIndex] = (float)lastSample / short.MaxValue;
                        _mWriteIndex = (_mWriteIndex + 1) % _audioBufferSize;
                    }
                }
                    break;
                case EncryptionType.Bit12:
                {
                    int lastSample = 0;
                    for (int i = 0; i < stream.Length; i += 3)
                    {
                        byte a = stream.Data[i];
                        byte b = stream.Data[i + 1];
                        byte c = stream.Data[i + 2];
                        
                        lastSample += a + ((c & 0xF) << 8) - 2048;
                        _mAudioBuffer[_mWriteIndex] = (float)lastSample / short.MaxValue;
                        _mWriteIndex = (_mWriteIndex + 1) % _audioBufferSize;
                        
                        lastSample += b + ((c & 0xF0) << 4) - 2048;
                        _mAudioBuffer[_mWriteIndex] = (float)lastSample / short.MaxValue;
                        _mWriteIndex = (_mWriteIndex + 1) % _audioBufferSize;
                    }
                }
                    break;
                case EncryptionType.Bit16:
                {
                    for (int i = 0; i < stream.Length; i += 2)
                    {
                        short data = (short)(stream.Data[i] | (stream.Data[i + 1] << 8));
                        _mAudioBuffer[_mWriteIndex] = (float)data / short.MaxValue;
                        _mWriteIndex = (_mWriteIndex + 1) % _audioBufferSize;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
        }

        /// <summary>
        /// Encoding types for data compression.
        /// </summary>
        public enum EncryptionType : byte
        {
            /// <summary>
            /// Encode samples as 8-bit delta.
            /// </summary>
            Bit8,
            /// <summary>
            /// Encode samples as 12-bit delta.
            /// </summary>
            Bit12,
            /// <summary>
            /// Full depth resolution.
            /// </summary>
            Bit16
        }
        
        /// <summary>
        /// Bit depth for audio quality.
        /// </summary>
        public enum BitDepth : byte
        {
            Bit8 = 8,
            Bit12 = 12,
            Forth = 14,
            Half = 15,
            Full = 16
        }

        private struct PendingIncomingVoiceStream
        {
            public readonly byte SyncId;
            public readonly int Length;
            public readonly byte[] Data;

            public PendingIncomingVoiceStream(Reader reader)
            {
                SyncId = reader.ReadByte();
                Data = reader.ReadByteArray();
                Length = Data.Length;
            }
        }
    }
}
