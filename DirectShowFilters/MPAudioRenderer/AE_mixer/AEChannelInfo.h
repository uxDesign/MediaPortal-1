#pragma once
/*
 *      Copyright (C) 2010-2012 Team XBMC
 *      http://xbmc.org
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
 *  along with XBMC; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#include "..\source\stdafx.h"
#include "StdString.h"

/**
 * The possible channels
 */
enum AEChannel
{
  AE_CH_NULL = -1,
  AE_CH_RAW ,

  AE_CH_FL  , AE_CH_FR , AE_CH_FC , AE_CH_LFE, AE_CH_BL  , AE_CH_BR  , AE_CH_FLOC,
  AE_CH_FROC, AE_CH_BC , AE_CH_SL , AE_CH_SR , AE_CH_TFL , AE_CH_TFR , AE_CH_TFC ,
  AE_CH_TC  , AE_CH_TBL, AE_CH_TBR, AE_CH_TBC, AE_CH_BLOC, AE_CH_BROC,

  /* p16v devices */
  AE_CH_UNKNOWN1,
  AE_CH_UNKNOWN2,
  AE_CH_UNKNOWN3,
  AE_CH_UNKNOWN4,
  AE_CH_UNKNOWN5,
  AE_CH_UNKNOWN6,
  AE_CH_UNKNOWN7,
  AE_CH_UNKNOWN8,

  AE_CH_MAX
};

/**
 * Standard channel layouts
 */
enum AEStdChLayout
{
  AE_CH_LAYOUT_INVALID = -1,

  AE_CH_LAYOUT_1_0 = 0,
  AE_CH_LAYOUT_2_0,
  AE_CH_LAYOUT_2_1,
  AE_CH_LAYOUT_3_0,
  AE_CH_LAYOUT_3_1,
  AE_CH_LAYOUT_4_0,
  AE_CH_LAYOUT_4_1,
  AE_CH_LAYOUT_5_0,
  AE_CH_LAYOUT_5_1,
  AE_CH_LAYOUT_6_1_0x13f,
  AE_CH_LAYOUT_6_1_0x70f,
  AE_CH_LAYOUT_7_0,
  AE_CH_LAYOUT_7_1,
  AE_AC3_CH_LAYOUT_1_0,     // Mono (Center only)
  AE_AC3_CH_LAYOUT_2_0,     // 2-channel stereo (Left + Right), optionally carrying matrixed Dolby Surround
  AE_AC3_CH_LAYOUT_2_S,     // 2-channel stereo with mono surround (Left, Right, Surround)
  AE_AC3_CH_LAYOUT_3_0,     // 3-channel stereo (Left, Center, Right)
  AE_AC3_CH_LAYOUT_3_S,     // 3-channel stereo with mono surround (Left, Center, Right, Surround)
  AE_AC3_CH_LAYOUT_4_0,     // 4-channel quadraphonic (Left, Right, Left Surround, Right Surround)
  AE_AC3_CH_LAYOUT_5_0,     // 5-channel surround (Left, Center, Right, Left Surround, Right Surround)
  AE_AC3_CH_LAYOUT_5_1,     // 5-channel surround + LFE (Left, Center, Right, Left Surround, Right Surround, LFE)

  AE_CH_LAYOUT_MAX
};

class CAEChannelInfo {
public:
  CAEChannelInfo();
  CAEChannelInfo(const enum AEChannel* rhs);
  CAEChannelInfo(const enum AEStdChLayout rhs);
  ~CAEChannelInfo();
  CAEChannelInfo& operator=(const CAEChannelInfo& rhs);
  CAEChannelInfo& operator=(const enum AEChannel* rhs);
  CAEChannelInfo& operator=(const enum AEStdChLayout rhs);
  bool operator==(const CAEChannelInfo& rhs);
  bool operator!=(const CAEChannelInfo& rhs);
  void operator+=(const enum AEChannel rhs);
  const enum AEChannel operator[](unsigned int i) const;
  operator std::string();

  /* remove any channels that dont exist in the provided info */
  void ResolveChannels(const CAEChannelInfo& rhs);
  void Reset();
  inline unsigned int Count() const { return m_channelCount; }
  static const char* GetChName(const enum AEChannel ch);
  bool HasChannel(const enum AEChannel ch) const;
  bool ContainsChannels(CAEChannelInfo& rhs) const;
private:
  unsigned int   m_channelCount;
  enum AEChannel m_channels[AE_CH_MAX];
};

