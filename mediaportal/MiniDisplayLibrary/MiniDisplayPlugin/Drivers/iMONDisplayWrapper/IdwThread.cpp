//------------------------------------------------------------------------------
#include "IdwThread.h"
#include "iMONDisplayAPI.h"
//------------------------------------------------------------------------------
#define CLASSNAME TEXT("IDW_IMON_COM_WNDCLASS")
//------------------------------------------------------------------------------
#define WM_IDW_IMON                (WM_USER + 1)
#define WM_IDW_INIT                (WM_USER + 2)
#define WM_IDW_UNINIT              (WM_USER + 3)
#define WM_IDW_ISINITED            (WM_USER + 4)
#define WM_IDW_ISPLUGINMODEENABLED (WM_USER + 5)
#define WM_IDW_SETVFDTEXT          (WM_USER + 6)
#define WM_IDW_SETVFDEQ            (WM_USER + 7)
#define WM_IDW_SETLCDTEXT          (WM_USER + 8)
#define WM_IDW_SETLCDEQ            (WM_USER + 9)
#define WM_IDW_SETLCDALLICONS      (WM_USER + 10)
#define WM_IDW_SETLCDORANGEICON    (WM_USER + 11)
#define WM_IDW_SETLCDMEDIATYPEICON (WM_USER + 12)
#define WM_IDW_SETLCDSPEAKERICON   (WM_USER + 13)
#define WM_IDW_SETLCDVIDEOCODEC    (WM_USER + 14)
#define WM_IDW_SETLCDAUDIOCODEC    (WM_USER + 15)
#define WM_IDW_SETLCDASPECTRATIO   (WM_USER + 16)
#define WM_IDW_SETLCDETCICON       (WM_USER + 17)
#define WM_IDW_SETLCDPROGRESS      (WM_USER + 18)
#define WM_IDW_INTERRUPT           (WM_USER + 100)
//------------------------------------------------------------------------------
struct IdwImonInitResult
{
  DSPResult result;
  DSPNInitResult initResult;
  DSPType dspType;
};
struct IdwSetVfdText
{
  DSPResult result;
  LPCWSTR pszLine1;
  LPCWSTR pszLine2;
};
struct IdwSetVfdEq
{
  DSPResult result;
  PDSPEQDATA pEqData;
};
struct IdwSetLcdText
{
  DSPResult result;
  LPCWSTR pszLine1;
};
struct IdwSetLcdEq
{
  DSPResult result;
  PDSPEQDATA pEqDataL;
  PDSPEQDATA pEqDataR;
};
struct IdwSetLcdAllIcons
{
  DSPResult result;
  BOOL bOn;
};
struct IdwSetLcdIcons
{
  DSPResult result;
  BYTE btIconData;
};
struct IdwSetLcdIcons2
{
  DSPResult result;
  BYTE btIconData1;
  BYTE btIconData2;
};
struct IdwSetLcdProgress
{
  DSPResult result;
  int nCurPos;
	int nTotal;
};
//------------------------------------------------------------------------------
IdwThread::IdwThread(HINSTANCE hInstance)
: m_hInstance(hInstance)
, m_hWnd(NULL)
{
}
//------------------------------------------------------------------------------
IdwThread::~IdwThread()
{
}
//------------------------------------------------------------------------------
void IdwThread::Interrupt()
{
  if (!WaitForWindow())
    return;
  PostMessage(m_hWnd, WM_IDW_INTERRUPT, 0, 0);
}
//------------------------------------------------------------------------------
DSPResult IdwThread::Init(IDW_INITRESULT* pInitResult)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwImonInitResult result;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_INIT, (WPARAM)&result, (LPARAM)&finished);
  finished.Await();
  if (pInitResult != NULL)
  {
    pInitResult->initResult = result.initResult;
    pInitResult->dspType = result.dspType;
  }

  return result.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::Uninit()
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  DSPResult result;  
  Event finished;
  PostMessage(m_hWnd, WM_IDW_UNINIT, (WPARAM)&result, (LPARAM)&finished);
  finished.Await();

  return result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::IsInited()
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  DSPResult result;  
  Event finished;
  PostMessage(m_hWnd, WM_IDW_ISINITED, (WPARAM)&result, (LPARAM)&finished);
  finished.Await();

  return result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::IsPluginModeEnabled()
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  DSPResult result;  
  Event finished;
  PostMessage(m_hWnd, WM_IDW_ISPLUGINMODEENABLED, (WPARAM)&result,
              (LPARAM)&finished);
  finished.Await();

  return result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetVfdText(LPCWSTR lpszLine1, LPCWSTR lpszLine2)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetVfdText text;
  text.pszLine1 = lpszLine1;
  text.pszLine2 = lpszLine2;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETVFDTEXT, (WPARAM)&text, (LPARAM)&finished);
  finished.Await();

  return text.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetVfdEqData(PDSPEQDATA pEqData)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetVfdEq eq;
  eq.pEqData = pEqData;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETVFDEQ, (WPARAM)&eq, (LPARAM)&finished);
  finished.Await();

  return eq.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdText(LPCWSTR lpszLine1)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdText text;
  text.pszLine1 = lpszLine1;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDTEXT, (WPARAM)&text, (LPARAM)&finished);
  finished.Await();

  return text.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdEqData(PDSPEQDATA pEqDataL, PDSPEQDATA pEqDataR)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdEq eq;
  eq.pEqDataL = pEqDataL;
  eq.pEqDataR = pEqDataR;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDEQ, (WPARAM)&eq, (LPARAM)&finished);
  finished.Await();

  return eq.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdAllIcons(BOOL bOn)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdAllIcons allIcons;
  allIcons.bOn = bOn;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDALLICONS, 
		(WPARAM)&allIcons, (LPARAM)&finished);
  finished.Await();

  return allIcons.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdOrangeIcon(BYTE btIconData1, BYTE btIconData2)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdIcons2 iconData;
  iconData.btIconData1 = btIconData1;
  iconData.btIconData2 = btIconData2;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDORANGEICON, 
		(WPARAM)&iconData, (LPARAM)&finished);
  finished.Await();

  return iconData.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdMediaTypeIcon(BYTE btIconData)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdIcons iconData;
  iconData.btIconData = btIconData;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDMEDIATYPEICON, 
		(WPARAM)&iconData, (LPARAM)&finished);
  finished.Await();

  return iconData.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdSpeakerIcon(BYTE btIconData1, BYTE btIconData2)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdIcons2 iconData;
  iconData.btIconData1 = btIconData1;
  iconData.btIconData2 = btIconData2;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDSPEAKERICON, 
		(WPARAM)&iconData, (LPARAM)&finished);
  finished.Await();

  return iconData.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdVideoCodecIcon(BYTE btIconData)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdIcons iconData;
  iconData.btIconData = btIconData;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDVIDEOCODEC, 
		(WPARAM)&iconData, (LPARAM)&finished);
  finished.Await();

  return iconData.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdAudioCodecIcon(BYTE btIconData)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdIcons iconData;
  iconData.btIconData = btIconData;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDAUDIOCODEC, 
		(WPARAM)&iconData, (LPARAM)&finished);
  finished.Await();

  return iconData.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdAspectRatioIcon(BYTE btIconData)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdIcons iconData;
  iconData.btIconData = btIconData;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDASPECTRATIO, 
		(WPARAM)&iconData, (LPARAM)&finished);
  finished.Await();

  return iconData.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdEtcIcon(BYTE btIconData)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdIcons iconData;
  iconData.btIconData = btIconData;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDETCICON, 
		(WPARAM)&iconData, (LPARAM)&finished);
  finished.Await();

  return iconData.result;
}
//------------------------------------------------------------------------------
DSPResult IdwThread::SetLcdProgress(int nCurPos, int nTotal)
{
  if (!WaitForWindow())
    return DSP_E_FAIL;

  IdwSetLcdProgress progressData;
  progressData.nCurPos = nCurPos;
	progressData.nTotal = nTotal;
  Event finished;
  PostMessage(m_hWnd, WM_IDW_SETLCDPROGRESS, 
		(WPARAM)&progressData, (LPARAM)&finished);
  finished.Await();

  return progressData.result;
}
//------------------------------------------------------------------------------
void IdwThread::Run()
{
  if (!RegisterClass())
  {
    m_eventWindowCreationDone.Signal();
    return;
  }
  if (!CreateMessageWindow())
  {
    m_eventWindowCreationDone.Signal();
    return;
  }
  AllowImonMessages();

  m_eventWindowCreationDone.Signal();

  MSG msg;
  BOOL fGotMessage;
  while ((fGotMessage = GetMessage(&msg, (HWND) NULL, 0, 0)) != 0
       && fGotMessage != -1) 
  { 
    TranslateMessage(&msg); 
    DispatchMessage(&msg); 
  } 
}
//------------------------------------------------------------------------------
bool IdwThread::RegisterClass()
{
  WNDCLASSEX wc;
  if (GetClassInfoEx(m_hInstance, CLASSNAME, &wc))
  {
    return true;
  }

  wc.cbSize = sizeof(wc);
  wc.style = CS_HREDRAW | CS_VREDRAW;
  wc.lpfnWndProc = IdwThread::WndProc;
  wc.cbClsExtra = 0;
  wc.cbWndExtra = 0;
  wc.hInstance = m_hInstance;
  wc.hIcon = NULL;
  wc.hCursor = NULL;
  wc.hbrBackground = NULL;
  wc.lpszMenuName =  NULL;
  wc.lpszClassName = CLASSNAME;
  wc.hIconSm = NULL;
  if (::RegisterClassEx(&wc))
  {
    return true;
  }
  return false;
}
//------------------------------------------------------------------------------
bool IdwThread::CreateMessageWindow()
{
  m_hWnd = CreateWindow( 
    CLASSNAME,
    TEXT("MP iMON MessageWindow"),
    0,
    0, 0, 0, 0,
    HWND_MESSAGE,
    NULL,
    m_hInstance,
    NULL);
 
  if (!m_hWnd) 
    return false;
  return true; 
}
//------------------------------------------------------------------------------
void IdwThread::AllowImonMessages()
{
  // Determine OS
  OSVERSIONINFO version;
  version.dwOSVersionInfoSize = sizeof(version);
  GetVersionEx(&version);
  if (version.dwMajorVersion < 6)
  {
    return;
  }

  // Determine and allow iMON message number
  UINT iMonMsg =
    RegisterWindowMessage(
    TEXT("iMonMessage-431F1DC6-F31A-4AC6-A1FA-A4BB9C44FF10"));
  ChangeWindowMessageFilter(iMonMsg, MSGFLT_ADD);  
}
//------------------------------------------------------------------------------
bool IdwThread::WaitForWindow()
{
  m_eventWindowCreationDone.Await();
  if (m_hWnd == NULL)
    return false;
  return true;
}
//------------------------------------------------------------------------------
LRESULT CALLBACK IdwThread::WndProc(HWND hwnd, UINT uMsg, WPARAM wParam,
                                    LPARAM lParam)
{
  static bool initing = false;
  static IdwImonInitResult* pInitResult = NULL;
  static Event* pInitFinished = NULL;

  DSPResult* pResult;
  Event* pFinished;

  switch (uMsg) 
  { 
    case WM_CREATE: 
      return 0; 
 
    case WM_DESTROY:
      PostQuitMessage(0);
      return 0;

    case WM_IDW_IMON:
      if (initing)
      {
        initing = false;
        pInitResult->initResult = (DSPNInitResult)wParam;
        pInitResult->dspType = (DSPType)lParam;
        pInitFinished->Signal();
      }
      return 0;

    case WM_IDW_INIT:
      initing = true;
      pInitResult = (IdwImonInitResult*)wParam;
      pInitFinished = (Event*)lParam;
      pInitResult->result = IMON_Display_Init(hwnd, WM_IDW_IMON);
      if (pInitResult->result != DSP_SUCCEEDED)
      {
        initing = false;
        pInitResult->initResult = DSPN_ERR_UNKNOWN;
        pInitResult->dspType = DSPN_DSP_NONE;
        pInitFinished->Signal();
      }
      return 0;

    case WM_IDW_UNINIT:
      pResult = (DSPResult*)wParam;
      pFinished = (Event*)lParam;
      *pResult = IMON_Display_Uninit();
      pFinished->Signal();
      return 0;

    case WM_IDW_ISINITED:
      pResult = (DSPResult*)wParam;
      pFinished = (Event*)lParam;
      *pResult = IMON_Display_IsInited();
      pFinished->Signal();
      return 0;

    case WM_IDW_ISPLUGINMODEENABLED:
      pResult = (DSPResult*)wParam;
      pFinished = (Event*)lParam;
      *pResult = IMON_Display_IsPluginModeEnabled();
      pFinished->Signal();
      return 0;

    case WM_IDW_SETVFDTEXT:
			{
				IdwSetVfdText* pSetVfdText = (IdwSetVfdText*)wParam;			
				WCHAR szLine1[20];
				WCHAR szLine2[20];
				pFinished = (Event*)lParam;
				MapChars(szLine1, pSetVfdText->pszLine1, 16);
				MapChars(szLine2, pSetVfdText->pszLine2, 16);
				pSetVfdText->result = IMON_Display_SetVfdText(szLine1, szLine2);
				pFinished->Signal();
				return 0;
			}

    case WM_IDW_SETVFDEQ:
			{
				IdwSetVfdEq* pSetVfdEq = (IdwSetVfdEq*)wParam;
				pFinished = (Event*)lParam;
				pSetVfdEq->result = IMON_Display_SetVfdEqData(pSetVfdEq->pEqData);
				pFinished->Signal();
				return 0;
			}

    case WM_IDW_SETLCDTEXT:
			{
				IdwSetLcdText* pSetLcdText = (IdwSetLcdText*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdText->result = IMON_Display_SetLcdText(pSetLcdText->pszLine1);
				pFinished->Signal();
				return 0;
			}

    case WM_IDW_SETLCDEQ:
			{
				IdwSetLcdEq* pSetLcdEq = (IdwSetLcdEq*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdEq->result = 
					IMON_Display_SetLcdEqData(pSetLcdEq->pEqDataL, pSetLcdEq->pEqDataR);
				pFinished->Signal();
				return 0;
			}

		case WM_IDW_SETLCDALLICONS:
			{
				IdwSetLcdAllIcons* pSetLcdAllIcons = (IdwSetLcdAllIcons*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdAllIcons->result = 
					IMON_Display_SetLcdAllIcons(pSetLcdAllIcons->bOn);
				pFinished->Signal();
				return 0;
			}

		case WM_IDW_SETLCDORANGEICON:
			{
				IdwSetLcdIcons2* pSetLcdIcons2 = (IdwSetLcdIcons2*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdIcons2->result = IMON_Display_SetLcdOrangeIcon(
					pSetLcdIcons2->btIconData1, pSetLcdIcons2->btIconData2);
				pFinished->Signal();
				return 0;
			}

	  case WM_IDW_SETLCDMEDIATYPEICON:
			{
				IdwSetLcdIcons* pSetLcdIcons = (IdwSetLcdIcons*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdIcons->result = 
					IMON_Display_SetLcdMediaTypeIcon(pSetLcdIcons->btIconData);
				pFinished->Signal();
				return 0;
			}

		case WM_IDW_SETLCDSPEAKERICON:
			{
				IdwSetLcdIcons2* pSetLcdIcons2 = (IdwSetLcdIcons2*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdIcons2->result = IMON_Display_SetLcdSpeakerIcon(
					pSetLcdIcons2->btIconData1, pSetLcdIcons2->btIconData2);
				pFinished->Signal();
				return 0;
			}

	  case WM_IDW_SETLCDVIDEOCODEC:
			{
				IdwSetLcdIcons* pSetLcdIcons = (IdwSetLcdIcons*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdIcons->result = 
					IMON_Display_SetLcdVideoCodecIcon(pSetLcdIcons->btIconData);
				pFinished->Signal();
				return 0;
			}

		case WM_IDW_SETLCDAUDIOCODEC:
			{
				IdwSetLcdIcons* pSetLcdIcons = (IdwSetLcdIcons*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdIcons->result = 
					IMON_Display_SetLcdAudioCodecIcon(pSetLcdIcons->btIconData);
				pFinished->Signal();
				return 0;
			}

		case WM_IDW_SETLCDASPECTRATIO:
			{
				IdwSetLcdIcons* pSetLcdIcons = (IdwSetLcdIcons*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdIcons->result = 
					IMON_Display_SetLcdAspectRatioIcon(pSetLcdIcons->btIconData);
				pFinished->Signal();
				return 0;
			}

		case WM_IDW_SETLCDETCICON:
			{
				IdwSetLcdIcons* pSetLcdIcons = (IdwSetLcdIcons*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdIcons->result = 
					IMON_Display_SetLcdEtcIcon(pSetLcdIcons->btIconData);
				pFinished->Signal();
				return 0;
			}

		case WM_IDW_SETLCDPROGRESS:
			{
				IdwSetLcdProgress* pSetLcdProgress = (IdwSetLcdProgress*)wParam;
				pFinished = (Event*)lParam;
				pSetLcdProgress->result = IMON_Display_SetLcdProgress(
					pSetLcdProgress->nCurPos, pSetLcdProgress->nTotal);
				pFinished->Signal();
				return 0;
			}

    case WM_IDW_INTERRUPT:
      DestroyWindow(hwnd);
      return 0;
	}
  return DefWindowProc(hwnd, uMsg, wParam, lParam);
}
//------------------------------------------------------------------------------
void IdwThread::MapChars(LPWSTR lpszTarget, LPCWSTR lpszSource,
                         int nMaxLength)
{
  int len = (int)wcslen(lpszSource);
  if ((nMaxLength > 0) && (len > nMaxLength))
    len = nMaxLength;
  lpszTarget[len] = 0;
  for (int i = 0; i < len; ++i)
  {
    wchar_t ch = lpszSource[i];
    lpszTarget[i] = MapChar(ch);
  }
}
//------------------------------------------------------------------------------
#define IN_RANGE(ch, s, e) ((ch >= s) && (ch <= e))
#define IS(ch, c) (ch == c)
wchar_t IdwThread::MapChar(wchar_t ch)
{
  if (IN_RANGE(ch, 0x0020, 0x005B)
   || IN_RANGE(ch, 0x005D, 0x007D)
   || IS(ch, 0x0401)
   || IS(ch, 0x0404)
   || IN_RANGE(ch, 0x0406, 0x0407)
   || IN_RANGE(ch, 0x0410, 0x044F)
   || IS(ch, 0x0451)
   || IS(ch, 0x0454)
   || IN_RANGE(ch, 0x0456, 0x0457)
   || IN_RANGE(ch, 0x0490, 0x0491))
    return ch;

  switch (ch)
  {
  case 0x5C:
    return 0x8C;
  case 0x7E:
    return 0x8E;
  case 0x7F:
    return 0x20;
  case 0xA2:
    return 0xEC;
  case 0xA3:
    return 0x92;
  case 0xA5:
    return 0x5C;
  case 0xA6:
    return 0x98;
  case 0xA7:
    return 0x8F;
  case 0xB0:
    return 0xDF;
  case 0xB5:
    return 0xE4;
  case 0xC2:
    return 0x82;
  case 0xC4:
    return 0x80;
  case 0xC5:
    return 0x81;
  case 0xC6:
    return 0x90;
  case 0xC7:
    return 0x99;
  case 0xD1:
    return 0xEE;
  case 0xD6:
    return 0x86;
  case 0xD8:
    return 0x88;
  case 0xDC:
    return 0x8A;
  case 0xDE:
    return 0xF0;
  case 0xDF:
    return 0xE2;
  case 0xE1:
    return 0x83;
  case 0xE4:
    return 0xE1;
  case 0xE5:
    return 0x84;
  case 0xE7:
    return 0x99;
  case 0xF1:
    return 0xD1;
  case 0xF6:
    return 0x87;
  case 0xF7:
    return 0xFD;
  case 0xF8:
    return 0x89;
  case 0xFC:
    return 0x8B;
  case 0xFE:
    return 0xF0;
  default:
    return L'#';
  }
}
//------------------------------------------------------------------------------
