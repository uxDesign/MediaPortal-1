/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#include "SvctParser.h"

extern void LogDebug(const char *fmt, ...);
extern bool DisableCRCCheck();

CSvctParser::CSvctParser(void)
{
  //SetPid(0);
  if (DisableCRCCheck())
  {
    EnableCrcCheck(false);
  }
  Reset();
  m_pCallBack = NULL;
}

CSvctParser::~CSvctParser(void)
{
}

void CSvctParser::Reset()
{
  LogDebug("SvctParser: reset");
  CSectionDecoder::Reset();
  m_mDefinedChannelMap.clear();
  m_mInverseChannelMap.clear();
  LogDebug("SvctParser: reset done");
}

void CSvctParser::SetCallBack(ISvctCallBack* callBack)
{
  m_pCallBack = callBack;
}

void CSvctParser::OnNewSection(CSection& sections)
{
  if (sections.table_id != 0xc4)
  {
    LogDebug("SvctParser: other table ID 0x%x", sections.table_id);
    return;
  }
  if (m_pCallBack == NULL)
  {
    return;
  }
  byte* section = sections.Data;

  try
  {
    int section_length = ((section[1] & 0x0f) << 8) + section[2];
    if (section_length > 1021 || section_length < 8)
    {
      LogDebug("SvctParser: invalid section length = %d", section_length);
      return;
    }
    int protocol_version = (section[3] & 0x1f);
    int transmission_medium = (section[4] >> 4);
    int table_subtype = (section[4] & 0xf);
    int vct_id = (section[5] << 8) + section[6];

    LogDebug("SvctParser: section length = %d, protocol version = %d, transmission medium = %d, table subtype = %d", section_length, protocol_version, transmission_medium, table_subtype);

    int pointer = 7;
    int subtableLength = 0;
    int endOfSection = section_length - 1;
    if (table_subtype == 0)
    {
      DecodeVirtualChannelMap(&section[7], section_length - 8, &subtableLength);
    }
    else if (table_subtype == 1)
    {
      DecodeDefinedChannelMap(&section[7], section_length - 8, &m_mDefinedChannelMap, &subtableLength);
    }
    else if (table_subtype == 2)
    {
      DecodeInverseChannelMap(&section[7], section_length - 8, &m_mInverseChannelMap, &subtableLength);
    }
    else
    {
      LogDebug("SvctParser: unsupported table subtype %d", table_subtype);
      return;   // attempting to parse descriptors after skipping the table would be bad!
    }

    pointer += subtableLength;

    while (pointer + 1 < endOfSection)
    {
      int tag = section[pointer++];
      int length = section[pointer++];
      LogDebug("SvctParser: descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d, end of section = %d, section length = %d", tag, length, pointer, pointer + length, endOfSection, section_length);
      if (pointer + length > endOfSection)
      {
        LogDebug("SvctParser: invalid descriptor length = %d, pointer = %d, end of descriptor = %d, end of section = %d, section length = %d", length, pointer, pointer + length, endOfSection, section_length);
        return;
      }

      pointer += length;
    }

    if (pointer != endOfSection)
    {
      LogDebug("SvctParser: section parsing error");
    }
  }
  catch (...)
  {
    LogDebug("SvctParser: unhandled exception in OnNewSection()");
  }
}

void CSvctParser::DecodeVirtualChannelMap(byte* b, int sectionLength, int* mapLength)
{
  const int VIRTUAL_CHANNEL_SIZE = 9;
  *mapLength = 0;
  if (sectionLength < 7)
  {
    LogDebug("SvctParser: invalid section length for virtual channel map = %d", sectionLength);
    return;
  }

  bool descriptors_included = ((b[0] & 0x20) != 0);
  bool splice = ((b[1] & 0x80) != 0);
  unsigned int activation_time = (b[2] << 24) + (b[3] << 16) + (b[4] << 8) + b[5];
  int number_of_vc_records = b[6];
  int pointer = 7;
  if ((pointer + (number_of_vc_records * VIRTUAL_CHANNEL_SIZE)) > sectionLength) // does not include descriptors - impossible to include them until we encounter them
  {
    LogDebug("SvctParser: invalid virtual channel map record count = %d, section length = %d", number_of_vc_records, sectionLength);
    return;
  }

  for (int i = 0; i < number_of_vc_records; i++)
  {
    CChannelInfo* info = NULL;
    DecodeVirtualChannel(&b[pointer], &info);
    pointer += VIRTUAL_CHANNEL_SIZE;

    if (descriptors_included)
    {
      int descriptors_count = b[pointer++];
      for (int d = 0; d < descriptors_count; d++)
      {
        if (pointer + 1 > sectionLength)
        {
          LogDebug("SvctParser: detected virtual channel descriptor count %d for channel %d is invalid in loop %d, pointer = %d, section length = %d", descriptors_count, i, d, pointer, sectionLength);
          return;
        }

        int tag = b[pointer++];
        int length = b[pointer++];
        LogDebug("SvctParser: virtual channel descriptor, tag = 0x%x, length = %d, pointer = %d, end of descriptor = %d, section length = %d", tag, length, pointer, pointer + length, sectionLength);
        if (pointer + length > sectionLength)
        {
          LogDebug("SvctParser: invalid virtual channel descriptor length = %d, pointer = %d, end of descriptor = %d, section length = %d", length, pointer, pointer + length, sectionLength);
          return;
        }

        pointer += length;
      }
    }

    if (info != NULL)
    {
      if (m_pCallBack != NULL)
      {
        m_pCallBack->OnSvctReceived(*info);
      }
      delete info;
    }
  }

  *mapLength = pointer;
}

void CSvctParser::DecodeDefinedChannelMap(byte* b, int sectionLength, map<int, bool>* dcm, int* mapLength)
{
  *mapLength = 0;
  if (sectionLength < 3)
  {
    LogDebug("SvctParser: invalid section length for defined channel map = %d", sectionLength);
    return;
  }
  if (dcm == NULL)
  {
    LogDebug("SvctParser: defined channel map pointer is NULL");
    return;
  }
  int first_virtual_channel = ((b[0] & 0xf) << 8) + b[1];
  int dcm_data_length = (b[2] & 0x7f);
  *mapLength = 3 + dcm_data_length;
  if (*mapLength > sectionLength)
  {
    LogDebug("SvctParser: invalid defined channel data length = %d, section length = %d", dcm_data_length, sectionLength);
    *mapLength = 0;
    return;
  }
  int pointer = 3;
  int currentChannel = first_virtual_channel;
  for (int i = 0; i < dcm_data_length; i++)
  {
    bool range_defined = ((b[pointer] & 0x80) != 0);
    int channels_count = (b[pointer++] & 0x7f);

    for (int c = 0; c < channels_count; c++)
    {
      (*dcm)[currentChannel++] = range_defined;
    }
  }
}

void CSvctParser::DecodeInverseChannelMap(byte* b, int sectionLength, map<int, int>* icm, int* mapLength)
{
  *mapLength = 0;
  if (sectionLength < 3)
  {
    LogDebug("SvctParser: invalid section length for inverse channel map = %d", sectionLength);
    return;
  }
  if (icm == NULL)
  {
    LogDebug("SvctParser: inverse channel map pointer is NULL");
    return;
  }
  int first_map_index = ((b[0] & 0xf) << 8) + b[1];
  int record_count = (b[2] & 0x7f);
  *mapLength = 3 + (record_count * 4);
  if (*mapLength > sectionLength)
  {
    LogDebug("SvctParser: invalid inverse channel record count = %d, section length = %d", record_count, sectionLength);
    *mapLength = 0;
    return;
  }
  int pointer = 3;
  for (int i = 0; i < record_count; i++)
  {
    int source_id = (b[pointer] << 8) + b[pointer + 1];
    pointer += 2;
    int virtual_channel_number = ((b[pointer] & 0x0f) << 8) + b[pointer + 1];
    pointer += 2;

    (*icm)[source_id] = virtual_channel_number;
  }
}

void CSvctParser::DecodeVirtualChannel(byte* b, CChannelInfo** info)
{
  int pointer = 0;
  int virtual_channel_number = ((b[pointer] & 0x0f) << 8) + b[pointer + 1];
  pointer += 2;
  bool application_virtual_channel = ((b[pointer] & 0x80) != 0);
  int path_select = ((b[pointer] >> 5) & 1);
  int transport_type = ((b[pointer] >> 4) & 1);
  int channel_type = (b[pointer++] & 0xf);
  int source_id = (b[pointer] << 8) + b[pointer + 1];
  pointer += 2;
  LogDebug("SvctParser: virtual channel, number = %d, is application = %d, path select = %d, transport type = %d, channel type = %d, source ID = 0x%x",
            virtual_channel_number, application_virtual_channel, path_select, transport_type, channel_type, source_id);

  int cds_reference = b[pointer++];
  int program_number = 0;
  int mms_reference = 0;
  bool scrambled = false;
  int video_standard = 0;
  if (transport_type == 0)  // MPEG-2 transport
  {
    program_number, mms_reference, 
    program_number = (b[pointer] << 8) + b[pointer + 1];
    pointer += 2;
    mms_reference = b[pointer++];
    LogDebug("SvctParser:   CDS reference = 0x%x, program number = 0x%x, MMS reference = 0x%x", cds_reference, program_number, mms_reference);
  }
  else  // (analog)
  {
    scrambled = ((b[pointer] & 0x80) != 0);
    video_standard = (b[pointer++] & 0x0f);
    pointer += 2;
    LogDebug("SvctParser:   CDS reference = 0x%x, scrambled = %d, video standard = %d", cds_reference, scrambled, video_standard);
  }

  // If the channel is analog, hidden, on an alternative path, or is an application, then the channel
  // is inaccessible or not meant to be shown to the user.
  if (transport_type != 0 || channel_type != 0 || path_select != 0 || application_virtual_channel)
  {
    LogDebug("SvctParser: not a user channel");
    return;
  }

  *info = new CChannelInfo();
  (*info)->LCN = virtual_channel_number;
  (*info)->NetworkId = source_id;
  (*info)->ServiceId = program_number;
}