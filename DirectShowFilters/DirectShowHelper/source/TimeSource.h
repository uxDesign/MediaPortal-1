// Copyright (C) 2005-2010 Team MediaPortal
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
//
// cMulDiv64 based on Richard van der Wal's ASM version (R.vdWal@xs4all.nl)


// LONGLONG GetCurrentTimestamp();
 __int64 _stdcall cMulDiv64(__int64 operant, __int64 multiplier, __int64 divider);

static BOOL           g_bTimerInitializer = false;
static BOOL           g_bQPCAvail = false;
static double         g_dPerfPeriodScaled;

static CCritSec lock;  // lock for timer initialization (multiple threads are using the timer during startup)

inline LONGLONG GetCurrentTimestamp()
{
  LONGLONG result;
  if (!g_bTimerInitializer)
  {
    CAutoLock lock(&lock);
    LARGE_INTEGER  g_lPerfFrequency;
    g_lPerfFrequency.QuadPart = 0;
    g_bQPCAvail = QueryPerformanceFrequency((LARGE_INTEGER*)&g_lPerfFrequency);
    g_bTimerInitializer = true;
    if( g_lPerfFrequency.QuadPart == 0)
    {
      // Bug in HW? Frequency cannot be zero
      g_bQPCAvail = false;
    }
    else
    {
      g_dPerfPeriodScaled = 10000000.0/(double)g_lPerfFrequency.QuadPart;
    }
  }
  if (g_bQPCAvail)
  {
    ULARGE_INTEGER tics;
    QueryPerformanceCounter((LARGE_INTEGER*)&tics);
    result = (LONGLONG)(g_dPerfPeriodScaled * (double)tics.QuadPart);
  }
  else
  {
    result = timeGetTime() * 10000; // ms to 100ns units
  }
  return result;
}
