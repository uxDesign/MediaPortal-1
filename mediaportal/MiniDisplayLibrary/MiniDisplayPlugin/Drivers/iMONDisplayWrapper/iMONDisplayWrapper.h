//------------------------------------------------------------------------------
#ifndef IMONDISPLAYWRAPPER_H_INCLUDED
#define IMONDISPLAYWRAPPER_H_INCLUDED
//------------------------------------------------------------------------------
#include "iMONDisplayDefines.h"
#include <windows.h>
//------------------------------------------------------------------------------
#ifdef IMONDISPLAYWRAPPER_EXPORTS
#define IMONDSPWRAPPER __declspec(dllexport)
#else
#define IMONDSPWRAPPER __declspec(dllimport)
#endif
//------------------------------------------------------------------------------
typedef struct _IDW_INITRESULT
{
  DSPNInitResult initResult;
  DSPType dspType;
} IDW_INITRESULT;
//------------------------------------------------------------------------------
#ifdef __cplusplus
extern "C" 
{
#endif
//------------------------------------------------------------------------------
IMONDSPWRAPPER DSPResult IDW_Init(IDW_INITRESULT* pInitResult);
IMONDSPWRAPPER DSPResult IDW_Uninit();
IMONDSPWRAPPER DSPResult IDW_IsInited();
IMONDSPWRAPPER DSPResult IDW_IsPluginModeEnabled();
IMONDSPWRAPPER DSPResult IDW_SetVfdText(LPCWSTR lpszLine1, LPCWSTR lpszLine2);
IMONDSPWRAPPER DSPResult IDW_SetVfdEqData(PDSPEQDATA pEqData);
IMONDSPWRAPPER DSPResult IDW_SetLcdText(LPCWSTR lpszLine1);
IMONDSPWRAPPER DSPResult IDW_SetLcdEqData(PDSPEQDATA pEqDataL, PDSPEQDATA pEqDataR);
IMONDSPWRAPPER DSPResult IDW_SetLcdAllIcons(BOOL bOn);
IMONDSPWRAPPER DSPResult IDW_SetLcdOrangeIcon(BYTE btIconData1, BYTE btIconData2);
IMONDSPWRAPPER DSPResult IDW_SetLcdMediaTypeIcon(BYTE btIconData);
IMONDSPWRAPPER DSPResult IDW_SetLcdSpeakerIcon(BYTE btIconData1, BYTE btIconData2);
IMONDSPWRAPPER DSPResult IDW_SetLcdVideoCodecIcon(BYTE btIconData);
IMONDSPWRAPPER DSPResult IDW_SetLcdAudioCodecIcon(BYTE btIconData);
IMONDSPWRAPPER DSPResult IDW_SetLcdAspectRatioIcon(BYTE btIconData);
IMONDSPWRAPPER DSPResult IDW_SetLcdEtcIcon(BYTE btIconData);
IMONDSPWRAPPER DSPResult IDW_SetLcdProgress(int nCurPos, int nTotal);
//------------------------------------------------------------------------------
#ifdef __cplusplus
}
#endif
//------------------------------------------------------------------------------
#endif
//------------------------------------------------------------------------------
