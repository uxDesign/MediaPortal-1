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
#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "channelscan.h"
#include "..\..\shared\channelinfo.h"
#include "tswriter.h"

extern void LogDebug(const char *fmt, ...) ;

CChannelScan::CChannelScan(LPUNKNOWN pUnk, HRESULT *phr, CMpTsFilter* filter) 
:CUnknown( NAME ("MpTsChannelScan"), pUnk)
{
  m_bIsParsingNIT=false;
	m_bIsParsing=false;
  m_bIsCableScan = false;
	m_pFilter=filter;
	m_pCallback=NULL;
}
CChannelScan::~CChannelScan(void)
{
  CleanUp();
}

STDMETHODIMP CChannelScan::SetCallBack(IChannelScanCallback* callback)
{
	m_pCallback=callback;
	return S_OK;
}
STDMETHODIMP CChannelScan::Start(bool waitForVCT, bool isCableScan)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		m_patParser.Reset(m_pCallback,waitForVCT);
		m_bIsParsing=true;
    m_bIsCableScan = isCableScan;
    if (isCableScan)
    {
      CleanUp();
      m_nttParser.Reset();
      m_nttParser.SetCallBack(this);
      m_svctParser.Reset();
      m_svctParser.SetCallBack(this);
    }
	}
	catch(...)
	{
		LogDebug("analyzer CChannelScan::Start exception");
	}
	return S_OK;
}
STDMETHODIMP CChannelScan::Stop()
{
	CEnterCriticalSection enter(m_section);
	m_bIsParsing=false;
	try
	{
		m_pCallback=NULL;
		m_patParser.Reset(NULL,false);
    m_nttParser.SetCallBack(NULL);
    m_svctParser.SetCallBack(NULL);
	}
	catch(...)
	{
		LogDebug("analyzer CChannelScan::Stop exception");
	}
	return S_OK;
}

STDMETHODIMP CChannelScan::GetCount(int* channelCount)
{
	CEnterCriticalSection enter(m_section);
	try
	{
    if (m_bIsCableScan)
    {
      *channelCount = m_mCableServices.size();
    }
    else
    {
		  *channelCount=m_patParser.Count();
    }
	}
	catch(...)
	{
		LogDebug("analyzer CChannelScan::GetCount exception");
	}
	return S_OK;
}

STDMETHODIMP CChannelScan::IsReady( BOOL* yesNo) 
{
	CEnterCriticalSection enter(m_section);
	try
	{
    if (m_bIsCableScan)
    {
      *yesNo = false;
      return S_OK;
    }
		*yesNo=m_patParser.IsReady();
		if (*yesNo)
		{
			m_bIsParsing=false;
		}
	}
	catch(...)
	{
		LogDebug("analyzer CChannelScan::IsReady exception");
	}
	return S_OK;
}
STDMETHODIMP CChannelScan::GetChannel(int index,
									 long* networkId,
									 long* transportId,
									 long* serviceId,
									 int* majorChannel,
									 int* minorChannel,
									 int* frequency,
									 int* lcn,
									 int* freeCAMode,
									 int* serviceType,
									 int* modulation,
									 char** providerName,
									 char** serviceName,
									 int* pmtPid,
									 int* hasVideo,
									 int* hasAudio,
									 int* hasCaDescriptor)
{
	static char sServiceName[128];
	static char sProviderName[128];
	CEnterCriticalSection enter(m_section);
	try
	{
		strcpy(sServiceName,"");
		strcpy(sProviderName,"");
		*networkId=0;
		*transportId=0;
		*serviceId=0;
		*pmtPid=0;
		*lcn=10000;

    if (m_bIsCableScan)
    {
      map<int, CChannelInfo*>::iterator it = m_mCableServices.begin();
      int pos = 0;
      while (it != m_mCableServices.end())
      {
        if (pos == index)
        {
          CChannelInfo* info = it->second;
          *networkId = it->first; // source_id
          *serviceId = info->ServiceId;
          *lcn = info->LCN;
          *pmtPid = 0;
          strcpy(sServiceName, info->ServiceName);
          *serviceName = sServiceName;
          *serviceType = 2; // digital TV
          LogDebug("%4d) %-25s  source ID = 0x%4x service ID = 0x%4x LCN = %d",
            index, info->ServiceName, info->NetworkId, info->ServiceId, info->LCN);
          return S_OK;
        }
        pos++;
        it++;
      }
      return S_OK;
    }

		CChannelInfo info;
		info.Reset();
		if ( m_patParser.GetChannel(index, info))
		{
			*lcn=info.LCN;
			*networkId=info.NetworkId;
			*transportId=info.TransportId;
			*serviceId=info.ServiceId;
			*majorChannel=info.MajorChannel;
			*minorChannel=info.MinorChannel;
			*serviceType=info.ServiceType;
			*modulation=info.Modulation;
			strcpy(sProviderName,info.ProviderName);
			strcpy(sServiceName,info.ServiceName);
			*providerName=sProviderName;
			*serviceName=sServiceName;
			*pmtPid=info.PidTable.PmtPid;
			*freeCAMode=info.FreeCAMode;
			*hasVideo=info.hasVideo;
			*hasAudio=info.hasAudio;
			*hasCaDescriptor=info.hasCaDescriptor;
		}
	}
	catch(...)
	{
		LogDebug("analyzer CChannelScan::GetChannel exception");
	}
	return S_OK;
}

void CChannelScan::OnTsPacket(byte* tsPacket)
{
	CEnterCriticalSection enter(m_section);

	if (m_bIsParsing)
		m_patParser.OnTsPacket(tsPacket);

	if (m_bIsParsingNIT)
    m_nit.OnTsPacket(tsPacket);
}

void CChannelScan::CleanUp()
{
  map<int, CChannelInfo*>::iterator it = m_mCableServices.begin();
  while (it != m_mCableServices.end())
  {
    CChannelInfo* info = it->second;
    if (info != NULL)
    {
      delete info;
    }
  }
  m_mCableServices.clear();
}

void CChannelScan::OnOobSiSection(CSection& section)
{
	CEnterCriticalSection enter(m_section);

	if (m_bIsParsing && m_bIsCableScan)
  {
    m_nttParser.OnNewSection(section);
    m_svctParser.OnNewSection(section);
  }
}

void CChannelScan::OnSvctReceived(const CChannelInfo& vctInfo)
{
  CChannelInfo* info = NULL;
  map<int, CChannelInfo*>::iterator it = m_mCableServices.find(vctInfo.NetworkId);
  if (it == m_mCableServices.end())
  {
    info = new CChannelInfo();
    m_mCableServices[vctInfo.NetworkId] = info;
  }
  else
  {
    info = it->second;
  }

  if (info != NULL)
  {
    info->NetworkId = vctInfo.NetworkId;
    info->LCN = vctInfo.LCN;
    info->ServiceId = vctInfo.ServiceId;
    info->SdtReceived = true;
  }
}

void CChannelScan::OnNttReceived(int sourceId, int applicationType, char* name, unsigned int lang)
{
  if (applicationType == 1)
  {
    // Not supported - a data service.
    return;
  }
  CChannelInfo* info = NULL;
  map<int, CChannelInfo*>::iterator it = m_mCableServices.find(sourceId);
  if (it == m_mCableServices.end())
  {
    info = new CChannelInfo();
    m_mCableServices[sourceId] = info;
  }
  else
  {
    info = it->second;
  }

  if (info != NULL)
  {
    info->NetworkId = sourceId;
    if (name != NULL && strlen(name) < 254)
    {
      strcpy(info->ServiceName, name);
    }
  }
}

STDMETHODIMP CChannelScan::ScanNIT()
{
  m_nit.Reset();
  m_bIsParsingNIT=true;
  return 0;
}

STDMETHODIMP CChannelScan::StopNIT()
{
  m_bIsParsingNIT=false;
  return 0;
}

STDMETHODIMP CChannelScan::GetNITCount(int* transponderCount)
{
  *transponderCount=0;
  if (m_nit.m_nit.satteliteNIT.size()>0) *transponderCount= m_nit.m_nit.satteliteNIT.size();
  else if (m_nit.m_nit.cableNIT.size()>0) *transponderCount= m_nit.m_nit.cableNIT.size();
  else if (m_nit.m_nit.terrestialNIT.size()>0) *transponderCount= m_nit.m_nit.terrestialNIT.size();
  return 0;
}

STDMETHODIMP CChannelScan::GetNITChannel(int channel,int* type,int* frequency,int *polarisation, int* modulation, int* symbolrate, int* bandwidth, int* fecInner, int* rollOff, char** networkName)
{
	static char sNetworkName[128];
	strcpy(sNetworkName,"");
  *frequency=0;
  *polarisation=0;
  *modulation=0;
  *symbolrate=0;
  *bandwidth=0;
  *fecInner=0;
  *type=-1;
  *rollOff = BDA_ROLL_OFF_NOT_SET;
	*networkName=sNetworkName;

  if (m_nit.m_nit.satteliteNIT.size()>0)
  {
    if (channel<0 || channel >=m_nit.m_nit.satteliteNIT.size()) return 0;
    NITSatDescriptor& des = m_nit.m_nit.satteliteNIT[channel];
    *frequency=des.Frequency;
    *polarisation=des.Polarisation;
    *modulation=des.Modulation;
    *symbolrate=des.Symbolrate;
    *fecInner=des.FECInner;
    *rollOff=des.RollOff;
    strcpy(sNetworkName,des.NetworkName.c_str());
	  *networkName=sNetworkName;
    *type=0;
    return 0;
  }
  if (m_nit.m_nit.cableNIT.size()>0)
  {
    if (channel<0 || channel >=m_nit.m_nit.cableNIT.size()) return 0;
    NITCableDescriptor& des = m_nit.m_nit.cableNIT[channel];
    *frequency=des.Frequency;
    *modulation=des.Modulation;
    *symbolrate=des.Symbolrate;
    *fecInner=des.FECInner;
    strcpy(sNetworkName,des.NetworkName.c_str());
	  *networkName=sNetworkName;
    *type=1;
    return 0;
  }
  if (m_nit.m_nit.terrestialNIT.size()>0)
  {
    if (channel<0 || channel >=m_nit.m_nit.terrestialNIT.size()) return 0;
    NITTerrestrialDescriptor& des = m_nit.m_nit.terrestialNIT[channel];
    *frequency=des.CentreFrequency;
    *bandwidth=des.Bandwidth;
    strcpy(sNetworkName,des.NetworkName.c_str());
	  *networkName=sNetworkName;
    *type=2;
    return 0;
  }
  return 0;
}
