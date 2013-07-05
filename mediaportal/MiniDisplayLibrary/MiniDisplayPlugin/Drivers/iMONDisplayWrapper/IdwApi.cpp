//------------------------------------------------------------------------------
#include "IdwApi.h"
#include <windows.h>
//------------------------------------------------------------------------------
IdwApi::IdwApi(HINSTANCE hInstance)
: m_hInstance(hInstance)
, m_pIdwThread(NULL)
, m_nInitCount(0)
{
}
//------------------------------------------------------------------------------
IdwApi::~IdwApi()
{
}
//------------------------------------------------------------------------------
DSPResult IdwApi::Init(IDW_INITRESULT* pInitResult)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_pIdwThread = new IdwThread(m_hInstance);
    m_pIdwThread->Start();
  }
  ++m_nInitCount;
  DSPResult ret = m_pIdwThread->Init(pInitResult);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::Uninit()
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->Uninit();
  if (ret == DSP_SUCCEEDED)
  {
    --m_nInitCount;
    if (m_nInitCount == 0)
    {
      m_pIdwThread->Interrupt();
      m_pIdwThread->Join();
      delete m_pIdwThread;
    }
  }
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::IsInited()
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_S_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->IsInited();
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::IsPluginModeEnabled()
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_S_NOT_IN_PLUGIN_MODE;
  }
  DSPResult ret = m_pIdwThread->IsPluginModeEnabled();
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetVfdText(LPCWSTR lpszLine1, LPCWSTR lpszLine2)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetVfdText(lpszLine1, lpszLine2);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetVfdEqData(PDSPEQDATA pEqData)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetVfdEqData(pEqData);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdText(LPCWSTR lpszLine1)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdText(lpszLine1);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdEqData(PDSPEQDATA pEqDataL, PDSPEQDATA pEqDataR)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdEqData(pEqDataL, pEqDataR);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdAllIcons(BOOL bOn)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdAllIcons(bOn);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdOrangeIcon(BYTE btIconData1, BYTE btIconData2)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdOrangeIcon(btIconData1, btIconData2);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdMediaTypeIcon(BYTE btIconData)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdMediaTypeIcon(btIconData);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdSpeakerIcon(BYTE btIconData1, BYTE btIconData2)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdSpeakerIcon(btIconData1, btIconData2);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdVideoCodecIcon(BYTE btIconData)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdVideoCodecIcon(btIconData);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdAudioCodecIcon(BYTE btIconData)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdAudioCodecIcon(btIconData);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdAspectRatioIcon(BYTE btIconData)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdAspectRatioIcon(btIconData);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdEtcIcon(BYTE btIconData)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdEtcIcon(btIconData);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------
DSPResult IdwApi::SetLcdProgress(int nCurPos, int nTotal)
{
  m_mutex.Request();
  if (m_nInitCount == 0)
  {
    m_mutex.Release();
    return DSP_E_NOT_INITED;
  }
  DSPResult ret = m_pIdwThread->SetLcdProgress(nCurPos, nTotal);
  m_mutex.Release();
  return ret;
}
//------------------------------------------------------------------------------