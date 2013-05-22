#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DirectShowLib.BDA;
using TvLibrary.Channels;
using TvLibrary.Implementations.Dri.Service;
using TvLibrary.Implementations.Dri.Parser;
using TvLibrary.Implementations.DVB;
using TvLibrary.Interfaces;
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri
{
  public delegate void TableCompleteDelegate(MgtTableType table);

  public class ScannerDri : ITVScanning, IDisposable
  {
    private enum ScanStage
    {
      NotScanning,
      Mgt,
      Nit,
      Vct,
      Ntt
    }

    private struct PhysicalChannel
    {
      public int Channel;
      public int Frequency;   // unit = kHz
    }

    private TunerDri _tuner = null;
    private FdcService _fdcService = null;

    private ScanStage _scanStage = ScanStage.NotScanning;
    private ManualResetEvent _scanEvent = null;

    // parsers
    private MgtParser _mgtParser = new MgtParser();
    private NitParser _nitParser = new NitParser();
    private NttParser _nttParser = new NttParser();
    private LvctParser _lvctParser = new LvctParser();
    private SvctParser _svctParser = new SvctParser();

    IList<ModulationType> _modulationModes = null;
    IList<PhysicalChannel> _carrierFrequencies = null;
    IDictionary<int, ATSCChannel> _channels = new Dictionary<int, ATSCChannel>();
    HashSet<int> _sourcesWithoutNames = new HashSet<int>();

    public ScannerDri(TunerDri tuner, FdcService fdcService)
    {
      _tuner = tuner;
      _fdcService = fdcService;
      _scanEvent = new ManualResetEvent(false);
    }

    private static int GetPhysicalChannelFromFrequency(int carrierFrequency)
    {
      // Calculate the physical channel number corresponding with a frequency
      // in the QAM standard frequency plan.
      if (carrierFrequency >= 651000)
      {
        return 100 + ((carrierFrequency - 651000) / 6000);
      }
      if (carrierFrequency >= 219000)
      {
        return 23 + ((carrierFrequency - 219000) / 6000);
      }
      if (carrierFrequency >= 177000)
      {
        return 7 + ((carrierFrequency - 177000) / 6000);
      }
      if (carrierFrequency >= 123000)
      {
        return 14 + ((carrierFrequency - 123000) / 6000);
      }
      if (carrierFrequency >= 93000)
      {
        return 95 + ((carrierFrequency - 93000) / 6000);
      }
      if (carrierFrequency >= 79000)
      {
        return 5 + ((carrierFrequency - 79000) / 6000);
      }
      if (carrierFrequency >= 57000)
      {
        return 2 + ((carrierFrequency - 57000) / 6000);
      }
      return 1;
    }

    private void OnTableSection(CpStateVariable stateVariable, object newValue)
    {
      try
      {
        if (!stateVariable.Name.Equals("TableSection"))
        {
          return;
        }
        byte[] section = (byte[])newValue;
        if (section == null || section.Length < 3)
        {
          return;
        }
        int pid = (section[0] << 8) + section[1];
        byte tableId = section[2];
        Log.Log.Debug("DRI CC: scan stage = {0}, PID = 0x{1:x}, table ID = 0x{2:x}, size = {3}", _scanStage, pid, tableId, section.Length);
        //DVB_MMI.DumpBinary(section, 0, section.Length);
        switch (_scanStage)
        {
          case ScanStage.Mgt:
            if (tableId == 0xc7)
            {
              _mgtParser.Decode(section);
            }
            break;
          case ScanStage.Nit:
            if (tableId == 0xc2)
            {
              _nitParser.Decode(section);
            }
            break;
          case ScanStage.Ntt:
            if (tableId == 0xc3)
            {
              _nttParser.Decode(section);
            }
            break;
          case ScanStage.Vct:
            if (tableId == 0xc8 || tableId == 0xc9)
            {
              _lvctParser.Decode(section);
            }
            else if (tableId == 0xc4)
            {
              _svctParser.Decode(section);
            }
            break;
        }

      }
      catch (Exception ex)
      {
        Log.Log.Error("DRI CC: failed to handle table section\r\n{0}", ex);
      }
    }

    public ITVCard TvCard
    {
      get
      {
        return _tuner;
      }
    }

    public void Reset()
    {
      _sourcesWithoutNames.Clear();
      _modulationModes = new ModulationType[255];
      _carrierFrequencies = new PhysicalChannel[255];
    }

    public void Dispose()
    {
      if (_scanEvent != null)
      {
        _scanEvent.Close();
      }
    }

    public void OnTableComplete(MgtTableType tableType)
    {
      Log.Log.Info("DRI CC: on table complete, table type = {0}", tableType);
      _scanEvent.Set();
    }

    public void OnCarrierDefinition(AtscTransmissionMedium transmissionMedium, byte index, int carrierFrequency)
    {
      if (transmissionMedium != AtscTransmissionMedium.Cable)
      {
        return;
      }
      if (carrierFrequency > 1750)
      {
        // Convert from centre frequency to the analog video carrier frequency.
        // This is a BDA convention.
        PhysicalChannel channel = new PhysicalChannel();
        channel.Frequency = carrierFrequency - 1750;
        channel.Channel = GetPhysicalChannelFromFrequency(carrierFrequency);
        _carrierFrequencies[index] = channel;
      }
    }

    public void OnModulationMode(AtscTransmissionMedium transmissionMedium, byte index, TransmissionSystem transmissionSystem,
      BinaryConvolutionCodeRate innerCodingMode, bool isSplitBitstreamMode, ModulationType modulationFormat, int symbolRate)
    {
      if (transmissionMedium != AtscTransmissionMedium.Cable)
      {
        return;
      }
      _modulationModes[index] = modulationFormat;
    }

    public void OnLvctChannelDetail(string shortName, int majorChannelNumber, int minorChannelNumber, ModulationMode modulationMode, 
      uint carrierFrequency, int channelTsid, int programNumber, EtmLocation etmLocation, bool accessControlled, bool hidden,
      int pathSelect, bool outOfBand, bool hideGuide, AtscServiceType serviceType, int sourceId)
    {
      if (programNumber == 0 || outOfBand || modulationMode == ModulationMode.Analog || modulationMode == ModulationMode.PrivateDescriptor ||
        (serviceType != AtscServiceType.Audio && serviceType != AtscServiceType.DigitalTelevision) || sourceId == 0)
      {
        // Not tunable/supported.
        return;
      }

      ATSCChannel channel = null;
      if (_channels.TryGetValue(sourceId, out channel))
      {
        Log.Log.Info("DRI CC: received repeated L-VCT channel detail for source 0x{0:x}", sourceId);
        return;
      }

      channel = new ATSCChannel();
      _channels.Add(sourceId, channel);

      carrierFrequency /= 1000; // Hz => kHz
      if (carrierFrequency > 1750)
      {
        // Convert from centre frequency to the analog video carrier
        // frequency. This is a BDA convention.
        channel.Frequency = carrierFrequency - 1750;
      }
      channel.PhysicalChannel = GetPhysicalChannelFromFrequency((int)carrierFrequency);

      switch (modulationMode)
      {
        case ModulationMode.Atsc8Vsb:
          channel.ModulationType = ModulationType.Mod8Vsb;
          break;
        case ModulationMode.Atsc16Vsb:
          channel.ModulationType = ModulationType.Mod16Vsb;
          break;
        case ModulationMode.ScteMode1:
          channel.ModulationType = ModulationType.Mod64Qam;
          break;
        default:
          channel.ModulationType = ModulationType.Mod256Qam;
          break;
      }
      channel.FreeToAir = !accessControlled;
      channel.IsTv = (serviceType == AtscServiceType.DigitalTelevision);
      channel.IsRadio = (serviceType == AtscServiceType.Audio);

      // SCTE cable supports both one-part and two-part channel numbers where
      // the major and minor channel number range is 0..999.
      // ATSC supports only two-part channel numbers where the major range is
      // 1..99 and the minor range is 0..999.
      // When the minor channel number is 0 it indicates an analog channel.
      if ((majorChannelNumber & 0x03f0) == 0x03f0)
      {
        channel.LogicalChannelNumber = ((majorChannelNumber & 0x0f) << 10) + minorChannelNumber;
        channel.MajorChannel = channel.LogicalChannelNumber;
        channel.MinorChannel = 0;
      }
      else
      {
        channel.LogicalChannelNumber = (majorChannelNumber * 1000) + minorChannelNumber;
        channel.MajorChannel = majorChannelNumber;
        channel.MinorChannel = minorChannelNumber;
      }
      channel.Name = shortName;
      channel.NetworkId = sourceId;
      channel.PmtPid = 0;   // TV Server will automatically lookup the correct PID from the PAT
      channel.ServiceId = programNumber;
      channel.TransportId = channelTsid;
    }

    public void OnSvctChannelDetail(AtscTransmissionMedium transmissionMedium, int vctId, int virtualChannelNumber, bool applicationVirtualChannel,
      int bitstreamSelect, int pathSelect, ChannelType channelType, int sourceId, byte cdsReference, int programNumber, byte mmsReference)
    {
      if (transmissionMedium != AtscTransmissionMedium.Cable || applicationVirtualChannel || programNumber == 0 || sourceId == 0)
      {
        // Not tunable/supported.
        return;
      }

      ATSCChannel channel = null;
      if (_channels.TryGetValue(sourceId, out channel))
      {
        Log.Log.Info("DRI CC: received repeated S-VCT channel detail for source 0x{0:x}", sourceId);
        return;
      }

      channel = new ATSCChannel();
      _channels.Add(sourceId, channel);

      channel.LogicalChannelNumber = virtualChannelNumber;
      channel.IsTv = true;
      channel.IsRadio = false;
      channel.FreeToAir = false;
      channel.Frequency = _carrierFrequencies[cdsReference].Frequency;
      channel.MajorChannel = virtualChannelNumber;
      channel.MinorChannel = 0;
      channel.ModulationType = _modulationModes[mmsReference];
      channel.NetworkId = sourceId;
      channel.PhysicalChannel = _carrierFrequencies[cdsReference].Channel;
      channel.PmtPid = 0;     // TV Server will automatically lookup the correct PID from the PAT
      channel.ServiceId = programNumber;
      channel.TransportId = 0;  // We don't have these details.
      _sourcesWithoutNames.Add(sourceId);
    }

    public void OnSourceName(AtscTransmissionMedium transmissionMedium,  bool isApplication, int sourceId, string name)
    {
      if (transmissionMedium != AtscTransmissionMedium.Cable || isApplication)
      {
        return;
      }
      ATSCChannel channel = null;
      if (_channels.TryGetValue(sourceId, out channel))
      {
        channel.Name = name;
        channel.NetworkId = sourceId;
        _sourcesWithoutNames.Remove(sourceId);
        if (_sourcesWithoutNames.Count == 0)
        {
          Log.Log.Info("DRI CC: all sources now have names, assuming NTT is complete");
          OnTableComplete(MgtTableType.NttSns);
        }
      }
    }

    public void OnMgtTableDetail(int tableType, int pid, int versionNumber, uint byteCount)
    {
      /*switch ((MgtTableType)tableType)
      {
        case MgtTableType.NitCds:
        case MgtTableType.NitMms:
        case MgtTableType.NttSns:
        case MgtTableType.SvctDcm:
        case MgtTableType.SvctIcm:
        case MgtTableType.SvctVcm:
      }*/
    }

    public List<IChannel> Scan(IChannel channel, ScanParameters settings)
    {
      try
      {
        _channels.Clear();
        _sourcesWithoutNames.Clear();

        _tuner.IsScanning = true;
        _tuner.Scan(0, channel);
        _fdcService.SubscribeStateVariables(OnTableSection);
        Thread.Sleep(1000);

        _scanStage = ScanStage.Mgt;
        _mgtParser.OnTableDetail += OnMgtTableDetail;
        _mgtParser.OnTableComplete += OnTableComplete;
        _fdcService.RequestTables(new List<byte> { 0xc7 });
        _scanEvent.Reset();
        _scanEvent.WaitOne(1000);

        _scanStage = ScanStage.Nit;
        _nitParser.Reset();
        _nitParser.OnCarrierDefinition += OnCarrierDefinition;
        _nitParser.OnModulationMode += OnModulationMode;
        _nitParser.OnTableComplete += OnTableComplete;
        _fdcService.RequestTables(new List<byte> { 0xc2 });
        _scanEvent.Reset();
        _scanEvent.WaitOne(_tuner.Parameters.TimeOutSDT * 1000);

        _scanStage = ScanStage.Vct;
        _svctParser.Reset();
        _svctParser.OnChannelDetail += OnSvctChannelDetail;
        _svctParser.OnTableComplete += OnTableComplete;
        _lvctParser.Reset();
        _lvctParser.OnChannelDetail += OnLvctChannelDetail;
        _lvctParser.OnTableComplete += OnTableComplete;
        _fdcService.RequestTables(new List<byte> { 0xc4, 0xc8, 0xc9 });
        _scanEvent.Reset();
        _scanEvent.WaitOne(_tuner.Parameters.TimeOutSDT * 1000);

        _scanStage = ScanStage.Ntt;
        _nttParser.Reset();
        _nttParser.OnSourceName += OnSourceName;
        _nttParser.OnTableComplete += OnTableComplete;
        _fdcService.RequestTables(new List<byte> { 0xc3 });
        _scanEvent.Reset();
        _scanEvent.WaitOne(_tuner.Parameters.TimeOutSDT * 1000);

        foreach (int sourceId in _sourcesWithoutNames)
        {
          Log.Log.Info("DRI CC: missing name for source 0x{0:x}", sourceId);
          ATSCChannel atscChannel = _channels[sourceId];
          if (atscChannel.MinorChannel != 0)
          {
            atscChannel.Name = string.Format("Unknown {0}-{1}", atscChannel.MajorChannel, atscChannel.MinorChannel);
          }
          else
          {
            atscChannel.Name = string.Format("Unknown {0} ({1}-{2})", atscChannel.LogicalChannelNumber, atscChannel.PhysicalChannel, atscChannel.ServiceId);
          }
        }
      }
      finally
      {
        _tuner.IsScanning = false;
        _fdcService.UnsubscribeStateVariables();
      }
      return _channels.Values.Select(x => (IChannel)x).ToList();
    }

    public List<IChannel> ScanNIT(IChannel channel, ScanParameters settings)
    {
      throw new NotImplementedException("DRI CC: NIT scanning not implemented");
    }
  }
}